using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Clarity_HSA.Models;
using System;
using Renci.SshNet;
using System.IO;
using System.Net;

namespace Clarity_HSA.Controllers
{
    public class AdminController : Controller
    {
        private bool CheckPermission()
        {
            return (Session["user_id"] != null && (bool)Session["has_admin"] == true);
        }

        public ActionResult Index()
        {
            if (!this.CheckPermission()) return Redirect("/");

            return View();
        }

        public ActionResult CronLogs()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            List<CronLog> logs = (from o in context.CronLogs select o).OrderByDescending(o => o.cron_timestamp).Take(14).ToList<CronLog>();
            ViewData["logs"] = logs;

            return View();
        }

        public ActionResult Organizations()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();
            DbSet<Organization> organizations = context.Organizations;

            List<Organization> clarity = (from o in context.Organizations where o.parent_organization == null select o).ToList<Organization>();
            ViewData["organizations"] = this.BuildOrganizationHierarchy(clarity, 0);

            return View();
        }

        public ActionResult OrganizationDetails(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            int organization_id = int.Parse(id);
            Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
            if (!((List<int>)Session["admin_organization_ids"]).Contains(organization.id)) return Redirect("/Admin/Organizations");

            ViewData["organization"] = organization;

            List<Organization> organizations = (from o in context.Organizations orderby o.name select o).ToList<Organization>();
            ViewData["organizations"] = organizations;

            return View();
        }

        public ActionResult OrganizationCreate()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            List<Organization> organizations = (from o in context.Organizations orderby o.name select o).ToList<Organization>();
            ViewData["organizations"] = organizations;

            return View();
        }

        public ActionResult OrganizationSave(int organization_id, string organization_name, int parent_organization_id, string logo_url, string color_1, string color_2, string company_id, string payroll_id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            Organization organization = new Organization();
            if (organization_id > 0)
            {
                organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                if (!((List<int>)Session["admin_organization_ids"]).Contains(organization.id)) return Redirect("/Admin/Organizations");
            }

            organization.name = organization_name;
            organization.parent_organization = null;
            organization.logo_url = logo_url;
            organization.color_1 = color_1;
            organization.color_2 = color_2;
            organization.company_id = company_id;
            organization.payroll_id = payroll_id;

            if (parent_organization_id != -1)
            {
                Organization parent_organization = (from o in context.Organizations where o.id == parent_organization_id select o).First<Organization>();
                organization.parent_organization = parent_organization;
            }

            if (organization_id == -1)
            {
                context.Organizations.Add(organization);
            }
            context.SaveChanges();

            this.RefreshCurrentUserPermissions();

            return Redirect("/Admin/Organizations");
        }

        public ActionResult OrganizationDelete(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            Organization organization = new Organization();
            int organization_id = int.Parse(id);

            if (organization_id > 0)
            {
                organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                if (!((List<int>)Session["admin_organization_ids"]).Contains(organization.id)) return Redirect("/Admin/Organizations");

                context.Organizations.Remove(organization);
                context.SaveChanges();
            }

            this.RefreshCurrentUserPermissions();

            return Redirect("/Admin/Organizations");
        }

        public ActionResult UserDetails(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            string[] split = id.Split('|');
            int organization_id = int.Parse(split[0]);
            int user_id = int.Parse(split[1]);

            ViewData["organization_id"] = organization_id;

            User user = (from u in context.Users where u.id == user_id select u).First<User>();
            Role role = (from o in context.Roles where o.user.id == user_id && o.organization.id == organization_id select o).First<Role>();
            Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
            if (!((List<int>)Session["admin_organization_ids"]).Contains(organization.id)) return Redirect("/Admin/Organizations");

            ViewData["user"] = user;
            ViewData["role"] = role;
            ViewData["organization"] = organization;

            return View();
        }
        
        public ActionResult UserCreate(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            int organization_id = int.Parse(id);
            ViewData["organization_id"] = organization_id;

            Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
            if (!((List<int>)Session["admin_organization_ids"]).Contains(organization.id)) return Redirect("/Admin/Organizations");
            ViewData["organization"] = organization;

            return View();
        }

        public ActionResult UserConnect(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            int organization_id = int.Parse(id);
            ViewData["organization_id"] = organization_id;

            Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
            if (!((List<int>)Session["admin_organization_ids"]).Contains(organization.id)) return Redirect("/Admin/Organizations");
            ViewData["organization"] = organization;

            List<User> users = (from u in context.Users orderby u.last_name select u).ToList<User>();
            ViewData["users"] = users;

            return View();
        }

        public ActionResult UserSaveNew(int organization_id, string username, string email, string first_name, string last_name, string password, int access_level)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
            if (!((List<int>)Session["admin_organization_ids"]).Contains(organization.id)) return Redirect("/Admin/Organizations");

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
            role.access_level = access_level;
            role.role_type = (access_level == 3) ? "admin" : "employer";
            context.Roles.Add(role);
            context.SaveChanges();

            return Redirect("/Admin/OrganizationDetails/" + organization_id);
        }

        public ActionResult UserSaveEdit(int organization_id, int user_id, string username, string email, string first_name, string last_name, string password, int access_level)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
            if (!((List<int>)Session["admin_organization_ids"]).Contains(organization.id)) return Redirect("/Admin/Organizations");
            User user = (from u in context.Users where u.id == user_id select u).First<User>();
            Role role = (from o in context.Roles where o.user.id == user_id && o.organization.id == organization_id select o).First<Role>();

            role.access_level = access_level;
            role.role_type = (access_level == 3) ? "admin" : "employer";

            user.username = username;
            user.email = email;
            user.first_name = first_name;
            user.last_name = last_name;
            if (password != "fvhjbfv") user.password = this.getHashSha256(password);

            context.SaveChanges();

            this.RefreshCurrentUserPermissions();

            return Redirect("/Admin/OrganizationDetails/" + organization_id);
        }

        public ActionResult UserSaveConnect(int organization_id, int user_id, int access_level)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
            if (!((List<int>)Session["admin_organization_ids"]).Contains(organization.id)) return Redirect("/Admin/Organizations");
            User user = (from u in context.Users where u.id == user_id select u).First<User>();

            Role role = new Role();
            role.user = user;
            role.organization = organization;
            role.access_level = access_level;
            role.role_type = (access_level == 3) ? "admin" : "employer";
            context.Roles.Add(role);
            context.SaveChanges();

            this.RefreshCurrentUserPermissions();

            return Redirect("/Admin/OrganizationDetails/" + organization_id);
        }

        public ActionResult UserDelete(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            string[] split = id.Split('|');
            int organization_id = int.Parse(split[0]);
            int user_id = int.Parse(split[1]);

            Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
            if (!((List<int>)Session["admin_organization_ids"]).Contains(organization.id)) return Redirect("/Admin/Organizations");

            if (user_id > 0 && organization_id > 0)
            {
                // Delete the role, not the user:
                Role role = (from o in context.Roles where o.user.id == user_id && o.organization.id == organization_id select o).First<Role>();

                context.Roles.Remove(role);
                context.SaveChanges();
            }

            this.RefreshCurrentUserPermissions();

            return Redirect("/Admin/OrganizationDetails/" + organization_id);
        }

        private string BuildOrganizationHierarchy(List<Organization> orgs, int level)
        {
            string base_html = "";

            foreach (Organization organization in orgs)
            {
                if (((List<int>)Session["admin_organization_ids"]).Contains(organization.id))
                {
                    base_html += "<tr><td class=\"organization-td\" style=\"margin-left:" + (level * 20) + "px;\"><a href=\"/Admin/OrganizationDetails/" + organization.id + "\" class=\"btn btn-link\">" + organization.name + "</a></td></tr>";
                }

                base_html += this.BuildOrganizationHierarchy(organization.child_organizations.ToList<Organization>(), level + 1);
            }

            return base_html;
        }

        private void RefreshCurrentUserPermissions()
        {
            ClarityContextContainer context = new ClarityContextContainer();

            User user = (User)Session["user"];
            int user_id = user.id;
            user = (from o in context.Users where o.id == user_id select o).First<User>();
            Session["user"] = user;

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