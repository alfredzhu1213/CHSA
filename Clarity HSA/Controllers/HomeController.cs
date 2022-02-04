using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Clarity_HSA.Models;
using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Clarity_HSA.Controllers
{
    

    public class HomeController : Controller
    {
        Tamir.SharpSsh.Sftp Alegeus_ftp = new Tamir.SharpSsh.Sftp("ftp.wealthcareadmin.com", "f", "xxx");
        Tamir.SharpSsh.Sftp Clarity_ftp = new Tamir.SharpSsh.Sftp("ftp.flexaccount.com", "f", "xxx");


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Cron()
        {
            string output = "";

            // BEGIN SCHEDULED REPORTS AND FEEDS

            ClarityContextContainer context = new ClarityContextContainer();
            List<Report> reports = (from r in context.Reports select r).ToList<Report>();
            List<Organization> cachedEmployerOrganizations = (List<Organization>)Session["employer_organizations"];

            foreach (Report report in reports)
            {
                try
                {
                    bool should_run = false;

                    if (report.organization != null)
                    {
                        output += "Evaluating Report Schedule for Report: " + report.name + "(Organization: " + report.organization.name + ")" + "<br />";
                    }
                    else
                    {
                        output += "Evaluating Report Schedule for Report: " + report.name + "(Global)" + "<br />";
                    }

                    // Should we run the report?
                    if (report.organization != null || true)
                    {
                        if (report.delivery_type == "ftp_sftp" || report.delivery_type == "secure_email")
                        {
                            foreach (CustomReportSchedule schedule in report.custom_report_schedules)
                            {
                                DateTime dt_schedule = Convert.ToDateTime(schedule.start_date);
                                DateTime dt_today = DateTime.Today;

                                if (schedule.recurrence == "None")
                                {
                                    if (dt_schedule.Year == dt_today.Year && dt_schedule.Month == dt_today.Month && dt_schedule.Day == dt_today.Day)
                                    {
                                        should_run = true;
                                        output += "Matching Schedule Record Found, Recurrence: " + schedule.recurrence + ", Start Date: " + schedule.start_date + "<br />";
                                    }
                                    else
                                    {
                                        output += "Non-Matching Schedule Record Found, Recurrence: " + schedule.recurrence + ", Start Date: " + schedule.start_date + "<br />";
                                    }
                                }
                                else if (schedule.recurrence == "Daily")
                                {
                                    should_run = true;
                                    output += "Matching Schedule Record Found, Recurrence: " + schedule.recurrence + ", Start Date: " + schedule.start_date + "<br />";
                                }
                                else if (schedule.recurrence == "Weekly")
                                {
                                    if (dt_schedule.DayOfWeek == dt_today.DayOfWeek)
                                    {
                                        should_run = true;
                                        output += "Matching Schedule Record Found, Recurrence: " + schedule.recurrence + ", Start Date: " + schedule.start_date + "<br />";
                                    }
                                    else
                                    {
                                        output += "Non-Matching Schedule Record Found, Recurrence: " + schedule.recurrence + ", Start Date: " + schedule.start_date + "<br />";
                                    }
                                }
                                else if (schedule.recurrence == "Bi-Weekly")
                                {
                                    if ((dt_today - Convert.ToDateTime(schedule.start_date)).TotalDays == 14)
                                    {
                                        should_run = true;
                                        output += "Matching Schedule Record Found, Recurrence: " + schedule.recurrence + ", Start Date: " + schedule.start_date + "<br />";
                                        schedule.start_date = dt_today.ToString("yyyy-MM-dd");
                                    }
                                    else
                                    {
                                        output += "Non-Matching Schedule Record Found, Recurrence: " + schedule.recurrence + ", Start Date: " + schedule.start_date + "<br />";
                                    }
                                    
                                }
                                else if (schedule.recurrence == "Monthly")
                                {
                                    if (dt_schedule.Day == dt_today.Day)
                                    {
                                        should_run = true;
                                        output += "Matching Schedule Record Found, Recurrence: " + schedule.recurrence + ", Start Date: " + schedule.start_date + "<br />";
                                    }
                                    else
                                    {
                                        output += "Non-Matching Schedule Record Found, Recurrence: " + schedule.recurrence + ", Start Date: " + schedule.start_date + "<br />";
                                    }
                                }
                            }
                        }
                        else
                        {
                            output += "Download File Report, Not Executing!" + "<br />";
                        }
                    }
                    else
                    {
                        output += "Global Report, Not Executing!" + "<br />";
                    }

                    if (should_run)
                    {
                        if (report.is_report_builder)
                        {
                            // Feed Builder:

                            if (report.xml_file_path != null && report.xml_file_path != "")
                            {
                                string xml_file = System.IO.File.ReadAllText(report.xml_file_path);
                                output += "Read XML File: " + report.xml_file_path + "<br />";

                                ReportBuilderController controller = new ReportBuilderController();
                                controller.RunActualReport(report, xml_file);

                                output += "Executed Feed Builder Report: " + report.name + "<br />";
                            }
                            else
                            {
                                output += "No XML File Path Defined for Feed Builder Report, Not Executing!" + "<br />";
                            }
                        }
                        else
                        {
                            // HSA Platform:

                            // Populate the proper session variables for employee roll-up:
                            List<Organization> org_list = new List<Organization>();
                            if (report.organization == null)
                            {
                                org_list = context.Organizations.ToList<Organization>();
                            }
                            else
                            {
                                org_list.Add(report.organization);
                            }
                            Session["employer_organizations"] = org_list;
                            Session["active_organization"] = -1;
                            Session["employee_search"] = "";

                            EmployerController controller = new EmployerController();
                            controller.RunActualReport(report);

                            output += "Executed HSA Platform Report: " + report.name + "<br />";
                        }
                    }
                    else
                    {
                        output += "Skipped Report: " + report.name + "<br />";
                    }
                }
                catch (Exception ee)
                {
                    output = ee.ToString();
                    //break;
                }
            }

            Session["employer_organizations"] = cachedEmployerOrganizations;

            // END SCHEDULED REPORTS AND FEEDS

            CronLog log = new CronLog();
            log.cron_timestamp = DateTime.Now;
            log.cron_type = "Reports";
            log.cron_log = output;
            context.CronLogs.Add(log);
            context.SaveChanges();

            ViewData["output"] = output;

            return View();
        }

        public ActionResult AlegeusCron()
        {
            string output = "";

            // BEGIN ALEGEUS INTEGRATION

            string mbi_template = System.IO.File.ReadAllText(Server.MapPath("~/Content/ilTemplate.mbi"));
          

            DateTime today = DateTime.Today;
            DateTime yesterday = DateTime.Today.AddDays(-1);

           


            string mbi = mbi_template.Replace("AAAAAAAA", yesterday.ToString("yyyyMMdd"));
            mbi = mbi.Replace("BBBBBBBB", today.ToString("yyyyMMdd"));
            output += "Read MBI Template for Alegeus IL File: " + Server.MapPath("~/Content/il.mbi") + "<br />";
            MemoryStream memoryStream = new MemoryStream();
            TextWriter tw = new StreamWriter(memoryStream);
            tw.Write(mbi);
            tw.Flush();
            tw.Close();

            //Write altered template data to actual outgoing il file
            FileStream file = new FileStream(Server.MapPath("~/Content/il.txt"), FileMode.Create, System.IO.FileAccess.Write);
            byte[] bytes = memoryStream.GetBuffer();
            file.Write(bytes, 0, bytes.Length);
            memoryStream.Close();
            file.Close();
            ///copy from txt to outgoing .mbi
            //string mbi_txt = System.IO.File.ReadAllText(Server.MapPath("~/Content/il.txt"));
            StreamWriter sw = new StreamWriter(Server.MapPath("~/Content/il.mbi"));
            sw.Write("");
            sw.Write(mbi);
            sw.Close();
            
           
                
            string filename = "HSA_IL_" + today.ToString("yyyyMMdd") + ".mbi";
            Alegeus_ftp.Connect();
            Alegeus_ftp.Put(Server.MapPath("~/Content/il.mbi"), filename);
            Alegeus_ftp.Close();

            output += "Wrote MBI File: " + "ftp://ftp.wealthcareadmin.com/" + filename + "<br />";

            // END ALEGEUS INTEGRATION

            ClarityContextContainer context = new ClarityContextContainer();
            CronLog log = new CronLog();
            log.cron_timestamp = DateTime.Now;
            log.cron_type = "Alegeus Data Request";
            log.cron_log = output;
            context.CronLogs.Add(log);
            context.SaveChanges();

            ViewData["output"] = output;

            return View();
        }

        public ActionResult AlegeusCron2()
        {
            string output = "";
           
            // BEGIN ALEGEUS INTEGRATION
            DateTime today = DateTime.Today;
           
            string export_textfile = "C:\\alegeus\\ilExportData.txt";
            
           

            string filename = "HSA_IL_" + today.ToString("yyyyMMdd") + ".mbi";
            ClarityContextContainer context = new ClarityContextContainer();

            try
            {

                string export_filename = "HSA_IL_" + today.ToString("yyyyMMdd") + ".exp";
                output += "Attempting to Find EXP File on FTP Server: " + export_filename + "<br />";

                Alegeus_ftp.Connect();
                Alegeus_ftp.Get(export_filename, export_textfile);
                Alegeus_ftp.Close();



                StreamReader reader = new StreamReader(export_textfile);
                string exp_contents = "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30\r\n" + reader.ReadToEnd();
                

                MemoryStream memoryStreamExport = new MemoryStream();
                TextWriter twExport = new StreamWriter(memoryStreamExport);
                twExport.Write(exp_contents);
                twExport.Flush();
                twExport.Close();

                using (FileStream file = new FileStream("C:/alegeus/il.txt", FileMode.Create, System.IO.FileAccess.Write))
                {
                    byte[] bytes = memoryStreamExport.GetBuffer();
                    file.Write(bytes, 0, bytes.Length);
                    memoryStreamExport.Close();
                    output += "Found EXP File, Saved to Local Filesystem: " + "C:/alegeus/il.txt" + "<br />";

                    List<Organization> cachedEmployerOrganizations = (List<Organization>)Session["employer_organizations"];
                    List<Organization> org_list = new List<Organization>();
                    Organization organization = (from o in context.Organizations where o.id == 1 select o).First<Organization>();
                    org_list.Add(organization);
                    Session["employer_organizations"] = org_list;
                    Session["active_organization"] = 1;
                    Session["employee_search"] = "";

                    file.Close();

                    EmployerController controller = new EmployerController();
                    controller.ImportFeeds();

                    output += "Imported IL File!<br />";

                    Session["employer_organizations"] = cachedEmployerOrganizations;
                }
            }
            catch (Exception ee)
            {
                output += "<br />Error! " + ee.ToString() + "<br /><br />";
                output += "No EXP File Found!<br />";
            }

            // END ALEGEUS INTEGRATION

            CronLog log = new CronLog();
            log.cron_timestamp = DateTime.Now;
            log.cron_type = "Alegeus Data Import";
            log.cron_log = output;
            context.CronLogs.Add(log);
            context.SaveChanges();

            ViewData["output"] = output;

            return View();
        }

        public ActionResult AlegeusCron3()

        {
            string output = "";

            // BEGIN II file creation from Database query of deposits
            DateTime today = DateTime.Today;
            


            string export_IIlocation = "C:\\alegeus\\II_Files\\";
            string IIfilename = "HSA_II_" + today.ToString("yyyyMMdd") + ".mbi";
            string export_IImbifile = export_IIlocation + IIfilename;

            ClarityContextContainer context = new ClarityContextContainer();


            DateTime payroll_startdate = DateTime.Today.AddDays(-1);
            DateTime payroll_enddate = DateTime.Today;



            try
            {
                string rowcount = "0";
                int DScount = 0;

                ////Database call to deposit table
                ////load up data sets for org settings and repayment tables
                DataSet DSclaims = new DataSet();
             
                SqlConnection conn = new SqlConnection("data source = EC2AMAZ-9HEIG7G; initial catalog = Clarity; Integrated Security = False; User Id = clarity; Password = clarity2017;");

                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT [payroll_date],[employee_contribution],dem.company_identifier, dem.social_security_num " +
                                                  "FROM[Clarity].[dbo].[Deposits] as d join[dbo].[Demographics] as dem " +
                                                  " on d.employee_id = dem.id where ii_posted is null or ii_posted <> 'Posted'", conn);
                
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(DSclaims);
                adapter.Dispose();
                cmd.Dispose();
                conn.Close();


                conn.Open();////////////////////////////////////////////////////////////////////////////////
                cmd = new SqlCommand("Update [Clarity].[dbo].[Deposits] set ii_posted = 'posted' where ii_posted is null or ii_posted <> 'Posted'", conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();



                ////
                DScount = 0;
                DScount = DSclaims.Tables[0].Rows.Count;
                rowcount = DScount.ToString();
                //StreamReader reader = new StreamReader(export_IItextfile);
                string II_Header = "IA," + rowcount + ",BENEFL1,HSA Platform,Standard Result Template,HSA Platform Template";
              
                MemoryStream memoryStreamExport = new MemoryStream();
                TextWriter twExport = new StreamWriter(memoryStreamExport);
                twExport.WriteLineAsync(II_Header.Trim());
               

                //"II", "BENEFL", BENCODE, SSN, "FHR", DATE1, DATE2, AMOUNT
                string BENCODE = "";
                string SSN = "";
                string DATE1 = "";
                string DATE2 = "";
                double IIAmount = 0;
                string II_data_row = "";

                DateTime thisDate1 = new DateTime();



                ////Loop to write II request data
                string II_body = "";
                foreach (DataRow charges in DSclaims.Tables[0].Rows)
                {
                    BENCODE = charges["company_identifier"].ToString();
                    SSN = charges["social_security_num"].ToString();
                    thisDate1 = (Convert.ToDateTime(charges["payroll_date"]));
                    DATE1 = thisDate1.ToString("yyyyMMdd");
                    DATE2 = thisDate1.ToString("yyyyMMdd");
                    IIAmount = (Convert.ToDouble(charges["employee_contribution"].ToString())) * -1; 

                    II_data_row = "";
                    II_data_row = "II,BENEFL," + BENCODE + "," + SSN + ",FHR," + DATE1 + "," + DATE2 + "," + IIAmount.ToString(); ////+ "\r\n"
                    II_data_row = II_data_row.Trim() + "\r\n";
                    twExport.Write(II_data_row);
                    II_body = II_body + II_data_row;

                }

                twExport.Flush();
                twExport.Close();



                using (FileStream file = new FileStream(export_IImbifile, FileMode.Create, System.IO.FileAccess.Write))
                {
                    byte[] bytes = memoryStreamExport.GetBuffer();
                    //file.Write(bytes, 0, bytes.Length);
                    file.Write(bytes,0,II_Header.Length+II_body.Length);
                    memoryStreamExport.Close();
                    file.Close();
                    output += "II file Saved to Local Filesystem: " + IIfilename + "<br />";
                    output += "Created and Exported II File!<br />";

                }

               
                ////Take the same file produced and post to the Clarity FTP-IT folder for II files
                //export_IImbifile
                if (DScount > 0)
                {

                    Clarity_ftp.Connect();
                    Clarity_ftp.Put(export_IImbifile, "//HSA_II_Deposits//" + IIfilename);
                    Clarity_ftp.Close();
                    ////
                    ///

                    /////Take the file just created and also post it to Alegeus FTP

                    Alegeus_ftp.Connect();
                    Alegeus_ftp.Put("C:\\alegeus\\II_Files\\" + IIfilename, IIfilename);
                    Alegeus_ftp.Close();
                }
             
                //////


            }
            catch (Exception ee)
            {
                output += "<br />Error! " + ee.ToString() + "<br /><br />";

            }

            // END ALEGEUS INTEGRATION

            CronLog log = new CronLog();
            log.cron_timestamp = DateTime.Now;
            log.cron_type = "Alegeus Deposit Data Export";
            log.cron_log = output;
            context.CronLogs.Add(log);
            context.SaveChanges();

            return View();
        }


        public string GetAllFootprints(Exception x)
        {
            var st = new StackTrace(x, true);
            var frames = st.GetFrames();
            var traceString = "";
            foreach (var frame in frames)
            {
                if (frame.GetFileLineNumber() < 1)
                    continue;

                traceString +=
                    "File: " + frame.GetFileName() +
                    ", Method:" + frame.GetMethod().Name +
                    ", LineNumber: " + frame.GetFileLineNumber();

                traceString += "  -->  ";
            }

            return traceString;
        }

        public string FlattenException(Exception exception)
        {
            var stringBuilder = new StringBuilder();

            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }

            return stringBuilder.ToString();
        }

        public ActionResult Login()
        {
            ViewData["error"] = "";

            return View();
        }

        public ActionResult LoginError()
        {
            ViewData["error"] = "You entered an invalid username or password!";

            return View("Login");
        }

        public ActionResult Logout()
        {
            Session["user_id"] = null;
            Session["user"] = null;

            Session["has_admin"] = false;
            Session["has_employer"] = false;
            Session["has_employee"] = false;
            Session["has_report_builder"] = false;

            Session["admin_organizations"] = null;
            Session["employer_organizations"] = null;
            Session["employee_organizations"] = null;

            return Redirect("/");
        }

        public ActionResult ForgotPassword(string email)
        {
            ClarityContextContainer context = new ClarityContextContainer();

            if (email != "")
            {
                User user = (from u in context.Users where u.username == email || u.email == email select u).FirstOrDefault<User>();

                if (user != null)
                {
                    if (user.email != "" && user.email != null)
                    {
                        string password = System.Web.Security.Membership.GeneratePassword(10, 1);
                        user.password = this.getHashSha256(password);
                        context.SaveChanges();

                        string alert = "Your new temporary password for Ready for Life HSA is:\r\n\r\n" + password;
                        string alert_html = "Your new temporary password for Ready for Life HSA is:<br /><br />" + password;

                        var apiKey = "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";
                        var client = new SendGridClient(apiKey);
                        var from = new EmailAddress("no-reply@readyforlifehsa.com", "Ready For Life HSA");
                        var subject = "Ready for Life HSA Temporary Password";
                        var to = new EmailAddress(user.email);
                        var plainTextContent = alert;
                        var htmlContent = alert_html;
                        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                        var response = client.SendEmailAsync(msg);
                    }
                }
            }

            return Redirect("/");
        }

        public ActionResult ProcessLogin(string username, string password)
        {
            ClarityContextContainer context = new ClarityContextContainer();

            if (username == "" || password == "")
            {
                Session["user_id"] = null;
                Session["user"] = null;

                Session["has_admin"] = false;
                Session["has_employer"] = false;
                Session["has_employee"] = false;

                Session["admin_organizations"] = null;
                Session["admin_organization_ids"] = null;
                Session["employer_organizations"] = null;
                Session["employee_organizations"] = null;

                Session["active_organization_name"] = "All Employees";
                Session["active_organization"] = -1;
                Session["employee_search"] = "";

                return Redirect("/Home/LoginError");
            }

            string passwordSha256 = this.getHashSha256(password);
            User user = (from u in context.Users where u.username == username && u.password == passwordSha256 select u).FirstOrDefault<User>();

            if (user != null)
            {
                Session["user_id"] = user.id;
                Session["user"] = user;

                Session["has_admin"] = false;
                Session["has_employer"] = false;
                Session["has_employee"] = false;
                Session["has_report_builder"] = true;

                // Determine the user's roles:

                List<Role> roles = user.roles.ToList<Role>();
                List<Organization> adminOrganizations = new List<Organization>();
                List<Organization> employerOrganizations = new List<Organization>();
                List<Organization> employeeOrganizations = new List<Organization>();
                List<int> adminOrganizationIds = new List<int>();

                for (int i = 0; i < roles.Count; i++)
                {
                    if (roles[i].role_type == "admin" && roles[i].access_level > 0)
                    {
                        Session["has_admin"] = true;
                        Session["has_employer"] = true;
                        this.processChildOrganizations(adminOrganizations, roles[i].organization);
                        this.processChildOrganizationIds(adminOrganizationIds, roles[i].organization);
                        this.processChildOrganizations(employerOrganizations, roles[i].organization);
                    }
                    else if (roles[i].role_type == "employer" && roles[i].access_level > 0)
                    {
                        Session["has_employer"] = true;
                        this.processChildOrganizations(employerOrganizations, roles[i].organization);
                    }
                    else if (roles[i].role_type == "employee" && roles[i].access_level > 0)
                    {
                        Session["has_employee"] = true;
                        this.processChildOrganizations(employeeOrganizations, roles[i].organization);
                    }
                }

                adminOrganizations = adminOrganizations.Distinct<Organization>().ToList<Organization>();
                adminOrganizationIds = adminOrganizationIds.Distinct<int>().ToList<int>();
                employerOrganizations = employerOrganizations.Distinct<Organization>().ToList<Organization>();
                employeeOrganizations = employeeOrganizations.Distinct<Organization>().ToList<Organization>();

                Session["admin_organizations"] = adminOrganizations;
                Session["admin_organization_ids"] = adminOrganizationIds;
                Session["employer_organizations"] = employerOrganizations;
                Session["employee_organizations"] = employeeOrganizations;

                Session["active_organization_name"] = "All Employees";
                Session["active_organization"] = -1;
                Session["employee_search"] = "";
            }
            else
            {
                Session["user_id"] = null;
                Session["user"] = null;

                Session["has_admin"] = false;
                Session["has_employer"] = false;
                Session["has_employee"] = false;

                Session["admin_organizations"] = null;
                Session["admin_organization_ids"] = null;
                Session["employer_organizations"] = null;
                Session["employee_organizations"] = null;

                Session["active_organization_name"] = "All Employees";
                Session["active_organization"] = -1;
                Session["employee_search"] = "";

                return Redirect("/Home/LoginError");
            }

            if ((bool)Session["has_admin"] == true) return Redirect("/Admin/Organizations");
            else if ((bool)Session["has_employer"] == true) return Redirect("/Employer/Dashboard");
            else if ((bool)Session["has_employee"] == true) return Redirect("/Employee/Dashboard");

            return Redirect("/Home/LoginError");
        }

        private void processChildOrganizations(List<Organization> organizationList, Organization org)
        {
            organizationList.Add(org);

            foreach (Organization childOrg in org.child_organizations)
            {
                processChildOrganizations(organizationList, childOrg);
            }
        }

        private void processChildOrganizationIds(List<int> organizationList, Organization org)
        {
            organizationList.Add(org.id);

            foreach (Organization childOrg in org.child_organizations)
            {
                processChildOrganizationIds(organizationList, childOrg);
            }
        }

        public ActionResult Register()
        {
            ClarityContextContainer context = new ClarityContextContainer();

            List<Organization> organizations = (from o in context.Organizations select o).ToList<Organization>();
            ViewData["organizations"] = organizations;
            ViewData["error"] = false;

            return View();
        }

        public ActionResult RegistrationError()
        {
            ClarityContextContainer context = new ClarityContextContainer();

            List<Organization> organizations = (from o in context.Organizations select o).ToList<Organization>();
            ViewData["organizations"] = organizations;
            ViewData["error"] = true;

            return View("Register");
        }

        public ActionResult ProcessRegistration(int organization_id, string username, string email, string first_name, string last_name, string ssn, string password, string dob, string phone_number)
        {
            ClarityContextContainer context = new ClarityContextContainer();

            Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();

            /*
            User user = new User();
            user.username = username;
            user.first_name = first_name;
            user.last_name = last_name;
            user.email = email;
            user.password = this.getHashSha256(password);
            context.Users.Add(user);
            context.SaveChanges();

            Role role = new Role();
            role.user = user;
            role.organization = organization;
            role.access_level = 2;
            role.role_type = "employee";
            context.Roles.Add(role);
            context.SaveChanges();
            */

            User user = (from u in context.Users where u.employee.social_security_last_four == ssn && u.email == email && u.last_name == last_name && u.username == "" select u).FirstOrDefault<User>();

            if (user != null)
            {
                bool org_match = false;

                foreach (Role role in user.roles)
                {
                    if (role.organization.id == organization_id && role.role_type == "employee")
                    {
                        org_match = true;
                    }
                }

                if (org_match)
                {
                    user.username = username;
                    user.password = this.getHashSha256(password);
                    context.SaveChanges();

                    this.ProcessLogin(username, password);

                    return Redirect("/Employee/Dashboard");
                }
            }

            return Redirect("/Home/RegistrationError");
        }

        private string getHashSha256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
    }
}
