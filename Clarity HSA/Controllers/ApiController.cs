using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Helpers;
using Clarity_HSA.Models;
using System;

namespace Clarity_HSA.Controllers
{
    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "http://192.168.188.129:8100");
            base.OnActionExecuting(filterContext);
        }
    }

    public class ApiController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [AllowCrossSiteJson]
        public ActionResult ProcessLogin(string username, string password)
        {
            ClarityContextContainer context = new ClarityContextContainer();

            string passwordSha256 = this.getHashSha256(password);
            User user = (from u in context.Users where u.username == username && u.password == passwordSha256 select u).FirstOrDefault<User>();

            if (user != null)
            {
                Session["user_id"] = user.id;
                Session["user"] = user;

                Session["has_admin"] = false;
                Session["has_employer"] = false;
                Session["has_employee"] = false;

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
                        this.processChildOrganizations(adminOrganizations, roles[i].organization);
                        this.processChildOrganizationIds(adminOrganizationIds, roles[i].organization);
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

                return Content("{\"result\":\"error\"}", "application/json");
            }

            if ((bool)Session["has_employee"] == true)
            {
                return Content("{\"result\":\"success\", \"data\":{\"user_id\":\"" + Session["user_id"] + "\"}}", "application/json");
            }

            return Content("{\"result\":\"error\"}", "application/json");
        }

        [AllowCrossSiteJson]
        public ActionResult ProcessRegistration(int organization_id, string username, string email, string first_name, string last_name, string password)
        {
            ClarityContextContainer context = new ClarityContextContainer();

            Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();

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

            return this.ProcessLogin(username, password);
        }

        [AllowCrossSiteJson]
        public ActionResult PortalData()
        {
            var employee = (Clarity_HSA.Models.User)Session["user"];

            string loans_json = "[";
            string payments_json = "[";
            /*
            foreach (Loan loan in employee.employee.loans)
            {
                if (loans_json != "[") loans_json += ", ";
                loans_json += "{\"loan_number\":\"" + loan.loan_number + "\", \"loan_amount\":\"" + loan.loan_amount + "\", \"current_balance\":\"" + loan.balance + "\", \"next_payment_amount\":\"" + loan.payment_amount + "\", \"next_payment_date\":\"" + loan.next_payment_date + "\"}";

                foreach (Payment payment in loan.payments)
                {
                    if (payments_json != "[") payments_json += ", ";
                    payments_json += "{\"loan_number\":\"" + loan.loan_number + "\", \"date\":\"" + payment.payment_date + "\", \"payment_amount\":\"" + payment.payment_amount + "\", \"remaining_balance\":\"" + payment.loan_remaining_balance + "\"}";
                }
            }
            */
            loans_json += "]";
            payments_json += "]";

            string user_json = "{\"id\":\"" + employee.id + "\", \"first_name\":\"" + employee.first_name + "\", \"last_name\":\"" + employee.last_name + "\"}";

            return Content("{\"loans\":" + loans_json + ", \"loan_history\":" + payments_json + ", \"payment_history\":" + payments_json + ", \"user\":" + user_json + "}", "application/json");
        }

        [AllowCrossSiteJson]
        public ActionResult GetOrganizations()
        {
            ClarityContextContainer context = new ClarityContextContainer();

            List<Organization> organizations = (from o in context.Organizations select o).ToList<Organization>();
            string organizations_json = "[";

            foreach (Organization organization in organizations)
            {
                if (organizations_json != "[") organizations_json += ", ";
                organizations_json += "{\"id\":\"" + organization.id + "\", \"name\":\"" + organization.name + "\"}";
            }

            organizations_json += "]";

            return Content("{\"organizations\":" + organizations_json + "}", "application/json");
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