using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Clarity_HSA.Models;
using System;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Clarity_HSA.Controllers
{
    public class EmployeeController : Controller
    {
        private bool CheckPermission()
        {
            return (Session["user_id"] != null && (bool)Session["has_employee"] == true);
        }

        public ActionResult Index()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ViewData["employee"] = (User)Session["user"];

            User user = (User)Session["user"];

            double charge_total = 0;
            double deposit_total = 0;

            foreach (Charge charge in user.employee.charges)
            {
                charge_total += charge.approved_amount;
            }

            foreach (Deposit deposit in user.employee.deposits)
            {
                deposit_total += deposit.employee_contribution;
            }

            ViewData["progress_percent"] = (charge_total - deposit_total) / charge_total;
            ViewData["charge_total"] = charge_total.ToString("C");
            ViewData["deposit_total"] = deposit_total.ToString("C");
            ViewData["charge_total_scrubbed"] = charge_total;
            ViewData["deposit_total_scrubbed"] = deposit_total;

            return View();
        }

        public ActionResult Dashboard()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ViewData["employee"] = (User)Session["user"];

            User user = (User)Session["user"];

            double charge_total = 0;
            double deposit_total = 0;

            foreach (Charge charge in user.employee.charges)
            {
                charge_total += charge.approved_amount;
            }

            foreach (Deposit deposit in user.employee.deposits)
            {
                deposit_total += deposit.employee_contribution;
            }

            ViewData["progress_percent"] = (charge_total - deposit_total) / charge_total;
            ViewData["charge_total"] = charge_total.ToString("C");
            ViewData["deposit_total"] = deposit_total.ToString("C");
            ViewData["charge_total_scrubbed"] = charge_total;
            ViewData["deposit_total_scrubbed"] = deposit_total;

            return View();
        }

        public ActionResult PaymentHistory()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ViewData["employee"] = (User)Session["user"];

            return View();
        }

        public ActionResult ContactUs()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ViewData["employee"] = (User)Session["user"];

            return View();
        }

        public ActionResult ContactSubmit(string message)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ViewData["employee"] = (User)Session["user"];

            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("no-reply@claritybenefits.com", "Clarity HSA Platform");
            var subject = "Clarity HSA Platform Contact Form: " + ((User)Session["user"]).first_name + " " + ((User)Session["user"]).last_name;
            var to = new EmailAddress("customerservice@claritybenefitsolutions.com");
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = client.SendEmailAsync(msg);

            return Redirect("/Employee/Dashboard");
        }
    }
}