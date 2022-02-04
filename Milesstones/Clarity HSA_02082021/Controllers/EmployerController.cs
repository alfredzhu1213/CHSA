using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;
using System.Net.Mail;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Clarity_HSA.Models;
using System;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using CsvHelper;
using System.Data;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using System.Data.SqlClient;

namespace Clarity_HSA.Controllers
{
    public class EmployerController : Controller
    {
        private bool CheckPermission()
        {
            return (Session["user_id"] != null && (bool)Session["has_employer"] == true);
        }

        public ActionResult Index()
        {
            if (!this.CheckPermission()) return Redirect("/");

            return View();
        }

        public ActionResult Dashboard()
        {
            if (!this.CheckPermission()) return Redirect("/");

            List<string> loanBalancesByEmployer_Employers = new List<string>();
            List<double> loanBalancesByEmployer_Balances = new List<double>();
            List<string> monthlyPaymentsByEmployer_Employers = new List<string>();
            List<double> monthlyPaymentsByEmployer_Payments = new List<double>();
            double paidVsOutstandingBalances_Paid = 0;
            double paidVsOutstandingBalances_Outstanding = 0;
            List<int> employeeLoanCounts = new List<int>();
            List<string> paymentsByMonth_Months = new List<string>();
            List<double> paymentsByMonth_Payments = new List<double>();

            employeeLoanCounts.Add(0);
            employeeLoanCounts.Add(0);
            employeeLoanCounts.Add(0);
            employeeLoanCounts.Add(0);
            employeeLoanCounts.Add(0);
            employeeLoanCounts.Add(0);

            DateTime now = DateTime.Now;
            now = now.AddYears(-1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());
            now = now.AddMonths(1);
            paymentsByMonth_Months.Add(now.Month.ToString() + "/" + now.Year.ToString());

            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);
            paymentsByMonth_Payments.Add(0);

            List<Organization> employerOrganizations = (List<Organization>)Session["employer_organizations"];

            foreach (Organization organization in employerOrganizations)
            {
                if ((int)Session["active_organization"] == -1 || (int)Session["active_organization"] == organization.id)
                {
                    loanBalancesByEmployer_Employers.Add(organization.name);
                    monthlyPaymentsByEmployer_Employers.Add(organization.name);
                    double balance = 0;
                    double monthlyPayment = 0;

                    foreach (Role role in organization.roles)
                    {
                        User user = role.user;

                        if (role.role_type == "employee")
                        {
                            if ((string)Session["employee_search"] == "" || (user.first_name + " " + user.last_name).Contains((string)Session["employee_search"]))
                            {
                                user.employee.balance = 0;
                                user.employee.monthly_payment = 0;

                                foreach (Loan loan in user.employee.loans)
                                {
                                    paidVsOutstandingBalances_Outstanding += loan.balance;
                                    user.employee.balance += loan.balance;
                                    user.employee.monthly_payment += loan.monthly_payment;
                                }

                                foreach (Deposit deposit in user.employee.deposits)
                                {
                                    paidVsOutstandingBalances_Paid += deposit.employee_contribution;

                                    for (int i = 0; i < paymentsByMonth_Months.Count; i++)
                                    {
                                        if (paymentsByMonth_Months[i] == (deposit.payroll_date.Month + "/" + deposit.payroll_date.Year))
                                        {
                                            paymentsByMonth_Payments[i] += deposit.employee_contribution;
                                            break;
                                        }
                                    }
                                }

                                if (user.employee.charges.Count < 5)
                                {
                                    employeeLoanCounts[user.employee.charges.Count]++;
                                }
                                else
                                {
                                    employeeLoanCounts[5]++;
                                }

                                balance += user.employee.balance;
                                monthlyPayment += user.employee.monthly_payment;
                            }
                        }
                    }

                    monthlyPaymentsByEmployer_Payments.Add(monthlyPayment);
                    loanBalancesByEmployer_Balances.Add(balance);
                }
            }

            ViewData["loanBalancesByEmployer_Employers"] = loanBalancesByEmployer_Employers;
            ViewData["loanBalancesByEmployer_Balances"] = loanBalancesByEmployer_Balances;
            ViewData["monthlyPaymentsByEmployer_Employers"] = monthlyPaymentsByEmployer_Employers;
            ViewData["monthlyPaymentsByEmployer_Payments"] = monthlyPaymentsByEmployer_Payments;
            ViewData["paidVsOutstandingBalances_Paid"] = paidVsOutstandingBalances_Paid;
            ViewData["paidVsOutstandingBalances_Outstanding"] = paidVsOutstandingBalances_Outstanding;
            ViewData["employeeLoanCounts"] = employeeLoanCounts;
            ViewData["paymentsByMonth_Months"] = paymentsByMonth_Months;
            ViewData["paymentsByMonth_Payments"] = paymentsByMonth_Payments;

            ClarityContextContainer context = new ClarityContextContainer();
            int user_id = (int)Session["user_id"];
            User currentUser = (from u in context.Users where u.id == user_id select u).FirstOrDefault<User>();

            if (currentUser.dashboard_config == null)
            {
                currentUser.dashboard_config = new DashboardConfig();
                currentUser.dashboard_config.quadrant_1 = "0";
                currentUser.dashboard_config.quadrant_2 = "0";
                currentUser.dashboard_config.quadrant_3 = "0";
                currentUser.dashboard_config.quadrant_4 = "0";
            }

            context.SaveChanges();

            Session["user"] = currentUser;
            ViewData["user"] = currentUser;

            return View();
        }

        public List<User> EmployeeRollUp(ClarityContextContainer context)
        {
            List<User> employees = new List<User>();
            List<int> employeeIds = new List<int>();
            List<Organization> employerOrganizations = new List<Organization>();
            try
            {
                employerOrganizations = (List<Organization>)Session["employer_organizations"];
            }
            catch (Exception ee)
            {
                employerOrganizations = (List<Organization>)System.Web.HttpContext.Current.Session["employer_organizations"];
            }
            

            foreach (Organization the_organization in employerOrganizations)
            {
                Organization organization = (from o in context.Organizations where o.id == the_organization.id select o).First<Organization>();

                if ((int)System.Web.HttpContext.Current.Session["active_organization"] == -1 || (int)System.Web.HttpContext.Current.Session["active_organization"] == organization.id)
                {
                    foreach (Role role in organization.roles)
                    {
                        User user = role.user;

                        if (role.role_type == "employee")
                        {
                            if ((string)System.Web.HttpContext.Current.Session["employee_search"] == "" || (user.first_name + " " + user.last_name).Contains((string)System.Web.HttpContext.Current.Session["employee_search"]))
                            {
                                if (!employeeIds.Contains(user.id))
                                {
                                    employeeIds.Add(user.id);
                                    employees.Add(user);

                                    user.employee.balance = 0;
                                    user.employee.monthly_payment = 0;

                                    foreach (Loan loan in user.employee.loans)
                                    {
                                        user.employee.balance += loan.balance;
                                        user.employee.monthly_payment += loan.monthly_payment;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            context.SaveChanges();

            employees.Sort((p, q) => (p.last_name + ", " + p.first_name).CompareTo(q.last_name + ", " + q.first_name));

            return employees;
        }

        public ActionResult Employees()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ViewData["employees"] = this.EmployeeRollUp(new ClarityContextContainer());

            return View();
        }

        public ActionResult Loans()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ViewData["employees"] = this.EmployeeRollUp(new ClarityContextContainer());

            return View();
        }

        public ActionResult Reports()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                List<Report> reports = organization.reports.ToList<Report>();

                List<Report> global_reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                foreach (Report global_report in global_reports)
                {
                    if (global_report.global_report_orgs != null)
                    {
                        if (global_report.global_report_orgs.Contains("~" + organization.id + "~"))
                        {
                            reports.Add(global_report);
                        }
                    }
                }

                ViewData["reports"] = reports;
            }
            else
            {
                List<Report> reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                ViewData["reports"] = reports;
            }

            return View();
        }

        public ActionResult Feeds()
        {
            if (!this.CheckPermission()) return Redirect("/");

            int organization_id = (int)Session["active_organization"];

            ClarityContextContainer context = new ClarityContextContainer();

            if (organization_id > 0)
            {

                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();

                if (organization.organization_settings == null)
                {
                    organization.organization_settings = new OrganizationSettings();
                    organization.organization_settings.alegeus_feed = "";
                    organization.organization_settings.demographics_feed = "";
                    organization.organization_settings.deposits_feed = "";
                    organization.organization_settings.withhold_entire_balance = true;

                    context.SaveChanges();
                }

                ViewData["organization"] = organization;
                ViewData["logs"] = organization.organization_settings.feed_logs.OrderByDescending(x => x.feed_timestamp).ToList<FeedLog>();
            }
            else
            {
                ViewData["organization"] = new Organization();
                ViewData["logs"] = context.FeedLogs.OrderByDescending(x => x.feed_timestamp).ToList<FeedLog>();
            }

            return View();
        }

        public ActionResult FeedLog(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            int organization_id = (int)Session["active_organization"];
            Organization the_organization = null;

            ClarityContextContainer context = new ClarityContextContainer();

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();

                if (organization.organization_settings == null)
                {
                    organization.organization_settings = new OrganizationSettings();
                    organization.organization_settings.alegeus_feed = "";
                    organization.organization_settings.demographics_feed = "";
                    organization.organization_settings.deposits_feed = "";
                    organization.organization_settings.withhold_entire_balance = true;

                    context.SaveChanges();
                }

                the_organization = organization;
            }
            else
            {
                the_organization = new Organization();
            }

            if (organization_id > 0)
            {
                foreach (FeedLog log in the_organization.organization_settings.feed_logs)
                {
                    if (log.Id == Int32.Parse(id))
                    {
                        ViewData["log"] = log;
                    }
                }
            }
            else
            {
                int feedLogId = Int32.Parse(id);
                ViewData["log"] = (from o in context.FeedLogs where o.Id == feedLogId select o).First<FeedLog>();
            }

            return View();
        }

        public ActionResult ImportFeeds()
        {
            // if (!this.CheckPermission()) return Redirect("/");

            int organization_id = 1;

            try
            {
                organization_id = (int)Session["active_organization"];
            }
            catch (Exception ee)
            {
                organization_id = 1;
            }

            if (organization_id > 0)
            {
                ClarityContextContainer context = new ClarityContextContainer();
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();

                // Log the import:
                FeedLog log = new FeedLog();
                log.feed_direction = "Import";
                log.feed_location = organization.organization_settings.alegeus_feed;
                log.feed_name = "Alegeus IL Composite File";
                log.feed_timestamp = DateTime.Now;
                log.successful = true;
                log.organization_setting = organization.organization_settings;

                if (organization.organization_settings.alegeus_feed != "")
                {
                    using (LumenWorks.Framework.IO.Csv.CsvReader csv = new LumenWorks.Framework.IO.Csv.CsvReader(new StreamReader(organization.organization_settings.alegeus_feed), true))
                    {
                        int fieldCount = csv.FieldCount;
                        string[] headers = csv.GetFieldHeaders();

                        // The Alegeus IL file has three parts: EC, EN, EB.
                        // For each row, determine what record type it is, and handle it accordingly.

                        while (csv.ReadNextRecord())
                        {
                            if (csv[0] == "EB")
                            {
                                // Demographic record:

                                string company_id = csv[2];

                                if ((from o in context.Organizations where o.company_id == company_id select o).Count<Organization>() > 0)
                                {
                                    Organization employee_org = (from o in context.Organizations where o.company_id == company_id select o).First<Organization>();

                                    string ssn = csv[3];
                                    string ssn_hashed = ssn; // this.getHashSha256(ssn);
                                    Demographic demographic;

                                    if ((from d in context.Demographics where d.social_security_num == ssn_hashed select d).Count<Demographic>() > 0)
                                    {
                                        demographic = (from d in context.Demographics where d.social_security_num == ssn_hashed select d).First<Demographic>();
                                    }
                                    else
                                    {
                                        // Setup User and Role:
                                        User user = new User();
                                        user.employee = new Demographic();

                                        user.username = "";
                                        user.first_name = csv[5];
                                        user.last_name = csv[4];
                                        user.email = csv[13];
                                        user.password = this.getHashSha256("");
                                        context.Users.Add(user);
                                        // context.SaveChanges();

                                        Role role = new Role();
                                        role.user = user;
                                        role.organization = employee_org;
                                        role.access_level = 2;
                                        role.role_type = "employee";
                                        context.Roles.Add(role);
                                        // context.SaveChanges();

                                        demographic = user.employee;

                                        FeedLogDetail detail2 = new FeedLogDetail();
                                        detail2.message_timestamp = DateTime.Now;
                                        string current_record_2 = csv[0] + "," +
                                                                csv[1] + "," +
                                                                csv[2] + "," +
                                                                csv[3] + "," +
                                                                csv[4] + "," +
                                                                csv[5] + "," +
                                                                csv[6] + "," +
                                                                csv[7] + "," +
                                                                csv[8] + "," +
                                                                csv[9] + "," +
                                                                csv[10] + "," +
                                                                csv[11] + "," +
                                                                csv[12] + "," +
                                                                csv[13] + "," +
                                                                csv[14] + "," +
                                                                csv[15] + "," +
                                                                csv[16] + "," +
                                                                csv[17] + "," +
                                                                csv[18];
                                        detail2.log_message = "Added User: " + user.first_name + " " + user.last_name + "<br />" + current_record_2;
                                        log.feed_log_details.Add(detail2);
                                    }

                                    demographic.user.first_name = csv[5];
                                    demographic.user.last_name = csv[4];

                                    demographic.company_identifier = csv[2];
                                   
                                    demographic.payroll_identifier = "";
                                    demographic.social_security_num = csv[3]; // this.getHashSha256(csv[3]);
                                    demographic.social_security_last_four = csv[3].Substring(Math.Max(0, csv[3].Length - 4));
                                    demographic.last_name = csv[4];
                                    demographic.first_name = csv[5];
                                    demographic.dob = new DateTime(int.Parse(csv[15].Substring(0, 4)), int.Parse(csv[15].Substring(4, 2)), int.Parse(csv[15].Substring(6, 2)));
                                    demographic.address = csv[6];
                                    demographic.city = csv[8];
                                    demographic.state = csv[9];
                                    demographic.zip = csv[10];
                                    demographic.country = csv[11];
                                    demographic.email = csv[13];
                                    demographic.goal = 0;
                                    demographic.company_payroll_identifier = "";
                                   

                                    try
                                    {
                                        demographic.payroll_identifier = csv[14];
                                        

                                    }
                                    catch (Exception ee)
                                    {
                                    }

                                    try
                                    {
                                        if (csv[16] != "")
                                        {
                                            demographic.termination_date = new DateTime(int.Parse(csv[16].Substring(0, 4)), int.Parse(csv[16].Substring(4, 2)), int.Parse(csv[16].Substring(6, 2)));
                                            demographic.terminated = true;
                                        }
                                        else
                                        {
                                            demographic.termination_date = null;
                                            demographic.terminated = false;
                                        }
                                    }
                                    catch (Exception ee)
                                    {
                                    }

                                    string current_record = csv[0] + "," +
                                                            csv[1] + "," +
                                                            csv[2] + "," +
                                                            csv[3] + "," +
                                                            csv[4] + "," +
                                                            csv[5] + "," +
                                                            csv[6] + "," +
                                                            csv[7] + "," +
                                                            csv[8] + "," +
                                                            csv[9] + "," +
                                                            csv[10] + "," +
                                                            csv[11] + "," +
                                                            csv[12] + "," +
                                                            csv[13] + "," +
                                                            csv[14] + "," +
                                                            csv[15] + "," +
                                                            csv[16] + "," +
                                                            csv[17] + "," +
                                                            csv[18];
                                    FeedLogDetail detail = new FeedLogDetail();
                                    detail.message_timestamp = DateTime.Now;
                                    detail.log_message = "Updated User: " + demographic.first_name + " " + demographic.last_name + "<br />" + current_record;
                                    
                                    log.feed_log_details.Add(detail);
                                }
                            }
                        }

                        context.SaveChanges();
                    }

                    using (LumenWorks.Framework.IO.Csv.CsvReader csv = new LumenWorks.Framework.IO.Csv.CsvReader(new StreamReader(organization.organization_settings.alegeus_feed), true))
                    {
                        int fieldCount = csv.FieldCount;
                        string[] headers = csv.GetFieldHeaders();

                        // The Alegeus IL file has three parts: EC, EN, EB.
                        // For each row, determine what record type it is, and handle it accordingly.

                        while (csv.ReadNextRecord())
                        {
                            if (csv[0] == "EC")
                            {
                                // Enrollment record:

                                string company_id = csv[2];
                                bool updated_demographic_1 = false;
                                bool updated_demographic_2 = false;

                                if ((from o in context.Organizations where o.company_id == company_id select o).Count<Organization>() > 0)
                                {
                                    Organization employee_org = (from o in context.Organizations where o.company_id == company_id select o).First<Organization>();

                                    string ssn = csv[3];
                                    string ssn_hashed = ssn; // this.getHashSha256(ssn);
                                    Demographic demographic;

                                    if ((from d in context.Demographics where d.social_security_num == ssn_hashed select d).Count<Demographic>() > 0)
                                    {
                                        demographic = (from d in context.Demographics where d.social_security_num == ssn_hashed select d).First<Demographic>();

                                        if (Double.Parse(csv[11]) >= 0 && csv[6] == "HSAADV")
                                        {
                                            demographic.maximum_advance_amount = Double.Parse(csv[11]);

                                            updated_demographic_1 = true;

                                            string current_record = csv[0] + "," +
                                                            csv[1] + "," +
                                                            csv[2] + "," +
                                                            csv[3] + "," +
                                                            csv[4] + "," +
                                                            csv[5] + "," +
                                                            csv[6] + "," +
                                                            csv[7] + "," +
                                                            csv[8] + "," +
                                                            csv[9] + "," +
                                                            csv[10] + "," +
                                                            csv[11] + "," +
                                                            csv[12] + "," +
                                                            csv[13] + "," +
                                                            csv[14] + "," +
                                                            csv[15];
                                            FeedLogDetail detail2 = new FeedLogDetail();
                                            detail2.message_timestamp = DateTime.Now;
                                            detail2.log_message = "Updated Maximum Advance Amount for User " + demographic.first_name + " " + demographic.last_name + " to " + demographic.maximum_advance_amount + "<br />" + current_record;
                                            log.feed_log_details.Add(detail2);
                                        }

                                        if (Double.Parse(csv[14]) >= 0 && csv[6] == "HSA")
                                        {
                                            demographic.hsa_balance = Double.Parse(csv[14]);

                                            updated_demographic_2 = true;

                                            string current_record = csv[0] + "," +
                                                            csv[1] + "," +
                                                            csv[2] + "," +
                                                            csv[3] + "," +
                                                            csv[4] + "," +
                                                            csv[5] + "," +
                                                            csv[6] + "," +
                                                            csv[7] + "," +
                                                            csv[8] + "," +
                                                            csv[9] + "," +
                                                            csv[10] + "," +
                                                            csv[11] + "," +
                                                            csv[12] + "," +
                                                            csv[13] + "," +
                                                            csv[14] + "," +
                                                            csv[15];
                                            FeedLogDetail detail2 = new FeedLogDetail();
                                            detail2.message_timestamp = DateTime.Now;
                                            detail2.log_message = "Updated HSA Balance for User " + demographic.first_name + " " + demographic.last_name + " to " + demographic.hsa_balance + "<br />" + current_record;
                                            log.feed_log_details.Add(detail2);
                                        }
                                    }
                                }

                                if (!updated_demographic_1)
                                {
                                    string current_record = csv[0] + "," +
                                                            csv[1] + "," +
                                                            csv[2] + "," +
                                                            csv[3] + "," +
                                                            csv[4] + "," +
                                                            csv[5] + "," +
                                                            csv[6] + "," +
                                                            csv[7] + "," +
                                                            csv[8] + "," +
                                                            csv[9] + "," +
                                                            csv[10] + "," +
                                                            csv[11] + "," +
                                                            csv[12] + "," +
                                                            csv[13] + "," +
                                                            csv[14] + "," +
                                                            csv[15];
                                    FeedLogDetail detail2 = new FeedLogDetail();
                                    detail2.message_timestamp = DateTime.Now;
                                    detail2.log_message = "Failed to Update Maximum Advance Amount for SSN: " + csv[3] + "<br />" + current_record;
                                    log.feed_log_details.Add(detail2);
                                }

                                if (!updated_demographic_2)
                                {
                                    string current_record = csv[0] + "," +
                                                            csv[1] + "," +
                                                            csv[2] + "," +
                                                            csv[3] + "," +
                                                            csv[4] + "," +
                                                            csv[5] + "," +
                                                            csv[6] + "," +
                                                            csv[7] + "," +
                                                            csv[8] + "," +
                                                            csv[9] + "," +
                                                            csv[10] + "," +
                                                            csv[11] + "," +
                                                            csv[12] + "," +
                                                            csv[13] + "," +
                                                            csv[14] + "," +
                                                            csv[15];
                                    FeedLogDetail detail2 = new FeedLogDetail();
                                    detail2.message_timestamp = DateTime.Now;
                                    detail2.log_message = "Failed to Update HSA Balance for SSN: " + csv[3] + "<br />" + current_record;
                                    log.feed_log_details.Add(detail2);
                                }
                            }
                        }

                        context.SaveChanges();
                    }

                    using (LumenWorks.Framework.IO.Csv.CsvReader csv = new LumenWorks.Framework.IO.Csv.CsvReader(new StreamReader(organization.organization_settings.alegeus_feed), true))
                    {
                        int fieldCount = csv.FieldCount;
                        string[] headers = csv.GetFieldHeaders();

                        // The Alegeus IL file has three parts: EC, EN, EB.
                        // For each row, determine what record type it is, and handle it accordingly.

                        while (csv.ReadNextRecord())
                        {
                            if (csv[0] == "EA")
                            {
                                // Header record, skip!
                            }
                            else if (csv[0] == "EN")
                            {
                                string ssn = csv[3];
                                string ssn_hashed = ssn; // this.getHashSha256(ssn);
                                Demographic demographic;
                                bool processed_en = false;
                                double monthlyPayment = 0;
                                if ((from d in context.Demographics where d.social_security_num == ssn_hashed select d).Count<Demographic>() > 0)
                                {
                                    demographic = (from d in context.Demographics where d.social_security_num == ssn_hashed select d).First<Demographic>();

                                    // Is this a charge or a deposit?
                                    if (csv[6] == "ADVANCE" || true)
                                    {
                                        if (csv[16].Contains("POS - Force Post"))
                                        {
                                            Charge charge;
                                            
                                            DateTime transaction_date = new DateTime(int.Parse(csv[9].Substring(0, 4)), int.Parse(csv[9].Substring(4, 2)), int.Parse(csv[9].Substring(6, 2)));
                                            Double approved_amount = Double.Parse(csv[7]);

                                            if ((from d in context.Charges where d.employee.id == demographic.id && d.approved_amount == approved_amount && d.transaction_date == transaction_date select d).Count<Charge>() > 0)
                                            {
                                                charge = (from d in context.Charges where d.employee.id == demographic.id && d.approved_amount == approved_amount && d.transaction_date == transaction_date select d).First<Charge>();
                                            }
                                            else
                                            {
                                                charge = new Charge();
                                                context.Charges.Add(charge);
                                            }

                                            charge.employee = demographic;
                                            charge.transaction_date = transaction_date;
                                            charge.claim_type = csv[16];
                                            charge.description = csv[12];
                                            charge.total_claim_amount = Double.Parse(csv[7]);
                                            charge.eligible_amount = 0;
                                            charge.approved_amount = Double.Parse(csv[7]);
                                            charge.ineligible_amount = 0;
                                            charge.pended_amount = 0;
                                            charge.denied_amount = 0;
                                            charge.denied_reason = "";
                                            charge.claim_number = csv[2];
                                            charge.scc_mcc = "";

                                            processed_en = true;

                                            string current_record = csv[0] + "," +
                                                            csv[1] + "," +
                                                            csv[2] + "," +
                                                            csv[3] + "," +
                                                            csv[4] + "," +
                                                            csv[5] + "," +
                                                            csv[6] + "," +
                                                            csv[7] + "," +
                                                            csv[8] + "," +
                                                            csv[9] + "," +
                                                            csv[10] + "," +
                                                            csv[11] + "," +
                                                            csv[12] + "," +
                                                            csv[13] + "," +
                                                            csv[14] + "," +
                                                            csv[15] + "," +
                                                            csv[16] + "," +
                                                            csv[17];
                                            FeedLogDetail detail2 = new FeedLogDetail();
                                            detail2.message_timestamp = DateTime.Now;
                                            detail2.log_message = "Added Charge for User " + demographic.first_name + " " + demographic.last_name + ": " + csv[7] + "<br />" + current_record;
                                            //detail2.log_message = demographic.company_identifier;///////////
                                            log.feed_log_details.Add(detail2);

                                            if (demographic.email != "" && demographic.email != null )
                                            {
                                                foreach (Charge charge2 in demographic.charges)
                                                {
                                                    //if (charge2.loan == null)
                                                    //{
                                                    //    if (demographic.loans.Count > 0)
                                                    //    {
                                                    //        charge2.loan = demographic.loans.First<Loan>();
                                                    //        context.SaveChanges();
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        Loan loan = new Loan();
                                                    //        loan.loan_id = Guid.NewGuid();
                                                    //        loan.last_payment_date = DateTime.Today;
                                                    //        loan.monthly_payment = 0;
                                                    //        loan.balance = 0;
                                                    //        demographic.loans.Add(loan);
                                                    //        context.SaveChanges();

                                                    //        charge2.loan = loan;
                                                    //        context.SaveChanges();
                                                    //    }
                                                    //}
                                                }

                                                if (demographic.loans.Count > 0)
                                                {
                                                    Loan loan = demographic.loans.First<Loan>();
                                                    loan.balance = 0;
                                                    loan.monthly_payment = 0;

                                                    foreach (Charge charge2 in loan.charges)
                                                    {
                                                        loan.balance += charge2.approved_amount;
                                                    }

                                                    foreach (Deposit deposit in demographic.deposits)
                                                    {
                                                        loan.balance -= deposit.employee_contribution;
                                                        loan.last_payment_date = deposit.payroll_date;
                                                    }

                                                    bool found_repayment_table = false;
                                                    foreach (RepaymentTable repayment in organization.organization_settings.repayment_table)
                                                    {
                                                        if (loan.balance >= repayment.loan_amount_min && loan.balance < repayment.loan_amount_max)
                                                        {
                                                            loan.monthly_payment = repayment.repaymant_amount;
                                                            
                                                            found_repayment_table = true;
                                                        }
                                                    }

                                                    if (found_repayment_table == false && organization.organization_settings.repayment_table.Count > 0)
                                                    {
                                                        RepaymentTable repayment = organization.organization_settings.repayment_table.Last<RepaymentTable>();
                                                        loan.monthly_payment = repayment.repaymant_amount;
                                                       

                                                    }

                                                    if (demographic.terminated && organization.organization_settings.withhold_entire_balance)
                                                    {
                                                        loan.monthly_payment = loan.balance;
                                                       
                                                    }
                                                  
                                                    monthlyPayment = loan.monthly_payment;
                                                    context.SaveChanges();
                                                    
                                                }
                                                


                                                string alert = "The transaction listed below has been processed against your Ready for Life HSA Payroll Advance account.\r\n";
                                                alert += "The payroll deductions associated with this transaction will begin in conjunction with your payroll and repayment schedule. Your payroll deduction will be " + monthlyPayment.ToString("C", CultureInfo.CurrentCulture) + ".\r\n";
                                                alert += "\r\n";
                                                alert += "Date of Transaction: " + transaction_date.ToShortDateString() + "\r\n";
                                                alert += "Amount of Transaction: " + charge.total_claim_amount.ToString("C", CultureInfo.CurrentCulture) + "\r\n";
                                                alert += "\r\n";
                                                alert += "To view details about this or any of your transactions please visit http://www.claritybenefitsolutions.com/.\r\n";
                                                alert += "Should you have further questions feel free to email our Customer Service Department at 888-423-6359\r\n";

                                                string alert_html = "The transaction listed below has been processed against your Ready for Life HSA Payroll Advance account.<br />";
                                                alert_html += "The payroll deductions associated with this transaction will begin in conjunction with your payroll and repayment schedule. Your payroll deduction will be " + monthlyPayment.ToString("C", CultureInfo.CurrentCulture) + ".<br />";
                                                alert_html += "<br />";
                                                alert_html += "Date of Transaction: " + transaction_date.ToShortDateString() + "<br />";
                                                alert_html += "Amount of Transaction: " + charge.total_claim_amount.ToString("C", CultureInfo.CurrentCulture) + "<br />";
                                                alert_html += "<br />";
                                                alert_html += "To view details about this or any of your transactions please visit http://www.claritybenefitsolutions.com/.<br />";
                                                alert_html += "Should you have further questions feel free to email our Customer Service Department at 888-423-6359<br />";

                                                var apiKey = "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";
                                                var client = new SendGridClient(apiKey);
                                                var from = new EmailAddress("no-reply@readyforlifehsa.com", "Ready For Life HSA");
                                                var subject = "Ready for Life HSA Charge Alert";
                                                var to = new EmailAddress(demographic.email);
                                                var plainTextContent = alert;
                                                var htmlContent = alert_html;
                                                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                                                var response = client.SendEmailAsync(msg);
                                            }
                                        }
                                        else if (csv[16].Contains("POS - Refund"))
                                        {
                                            
                                            Charge charge;
                                            DateTime transaction_date = new DateTime(int.Parse(csv[9].Substring(0, 4)), int.Parse(csv[9].Substring(4, 2)), int.Parse(csv[9].Substring(6, 2)));
                                            Double approved_amount = Double.Parse(csv[7]);

                                            if ((from d in context.Charges where d.employee.id == demographic.id && d.approved_amount == approved_amount && d.transaction_date == transaction_date select d).Count<Charge>() > 0)
                                            {
                                                charge = (from d in context.Charges where d.employee.id == demographic.id && d.approved_amount == approved_amount && d.transaction_date == transaction_date select d).First<Charge>();
                                            }
                                            else
                                            {
                                                charge = new Charge();
                                                context.Charges.Add(charge);
                                            }

                                            charge.employee = demographic;
                                            charge.transaction_date = transaction_date;
                                            charge.claim_type = csv[16];
                                            charge.description = csv[12];
                                            charge.total_claim_amount = (Double.Parse(csv[7]))   ;
                                            charge.eligible_amount = 0;
                                            charge.approved_amount = (Double.Parse(csv[7]));
                                            charge.ineligible_amount = 0;
                                            charge.pended_amount = 0;
                                            charge.denied_amount = 0;
                                            charge.denied_reason = "";
                                            charge.claim_number = csv[2];
                                            charge.scc_mcc = "";
                                           
                                            processed_en = true;

                                            FeedLogDetail detail2 = new FeedLogDetail();
                                            detail2.message_timestamp = DateTime.Now;
                                            detail2.log_message = "Added POS Refund for User " + demographic.first_name + " " + demographic.last_name + ": " + -1 * Double.Parse(csv[7]);
                                            log.feed_log_details.Add(detail2);
                                            
                                        }
                                        else if (csv[16].Contains("Admin - Payroll Deposit"))
                                        {
                                            Deposit deposit;
                                            DateTime payroll_date = new DateTime(int.Parse(csv[5].Substring(0, 4)), int.Parse(csv[5].Substring(4, 2)), int.Parse(csv[5].Substring(6, 2)));
                                            Double employee_contribution = Double.Parse(csv[7]);

                                            if (csv[16].Contains("Void"))
                                            {
                                                employee_contribution = -1 * employee_contribution;
                                            }

                                            if ((from d in context.Deposits where d.employee.id == demographic.id && d.employee_contribution == employee_contribution && d.payroll_date == payroll_date select d).Count<Deposit>() > 0)
                                            {
                                                deposit = (from d in context.Deposits where d.employee.id == demographic.id && d.employee_contribution == employee_contribution && d.payroll_date == payroll_date select d).First<Deposit>();
                                            }
                                            else
                                            {
                                                deposit = new Deposit();
                                                context.Deposits.Add(deposit);
                                            }

                                            deposit.employee = demographic;
                                            deposit.deposit_type = csv[16];
                                            deposit.plan_type = csv[10];
                                            deposit.plan_begin = "";
                                            deposit.plan_end = "";
                                            deposit.employee_contribution = employee_contribution;
                                            deposit.employer_contribution = "";
                                            deposit.payroll_date = payroll_date;

                                            processed_en = true;

                                            string current_record = csv[0] + "," +
                                                            csv[1] + "," +
                                                            csv[2] + "," +
                                                            csv[3] + "," +
                                                            csv[4] + "," +
                                                            csv[5] + "," +
                                                            csv[6] + "," +
                                                            csv[7] + "," +
                                                            csv[8] + "," +
                                                            csv[9] + "," +
                                                            csv[10] + "," +
                                                            csv[11] + "," +
                                                            csv[12] + "," +
                                                            csv[13] + "," +
                                                            csv[14] + "," +
                                                            csv[15] + "," +
                                                            csv[16] + "," +
                                                            csv[17];
                                            FeedLogDetail detail2 = new FeedLogDetail();
                                            detail2.message_timestamp = DateTime.Now;
                                            detail2.log_message = "Added Deposit for User " + demographic.first_name + " " + demographic.last_name + ": " + employee_contribution + "<br />" + current_record;
                                            log.feed_log_details.Add(detail2);
                                        }
                                        else if (csv[16].Contains("Admin - Prefunded Deposit"))
                                        {
                                            // Double employee_contribution = Double.Parse(csv[7]);
                                            // demographic.hsa_balance += employee_contribution;
                                        }
                                    }
                                }

                                if (!processed_en)
                                {
                                    string current_record = csv[0] + "," +
                                                            csv[1] + "," +
                                                            csv[2] + "," +
                                                            csv[3] + "," +
                                                            csv[4] + "," +
                                                            csv[5] + "," +
                                                            csv[6] + "," +
                                                            csv[7] + "," +
                                                            csv[8] + "," +
                                                            csv[9] + "," +
                                                            csv[10] + "," +
                                                            csv[11] + "," +
                                                            csv[12] + "," +
                                                            csv[13] + "," +
                                                            csv[14] + "," +
                                                            csv[15] + "," +
                                                            csv[16] + "," +
                                                            csv[17];
                                    FeedLogDetail detail2 = new FeedLogDetail();
                                    detail2.message_timestamp = DateTime.Now;
                                    detail2.log_message = "Failed to Process EN Record: " + csv[7] + "<br />" + current_record;
                                    log.feed_log_details.Add(detail2);
                                }
                            }
                        }

                        context.SaveChanges();
                    }

                    
                    context.FeedLogs.Add(log);
                    context.SaveChanges();
                }

                // Process Loan Logic:
                List<Demographic> employees = (from d in context.Demographics select d).ToList <Demographic>();



               
                foreach (Demographic employee in employees)
                {
                   

                    int empID = 0;
                    int empOrgSettingsID = 0;
                    empID = employee.id;
                    

                    ////Database all to get proper Org_ID
                    string sql = "Select top 1 os.id from Users as u join roles as r on u.id = r.[user_id] join [dbo].[OrganizationSettings] as os on os.organization_id = r.organization_id where u.employee_id = @IDnum";
                    using (SqlConnection conn = new SqlConnection("data source = EC2AMAZ-9HEIG7G; initial catalog = Clarity; Integrated Security = False; User Id = clarity; Password = clarity2017;"))
                    {
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@IDnum", empID);
                        try
                        {
                            conn.Open();
                            empOrgSettingsID = (int)cmd.ExecuteScalar();
                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    /////////

                    foreach (Charge charge in employee.charges)
                    {
                        if (charge.loan == null)
                        {
                            if (employee.loans.Count > 0)
                            {
                                charge.loan = employee.loans.First<Loan>();
                                context.SaveChanges();
                            }
                            else
                            {
                                Loan loan = new Loan();
                                loan.loan_id = Guid.NewGuid();
                                loan.last_payment_date = DateTime.Today;
                                loan.monthly_payment = -1;
                                loan.balance = -1;
                                employee.loans.Add(loan);
                                context.SaveChanges();

                                charge.loan = loan;
                                context.SaveChanges();
                            }
                        }
                    }

                    if (employee.loans.Count > 0)
                    {
                        Loan loan = employee.loans.First<Loan>();
                        loan.balance = 0;
                        loan.monthly_payment = 0;

                        foreach (Charge charge in loan.charges)
                        {
                            loan.balance += charge.approved_amount;
                        }

                        foreach (Deposit deposit in employee.deposits)
                        {
                            loan.balance -= deposit.employee_contribution;
                            loan.last_payment_date = deposit.payroll_date;
                        }

                        bool found_repayment_table = false;


                        ////load up data sets for org settings and repayment tables
                        DataSet DSOrgSettings = new DataSet();
                        DataSet DSRepayments = new DataSet();
                        double MaxRepayment = 0;
                        Boolean withholdBal = true;


                        SqlConnection conn = new SqlConnection("data source = EC2AMAZ-9HEIG7G; initial catalog = Clarity; Integrated Security = False; User Id = clarity; Password = clarity2017;");

                        conn.Open();
                        SqlCommand cmd = new SqlCommand("SELECT * FROM [dbo].[RepaymentTables] where organization_settings_id = @orgsetid", conn);
                        cmd.Parameters.AddWithValue("@orgsetid", empOrgSettingsID);
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        adapter.SelectCommand = cmd;
                        adapter.Fill(DSRepayments);
                        adapter.Dispose();
                        cmd.Dispose();
                        conn.Close();

                        string sql2 = "SELECT max(repaymant_amount) FROM[dbo].[RepaymentTables] where organization_settings_id = @orgsetid";
                        using (SqlConnection conn2 = new SqlConnection("data source = EC2AMAZ-9HEIG7G; initial catalog = Clarity; Integrated Security = False; User Id = clarity; Password = clarity2017;"))
                        {
                            SqlCommand cmd2 = new SqlCommand(sql2, conn2);
                            cmd2.Parameters.AddWithValue("@orgsetid", empOrgSettingsID);
                            try
                            {
                                conn2.Open();
                                MaxRepayment = (double)cmd2.ExecuteScalar();
                                conn2.Close();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                        string sql3 = "select top 1 withhold_entire_balance from [dbo].[OrganizationSettings] where id = @orgsetid";
                        using (SqlConnection conn3 = new SqlConnection("data source = EC2AMAZ-9HEIG7G; initial catalog = Clarity; Integrated Security = False; User Id = clarity; Password = clarity2017;"))
                        {
                            SqlCommand cmd3 = new SqlCommand(sql3, conn3);
                            cmd3.Parameters.AddWithValue("@orgsetid", empOrgSettingsID);
                            try
                            {
                                conn3.Open();
                                withholdBal = (Boolean)cmd3.ExecuteScalar();
                                conn3.Close();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                        ////////////////////////////////////////

                        foreach (DataRow repayments in DSRepayments.Tables[0].Rows)
                        {
                            if (loan.balance >= Convert.ToDouble(repayments["loan_amount_min"]) && loan.balance < Convert.ToDouble(repayments["loan_amount_max"]))
                            {///////////////////////////////////////////////////////////
                                loan.monthly_payment = Convert.ToDouble(repayments["repaymant_amount"]);
                                found_repayment_table = true;
                            }
                          

                        }


                        if (found_repayment_table == false && DSRepayments.Tables[0].Rows.Count > 0)
                        {

                            loan.monthly_payment = MaxRepayment;

                        }



                        if (employee.terminated && withholdBal)
                        {
                            loan.monthly_payment = loan.balance;
                        }

                        context.SaveChanges();
                    }
                }
               
            }
            
            return Redirect("/Employer/Feeds");
        }

        public ActionResult Rules()
        {
            if (!this.CheckPermission()) return Redirect("/");

            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                ClarityContextContainer context = new ClarityContextContainer();

                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();

                if (organization.organization_settings == null)
                {
                    organization.organization_settings = new OrganizationSettings();
                    organization.organization_settings.alegeus_feed = "";
                    organization.organization_settings.demographics_feed = "";
                    organization.organization_settings.deposits_feed = "";
                    organization.organization_settings.withhold_entire_balance = true;

                    context.SaveChanges();
                }

                ViewData["organization"] = organization;
            }
            else
            {
                ViewData["organization"] = new Organization();
            }

            return View();
        }

        public ActionResult SaveRules(string withhold_entire_balance)
        {
            if (!this.CheckPermission()) return Redirect("/");

            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                ClarityContextContainer context = new ClarityContextContainer();

                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                organization.organization_settings.withhold_entire_balance = (withhold_entire_balance == "true");
                context.SaveChanges();
            }

            return Redirect("/Employer/Rules");
        }

        public ActionResult SaveRepaymentTable(string repayments_json)
        {
            if (!this.CheckPermission()) return Redirect("/");

            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                ClarityContextContainer context = new ClarityContextContainer();

                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                List<int> repayment_table_ids = new List<int>();

                foreach (RepaymentTable table in organization.organization_settings.repayment_table)
                {
                    repayment_table_ids.Add(table.id);
                }
                foreach (int table_id in repayment_table_ids)
                {
                    RepaymentTable table = (from o in context.RepaymentTables where o.id == table_id select o).First<RepaymentTable>();
                    context.RepaymentTables.Remove(table);
                }
                context.SaveChanges();
                organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                var resultObjects = AllChildren(JObject.Parse(repayments_json)).First(c => c.Type == JTokenType.Array && c.Path.Contains("repayments")).Children<JObject>();

                foreach (JObject result in resultObjects)
                {
                    RepaymentTable repayment = new RepaymentTable();
                    repayment.organization_settings = organization.organization_settings;

                    foreach (JProperty property in result.Properties())
                    {
                        if (property.Name == "loan_amount_min")
                        {
                            repayment.loan_amount_min = Double.Parse(property.Value.ToString());
                        }
                        else if (property.Name == "loan_amount_max")
                        {
                            repayment.loan_amount_max = Double.Parse(property.Value.ToString());
                        }
                        else if (property.Name == "repayment_amount")
                        {
                            repayment.repaymant_amount = Double.Parse(property.Value.ToString());
                        }
                    }

                    organization.organization_settings.repayment_table.Add(repayment);
                }

                context.SaveChanges();
            }

            return Redirect("/Employer/Rules");
        }

        private static IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }

        public ActionResult EmployeeDetails(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            List<User> employees = this.EmployeeRollUp(new ClarityContextContainer());
            User found_employee = new User();
            bool found = false;


            foreach (User employee in employees)
            {
                if (employee.id.ToString() == id)
                {
                    ViewData["employee"] = employee;
                    found_employee = employee;
                    found = true;
                }
              
            }

            if (!found)
            {
                return Redirect("/");
            }

            double transactions_to_date = 0;
            foreach (Charge charge in found_employee.employee.charges)
            {
                transactions_to_date += charge.approved_amount;
            }

            ViewData["transactions_to_Date"] = transactions_to_date.ToString("#,##0.00");

            return View();
        }

        public ActionResult DeleteEmployee(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();
            List<User> employees = this.EmployeeRollUp(context);
            User found_employee = new User();
            bool found = false;

            foreach (User employee in employees)
            {
                if (employee.id.ToString() == id)
                {
                    ViewData["employee"] = employee;
                    found_employee = employee;
                    found = true;
                }
            }

            if (!found)
            {
                return Redirect("/");
            }

            List<int> role_ids = new List<int>();

            foreach (Role role in found_employee.roles)
            {
                role_ids.Add(role.id);
            }

            foreach (int role_id in role_ids)
            {
                Role role = (from r in context.Roles where r.id == role_id select r).First<Role>();
                context.Roles.Remove(role);
                context.SaveChanges();
            }

            context.Demographics.Remove(found_employee.employee);
            context.SaveChanges();

            context.Users.Remove(found_employee);
            context.SaveChanges();

            return Redirect("/Employer/Employees");
        }

        public ActionResult AddEmployee()
        {
            ClarityContextContainer context = new ClarityContextContainer();

            int organization_id = (int)Session["active_organization"];
            Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();

            User user = new User();
            user.employee = new Demographic();
            user.employee.company_identifier = "";
            user.employee.payroll_identifier = "";

            
           

            user.employee.social_security_num = ""; // this.getHashSha256(csv[3]);
            user.employee.social_security_last_four = "";
            user.employee.last_name = "";
            user.employee.first_name = "";
            user.employee.dob = new DateTime(1900, 1, 1);
            user.employee.address = "";
            user.employee.city = "";
            user.employee.state = "";
            user.employee.zip = "";
            user.employee.country = "";
            user.employee.email = "";

            user.username = "";
            user.first_name = "";
            user.last_name = "";
            user.email = "";
            user.password = this.getHashSha256("");
            context.Users.Add(user);
            
            Role role = new Role();
            role.user = user;
            role.organization = organization;
            role.access_level = 2;
            role.role_type = "employee";
            context.Roles.Add(role);

            context.SaveChanges();

            return Redirect("/Employer/EmployeeDetails/" + user.id);
        }

        public ActionResult UpdateEmployee(string employee_id, string first_name, string last_name, string status, string email, string address, string city, string state, string zip, string social_security_num, string company_identifier, string payroll_identifier)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();
            List<User> employees = this.EmployeeRollUp(context);
            User employee_to_update = null;

            foreach (User employee in employees)
            {
                if (employee.id.ToString() == employee_id)
                {
                    employee_to_update = employee;
                }
            }

            if (employee_to_update != null)
            {
                employee_to_update.first_name = first_name;
                employee_to_update.last_name = last_name;
                employee_to_update.employee.status = status;
                employee_to_update.email = email;
                employee_to_update.employee.address = address;
                employee_to_update.employee.city = city;
                employee_to_update.employee.state = state;
                employee_to_update.employee.zip = zip;
                // employee_to_update.employee.social_security_num = social_security_num;
                employee_to_update.employee.company_identifier = company_identifier;
                employee_to_update.employee.payroll_identifier = payroll_identifier;
                



                context.SaveChanges();
            }

            return Redirect("/Employer/EmployeeDetails/" + employee_id);
        }

        public ActionResult Terminate(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();
            List<User> employees = this.EmployeeRollUp(context);
            bool found = false;

            foreach (User employee in employees)
            {
                if (employee.id.ToString() == id)
                {
                    employee.employee.terminated = true;
                    context.SaveChanges();
                }
            }

            return Redirect("/Employer/EmployeeDetails/" + id);
        }

        public ActionResult LoanDetails(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            List<User> employees = this.EmployeeRollUp(new ClarityContextContainer());
            bool found = false;

            foreach (User employee in employees)
            {
                foreach (Loan loan in employee.employee.loans)
                {
                    if (loan.id.ToString() == id)
                    {
                        ViewData["employee"] = employee;
                        ViewData["loan"] = loan;
                        found = true;
                    }
                }
            }

            if (!found)
            {
                return Redirect("/");
            }

            return View();
        }

        public ActionResult NewReport()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();
            int organization_id = (int)Session["active_organization"];

            Report report = new Report();
            report.output_format = "excel";
            report.delivery_type = "download_file";
            report.schedule_type = "weekly";
            report.delivery_email_address = "";
            report.delivery_ftp_address = "";
            report.delivery_ftp_password = "";
            report.delivery_ftp_path = "";
            report.delivery_ftp_port = "";
            report.delivery_ftp_username = "";
            report.grouping_field = "";
            report.is_report_builder = false;
            report.name = "";
            report.show_subtotals = false;
            report.show_totals = false;

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                report.organization = organization;
            }
            else
            {
                report.organization = null;
            }

            context.Reports.Add(report);
            context.SaveChanges();

            ViewData["report"] = report;
            ViewData["fields"] = (from f in context.Fields orderby f.source, f.name select f).ToArray<Field>();

            return View("ReportDetails");
        }

        public ActionResult ReportDetails(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            bool found = false;
            List<Report> reports = null;
            int report_id = int.Parse(id);
            int organization_id = (int)Session["active_organization"];
            Role[] contacts = new Role[0];

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                reports = organization.reports.ToList<Report>();

                List<Report> global_reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                foreach (Report global_report in global_reports)
                {
                    if (global_report.global_report_orgs != null)
                    {
                        if (global_report.global_report_orgs.Contains("~" + organization.id + "~"))
                        {
                            reports.Add(global_report);
                        }
                    }
                }

                contacts = organization.roles.ToArray<Role>();
            }
            else
            {
                reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
            }

            foreach (Report report in reports)
            {
                if (report.id == report_id)
                {
                    ViewData["report"] = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            ViewData["fields"] = (from f in context.Fields orderby f.source, f.name select f).ToArray<Field>();
            ViewData["contacts"] = contacts;

            return View();
        }

        public ActionResult ReportFieldDelete(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            int field_id = int.Parse(id);
            ReportField field = (from r in context.ReportFields where r.id == field_id select r).First<ReportField>();

            bool found = false;
            List<Report> reports = null;
            int report_id = field.report.id;
            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                reports = organization.reports.ToList<Report>();

                List<Report> global_reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                foreach (Report global_report in global_reports)
                {
                    if (global_report.global_report_orgs != null)
                    {
                        if (global_report.global_report_orgs.Contains("~" + organization.id + "~"))
                        {
                            reports.Add(global_report);
                        }
                    }
                }
            }
            else
            {
                reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
            }

            foreach (Report report in reports)
            {
                if (report.id == report_id)
                {
                    ViewData["report"] = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            context.ReportFields.Remove(field);
            context.SaveChanges();

            return Redirect("/Employer/ReportDetails/" + report_id);
        }

        public ActionResult ReportSave(string report_id, string page, string group_by, string totals, string subtotals, string output_format, string delivery_type, string payroll_calendar, string delivery_email_address, string delivery_ftp_address, string delivery_ftp_username, string delivery_ftp_password, string delivery_ftp_port, string delivery_ftp_path, string global_report_orgs)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            bool found = false;
            List<Report> reports = null;
            Report theReport = null;
            int report_id_int = int.Parse(report_id);
            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                reports = organization.reports.ToList<Report>();

                List<Report> global_reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                foreach (Report global_report in global_reports)
                {
                    if (global_report.global_report_orgs != null)
                    {
                        if (global_report.global_report_orgs.Contains("~" + organization.id + "~"))
                        {
                            reports.Add(global_report);
                        }
                    }
                }
            }
            else
            {
                reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
            }

            foreach (Report report in reports)
            {
                if (report.id == report_id_int)
                {
                    theReport = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            if (page == "1")
            {
                theReport.show_totals = (totals == "on");
                theReport.show_subtotals = (subtotals == "on");
                theReport.grouping_field = group_by;
            }
            else if (page == "2")
            {
                theReport.output_format = output_format;
                theReport.delivery_type = delivery_type;
                theReport.delivery_email_address = delivery_email_address;
                theReport.delivery_ftp_address = delivery_ftp_address;
                theReport.delivery_ftp_username = delivery_ftp_username;
                theReport.delivery_ftp_password = delivery_ftp_password;
                theReport.delivery_ftp_port = delivery_ftp_port;
                theReport.delivery_ftp_path = delivery_ftp_path;
            }
            else if (page == "3")
            {
                theReport.schedule_type = payroll_calendar;
            }
            else if (page == "4")
            {
                theReport.global_report_orgs = global_report_orgs;
            }

            context.SaveChanges();

            return Redirect("/Employer/ReportDetails/" + report_id);
        }

        public ActionResult SaveReportScheduling(string report_id, string schedule_json)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            bool found = false;
            List<Report> reports = null;
            Report theReport = null;
            int report_id_int = int.Parse(report_id);
            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                reports = organization.reports.ToList<Report>();

                List<Report> global_reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                foreach (Report global_report in global_reports)
                {
                    if (global_report.global_report_orgs != null)
                    {
                        if (global_report.global_report_orgs.Contains("~" + organization.id + "~"))
                        {
                            reports.Add(global_report);
                        }
                    }
                }
            }
            else
            {
                reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
            }

            foreach (Report report in reports)
            {
                if (report.id == report_id_int)
                {
                    theReport = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            theReport.custom_report_schedules.Clear();
            context.SaveChanges();
            var resultObjects = AllChildren(JObject.Parse(schedule_json)).First(c => c.Type == JTokenType.Array && c.Path.Contains("schedules")).Children<JObject>();

            foreach (JObject result in resultObjects)
            {
                CustomReportSchedule schedule = new CustomReportSchedule();
                schedule.day = 0;
                schedule.month = 0;

                foreach (JProperty property in result.Properties())
                {
                    if (property.Name == "start_date")
                    {
                        schedule.start_date = property.Value.ToString();
                    }
                    else if (property.Name == "recurrence")
                    {
                        schedule.recurrence = property.Value.ToString();
                    }
                }

                theReport.custom_report_schedules.Add(schedule);
            }

            context.SaveChanges();

            return Redirect("/Employer/ReportDetails/" + report_id);
        }

        public ActionResult AddField(string report_id, string field_key)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            bool found = false;
            List<Report> reports = null;
            Report theReport = null;
            int report_id_int = int.Parse(report_id);
            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                reports = organization.reports.ToList<Report>();

                List<Report> global_reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                foreach (Report global_report in global_reports)
                {
                    if (global_report.global_report_orgs != null)
                    {
                        if (global_report.global_report_orgs.Contains("~" + organization.id + "~"))
                        {
                            reports.Add(global_report);
                        }
                    }
                }
            }
            else
            {
                reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
            }

            foreach (Report report in reports)
            {
                if (report.id == report_id_int)
                {
                    theReport = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            ReportField field = new ReportField();
            field.report = theReport;

            Field base_field = (from f in context.Fields where f.field_key == field_key select f).First<Field>();
            field.field = base_field;
            field.name = base_field.name;
            field.source = base_field.source;
            field.format = base_field.field_format;

            int order = 0;
            foreach (ReportField existingField in theReport.fields)
            {
                if (existingField.field_order > order)
                {
                    order = existingField.field_order;
                }
            }
            order++;
            field.field_order = order;

            if (field_key == "calculated")
            {
                field.field_type = "Calculated";
                field.calculation = "";
            }
            else
            {
                field.field_type = "Non-Calculated";
                field.calculation = "";
            }

            context.ReportFields.Add(field);
            context.SaveChanges();

            return Redirect("/Employer/ReportDetails/" + report_id);
        }

        public ActionResult UpdateField(string report_id, string field_id, string field_key)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            int field_id_int = int.Parse(field_id);
            ReportField field = (from r in context.ReportFields where r.id == field_id_int select r).First<ReportField>();

            bool found = false;
            List<Report> reports = null;
            int report_id_int = field.report.id;
            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                reports = organization.reports.ToList<Report>();

                List<Report> global_reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                foreach (Report global_report in global_reports)
                {
                    if (global_report.global_report_orgs != null)
                    {
                        if (global_report.global_report_orgs.Contains("~" + organization.id + "~"))
                        {
                            reports.Add(global_report);
                        }
                    }
                }
            }
            else
            {
                reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
            }

            foreach (Report report in reports)
            {
                if (report.id == report_id_int)
                {
                    ViewData["report"] = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            Field base_field = (from f in context.Fields where f.field_key == field_key select f).First<Field>();
            field.field = base_field;
            field.name = base_field.name;
            field.source = base_field.source;
            field.format = base_field.field_format;

            if (field_key == "calculated")
            {
                field.field_type = "Calculated";
            }
            else
            {
                field.field_type = "Non-Calculated";
                field.calculation = "";
            }
            context.SaveChanges();

            return Redirect("/Employer/ReportDetails/" + report_id);
        }

        public ActionResult UpdateCalculation(string report_id, string field_id, string calculation)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            int field_id_int = int.Parse(field_id);
            ReportField field = (from r in context.ReportFields where r.id == field_id_int select r).First<ReportField>();

            bool found = false;
            List<Report> reports = null;
            int report_id_int = field.report.id;
            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                reports = organization.reports.ToList<Report>();

                List<Report> global_reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                foreach (Report global_report in global_reports)
                {
                    if (global_report.global_report_orgs != null)
                    {
                        if (global_report.global_report_orgs.Contains("~" + organization.id + "~"))
                        {
                            reports.Add(global_report);
                        }
                    }
                }
            }
            else
            {
                reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
            }

            foreach (Report report in reports)
            {
                if (report.id == report_id_int)
                {
                    ViewData["report"] = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            field.calculation = calculation;
            context.SaveChanges();

            return Redirect("/Employer/ReportDetails/" + report_id);
        }

        public ActionResult UpdateFieldFormat(string report_id, string field_id, string field_format)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            int field_id_int = int.Parse(field_id);
            ReportField field = (from r in context.ReportFields where r.id == field_id_int select r).First<ReportField>();

            bool found = false;
            List<Report> reports = null;
            int report_id_int = field.report.id;
            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                reports = organization.reports.ToList<Report>();

                List<Report> global_reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                foreach (Report global_report in global_reports)
                {
                    if (global_report.global_report_orgs != null)
                    {
                        if (global_report.global_report_orgs.Contains("~" + organization.id + "~"))
                        {
                            reports.Add(global_report);
                        }
                    }
                }
            }
            else
            {
                reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
            }

            foreach (Report report in reports)
            {
                if (report.id == report_id_int)
                {
                    ViewData["report"] = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            field.format = field_format;
            context.SaveChanges();

            return Redirect("/Employer/ReportDetails/" + report_id);
        }

        public ActionResult UpdateFieldName(string report_id, string field_id, string field_name)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            int field_id_int = int.Parse(field_id);
            ReportField field = (from r in context.ReportFields where r.id == field_id_int select r).First<ReportField>();

            bool found = false;
            List<Report> reports = null;
            int report_id_int = field.report.id;
            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                reports = organization.reports.ToList<Report>();

                List<Report> global_reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                foreach (Report global_report in global_reports)
                {
                    if (global_report.global_report_orgs != null)
                    {
                        if (global_report.global_report_orgs.Contains("~" + organization.id + "~"))
                        {
                            reports.Add(global_report);
                        }
                    }
                }
            }
            else
            {
                reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
            }

            foreach (Report report in reports)
            {
                if (report.id == report_id_int)
                {
                    ViewData["report"] = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            field.name = field_name;
            context.SaveChanges();

            return Redirect("/Employer/ReportDetails/" + report_id);
        }

        public ActionResult EmployeeSearch(string search)
        {
            if (!this.CheckPermission()) return Redirect("/");

            Session["employee_search"] = search;

            return Redirect("/Employer/Employees");
        }

        public ActionResult EmployeeSetOrganization(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            Session["active_organization"] = int.Parse(id);

            if (id == "-1")
            {
                Session["active_organization_name"] = "All Employees";
            }
            else
            {
                ClarityContextContainer context = new ClarityContextContainer();

                int organization_id = int.Parse(id);
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                Session["active_organization_name"] = organization.name;
                Session["active_organization_logo_url"] = organization.logo_url;
                Session["active_organization_color_1"] = organization.color_1;
                Session["active_organization_color_2"] = organization.color_2;
            }

            return Redirect("/Employer/Employees");
        }

        public ActionResult DashboardUpdate(string quadrant_number, string quadrant_chart)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();
            int user_id = (int)Session["user_id"];
            User currentUser = (from u in context.Users where u.id == user_id select u).FirstOrDefault<User>();

            if (currentUser.dashboard_config == null)
            {
                currentUser.dashboard_config = new DashboardConfig();
                currentUser.dashboard_config.quadrant_1 = "0";
                currentUser.dashboard_config.quadrant_2 = "0";
                currentUser.dashboard_config.quadrant_3 = "0";
                currentUser.dashboard_config.quadrant_4 = "0";
            }

            if (quadrant_number == "1")
            {
                currentUser.dashboard_config.quadrant_1 = quadrant_chart;
            }
            else if (quadrant_number == "2")
            {
                currentUser.dashboard_config.quadrant_2 = quadrant_chart;
            }
            else if (quadrant_number == "3")
            {
                currentUser.dashboard_config.quadrant_3 = quadrant_chart;
            }
            else if (quadrant_number == "4")
            {
                currentUser.dashboard_config.quadrant_4 = quadrant_chart;
            }

            context.SaveChanges();

            Session["user"] = currentUser;

            return Redirect("/Employer/Dashboard");
        }

        public ActionResult FeedUpdate(string alegeus_feed, string demographics_feed, string deposits_feed)
        {
            if (!this.CheckPermission()) return Redirect("/");

            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                ClarityContextContainer context = new ClarityContextContainer();

                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();

                if (organization.organization_settings == null)
                {
                    organization.organization_settings = new OrganizationSettings();
                    organization.organization_settings.alegeus_feed = "";
                    organization.organization_settings.demographics_feed = "";
                    organization.organization_settings.deposits_feed = "";
                }

                organization.organization_settings.alegeus_feed = alegeus_feed;
                organization.organization_settings.demographics_feed = demographics_feed;
                organization.organization_settings.deposits_feed = deposits_feed;

                context.SaveChanges();
            }

            return Redirect("/Employer/Feeds");
        }

        public ActionResult RunReport(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            bool found = false;
            List<Report> reports = null;
            int report_id = int.Parse(id);
            int organization_id = (int)Session["active_organization"];

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                reports = organization.reports.ToList<Report>();

                List<Report> global_reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
                foreach (Report global_report in global_reports)
                {
                    if (global_report.global_report_orgs != null)
                    {
                        if (global_report.global_report_orgs.Contains("~" + organization.id + "~"))
                        {
                            reports.Add(global_report);
                        }
                    }
                }
            }
            else
            {
                reports = (from r in context.Reports where r.organization == null select r).ToList<Report>();
            }

            Report report_to_run = new Report();
            foreach (Report report in reports)
            {
                if (report.id == report_id)
                {
                    report_to_run = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            return this.RunActualReport(report_to_run);
        }

        public ActionResult RunActualReport(Report report_to_run)
        {
            List<User> employees = this.EmployeeRollUp(new ClarityContextContainer());
            //Add datestamp
            DateTime dt_today = DateTime.Today;
            String basename = report_to_run.name;
            report_to_run.name = report_to_run.name + "_" + (dt_today.Year.ToString()) + (dt_today.Month.ToString()) + (dt_today.Day.ToString());
            /////////
            if (report_to_run.grouping_field != "" && report_to_run.grouping_field != null)
            {
                if (report_to_run.grouping_field == "first_name")
                {
                    employees = employees.OrderBy(o => o.employee.first_name).ToList();
                }
                else if (report_to_run.grouping_field == "last_name")
                {
                    employees = employees.OrderBy(o => o.employee.last_name).ToList();
                }
                else if (report_to_run.grouping_field == "company_identifier")
                {
                    employees = employees.OrderBy(o => o.employee.company_identifier).ToList();
                }
                else if (report_to_run.grouping_field == "payroll_identifier")
                {
                    employees = employees.OrderBy(o => o.employee.payroll_identifier).ToList();
                }
              
                else if (report_to_run.grouping_field == "dob")
                {
                    employees = employees.OrderBy(o => o.employee.dob).ToList();
                }
                else if (report_to_run.grouping_field == "address")
                {
                    employees = employees.OrderBy(o => o.employee.address).ToList();
                }
                else if (report_to_run.grouping_field == "city")
                {
                    employees = employees.OrderBy(o => o.employee.city).ToList();
                }
                else if (report_to_run.grouping_field == "state")
                {
                    employees = employees.OrderBy(o => o.employee.state).ToList();
                }
                else if (report_to_run.grouping_field == "zip")
                {
                    employees = employees.OrderBy(o => o.employee.zip).ToList();
                }
                else if (report_to_run.grouping_field == "country")
                {
                    employees = employees.OrderBy(o => o.employee.country).ToList();
                }
                else if (report_to_run.grouping_field == "email")
                {
                    employees = employees.OrderBy(o => o.employee.email).ToList();
                }
                else if (report_to_run.grouping_field == "social_security_last_four")
                {
                    employees = employees.OrderBy(o => o.employee.social_security_last_four).ToList();
                }
                else if (report_to_run.grouping_field == "balance")
                {
                    employees = employees.OrderBy(o => o.employee.balance).ToList();
                }
                else if (report_to_run.grouping_field == "monthly_payment")
                {
                    employees = employees.OrderBy(o => o.employee.monthly_payment).ToList();
                }
                else if (report_to_run.grouping_field == "terminated")
                {
                    employees = employees.OrderBy(o => o.employee.terminated).ToList();
                }
                else if (report_to_run.grouping_field == "status")
                {
                    employees = employees.OrderBy(o => o.employee.status).ToList();
                }
              
            }

            MemoryStream memoryStream = new MemoryStream();
            TextWriter tw = new StreamWriter(memoryStream);

            if (report_to_run.output_format == "csv")
            {
                var csv = new CsvWriter(tw);
                foreach (ReportField field in report_to_run.fields)
                {
                    csv.WriteField(field.name);
                }
                csv.NextRecord();

                string grouping_value = null;
                double[] totals = new double[report_to_run.fields.Count()];
                double[] subtotals = new double[report_to_run.fields.Count()];

                for (int i = 0; i < totals.Length; i++)
                {
                    totals[i] = 0;
                    subtotals[i] = 0;
                }

                foreach (User employee in employees)
                {
                    if (grouping_value != null && grouping_value != groupingValueForEmployee(report_to_run, employee))
                    {
                        if (report_to_run.show_subtotals)
                        {
                            int k = 0;
                            foreach (ReportField field in report_to_run.fields)
                            {
                                if (field.format == "Numeric")
                                {
                                    csv.WriteField(String.Format("{0:F2}", Convert.ToDouble(subtotals[k])));
                                }
                                else if (field.format == "Currency")
                                {
                                    csv.WriteField("$" + String.Format("{0:C}", Convert.ToDouble(subtotals[k])));
                                }
                                else
                                {
                                    csv.WriteField("");
                                }

                                k++;
                            }

                            csv.NextRecord();

                            for (int j = 0; j < totals.Length; j++)
                            {
                                subtotals[j] = 0;
                            }
                        }
                    }

                    grouping_value = groupingValueForEmployee(report_to_run, employee);

                    int i = 0;

                    foreach (ReportField field in report_to_run.fields)
                    {
                        string val = this.valForField(field, employee, null, null);

                        double dbl;
                        bool res = Double.TryParse(val.Replace("$", "").Replace(",", ""), out dbl);
                        if (res)
                        {
                            subtotals[i] += dbl;
                            totals[i] += dbl;
                        }

                        csv.WriteField(val);
                        i++;
                    }
                    csv.NextRecord();

                    // Are there charges or deposits to loop through?

                    bool has_charges = false;
                    bool has_deposits = false;

                    foreach (ReportField field in report_to_run.fields)
                    {
                        if (field.source == "Charge")
                        {
                            has_charges = true;
                        }
                        else if (field.source == "Deposit")
                        {
                            has_deposits = true;
                        }
                    }

                    if (has_charges)
                    {
                        foreach (Charge charge in employee.employee.charges)
                        {
                            foreach (ReportField field in report_to_run.fields)
                            {
                                string val = this.valForField(field, employee, null, charge);
                                csv.WriteField(val);
                            }
                            csv.NextRecord();
                        }
                    }

                    if (has_deposits)
                    {
                        foreach (Deposit deposit in employee.employee.deposits)
                        {
                            foreach (ReportField field in report_to_run.fields)
                            {
                                string val = this.valForField(field, employee, deposit, null);
                                csv.WriteField(val);
                            }
                            csv.NextRecord();
                        }
                    }
                }

                if (grouping_value != null)
                {
                    if (report_to_run.show_subtotals)
                    {
                        int k = 0;
                        foreach (ReportField field in report_to_run.fields)
                        {
                            if (field.format == "Numeric")
                            {
                                csv.WriteField(String.Format("{0:F2}", Convert.ToDouble(subtotals[k])));
                            }
                            else if (field.format == "Currency")
                            {
                                csv.WriteField(String.Format("{0:C}", Convert.ToDouble(subtotals[k])));
                            }
                            else
                            {
                                csv.WriteField("");
                            }

                            k++;
                        }

                        csv.NextRecord();

                        for (int j = 0; j < totals.Length; j++)
                        {
                            subtotals[j] = 0;
                        }
                    }
                }

                if (report_to_run.show_totals)
                {
                    int k = 0;
                    foreach (ReportField field in report_to_run.fields)
                    {
                        if (field.format == "Numeric")
                        {
                            csv.WriteField(String.Format("{0:F2}", Convert.ToDouble(totals[k])));
                        }
                        else if (field.format == "Currency")
                        {
                            csv.WriteField(String.Format("{0:C}", Convert.ToDouble(totals[k])));
                        }
                        else
                        {
                            csv.WriteField("");
                        }

                        k++;
                    }

                    csv.NextRecord();
                }

                tw.Flush();
                tw.Close();

              
               
               
              
                
                if (report_to_run.delivery_type == "download_file")
                {
                    return File(memoryStream.GetBuffer(), "text/csv", report_to_run.name + ".csv");
                }
                else if (report_to_run.delivery_type == "ftp_sftp")
                {
                    /*
                    using (WebClient client = new WebClient())
                    {
                        client.Credentials = new NetworkCredential(report_to_run.delivery_ftp_username, report_to_run.delivery_ftp_password);
                        client.UploadFile(report_to_run.delivery_ftp_address + report_to_run.delivery_ftp_path + report_to_run.name + ".csv", "STOR", );
                    }
                    */
                    

                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(report_to_run.delivery_ftp_address + report_to_run.delivery_ftp_path + report_to_run.name + ".csv");
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(report_to_run.delivery_ftp_username, report_to_run.delivery_ftp_password);
                    request.ContentLength = memoryStream.GetBuffer().Length;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(memoryStream.GetBuffer(), 0, memoryStream.GetBuffer().Length);
                    requestStream.Close();
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();
                }
                else if (report_to_run.delivery_type == "secure_email")
                {
                    var apiKey = "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";
                    var client = new SendGridClient(apiKey);
                    var from = new EmailAddress("no-reply@claritybenefits.com", "Clarity HSA Reports");
                    var subject = "Clarity HSA Report: " + report_to_run.name;
                    var to = new EmailAddress(report_to_run.delivery_email_address);
                    var plainTextContent = "Your report is attached.\n\nCheers,\nClarity Benefits";
                    var htmlContent = "Your report is attached<br /><br />Cheers,<br />Clarity Benefits";
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                    var file = Convert.ToBase64String(memoryStream.GetBuffer());
                    msg.AddAttachment(report_to_run.name + ".csv", file, "text/csv", "attachment");
                    var response = client.SendEmailAsync(msg);
                }
            }
            else if (report_to_run.output_format == "txt")
            {
                string line = "";
                foreach (ReportField field in report_to_run.fields)
                {
                    if (line != "") line += "\t";
                    line += field.name;
                }
                tw.WriteLine(line);

                string grouping_value = null;
                double[] totals = new double[report_to_run.fields.Count()];
                double[] subtotals = new double[report_to_run.fields.Count()];

                for (int i = 0; i < totals.Length; i++)
                {
                    totals[i] = 0;
                    subtotals[i] = 0;
                }

                foreach (User employee in employees)
                {
                    if (grouping_value != null && grouping_value != groupingValueForEmployee(report_to_run, employee))
                    {
                        if (report_to_run.show_subtotals)
                        {
                            int k = 0;
                            line = "";
                            foreach (ReportField field in report_to_run.fields)
                            {
                                if (line != "") line += "\t";

                                if (field.format == "Numeric")
                                {
                                    line += String.Format("{0:F2}", Convert.ToDouble(subtotals[k]));
                                }
                                else if (field.format == "Currency")
                                {
                                    line += String.Format("{0:C}", Convert.ToDouble(subtotals[k]));
                                }
                                else
                                {
                                    line += "";
                                }

                                k++;
                            }

                            tw.WriteLine(line);

                            for (int j = 0; j < totals.Length; j++)
                            {
                                subtotals[j] = 0;
                            }
                        }
                    }

                    grouping_value = groupingValueForEmployee(report_to_run, employee);

                    int i = 0;
                    line = "";
                    foreach (ReportField field in report_to_run.fields)
                    {
                        string val = this.valForField(field, employee, null, null);

                        double dbl;
                        bool res = Double.TryParse(val.Replace("$", "").Replace(",", ""), out dbl);
                        if (res)
                        {
                            subtotals[i] += dbl;
                            totals[i] += dbl;
                        }

                        if (line != "") line += "\t";
                        line += val;
                        i++;
                    }
                    tw.WriteLine(line);

                    // Are there charges or deposits to loop through?

                    bool has_charges = false;
                    bool has_deposits = false;

                    foreach (ReportField field in report_to_run.fields)
                    {
                        if (field.source == "Charge")
                        {
                            has_charges = true;
                        }
                        else if (field.source == "Deposit")
                        {
                            has_deposits = true;
                        }
                    }

                    if (has_charges)
                    {
                        foreach (Charge charge in employee.employee.charges)
                        {
                            line = "";
                            foreach (ReportField field in report_to_run.fields)
                            {
                                string val = this.valForField(field, employee, null, charge);
                                if (line != "") line += "\t";
                                line += val;
                            }
                            tw.WriteLine(line);
                        }
                    }

                    if (has_deposits)
                    {
                        foreach (Deposit deposit in employee.employee.deposits)
                        {
                            line = "";
                            foreach (ReportField field in report_to_run.fields)
                            {
                                string val = this.valForField(field, employee, deposit, null);
                                if (line != "") line += "\t";
                                line += val;
                            }
                            tw.WriteLine(line);
                        }
                    }
                }

                if (grouping_value != null)
                {
                    if (report_to_run.show_subtotals)
                    {
                        int k = 0;
                        line = "";
                        foreach (ReportField field in report_to_run.fields)
                        {
                            if (line != "") line += "\t";

                            if (field.format == "Numeric")
                            {
                                line += String.Format("{0:F2}", Convert.ToDouble(subtotals[k]));
                            }
                            else if (field.format == "Currency")
                            {
                                line += String.Format("{0:C}", Convert.ToDouble(subtotals[k]));
                            }
                            else
                            {
                                line += "";
                            }

                            k++;
                        }

                        tw.WriteLine(line);

                        for (int j = 0; j < totals.Length; j++)
                        {
                            subtotals[j] = 0;
                        }
                    }
                }

                if (report_to_run.show_totals)
                {
                    int k = 0;
                    line = "";
                    foreach (ReportField field in report_to_run.fields)
                    {
                        if (line != "") line += "\t";

                        if (field.format == "Numeric")
                        {
                            line += String.Format("{0:F2}", Convert.ToDouble(totals[k]));
                        }
                        else if (field.format == "Currency")
                        {
                            line += String.Format("{0:C}", Convert.ToDouble(totals[k]));
                        }
                        else
                        {
                            line += "";
                        }

                        k++;
                    }

                    tw.WriteLine(line);
                }

                tw.Flush();
                tw.Close();

                if (report_to_run.delivery_type == "download_file")
                {
                    return File(memoryStream.GetBuffer(), "text/plain", report_to_run.name + ".txt");
                }
                else if (report_to_run.delivery_type == "ftp_sftp")
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(report_to_run.delivery_ftp_address + report_to_run.delivery_ftp_path + report_to_run.name + ".txt");
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(report_to_run.delivery_ftp_username, report_to_run.delivery_ftp_password);
                    request.ContentLength = memoryStream.GetBuffer().Length;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(memoryStream.GetBuffer(), 0, memoryStream.GetBuffer().Length);
                    requestStream.Close();
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();
                }
                else if (report_to_run.delivery_type == "secure_email")
                {
                    var apiKey = "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";
                    var client = new SendGridClient(apiKey);
                    var from = new EmailAddress("no-reply@claritybenefits.com", "Clarity HSA Reports");
                    var subject = "Clarity HSA Report: " + report_to_run.name;
                    var to = new EmailAddress(report_to_run.delivery_email_address);
                    var plainTextContent = "Your report is attached.\n\nCheers,\nClarity Benefits";
                    var htmlContent = "Your report is attached<br /><br />Cheers,<br />Clarity Benefits";
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                    var file = Convert.ToBase64String(memoryStream.GetBuffer());
                    msg.AddAttachment(report_to_run.name + ".txt", file, "text/plain", "attachment");
                    var response = client.SendEmailAsync(msg);
                }
            }
            else if (report_to_run.output_format == "excel")
            {
                ExcelPackage pck = new ExcelPackage(memoryStream);
                var ws = pck.Workbook.Worksheets.Add(report_to_run.name);
                //var ws = pck.Workbook.Worksheets.Add("testing123");
                int row = 1;
                int col = 1;

                foreach (ReportField field in report_to_run.fields)
                {
                    ws.Cells[colForInt(col) + row].Value = field.name;
                    col++;
                }
                row++;

                string grouping_value = null;
                double[] totals = new double[report_to_run.fields.Count()];
                double[] subtotals = new double[report_to_run.fields.Count()];

                for (int i = 0; i < totals.Length; i++)
                {
                    totals[i] = 0;
                    subtotals[i] = 0;
                }

                foreach (User employee in employees)
                {


                    employee.employee.company_payroll_identifier = ""; //"testing444567";
                    employee.employee.goal = 0;


                    ////database call to get addtional field values
                    int employeeID = employee.employee.id;
                    //double goalamt = 0;
                    //string compPayrollID = "";
                    //If Transaction if Refund
                    string sql = "SELECT top 1 goal_amount FROM [extra_report_fields] where id = @IDnum";
                    using (SqlConnection conn = new SqlConnection("data source = EC2AMAZ-9HEIG7G; initial catalog = Clarity; Integrated Security = False; User Id = clarity; Password = clarity2017;"))
                    {
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@IDnum", employeeID);
                        try
                        {
                            conn.Open();
                            employee.employee.goal = (double)cmd.ExecuteScalar();
                            conn.Close();
                        }
                        catch (Exception ex)
                        {

                            employee.employee.goal = 0;
                        }
                    }
                    string sql2 = "SELECT top 1 company_payroll_id FROM [extra_report_fields] where id = @IDnum";
                    using (SqlConnection conn2 = new SqlConnection("data source = EC2AMAZ-9HEIG7G; initial catalog = Clarity; Integrated Security = False; User Id = clarity; Password = clarity2017;"))
                    {
                        SqlCommand cmd2 = new SqlCommand(sql2, conn2);
                        cmd2.Parameters.AddWithValue("@IDnum", employeeID);
                        try
                        {
                            conn2.Open();
                            employee.employee.company_payroll_identifier = (string)cmd2.ExecuteScalar();


                            conn2.Close();
                        }
                        catch (Exception ex)
                        {
                            employee.employee.company_payroll_identifier = "B9282";
                        }
                    }

                    string sql3 = "SELECT top 1 Balance FROM [Clarity].[dbo].[Loans] where id = @IDnum";
                    using (SqlConnection conn3 = new SqlConnection("data source = EC2AMAZ-9HEIG7G; initial catalog = Clarity; Integrated Security = False; User Id = clarity; Password = clarity2017;"))
                    {
                        SqlCommand cmd3 = new SqlCommand(sql3, conn3);
                        cmd3.Parameters.AddWithValue("@IDnum", employeeID);
                        try
                        {
                            conn3.Open();
                            employee.employee.balance = (double)cmd3.ExecuteScalar();
                            conn3.Close();
                        }
                        catch (Exception ex)
                        {

                            employee.employee.balance = employee.employee.balance;
                        }
                    }


                    if (employee.employee.goal != 0 && employee.employee.balance != 0)//////////////////////Dont show zero balance employees on reports
                    {
                        
                       
                        if (grouping_value != null && grouping_value != groupingValueForEmployee(report_to_run, employee))
                        {
                            if (report_to_run.show_subtotals)
                            {
                                int k = 0;
                                col = 1;
                                foreach (ReportField field in report_to_run.fields)
                                {
                                    if (field.format == "Numeric")
                                    {
                                        ws.Cells[colForInt(col) + row].Value = String.Format("{0:F2}", Convert.ToDouble(subtotals[k]));
                                        col++;
                                    }
                                    else if (field.format == "Currency")
                                    {
                                        ws.Cells[colForInt(col) + row].Value = "$" + String.Format("{0:C}", Convert.ToDouble(subtotals[k]));
                                        col++;
                                    }
                                    else
                                    {
                                        ws.Cells[colForInt(col) + row].Value = "";
                                        col++;
                                    }

                                    k++;
                                }

                                row++;

                                for (int j = 0; j < totals.Length; j++)
                                {
                                    subtotals[j] = 0;
                                }
                            }
                        }

                        grouping_value = groupingValueForEmployee(report_to_run, employee);

                        int i = 0;
                        col = 1;
                        foreach (ReportField field in report_to_run.fields)
                        {
                            string val = this.valForField(field, employee, null, null);

                            double dbl;
                            bool res = Double.TryParse(val.Replace("$", "").Replace(",", ""), out dbl);
                            if (res)
                            {
                                subtotals[i] += dbl;
                                totals[i] += dbl;
                            }

                            ws.Cells[colForInt(col) + row].Value = val;
                            col++;
                            i++;
                        }
                        row++;

                        // Are there charges or deposits to loop through?

                        bool has_charges = false;
                        bool has_deposits = false;

                        foreach (ReportField field in report_to_run.fields)
                        {
                            if (field.source == "Charge")
                            {
                                has_charges = true;
                            }
                            else if (field.source == "Deposit")
                            {
                                has_deposits = true;
                            }
                        }

                        if (has_charges)
                        {
                            foreach (Charge charge in employee.employee.charges)
                            {
                                col = 1;
                                foreach (ReportField field in report_to_run.fields)
                                {
                                    string val = this.valForField(field, employee, null, charge);
                                    ws.Cells[colForInt(col) + row].Value = val;
                                    col++;
                                }
                                row++;
                            }
                        }

                        if (has_deposits)
                        {
                            foreach (Deposit deposit in employee.employee.deposits)
                            {
                                col = 1;
                                foreach (ReportField field in report_to_run.fields)
                                {
                                    string val = this.valForField(field, employee, deposit, null);
                                    ws.Cells[colForInt(col) + row].Value = val;
                                    col++;
                                }
                                row++;
                            }
                        }
                    }

                    if (grouping_value != null)
                    {
                        if (report_to_run.show_subtotals)
                        {
                            int k = 0;
                            col = 1;
                            foreach (ReportField field in report_to_run.fields)
                            {
                                if (field.format == "Numeric")
                                {
                                    ws.Cells[colForInt(col) + row].Value = String.Format("{0:F2}", Convert.ToDouble(subtotals[k]));
                                    col++;
                                }
                                else if (field.format == "Currency")
                                {
                                    ws.Cells[colForInt(col) + row].Value = "$" + String.Format("{0:C}", Convert.ToDouble(subtotals[k]));
                                    col++;
                                }
                                else
                                {
                                    ws.Cells[colForInt(col) + row].Value = "";
                                    col++;
                                }

                                k++;
                            }

                            row++;

                            for (int j = 0; j < totals.Length; j++)
                            {
                                subtotals[j] = 0;
                            }
                        }
                    }

                    if (report_to_run.show_totals)
                    {
                        int k = 0;
                        col = 1;
                        foreach (ReportField field in report_to_run.fields)
                        {
                            if (field.format == "Numeric")
                            {
                                ws.Cells[colForInt(col) + row].Value = String.Format("{0:F2}", Convert.ToDouble(totals[k]));
                                col++;
                            }
                            else if (field.format == "Currency")
                            {
                                ws.Cells[colForInt(col) + row].Value = "$" + String.Format("{0:C}", Convert.ToDouble(totals[k]));
                                col++;
                            }
                            else
                            {
                                ws.Cells[colForInt(col) + row].Value = "";
                                col++;
                            }

                            k++;
                        }

                        row++;
                    }
                }

                pck.Save();
                if (report_to_run.delivery_type == "download_file")
                {
                    return File(memoryStream.GetBuffer(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", report_to_run.name + ".xlsx");
                }
                else if (report_to_run.delivery_type == "ftp_sftp")
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(report_to_run.delivery_ftp_address + report_to_run.delivery_ftp_path + report_to_run.name + ".xlsx");
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(report_to_run.delivery_ftp_username, report_to_run.delivery_ftp_password);
                    request.ContentLength = memoryStream.GetBuffer().Length;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(memoryStream.GetBuffer(), 0, memoryStream.GetBuffer().Length);
                    requestStream.Close();
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();
                }
                else if (report_to_run.delivery_type == "secure_email")
                {
                    var apiKey = "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";
                    var client = new SendGridClient(apiKey);
                    var from = new EmailAddress("no-reply@claritybenefits.com", "Clarity HSA Reports");
                    var subject = "Clarity HSA Report: " + report_to_run.name;
                    var to = new EmailAddress(report_to_run.delivery_email_address);
                    var plainTextContent = "Your new billing report is located in /FTP-IT/HSA_BillingReports.\n\nCheers,\nClarity Benefits";
                    var htmlContent = "Your new billing report is located in /FTP-IT/HSA_BillingReports.<br /><br />Cheers,<br />Clarity Benefits";
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                    var file = Convert.ToBase64String(memoryStream.GetBuffer());
                    //var file = Convert.ToString(memoryStream.GetBuffer());
                    //msg.AddAttachment(report_to_run.name + ".xlsx", file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "attachment");

                    var ms = memoryStream;
                    var attach_path = report_to_run.name + ".xlsx";
                    var path = "C:/alegeus/Reports/" + report_to_run.name + ".xlsx";
                    Tamir.SharpSsh.Sftp Clarity_ftp = new Tamir.SharpSsh.Sftp("ftp.flexaccount.com", "ClarityIT", "7R7MvTKK");


                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        // Write content of your memory stream into file stream
                       ms.WriteTo(fs);
                       fs.Close();
                       ms.Close();


                       Clarity_ftp.Connect();
                       Clarity_ftp.Put(path, "//HSA_BillingReports//" + attach_path);
                       Clarity_ftp.Close();

                    }

                    //var mailto = "dkaelin@claritybenefitsolutions.com";
                    //var mailfrom = "dkaelin@claritybenefitsolutions.com";
                    //MailMessage message = new MailMessage(mailfrom, mailto);
                    //message.Subject = "Using the new SMTP client.";
                    //message.Body = @"Using this new feature, you can send an email message from an application very easily.";
                    //SmtpClient mailclient = new SmtpClient("smtp.talktalk.net");
                    //// Credentials are necessary if the server requires the client
                    //// to authenticate before it will send email on the client's behalf.
                    //mailclient.UseDefaultCredentials = true;

                    var response = client.SendEmailAsync(msg);
                }
            }
            else if (report_to_run.output_format == "pdf")
            {
                string html_content = "";

                string line = "";
                foreach (ReportField field in report_to_run.fields)
                {
                    line += "<td>" + field.name + "</td>";
                }
                html_content += "<tr>" + line + "</tr>";

                string grouping_value = null;
                double[] totals = new double[report_to_run.fields.Count()];
                double[] subtotals = new double[report_to_run.fields.Count()];

                for (int i = 0; i < totals.Length; i++)
                {
                    totals[i] = 0;
                    subtotals[i] = 0;
                }

                foreach (User employee in employees)
                {
                    if (grouping_value != null && grouping_value != groupingValueForEmployee(report_to_run, employee))
                    {
                        if (report_to_run.show_subtotals)
                        {
                            int k = 0;
                            line = "";
                            foreach (ReportField field in report_to_run.fields)
                            {
                                if (field.format == "Numeric")
                                {
                                    line += "<td>" + String.Format("{0:F2}", Convert.ToDouble(subtotals[k])) + "</td>";
                                }
                                else if (field.format == "Currency")
                                {
                                    line += "<td>$" + String.Format("{0:C}", Convert.ToDouble(subtotals[k])) + "</td>";
                                }
                                else
                                {
                                    line += "<td></td>";
                                }

                                k++;
                            }

                            html_content += "<tr>" + line + "</tr>";

                            for (int j = 0; j < totals.Length; j++)
                            {
                                subtotals[j] = 0;
                            }
                        }
                    }

                    grouping_value = groupingValueForEmployee(report_to_run, employee);

                    int i = 0;
                    line = "";
                    foreach (ReportField field in report_to_run.fields)
                    {
                        string val = this.valForField(field, employee, null, null);

                        double dbl;
                        bool res = Double.TryParse(val.Replace("$", "").Replace(",", ""), out dbl);
                        if (res)
                        {
                            subtotals[i] += dbl;
                            totals[i] += dbl;
                        }

                        line += "<td>" + val + "</td>";
                        i++;
                    }
                    html_content += "<tr>" + line + "</tr>";

                    // Are there charges or deposits to loop through?

                    bool has_charges = false;
                    bool has_deposits = false;

                    foreach (ReportField field in report_to_run.fields)
                    {
                        if (field.source == "Charge")
                        {
                            has_charges = true;
                        }
                        else if (field.source == "Deposit")
                        {
                            has_deposits = true;
                        }
                    }

                    if (has_charges)
                    {
                        foreach (Charge charge in employee.employee.charges)
                        {
                            line = "";
                            foreach (ReportField field in report_to_run.fields)
                            {
                                string val = this.valForField(field, employee, null, charge);
                                line += "<td>" + val + "</td>";
                            }
                            html_content += "<tr>" + line + "</tr>";
                        }
                    }

                    if (has_deposits)
                    {
                        foreach (Deposit deposit in employee.employee.deposits)
                        {
                            line = "";
                            foreach (ReportField field in report_to_run.fields)
                            {
                                string val = this.valForField(field, employee, deposit, null);
                                line += "<td>" + val + "</td>";
                            }
                            html_content += "<tr>" + line + "</tr>";
                        }
                    }
                }

                if (grouping_value != null)
                {
                    if (report_to_run.show_subtotals)
                    {
                        int k = 0;
                        line = "";
                        foreach (ReportField field in report_to_run.fields)
                        {
                            if (field.format == "Numeric")
                            {
                                line += "<td>" + String.Format("{0:F2}", Convert.ToDouble(subtotals[k])) + "</td>";
                            }
                            else if (field.format == "Currency")
                            {
                                line += "<td>$" + String.Format("{0:C}", Convert.ToDouble(subtotals[k])) + "</td>";
                            }
                            else
                            {
                                line += "<td></td>";
                            }

                            k++;
                        }

                        html_content += "<tr>" + line + "</tr>";

                        for (int j = 0; j < totals.Length; j++)
                        {
                            subtotals[j] = 0;
                        }
                    }
                }

                if (report_to_run.show_totals)
                {
                    int k = 0;
                    line = "";
                    foreach (ReportField field in report_to_run.fields)
                    {
                        if (field.format == "Numeric")
                        {
                            line += "<td>" + String.Format("{0:F2}", Convert.ToDouble(totals[k])) + "</td>";
                        }
                        else if (field.format == "Currency")
                        {
                            line += "<td>$" + String.Format("{0:C}", Convert.ToDouble(totals[k])) + "</td>";
                        }
                        else
                        {
                            line += "<td></td>";
                        }

                        k++;
                    }

                    html_content += "<tr>" + line + "</tr>";
                }

                html_content += "<table>" + html_content + "</table>";

                Document document = new Document();
                document = new Document(PageSize.A4, 5, 5, 15, 5);
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();
                System.Xml.XmlTextReader _xmlr = new System.Xml.XmlTextReader(new StringReader(html_content));
                var htmlWorker = new iTextSharp.text.html.simpleparser.HTMLWorker(document);
                StringReader str = new StringReader(html_content);
                htmlWorker.Parse(str);
                document.Close();

                if (report_to_run.delivery_type == "download_file")
                {
                    return File(memoryStream.GetBuffer(), "application/pdf", report_to_run.name + ".pdf");
                }
                else if (report_to_run.delivery_type == "ftp_sftp")
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(report_to_run.delivery_ftp_address + report_to_run.delivery_ftp_path + report_to_run.name + ".pdf");
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(report_to_run.delivery_ftp_username, report_to_run.delivery_ftp_password);
                    request.ContentLength = memoryStream.GetBuffer().Length;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(memoryStream.GetBuffer(), 0, memoryStream.GetBuffer().Length);
                    requestStream.Close();
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();
                }
                else if (report_to_run.delivery_type == "secure_email")
                {
                    var apiKey = "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";
                    var client = new SendGridClient(apiKey);
                    var from = new EmailAddress("no-reply@claritybenefits.com", "Clarity HSA Reports");
                    var subject = "Clarity HSA Report: " + report_to_run.name;
                    var to = new EmailAddress(report_to_run.delivery_email_address);
                    var plainTextContent = "Your report is attached.\n\nCheers,\nClarity Benefits";
                    var htmlContent = "Your report is attached<br /><br />Cheers,<br />Clarity Benefits";
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                    var file = Convert.ToBase64String(memoryStream.GetBuffer());
                    msg.AddAttachment(report_to_run.name + ".pdf", file, "application/pdf", "attachment");
                    var response = client.SendEmailAsync(msg);
                }
            }
            else if (report_to_run.output_format == "xml")
            {
                string xml_content = "";
                string line = "";

                foreach (User employee in employees)
                {
                    // Are there charges or deposits to loop through?

                    bool has_charges = false;
                    bool has_deposits = false;
                    

                    foreach (ReportField field in report_to_run.fields)
                    {
                        if (field.source == "Charge")
                        {
                            has_charges = true;
                        }
                        else if (field.source == "Deposit")
                        {
                            has_deposits = true;
                        }
                    }

                    if (has_charges)
                    {
                        foreach (Charge charge in employee.employee.charges)
                        {
                            line = "";
                            foreach (ReportField field in report_to_run.fields)
                            {
                                string val = this.valForField(field, employee, null, charge);
                                line += "\r\n\t\t<" + field.name + ">" + val + "</" + field.name + ">";
                            }
                            xml_content += "\r\n\t<Employee>" + line + "\r\n\t</Employee>";
                        }
                    }

                    if (has_deposits)
                    {
                        foreach (Deposit deposit in employee.employee.deposits)
                        {
                            line = "";
                            foreach (ReportField field in report_to_run.fields)
                            {
                                string val = this.valForField(field, employee, deposit, null);
                                line += "\r\n\t\t<" + field.name + ">" + val + "</" + field.name + ">";
                            }
                            xml_content += "\r\n\t<Employee>" + line + "\r\n\t</Employee>";
                        }
                    }

                    if (!has_charges && !has_deposits)
                    {
                        line = "";
                        foreach (ReportField field in report_to_run.fields)
                        {
                            string val = this.valForField(field, employee, null, null);

                            line += "\r\n\t\t<" + field.name + ">" + val + "</" + field.name + ">";
                        }
                        xml_content += "\r\n\t<Employee>" + line + "\r\n\t</Employee>";
                    }
                }

                xml_content = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Data xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n<Employees>" + xml_content + "\r\n</Employees>\r\n</Data>";

                tw.Write(xml_content);
                tw.Flush();
                tw.Close();

                if (report_to_run.delivery_type == "download_file")
                {
                    return File(memoryStream.ToArray(), "text/plain", report_to_run.name + ".xml");
                }
                else if (report_to_run.delivery_type == "ftp_sftp")
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(report_to_run.delivery_ftp_address + report_to_run.delivery_ftp_path + report_to_run.name + ".xml");
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(report_to_run.delivery_ftp_username, report_to_run.delivery_ftp_password);
                    request.ContentLength = memoryStream.GetBuffer().Length;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(memoryStream.GetBuffer(), 0, memoryStream.GetBuffer().Length);
                    requestStream.Close();
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();
                }
                else if (report_to_run.delivery_type == "secure_email")
                {
                    var apiKey = "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";
                    var client = new SendGridClient(apiKey);
                    var from = new EmailAddress("no-reply@claritybenefits.com", "Clarity HSA Reports");
                    var subject = "Clarity HSA Report: " + report_to_run.name;
                    var to = new EmailAddress(report_to_run.delivery_email_address);
                    var plainTextContent = "Your report is attached.\n\nCheers,\nClarity Benefits";
                    var htmlContent = "Your report is attached<br /><br />Cheers,<br />Clarity Benefits";
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                    var file = Convert.ToBase64String(memoryStream.GetBuffer());
                    msg.AddAttachment(report_to_run.name + ".xml", file, "text/plain", "attachment");
                    var response = client.SendEmailAsync(msg);
                }
            }
            //report_to_run.name = basename;
            return Redirect("/Employer/Reports");
        }

        private string groupingValueForEmployee(Report report_to_run, User employee)
        {
            if (report_to_run.grouping_field != "" && report_to_run.grouping_field != null)
            {
                if (report_to_run.grouping_field == "first_name")
                {
                    return employee.employee.first_name;
                }
                else if (report_to_run.grouping_field == "last_name")
                {
                    return employee.employee.last_name;
                }
                else if (report_to_run.grouping_field == "company_identifier")
                {
                    return employee.employee.company_identifier;
                }
                else if (report_to_run.grouping_field == "payroll_identifier")
                {
                    return employee.employee.payroll_identifier;
                }
               
                else if (report_to_run.grouping_field == "dob")
                {
                    return employee.employee.dob.ToShortDateString();
                }
                else if (report_to_run.grouping_field == "address")
                {
                    return employee.employee.address;
                }
                else if (report_to_run.grouping_field == "city")
                {
                    return employee.employee.city;
                }
                else if (report_to_run.grouping_field == "state")
                {
                    return employee.employee.state;
                }
                else if (report_to_run.grouping_field == "zip")
                {
                    return employee.employee.zip;
                }
                else if (report_to_run.grouping_field == "country")
                {
                    return employee.employee.country;
                }
                else if (report_to_run.grouping_field == "email")
                {
                    return employee.employee.email;
                }
                else if (report_to_run.grouping_field == "social_security_last_four")
                {
                    return employee.employee.social_security_last_four;
                }
                else if (report_to_run.grouping_field == "terminated")
                {
                    return (employee.employee.terminated ? "True" : "False");
                }
                else if (report_to_run.grouping_field == "status")
                {
                    return employee.employee.status;
                }
                else if (report_to_run.grouping_field == "balance")
                {
                    return "$" + String.Format("{0:C}", Convert.ToDouble(employee.employee.balance));
                }
                else if (report_to_run.grouping_field == "monthly_payment")
                {
                    return String.Format("{0:F2}", Convert.ToDouble(employee.employee.monthly_payment));
                }
                else if (report_to_run.grouping_field == "unrecouped_funds")
                {
                    if (employee.employee.terminated)
                    {
                        return "$" + String.Format("{0:C}", Convert.ToDouble(employee.employee.balance));
                    }
                    else
                    {
                        return "$" + String.Format("{0:C}", 0);
                    }
                }
            }

            return "";
        }

        private string colForInt(int col)
        {
            if (col == 1) return "A";
            else if (col == 2) return "B";
            else if (col == 3) return "C";
            else if (col == 4) return "D";
            else if (col == 5) return "E";
            else if (col == 6) return "F";
            else if (col == 7) return "G";
            else if (col == 8) return "H";
            else if (col == 9) return "I";
            else if (col == 10) return "J";
            else if (col == 11) return "K";
            else if (col == 12) return "L";
            else if (col == 13) return "M";
            else if (col == 14) return "N";
            else if (col == 15) return "O";
            else if (col == 16) return "P";
            else if (col == 17) return "Q";
            else if (col == 18) return "R";
            else if (col == 19) return "S";
            else if (col == 20) return "T";
            else if (col == 21) return "U";
            else if (col == 22) return "V";
            else if (col == 23) return "W";
            else if (col == 24) return "X";
            else if (col == 25) return "Y";
            else if (col == 26) return "Z";
            
            return "A";
        }

        private string valForField(ReportField field, User employee, Deposit deposit, Charge charge)
        {
            string val = "";

            if (field.field_type == "Calculated")
            {
                string formula = field.calculation;

                formula = formula.Replace("{Demographics: First Name}", employee.employee.first_name);
                formula = formula.Replace("{Demographics: Last Name}", employee.employee.last_name);
                formula = formula.Replace("{Demographics: Company Identifier}", employee.employee.company_identifier);
                formula = formula.Replace("{Demographics: Payroll Identifier}", employee.employee.payroll_identifier);
                formula = formula.Replace("{Demographics: Company Payroll Identifier}", employee.employee.company_payroll_identifier);
                formula = formula.Replace("{Demographics: Date of Birth}", employee.employee.dob.ToShortDateString());
                formula = formula.Replace("{Demographics: Address}", employee.employee.address);
                formula = formula.Replace("{Demographics: City}", employee.employee.city);
                formula = formula.Replace("{Demographics: State}", employee.employee.state);
                formula = formula.Replace("{Demographics: Zip}", employee.employee.zip);
                formula = formula.Replace("{Demographics: Country}", employee.employee.country);
                formula = formula.Replace("{Demographics: Email}", employee.employee.email);
                formula = formula.Replace("{Demographics: Social Security (Last 4)}", employee.employee.social_security_last_four);
                formula = formula.Replace("{Demographics: Social Security (Full)}", employee.employee.social_security_num);
                formula = formula.Replace("{Demographics: Terminated}", (employee.employee.terminated ? "True" : "False"));
                formula = formula.Replace("{Demographics: Status}", employee.employee.status);
                formula = formula.Replace("{User Computed: Balance}", String.Format("{0}", Convert.ToDouble(employee.employee.balance)));
                formula = formula.Replace("{User Computed: Goal}", String.Format("{0}", Convert.ToDouble(employee.employee.goal)));
                formula = formula.Replace("{User Computed: Monthly Payment}", String.Format("{0}", Convert.ToDouble(employee.employee.monthly_payment)));
                formula = formula.Replace("{User Computed: Current Date}", DateTime.Now.ToShortDateString());

                string effective_date = "";
                try
                {
                    effective_date = employee.employee.charges.ElementAt<Charge>(0).transaction_date.ToShortDateString();
                }
                catch (Exception ee)
                {
                    effective_date = "";
                }
                formula = formula.Replace("{User Computed: Effective Date}", effective_date);

                if (deposit != null)
                {
                    formula = formula.Replace("{Deposit: Deposit ID}", deposit.id.ToString());
                    formula = formula.Replace("{Deposit: Company Identifier}", employee.employee.company_identifier);
                    formula = formula.Replace("{Deposit: Plan}", deposit.plan_type);
                    formula = formula.Replace("{Deposit: Plan End}", deposit.plan_end);
                    formula = formula.Replace("{Deposit: Employee Contribution}", String.Format("{0}", Convert.ToDouble(deposit.employee_contribution)));
                    formula = formula.Replace("{Deposit: Employer Contribution}", "0"); // String.Format("{0}", Convert.ToDouble(deposit.employer_contribution)));
                    formula = formula.Replace("{Deposit: Payroll Date}", deposit.payroll_date.ToString());
                }

                if (charge != null)
                {
                    formula = formula.Replace("{Charge: Charge ID}", charge.id.ToString());
                    formula = formula.Replace("{Charge: Transaction Date}", charge.transaction_date.ToString());
                    formula = formula.Replace("{Charge: Claim Type}", charge.claim_type);
                    formula = formula.Replace("{Charge: Description}", charge.description);
                    formula = formula.Replace("{Charge: Total Claim Amount}", String.Format("{0}", Convert.ToDouble(charge.total_claim_amount)));
                    formula = formula.Replace("{Charge: Eligible Amount}", String.Format("{0}", Convert.ToDouble(charge.eligible_amount)));
                    formula = formula.Replace("{Charge: Approved Amount}", String.Format("{0}", Convert.ToDouble(charge.approved_amount)));
                    formula = formula.Replace("{Charge: Ineligible Amount}", String.Format("{0}", Convert.ToDouble(charge.ineligible_amount)));
                    formula = formula.Replace("{Charge: Pended Amount}", String.Format("{0}", Convert.ToDouble(charge.pended_amount)));
                    formula = formula.Replace("{Charge: Denied Amount}", String.Format("{0}", Convert.ToDouble(charge.denied_amount)));
                    formula = formula.Replace("{Charge: Denied Reason}", charge.denied_reason);
                    formula = formula.Replace("{Charge: Claim Number}", charge.claim_number);
                    formula = formula.Replace("{Charge: SCC/MCC}", charge.scc_mcc);
                }

                if (employee.employee.terminated)
                {
                    formula = formula.Replace("{User Computed: Unrecouped Funds}", String.Format("{0:C}", Convert.ToDouble(employee.employee.balance)));
                }
                else
                {
                    formula = formula.Replace("{User Computed: Balance}", String.Format("{0:C}", 0));
                }

                if (formula.Contains("\"")) // (formula.Contains("\"+\""))
                {
                    string[] split = formula.Split('+');
                    foreach (string component in split)
                    {
                        var trimmed_component = component.Replace("\"", "");
                        val += trimmed_component;
                    }
                }
                else
                {
                    try
                    {
                        DataTable dt = new DataTable();
                        double dbl = Double.Parse(dt.Compute(formula, "").ToString());

                        if (field.format == "Currency")
                        {
                            val = "$" + String.Format("{0:C}", dbl);
                        }
                        else if (field.format == "Numeric")
                        {
                            val = String.Format("{0:F2}", dbl);
                        }
                        else
                        {
                            val = dbl.ToString();
                        }
                    }
                    catch (Exception ee)
                    {
                        val = "";
                    }
                }
            }
            else
            {
                if (field.field.source == "Demographics")
                {
                    if (field.field.field_key == "first_name")
                    {
                        val = employee.employee.first_name;
                    }
                    else if (field.field.field_key == "last_name")
                    {
                        val = employee.employee.last_name;
                    }
                    else if (field.field.field_key == "company_identifier")
                    {
                        val = employee.employee.company_identifier;
                    }
                    else if (field.field.field_key == "company_payroll_identifier")
                    {
                        val = employee.employee.company_payroll_identifier;

                    }
                    else if (field.field.field_key == "goal")
                    {
                        val = employee.employee.goal.ToString();
                    }
                    else if (field.field.field_key == "payroll_identifier")
                    {
                        val = employee.employee.payroll_identifier;
                    }
                    
                    else if (field.field.field_key == "dob")
                    {
                        val = employee.employee.dob.ToShortDateString();
                    }
                    else if (field.field.field_key == "address")
                    {
                        val = employee.employee.address;
                    }
                    else if (field.field.field_key == "city")
                    {
                        val = employee.employee.city;
                    }
                    else if (field.field.field_key == "state")
                    {
                        val = employee.employee.state;
                    }
                    else if (field.field.field_key == "zip")
                    {
                        val = employee.employee.zip;
                    }
                    else if (field.field.field_key == "country")
                    {
                        val = employee.employee.country;
                    }
                    else if (field.field.field_key == "email")
                    {
                        val = employee.employee.email;
                    }
                    else if (field.field.field_key == "social_security_last_four")
                    {
                        val = employee.employee.social_security_last_four;
                    }
                    else if (field.field.field_key == "social_security_full")
                    {
                        val = employee.employee.social_security_num;
                    }
                    else if (field.field.field_key == "terminated")
                    {
                        val = (employee.employee.terminated ? "True" : "False");
                    }
                    else if (field.field.field_key == "status")
                    {
                        val = employee.employee.status;
                    }
                  
                }
                else if (field.field.source == "Deposit")
                {
                    val = "";

                    if (deposit != null)
                    {
                        if (field.field.field_key == "company_identifier")
                        {
                            val = employee.employee.first_name;
                        }
                        else if (field.field.field_key == "plan_type")
                        {
                            val = deposit.plan_type;
                        }
                        else if (field.field.field_key == "plan_end")
                        {
                            val = deposit.plan_end;
                        }
                        else if (field.field.field_key == "employee_contribution")
                        {
                            val = String.Format("{0:C}", Convert.ToDouble(deposit.employee_contribution));
                        }
                        else if (field.field.field_key == "employer_contribution")
                        {
                            val = "$0.00"; // String.Format("{0:C}", Convert.ToDouble(deposit.employer_contribution));
                        }
                        else if (field.field.field_key == "payroll_date")
                        {
                            val = deposit.payroll_date.ToString();
                        }
                        else if (field.field.field_key == "deposit_id")
                        {
                            val = deposit.id.ToString();
                        }
                    }
                }
                else if (field.field.source == "Charge")
                {
                    val = "";

                    if (charge != null)
                    {
                        if (field.field.field_key == "transaction_date")
                        {
                            val = charge.transaction_date.ToString();
                        }
                        else if (field.field.field_key == "claim_type")
                        {
                            val = charge.claim_type;
                        }
                        else if (field.field.field_key == "description")
                        {
                            val = charge.description;
                        }
                        else if (field.field.field_key == "total_claim_amount")
                        {
                            val = String.Format("{0:C}", Convert.ToDouble(charge.total_claim_amount));
                        }
                        else if (field.field.field_key == "eligible_amount")
                        {
                            val = String.Format("{0:C}", Convert.ToDouble(charge.eligible_amount));
                        }
                        else if (field.field.field_key == "approved_amount")
                        {
                            val = String.Format("{0:C}", Convert.ToDouble(charge.approved_amount));
                        }
                        else if (field.field.field_key == "ineligible_amount")
                        {
                            val = String.Format("{0:C}", Convert.ToDouble(charge.ineligible_amount));
                        }
                        else if (field.field.field_key == "pended_amount")
                        {
                            val = String.Format("{0:C}", Convert.ToDouble(charge.pended_amount));
                        }
                        else if (field.field.field_key == "denied_amount")
                        {
                            val = String.Format("{0:C}", Convert.ToDouble(charge.denied_amount));
                        }
                        else if (field.field.field_key == "denied_reason")
                        {
                            val = charge.denied_reason;
                        }
                        else if (field.field.field_key == "claim_number")
                        {
                            val = charge.claim_number;
                        }
                        else if (field.field.field_key == "scc_mcc")
                        {
                            val = charge.scc_mcc;
                        }
                        else if (field.field.field_key == "charge_id")
                        {
                            val = charge.id.ToString();
                        }
                    }
                }
                else if (field.field.source == "User Computed")
                {
                    if (field.field.field_key == "balance")
                    {
                        val = String.Format("{0:C}", Convert.ToDouble(employee.employee.balance));
                    }
                    if (field.field.field_key == "goal")
                    {
                        val = String.Format("{0:F2}", Convert.ToDouble(employee.employee.goal.ToString()));
                    }
                    else if (field.field.field_key == "monthly_payment")
                    {
                        val = String.Format("{0:F2}", Convert.ToDouble(employee.employee.monthly_payment));
                    }
                    else if (field.field.field_key == "unrecouped_funds")
                    {
                        if (employee.employee.terminated)
                        {
                            val = String.Format("{0:C}", Convert.ToDouble(employee.employee.balance));
                        }
                        else
                        {
                            val = String.Format("{0:C}", 0);
                        }
                    }
                    else if (field.field.field_key == "current_date")
                    {
                        val = DateTime.Now.ToShortDateString();
                    }
                    else if (field.field.field_key == "effective_date")
                    {
                        val = "";
                        try
                        {
                            val = employee.employee.charges.ElementAt<Charge>(0).transaction_date.ToShortDateString();
                        }
                        catch (Exception ee)
                        {
                            val = "";
                        }
                    }
                }
            }

            return val;
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