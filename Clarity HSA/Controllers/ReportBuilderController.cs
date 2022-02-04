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
using System.Web;
using System.Xml.Linq;
using WinSCP;
using Renci.SshNet;

namespace Clarity_HSA.Controllers
{
    public class ReportBuilderController : Controller
    {
        private bool CheckPermission()
        {
            return (Session["user_id"] != null && (bool)Session["has_report_builder"] == true);
        }

        public ActionResult Index()
        {
            if (!this.CheckPermission()) return Redirect("/");

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

        public ActionResult NewReport()
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();
            int organization_id = (int)Session["active_organization"];
            Role[] contacts = new Role[0];

            Report report = new Report();
            report.output_format = "txt";
            report.delivery_type = "download_file";
            report.schedule_type = "weekly";
            report.delivery_email_address = "";
            report.delivery_ftp_address = "";
            report.delivery_ftp_password = "";
            report.delivery_ftp_path = "";
            report.delivery_ftp_port = "";
            report.delivery_ftp_username = "";
            report.grouping_field = "";
            report.is_report_builder = true;
            report.name = "";
            report.show_subtotals = false;
            report.show_totals = false;

            if (organization_id > 0)
            {
                Organization organization = (from o in context.Organizations where o.id == organization_id select o).First<Organization>();
                report.organization = organization;

                contacts = organization.roles.ToArray<Role>();
            }
            else
            {
                report.organization = null;
            }

            context.Reports.Add(report);
            context.SaveChanges();

            Session["current_tab"] = "tab_loops";
            Session["search_text"] = "";
            Session["left_scroll"] = "0";
            Session["right_scroll"] = "0";

            List<AvailableEntity> all_available_entities = (from e in context.AvailableEntities orderby e.name select e).ToList<AvailableEntity>();
            List<AvailableEntity> filtered_available_entities = new List<AvailableEntity>();
            List<string> available_entity_names = new List<string>();

            foreach (AvailableEntity entity in all_available_entities)
            {
                if (!available_entity_names.Contains(entity.name))
                {
                    available_entity_names.Add(entity.name);
                    filtered_available_entities.Add(entity);
                }
            }

            ViewData["report"] = report;
            ViewData["available_entities"] = filtered_available_entities;
            ViewData["contacts"] = contacts;
            ViewData["current_tab"] = Session["current_tab"];
            ViewData["search_text"] = Session["search_text"];
            ViewData["left_scroll"] = Session["left_scroll"];
            ViewData["right_scroll"] = Session["right_scroll"];

            return View("ReportDetails");
        }

        public ActionResult CloneReport(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            bool found = false;
            List<Report> reports = null;
            int report_id = int.Parse(id);
            int organization_id = (int)Session["active_organization"];
            Role[] contacts = new Role[0];
            Report report_to_clone = new Report();

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
                    report_to_clone = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            var original_entity = context.Reports.AsNoTracking().FirstOrDefault(e => e.id == report_to_clone.id);
            original_entity.name = original_entity.name + " Copy";
            original_entity.report_entities = this.CloneReportEntities(context, original_entity.report_entities.ToList<ReportEntity>(), null, null, original_entity);
            context.Reports.Add(original_entity);
            context.SaveChanges();

            return Redirect("/ReportBuilder/Reports");
        }

        List<ReportEntity> CloneReportEntities(ClarityContextContainer context, List<ReportEntity> original_report_entities, ReportEntity original_parent_entity, ReportEntity parent_entity, Report parent_report)
        {
            List<ReportEntity> cloned_report_entities = new List<ReportEntity>();

            foreach (ReportEntity report_entity in original_report_entities)
            {
                var original_report_entity = context.ReportEntities.AsNoTracking().FirstOrDefault(e => e.id == report_entity.id);
                if (report_entity.parent_entities == null && original_parent_entity == null)
                {
                    original_report_entity.report = parent_report;
                    original_report_entity.parent_entities = parent_entity;
                    original_report_entity.child_entities = this.CloneReportEntities(context, original_report_entity.child_entities.ToList<ReportEntity>(), report_entity, original_report_entity, parent_report);

                    var available_entity = context.AvailableEntities.FirstOrDefault(e => e.id == report_entity.available_entity.id);
                    original_report_entity.available_entity = available_entity;

                    cloned_report_entities.Add(original_report_entity);
                }
                else if (report_entity.parent_entities != null && original_parent_entity != null)
                {
                    if (report_entity.parent_entities.id == original_parent_entity.id)
                    {
                        original_report_entity.report = parent_report;
                        original_report_entity.parent_entities = parent_entity;
                        original_report_entity.child_entities = this.CloneReportEntities(context, original_report_entity.child_entities.ToList<ReportEntity>(), report_entity, original_report_entity, parent_report);

                        var available_entity = context.AvailableEntities.FirstOrDefault(e => e.id == report_entity.available_entity.id);
                        original_report_entity.available_entity = available_entity;

                        cloned_report_entities.Add(original_report_entity);
                    }
                }
            }

            return cloned_report_entities;
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

            if (Session["current_tab"] == null)
            {
                Session["current_tab"] = "tab_loops";
            }

            if (Session["search_text"] == null)
            {
                Session["search_text"] = "";
            }

            if (Session["left_scroll"] == null || (string)Session["left_scroll"] == "")
            {
                Session["left_scroll"] = "0";
            }

            if (Session["right_scroll"] == null || (string)Session["right_scroll"] == "")
            {
                Session["right_scroll"] = "0";
            }

            List<AvailableEntity> all_available_entities = (from e in context.AvailableEntities orderby e.name select e).ToList<AvailableEntity>();
            List<AvailableEntity> filtered_available_entities = new List<AvailableEntity>();
            List<string> available_entity_names = new List<string>();

            foreach (AvailableEntity entity in all_available_entities)
            {
                if (!available_entity_names.Contains(entity.name))
                {
                    available_entity_names.Add(entity.name);
                    filtered_available_entities.Add(entity);
                }
            }

            ViewData["available_entities"] = filtered_available_entities;
            ViewData["contacts"] = contacts;
            ViewData["current_tab"] = Session["current_tab"];
            ViewData["search_text"] = Session["search_text"];
            ViewData["left_scroll"] = Session["left_scroll"];
            ViewData["right_scroll"] = Session["right_scroll"];

            return View();
        }

        private List<ReportEntity> ParseReportEntities(ClarityContextContainer context, Report report, ReportEntity parent, JToken entities)
        {
            List<ReportEntity> reportEntities = new List<ReportEntity>();

            foreach (JToken entity in entities)
            {
                ReportEntity reportEntity = new ReportEntity();
                reportEntity.caption = "";
                reportEntity.format = "";
                reportEntity.literal_value = "";
                reportEntity.section_delimiter = ",";
                reportEntity.map_json = "";
                reportEntity.format_json = "";
                reportEntity.indentation_level = 1;
                reportEntity.parent_entities = parent;
                reportEntity.report = report;

                foreach (JProperty property in entity)
                {
                    if (property.Name == "caption")
                    {
                        reportEntity.caption = property.Value.ToString();
                    }
                    else if (property.Name == "format")
                    {
                        reportEntity.format = property.Value.ToString();
                    }
                    else if (property.Name == "indentation_level")
                    {
                        reportEntity.indentation_level = Int32.Parse(property.Value.ToString());
                    }
                    else if (property.Name == "literal_value")
                    {
                        reportEntity.literal_value = property.Value.ToString();
                    }
                    else if (property.Name == "section_delimiter")
                    {
                        reportEntity.section_delimiter = property.Value.ToString();
                    }
                    else if (property.Name == "map_json")
                    {
                        reportEntity.map_json = property.Value.ToString();
                    }
                    else if (property.Name == "format_json")
                    {
                        reportEntity.format_json = property.Value.ToString();
                    }
                    else if (property.Name == "format_option_fixed_width")
                    {
                        try
                        {
                            reportEntity.format_option_fixed_width = Int32.Parse(property.Value.ToString());
                        }
                        catch (Exception ee)
                        {
                            reportEntity.format_option_fixed_width = 0;
                        }
                    }
                    else if (property.Name == "format_option_fixed_width_val")
                    {
                        try
                        {
                            reportEntity.format_option_fixed_width_val = Int32.Parse(property.Value.ToString());
                        }
                        catch (Exception ee)
                        {
                            reportEntity.format_option_fixed_width_val = 0;
                        }
                    }
                    else if (property.Name == "format_option_capitalize_entire_element")
                    {
                        try
                        {
                            reportEntity.format_option_capitalize_entire_element = Int32.Parse(property.Value.ToString());
                        }
                        catch (Exception ee)
                        {
                            reportEntity.format_option_capitalize_entire_element = 0;
                        }
                    }
                    else if (property.Name == "format_option_remove_characters")
                    {
                        try
                        {
                            reportEntity.format_option_remove_characters = Int32.Parse(property.Value.ToString());
                        }
                        catch (Exception ee)
                        {
                            reportEntity.format_option_remove_characters = 0;
                        }
                    }
                    else if (property.Name == "format_option_remove_characters_list")
                    {
                        reportEntity.format_option_remove_characters_list = property.Value.ToString();
                    }
                    else if (property.Name == "format_option_date")
                    {
                        try
                        {
                            reportEntity.format_option_date = Int32.Parse(property.Value.ToString());
                        }
                        catch (Exception ee)
                        {
                            reportEntity.format_option_date = 0;
                        }
                    }
                    else if (property.Name == "format_option_date_format")
                    {
                        reportEntity.format_option_date_format = property.Value.ToString();
                    }
                    else if (property.Name == "format_option_currency")
                    {
                        reportEntity.format_option_currency = Int32.Parse(property.Value.ToString());
                        try
                        {
                            reportEntity.format_option_currency = Int32.Parse(property.Value.ToString());
                        }
                        catch (Exception ee)
                        {
                            reportEntity.format_option_currency = 0;
                        }
                    }
                    else if (property.Name == "format_option_currency_format")
                    {
                        reportEntity.format_option_currency_format = property.Value.ToString();
                    }
                    else if (property.Name == "option")
                    {
                        reportEntity.option = property.Value.ToString();
                    }
                    else if (property.Name == "available_entity_id")
                    {
                        int available_entity_id = Int32.Parse(property.Value.ToString());
                        reportEntity.available_entity = (from e in context.AvailableEntities where e.id == available_entity_id select e).First<AvailableEntity>();
                    }
                    else if (property.Name == "collapsed")
                    {
                        int collapsed = Int32.Parse(property.Value.ToString());
                        reportEntity.collapsed = (collapsed == 1);
                    }
                    else if (property.Name == "child_entities")
                    {
                        reportEntity.child_entities = ParseReportEntities(context, report, reportEntity, property.Value);
                    }
                }

                if (reportEntity.format_option_currency_format == null) reportEntity.format_option_currency_format = "";
                if (reportEntity.format_option_date_format == null) reportEntity.format_option_date_format = "";
                if (reportEntity.format_option_fixed_width_val == null) reportEntity.format_option_fixed_width_val = 0;
                if (reportEntity.format_option_remove_characters_list == null) reportEntity.format_option_remove_characters_list = "";
                if (reportEntity.option == null) reportEntity.option = "";

                reportEntities.Add(reportEntity);
            }

            return reportEntities;
        }

        public ActionResult ReportSave(string report_id,
            string report_name,
            string entities_json,
            string output_format,
            string delivery_type,
            string delivery_email_address,
            string delivery_ftp_address,
            string delivery_ftp_username,
            string delivery_ftp_password,
            string delivery_ftp_port,
            string delivery_ftp_path,
            string header,
            string footer,
            string global_report_orgs,
            string current_tab,
            string search_text,
            string left_scroll,
            string right_scroll,
            string output_filename,
            string isa02,
            string isa03,
            string isa04,
            string isa05,
            string isa06,
            string isa07,
            string isa08,
            string isa13,
            string isa14,
            string gs02,
            string gs03,
            string gs06,
            string gs08,
            string bgn02,
            string bgn08,
            string st02,
            string dtp03,
            string isa01,
            string isa15,
            string bgn06,
            string ref02,
            string dtp01,
            string n102a,
            string n103a,
            string n104a,
            string n102b,
            string n103b,
            string n104b,
            string n102c,
            string n103c,
            string n104c,
            string dtp01b,
            string dtp03b,
            string include_qty_a,
            string include_qty_b,
            string include_qty_c,
            string include_n1_a,
            string include_n1_b,
            string include_n1_c,
            string include_dtp_a,
            string include_dtp_b,
            string remove_line_breaks,
            string remove_tildes)
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

            theReport.name = report_name;
            theReport.output_format = output_format;
            theReport.delivery_type = delivery_type;
            theReport.delivery_email_address = delivery_email_address;
            theReport.delivery_ftp_address = delivery_ftp_address;
            theReport.delivery_ftp_username = delivery_ftp_username;
            theReport.delivery_ftp_password = delivery_ftp_password;
            theReport.delivery_ftp_port = delivery_ftp_port;
            theReport.delivery_ftp_path = delivery_ftp_path;
            theReport.global_report_orgs = global_report_orgs;
            theReport.header = header;
            theReport.footer = footer;
            theReport.output_filename = output_filename;

            List<int> oldReportEntitites = new List<int>();

            foreach (ReportEntity entity in theReport.report_entities)
            {
                oldReportEntitites.Add(entity.id);
            }

            context.ReportEntities.RemoveRange(context.ReportEntities.Where(x => oldReportEntitites.Contains(x.id)));
            context.SaveChanges();

            var entities = AllChildren(JObject.Parse(entities_json)).First(c => c.Type == JTokenType.Array && c.Path.Contains("entities")).Children<JObject>();
            JObject entities2 = JObject.Parse(entities_json);
            theReport.report_entities = ParseReportEntities(context, theReport, null, entities2["entities"]);

            if (theReport.report_834_options == null)
            {
                theReport.report_834_options = new Report834Options();
            }

            theReport.report_834_options.isa02 = (isa02 == null ? "" : isa02);
            theReport.report_834_options.isa03 = (isa03 == null ? "" : isa03);
            theReport.report_834_options.isa04 = (isa04 == null ? "" : isa04);
            theReport.report_834_options.isa05 = (isa05 == null ? "" : isa05);
            theReport.report_834_options.isa06 = (isa06 == null ? "" : isa06);
            theReport.report_834_options.isa07 = (isa07 == null ? "" : isa07);
            theReport.report_834_options.isa08 = (isa08 == null ? "" : isa08);
            theReport.report_834_options.isa13 = (isa13 == null ? "" : isa13);
            theReport.report_834_options.isa14 = (isa14 == null ? "" : isa14);
            theReport.report_834_options.gs02 = (gs02 == null ? "" : gs02);
            theReport.report_834_options.gs03 = (gs03 == null ? "" : gs03);
            theReport.report_834_options.gs06 = (gs06 == null ? "" : gs06);
            theReport.report_834_options.gs08 = (gs08 == null ? "" : gs08);
            theReport.report_834_options.bgn02 = (bgn02 == null ? "" : bgn02);
            theReport.report_834_options.bgn08 = (bgn08 == null ? "" : bgn08);
            theReport.report_834_options.st02 = (st02 == null ? "" : st02);
            theReport.report_834_options.dtp03 = (dtp03 == null ? "" : dtp03);
            theReport.report_834_options.isa01 = (isa01 == null ? "" : isa01);
            theReport.report_834_options.isa15 = (isa15 == null ? "" : isa15);
            theReport.report_834_options.bgn06 = (bgn06 == null ? "" : bgn06);
            theReport.report_834_options.ref02 = (ref02 == null ? "" : ref02);
            theReport.report_834_options.dtp01 = (dtp01 == null ? "" : dtp01);
            theReport.report_834_options.n102a = (n102a == null ? "" : n102a);
            theReport.report_834_options.n103a = (n103a == null ? "" : n103a);
            theReport.report_834_options.n104a = (n104a == null ? "" : n104a);
            theReport.report_834_options.n102b = (n102b == null ? "" : n102b);
            theReport.report_834_options.n103b = (n103b == null ? "" : n103b);
            theReport.report_834_options.n104b = (n104b == null ? "" : n104b);
            theReport.report_834_options.n102c = (n102c == null ? "" : n102c);
            theReport.report_834_options.n103c = (n103c == null ? "" : n103c);
            theReport.report_834_options.n104c = (n104c == null ? "" : n104c);
            theReport.report_834_options.dtp01b = (dtp01b == null ? "" : dtp01b);
            theReport.report_834_options.dtp03b = (dtp03b == null ? "" : dtp03b);

            theReport.report_834_options.include_qty_a = (include_qty_a == null ? 0 : 1);
            theReport.report_834_options.include_qty_b = (include_qty_b == null ? 0 : 1);
            theReport.report_834_options.include_qty_c = (include_qty_c == null ? 0 : 1);
            theReport.report_834_options.include_n1_a = (include_n1_a == null ? 0 : 1);
            theReport.report_834_options.include_n1_b = (include_n1_b == null ? 0 : 1);
            theReport.report_834_options.include_n1_c = (include_n1_c == null ? 0 : 1);
            theReport.report_834_options.include_dtp_a = (include_dtp_a == null ? 0 : 1);
            theReport.report_834_options.include_dtp_b = (include_dtp_b == null ? 0 : 1);
            theReport.report_834_options.remove_line_breaks = (remove_line_breaks == null ? false : true);
            theReport.report_834_options.remove_tildes = (remove_tildes == null ? false : true);

            context.SaveChanges();

            Session["current_tab"] = current_tab;
            Session["search_text"] = search_text;
            Session["left_scroll"] = left_scroll;
            Session["right_scroll"] = right_scroll;

            return Redirect("/ReportBuilder/ReportDetails/" + report_id);
        }

        public ActionResult DeleteReport(string id)
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

            Report theReport = null;

            foreach (Report report in reports)
            {
                if (report.id == report_id)
                {
                    theReport = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            List<int> oldReportEntitites = new List<int>();

            foreach (ReportEntity entity in theReport.report_entities)
            {
                oldReportEntitites.Add(entity.id);
            }

            context.ReportEntities.RemoveRange(context.ReportEntities.Where(x => oldReportEntitites.Contains(x.id)));
            context.SaveChanges();

            List<int> oldScheduleEntitites = new List<int>();

            foreach (CustomReportSchedule entity in theReport.custom_report_schedules)
            {
                oldScheduleEntitites.Add(entity.id);
            }

            context.CustomReportSchedules.RemoveRange(context.CustomReportSchedules.Where(x => oldScheduleEntitites.Contains(x.id)));
            context.SaveChanges();

            List<int> oldFieldChangeEntities = new List<int>();

            foreach (ReportFieldChange entity in theReport.report_field_changes)
            {
                oldFieldChangeEntities.Add(entity.id);
            }

            context.ReportFieldChanges.RemoveRange(context.ReportFieldChanges.Where(x => oldFieldChangeEntities.Contains(x.id)));
            context.SaveChanges();

            if (theReport.report_834_options != null)
            {
                context.Report834Options.Remove(theReport.report_834_options);
                context.SaveChanges();
            }

            List<int> oldTerminationEntities = new List<int>();

            foreach (Termination entity in theReport.terminations)
            {
                oldTerminationEntities.Add(entity.id);
            }

            context.Terminations.RemoveRange(context.Terminations.Where(x => oldTerminationEntities.Contains(x.id)));
            context.SaveChanges();

            context.Reports.Remove(theReport);
            context.SaveChanges();

            return Redirect("/ReportBuilder/Reports");
        }

        public ActionResult RunReport(string report_id, HttpPostedFileBase report_xml)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            bool found = false;
            List<Report> reports = null;
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

            Report report_to_run = null;

            foreach (Report report in reports)
            {
                if (report.id == report_id_int)
                {
                    report_to_run = report;
                    found = true;
                }
            }

            if (!found) return Redirect("/");

            BinaryReader b = new BinaryReader(report_xml.InputStream);
            byte[] binData = b.ReadBytes((int)report_xml.InputStream.Length);
            string report_xml_string = System.Text.Encoding.UTF8.GetString(binData);

            return this.RunActualReport(report_to_run, report_xml_string);
        }

        // private int total_employees;
        // private int total_dependents;

        private List<string> unique_employees;
        private List<string> unique_dependents;

        public ActionResult RunActualReport(Report report_to_run, string report_xml_string)
        {
            ClarityContextContainer context = new ClarityContextContainer();

            XElement employee_navigator = XElement.Parse(report_xml_string);
            IEnumerable<XElement> employees = from seg in employee_navigator.Descendants("Employee") select seg;

            // Handle Excel, TXT, and PDF!

            MemoryStream memoryStream = new MemoryStream();
            TextWriter tw = new StreamWriter(memoryStream);

            this.unique_employees = new List<string>();
            this.unique_dependents = new List<string>();

            if (report_to_run.output_format == "csv" && false)
            {
                CsvWriter csv = new CsvWriter(tw);
                int row = 1;
                int col = 1;
                string html_content = "";

                foreach (ReportEntity entity in report_to_run.report_entities)
                {
                    if (entity.parent_entities == null)
                    {
                        
                        this.RenderEntity(csv, null, null, ref row, ref col, ref html_content, report_to_run.header, report_to_run.footer, entity, employees, null, null, null, null, null, report_to_run, context, null, employee_navigator, null);
                    }
                }
            }
            else if (report_to_run.output_format == "csv" || report_to_run.output_format == "txt" || report_to_run.output_format == "mbi" || report_to_run.output_format == "834")
            {
                int row = 1;
                int col = 1;
                string html_content = "";

                foreach (ReportEntity entity in report_to_run.report_entities)
                {
                    if (entity.parent_entities == null)
                    {
                        if (report_to_run.output_format == "834")
                        {
                            report_to_run.header = "834";
                            report_to_run.footer = "834";
                        }

                        this.RenderEntity(null, tw, null, ref row, ref col, ref html_content, report_to_run.header, report_to_run.footer, entity, employees, null, null, null, null, null, report_to_run, context, null, employee_navigator, null);
                    }
                }

                tw.Flush();
                tw.Close();

                byte[] bytes = memoryStream.GetBuffer();
                var result_string = System.Text.Encoding.Default.GetString(bytes);
                var number_of_lines = result_string.Split('\n').Length;
                if (number_of_lines > 0) number_of_lines--;
                result_string = result_string.Replace("{TOTAL LINE COUNT}", number_of_lines.ToString());
                result_string = result_string.Replace("{TOTAL LINE COUNT 834}", (number_of_lines - 4).ToString());
                result_string = result_string.Replace("{TOTAL EMPLOYEES}", this.unique_employees.Count.ToString());
                result_string = result_string.Replace("{TOTAL EMPLOYEE COUNT}", this.unique_employees.Count.ToString());
                result_string = result_string.Replace("{TOTAL DEPENDENTS}", this.unique_dependents.Count.ToString());
                result_string = result_string.Replace("{TOTAL DEPENDENT COUNT}", this.unique_dependents.Count.ToString());
                result_string = result_string.Replace("{TOTAL EMPLOYEES PLUS DEPENDENTS}", ((int)this.unique_employees.Count + (int)this.unique_dependents.Count).ToString());
                result_string = result_string.Replace("{TOTAL EMPLOYEE PLUS DEPENDENT COUNT}", ((int)this.unique_employees.Count + (int)this.unique_dependents.Count).ToString());

                // Trim extra spaces at the end of the file:
                result_string = result_string.TrimEnd(' ');
                result_string = result_string.TrimEnd('\0');

                // Handle tilde line endings:
                if (report_to_run.output_format == "834")
                {
                    string[] lines = result_string.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Length > 1)
                        {
                            if (lines[i][lines[i].Length - 1] != '~')
                            {
                                // Remove any trailing delimiters from the end of the line:
                                int characters_to_remove = 0;

                                for (int j = 1; j < lines[i].Length; j++)
                                {
                                    if (lines[i][lines[i].Length - j] == '*') // && lines[i][lines[i].Length - j - 1] == '*')
                                    {
                                        characters_to_remove++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                lines[i] = lines[i].Substring(0, lines[i].Length - characters_to_remove);
                                lines[i] = lines[i] + "~";
                            }
                        }
                    }

                    result_string = string.Join("\r\n", lines);

                    if (report_to_run.report_834_options.remove_line_breaks)
                    {
                        result_string = result_string.Replace("\r\n", "");
                    }

                    if (report_to_run.report_834_options.remove_tildes)
                    {
                        result_string = result_string.Replace("~", "");
                    }
                }

                memoryStream = new MemoryStream();
                tw = new StreamWriter(memoryStream);
                
                tw.Write(result_string);
            }
            else if (report_to_run.output_format == "excel")
            {
                ExcelPackage pck = new ExcelPackage(memoryStream);
                //ExcelWorksheet ws = pck.Workbook.Worksheets.Add(report_to_run.name + "_v2");
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add(report_to_run.name);
                //ExcelWorksheet ws = pck.Workbook.Worksheets.Add("test123v2");
                int row = 1;
                int col = 1;
                string html_content = "";

                foreach (ReportEntity entity in report_to_run.report_entities)
                {
                    if (entity.parent_entities == null)
                    {
                        this.RenderEntity(null, null, ws, ref row, ref col, ref html_content, report_to_run.header, report_to_run.footer, entity, employees, null, null, null, null, null, report_to_run, context, null, employee_navigator, null);
                    }
                }

                pck.Save();
            }
            else if (report_to_run.output_format == "pdf")
            {
                string html_content = "";
                int row = 1;
                int col = 1;

                foreach (ReportEntity entity in report_to_run.report_entities)
                {
                    if (entity.parent_entities == null)
                    {
                        this.RenderEntity(null, null, null, ref row, ref col, ref html_content, report_to_run.header, report_to_run.footer, entity, employees, null, null, null, null, null, report_to_run, context, null, employee_navigator, null);
                    }
                }

                html_content = "<table>" + html_content + "</table>";

                Document document = new Document();
                document = new Document(PageSize.A4, 5, 5, 15, 5);
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();
                System.Xml.XmlTextReader _xmlr = new System.Xml.XmlTextReader(new StringReader(html_content));
                var htmlWorker = new iTextSharp.text.html.simpleparser.HTMLWorker(document);
                StringReader str = new StringReader(html_content);
                htmlWorker.Parse(str);
                document.Close();
            }

            tw.Flush();
            tw.Close();

            string mime = "text/csv";
            string extension = ".csv";

            if (report_to_run.output_format == "txt")
            {
                mime = "text/plain";
                extension = ".txt";
            }
            else if (report_to_run.output_format == "834")
            {
                mime = "text/plain";
                extension = ".txt";
            }
            else if (report_to_run.output_format == "mbi")
            {
                mime = "text/plain";
                extension = ".mbi";
            }
            if (report_to_run.output_format == "excel")
            {
                mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                extension = ".xlsx";
            }
            if (report_to_run.output_format == "pdf")
            {
                mime = "application/pdf";
                extension = ".pdf";
            }

            string filename = report_to_run.name + extension;
            
            if (report_to_run.output_filename != null)
            {
                if (report_to_run.output_filename != "")
                {
                    filename = report_to_run.output_filename;
                }
            }

            filename = filename.Replace("mmddyyyy", DateTime.Now.ToString("MMddyyyy"));
            filename = filename.Replace("MMddyyyy", DateTime.Now.ToString("MMddyyyy"));
            filename = filename.Replace("yyyymmdd", DateTime.Now.ToString("yyyyMMdd"));
            filename = filename.Replace("yyyyMMdd", DateTime.Now.ToString("yyyyMMdd"));

            if (report_to_run.delivery_type == "download_file")
            {
                byte[] fileContents = memoryStream.ToArray();
                return File(fileContents, mime, filename);
            }
            else if (report_to_run.delivery_type == "ftp_sftp")
            {
                if (report_to_run.delivery_ftp_port == "22")
                {
                    using (var sftp = new SftpClient(report_to_run.delivery_ftp_address, Int32.Parse(report_to_run.delivery_ftp_port), report_to_run.delivery_ftp_username, report_to_run.delivery_ftp_password))
                    {
                        using (FileStream file = new FileStream("C:/tmp/tmp.clarity", FileMode.Create, System.IO.FileAccess.Write))
                        {
                            byte[] bytes = memoryStream.ToArray();
                            file.Write(bytes, 0, bytes.Length);
                            memoryStream.Close();
                        }

                        using (MemoryStream ms = new MemoryStream())
                        using (FileStream file = new FileStream("C:/tmp/tmp.clarity", FileMode.Open, FileAccess.Read))
                        {
                            sftp.Connect();
                            sftp.UploadFile(file, report_to_run.delivery_ftp_path + filename);
                            sftp.Disconnect();
                        }
                    }
                }
                else
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(report_to_run.delivery_ftp_address + report_to_run.delivery_ftp_path + filename);
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(report_to_run.delivery_ftp_username, report_to_run.delivery_ftp_password);
                    request.ContentLength = memoryStream.ToArray().Length;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
                    requestStream.Close();
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();
                }
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
                var file = Convert.ToBase64String(memoryStream.ToArray());
                msg.AddAttachment(filename, file, mime, "attachment");
                var response = client.SendEmailAsync(msg);
            }

            return Redirect("/ReportBuilder/Reports");
        }

        public ActionResult EmployeeSetOrganization(string id)
        {
            if (!this.CheckPermission()) return Redirect("/");

            Session["active_organization"] = int.Parse(id);

            if (id == "-1")
            {
                Session["active_organization_name"] = "Global Reports";
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

            return Redirect("/ReportBuilder/Reports");
        }

        public ActionResult AddFieldElement(string report_id, string field_element_name)
        {
            if (!this.CheckPermission()) return Redirect("/");

            ClarityContextContainer context = new ClarityContextContainer();

            AvailableEntity entity = new AvailableEntity();
            entity.entity_type = "Field";
            entity.valid_parent_type = "section";
            entity.name = field_element_name;

            context.AvailableEntities.Add(entity);
            context.SaveChanges();

            return Redirect("/ReportBuilder/ReportDetails/" + report_id);
        }

        public ActionResult SaveReportScheduling(string report_id, string schedule_json, string xml_file_path)
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

            List<int> oldScheduleEntitites = new List<int>();

            foreach (CustomReportSchedule entity in theReport.custom_report_schedules)
            {
                oldScheduleEntitites.Add(entity.id);
            }

            context.CustomReportSchedules.RemoveRange(context.CustomReportSchedules.Where(x => oldScheduleEntitites.Contains(x.id)));
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

            theReport.xml_file_path = xml_file_path;
            context.SaveChanges();

            return Redirect("/ReportBuilder/ReportDetails/" + report_id);
        }

        private string ComputeCalculatedField(ReportEntity field_entity, XElement employee)
        {
            string val = "";
            string formula = field_entity.literal_value;

            // formula = formula.Replace("{Demographics: First Name}", employee.employee.first_name);

            ClarityContextContainer context = new ClarityContextContainer();
            List<AvailableEntity> entities = (from e in context.AvailableEntities where e.entity_type == "Field" orderby e.name select e).ToList<AvailableEntity>();

            foreach (AvailableEntity entity in entities)
            {
                string replacement = (from seg in employee.Elements(this.ScrubFieldName(entity.name)) select (string)seg).FirstOrDefault<string>();
                if (replacement == null) replacement = "";

                formula = formula.Replace("{" + entity.name + "}", replacement);
            }

            if (formula.Contains("\"+\""))
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
                    val = dbl.ToString();
                }
                catch (Exception ee)
                {
                    val = "";
                }
            }

            if (field_entity.format_option_currency == 1)
            {
                /*
                string format_1 = string.Format("${0:00.00}", val);
                string format_2 = string.Format("{0:0.0}", val);
                string format_3 = string.Format("{0:000}", val);
                */

                try
                {
                    if (field_entity.format_option_currency_format.Contains(""))
                    {
                        val = val.Replace("", "");
                        val = val.Replace(",", "");
                        string[] split = field_entity.format_option_currency_format.Split('.');
                        int number_of_digits = split[0].Length - 1;
                        int number_of_decimals = 0;
                        if (split.Length > 1) number_of_decimals = split[1].Length;

                        string prefix_zeroes = "";
                        string suffix_zeroes = "";
                        for (int i = 0; i < number_of_digits; i++) { prefix_zeroes += "0"; }
                        for (int i = 0; i < number_of_decimals; i++) { suffix_zeroes += "0"; }

                        if (val == "") val = "0";

                        if (number_of_decimals > 0)
                        {
                            val = string.Format("${0:" + prefix_zeroes + "." + suffix_zeroes + "}", Double.Parse(val));
                        }
                        else
                        {
                            val = string.Format("${0:" + prefix_zeroes + "}", Double.Parse(val));
                        }

                        // val = String.Format("{0:C}", Convert.ToDouble(val));
                        // val = Convert.ToDouble(val).ToString("C" + number_of_decimals);
                    }
                    else
                    {
                        val = val.Replace("", "");
                        val = val.Replace(",", "");
                        string[] split = field_entity.format_option_currency_format.Split('.');
                        int number_of_digits = split[0].Length;
                        int number_of_decimals = 0;
                        if (split.Length > 1) number_of_decimals = split[1].Length;

                        string prefix_zeroes = "";
                        string suffix_zeroes = "";
                        for (int i = 0; i < number_of_digits; i++) { prefix_zeroes += "0"; }
                        for (int i = 0; i < number_of_decimals; i++) { suffix_zeroes += "0"; }

                        if (val == "") val = "0";

                        if (number_of_decimals > 0)
                        {
                            val = string.Format("{0:" + prefix_zeroes + "." + suffix_zeroes + "}", Double.Parse(val));
                        }
                        else
                        {
                            val = string.Format("{0:" + prefix_zeroes + "}", Double.Parse(val));
                        }

                        // val = String.Format("{0}", Convert.ToDouble(val));
                        // val = Convert.ToDouble(val).ToString("F" + number_of_decimals);
                    }
                }
                catch (Exception ee)
                {
                    val = "";
                }
            }

            return val;
        }

        private string ComputeMappedField(ReportEntity field_entity, XElement employee, XElement currentEmployee, XElement currentEnrollment, string current_loop_name)
        {
            string val = "";
            string formula = field_entity.map_json;
            string[] maps = formula.Split('\n');

            ClarityContextContainer context = new ClarityContextContainer();
            List<AvailableEntity> entities = (from e in context.AvailableEntities where e.entity_type == "Field" orderby e.name select e).ToList<AvailableEntity>();

            try
            {
                foreach (string map in maps)
                {
                    string[] map_split_1 = map.Split(new string[] { " | " }, StringSplitOptions.None);
                    string[] map_split_2 = map_split_1[0].Split(new string[] { " && " }, StringSplitOptions.None);
                    bool match = true;

                    foreach (string clause in map_split_2)
                    {
                        string[] map_split_3 = clause.Split(new string[] { " == " }, StringSplitOptions.None);
                        string replacement = null;

                        if (current_loop_name == "Dependent Enrollees Loop")
                        {
                            if (map_split_3[0].Contains("Employee: "))
                            {
                                replacement = (from seg in currentEmployee.Elements(this.ScrubFieldName(map_split_3[0].Replace("}", "").Replace("{", ""))) select (string)seg).FirstOrDefault<string>();
                            }
                            else if (map_split_3[0].Contains("Enrollment: "))
                            {
                                replacement = (from seg in currentEnrollment.Elements(this.ScrubFieldName(map_split_3[0].Replace("}", "").Replace("{", ""))) select (string)seg).FirstOrDefault<string>();
                            }
                            else if (map_split_3[0].Contains("Dependent: "))
                            {
                                // We need to find the dependent with the same sequence number as this dependent enrollee:
                                string sequence_number = (from seg in employee.Elements("SequenceNumber") select (string)seg).FirstOrDefault<string>();
                                IEnumerable<XElement> dependents = from seg in currentEmployee.Descendants("Dependent") select seg;

                                foreach (XElement dependent in dependents)
                                {
                                    string dependent_sequence_number = (from seg in dependent.Elements("SequenceNumber") select (string)seg).FirstOrDefault<string>();

                                    if (sequence_number.Equals(dependent_sequence_number))
                                    {
                                        replacement = (from seg in dependent.Elements(this.ScrubFieldName(map_split_3[0].Replace("}", "").Replace("{", ""))) select (string)seg).FirstOrDefault<string>();
                                    }
                                }
                            }
                            else
                            {
                                replacement = (from seg in employee.Elements(this.ScrubFieldName(map_split_3[0].Replace("}", "").Replace("{", ""))) select (string)seg).FirstOrDefault<string>();
                            }
                        }
                        else
                        {
                            if (map_split_3[0].Contains("Employee: ") && currentEmployee != null)
                            {
                                replacement = (from seg in currentEmployee.Elements(this.ScrubFieldName(map_split_3[0].Replace("}", "").Replace("{", ""))) select (string)seg).FirstOrDefault<string>();
                            }
                            else
                            {
                                replacement = (from seg in employee.Elements(this.ScrubFieldName(map_split_3[0].Replace("}", "").Replace("{", ""))) select (string)seg).FirstOrDefault<string>();
                            }
                        }

                        if (replacement == null) replacement = "";

                        if (replacement != map_split_3[1] && map_split_3[1] != "*")
                        {
                            match = false;
                        }
                    }

                    if (match)
                    {
                        val = map_split_1[1];

                        foreach (AvailableEntity entity in entities)
                        {
                            string replacement = null;

                            if (current_loop_name == "Dependent Enrollees Loop")
                            {
                                if (entity.name.Contains("Employee: "))
                                {
                                    replacement = (from seg in currentEmployee.Elements(this.ScrubFieldName(entity.name)) select (string)seg).FirstOrDefault<string>();
                                }
                                else if (entity.name.Contains("Enrollment: "))
                                {
                                    replacement = (from seg in currentEnrollment.Elements(this.ScrubFieldName(entity.name)) select (string)seg).FirstOrDefault<string>();
                                }
                                else if (entity.name.Contains("Dependent: "))
                                {
                                    // We need to find the dependent with the same sequence number as this dependent enrollee:
                                    string sequence_number = (from seg in employee.Elements("SequenceNumber") select (string)seg).FirstOrDefault<string>();
                                    IEnumerable<XElement> dependents = from seg in currentEmployee.Descendants("Dependent") select seg;

                                    foreach (XElement dependent in dependents)
                                    {
                                        string dependent_sequence_number = (from seg in dependent.Elements("SequenceNumber") select (string)seg).FirstOrDefault<string>();

                                        if (sequence_number.Equals(dependent_sequence_number))
                                        {
                                            replacement = (from seg in dependent.Elements(this.ScrubFieldName(entity.name)) select (string)seg).FirstOrDefault<string>();
                                        }
                                    }
                                }
                                else
                                {
                                    replacement = (from seg in employee.Elements(this.ScrubFieldName(entity.name)) select (string)seg).FirstOrDefault<string>();
                                }
                            }
                            else
                            {
                                if (entity.name.Contains("Employee: ") && currentEmployee != null)
                                {
                                    replacement = (from seg in currentEmployee.Elements(this.ScrubFieldName(entity.name)) select (string)seg).FirstOrDefault<string>();
                                }
                                else
                                {
                                    replacement = (from seg in employee.Elements(this.ScrubFieldName(entity.name)) select (string)seg).FirstOrDefault<string>();
                                }
                            }

                            if (replacement == null) replacement = "";

                            val = val.Replace("{" + entity.name + "}", replacement);
                        }

                        return val;
                    }
                }
            }
            catch (Exception ee)
            {
                val = "";
            }

            /*
            {Employee: TobaccoUser} == YES && {Employee: USCitizen} == YES | Smoker
            {Employee: TobaccoUser} == NO && {Employee: USCitizen} == YES | Non Smoke
            {Employee: TobaccoUser} == * && {Employee: USCitizen} == * | N/A
            */

            return val;
        }

        private string ComputeJoinField(ReportEntity field_entity, XElement employee, XElement employee_navigator)
        {
            string val = "";

            string entity_name = field_entity.option;
            string common_fields = field_entity.format_option_remove_characters_list;
            string render_field = field_entity.map_json;

            string[] common_field_split = common_fields.Split(',');

            if (common_field_split.Length > 1)
            {
                IEnumerable<XElement> entities = from seg in employee_navigator.Descendants(entity_name) select seg;
                string local_value = (from seg in employee.Elements(common_field_split[0]) select (string)seg).FirstOrDefault<string>();

                foreach (XElement entity in entities)
                {
                    string external_value = (from seg in entity.Elements(common_field_split[1]) select (string)seg).FirstOrDefault<string>();

                    if (local_value == external_value)
                    {
                        val = (from seg in entity.Elements(render_field) select (string)seg).FirstOrDefault<string>();
                    }
                }
            }

            return val;
        }

        private string ScrubXMLTagName(string tag_name)
        {
            return tag_name;
        }

        private string ProcessFieldOptions(ReportEntity field_entity, string val)
        {
            /*
            [x] format_option_fixed_width
            [x] format_option_fixed_width
            [x] format_option_capitalize_entire_element
            [x] format_option_remove_characters
            [x] format_option_remove_characters_list
            [x] format_option_option_date
            [x] format_option_option_date_format
            [x] format_option_option_currency
            [x] format_option_option_currency_format
            */

            if (field_entity.format_option_fixed_width == 1)
            {
                val = val.PadRight((int)field_entity.format_option_fixed_width_val);
                if (val.Length > (int)field_entity.format_option_fixed_width_val) val = val.Substring(0, (int)field_entity.format_option_fixed_width_val);
            }

            if (field_entity.format_option_capitalize_entire_element == 1)
            {
                val = val.ToUpper();
            }

            if (field_entity.format_option_remove_characters == 1)
            {
                foreach (char c in field_entity.format_option_remove_characters_list)
                {
                    val = val.Replace(c.ToString(), "");
                }
            }

            if (field_entity.format_option_date == 1)
            {
                try
                {
                    DateTime convertedDate = DateTime.Parse(val);
                    val = convertedDate.ToString(field_entity.format_option_date_format);
                }
                catch (Exception ee)
                {
                    val = "";
                }
            }

            if (field_entity.format_option_currency == 1)
            {
                /*
                string format_1 = string.Format("${0:00.00}", val);
                string format_2 = string.Format("{0:0.0}", val);
                string format_3 = string.Format("{0:000}", val);
                */

                try
                {
                    if (field_entity.format_option_currency_format.Contains(""))
                    {
                        val = val.Replace("", "");
                        val = val.Replace(",", "");
                        string[] split = field_entity.format_option_currency_format.Split('.');
                        int number_of_digits = split[0].Length - 1;
                        int number_of_decimals = 0;
                        if (split.Length > 1) number_of_decimals = split[1].Length;

                        string prefix_zeroes = "";
                        string suffix_zeroes = "";
                        for (int i = 0; i < number_of_digits; i++) { prefix_zeroes += "0"; }
                        for (int i = 0; i < number_of_decimals; i++) { suffix_zeroes += "0"; }

                        if (val == "") val = "0";

                        if (number_of_decimals > 0)
                        {
                            val = string.Format("${0:" + prefix_zeroes + "." + suffix_zeroes + "}", Double.Parse(val));
                        }
                        else
                        {
                            val = string.Format("${0:" + prefix_zeroes + "}", Double.Parse(val));
                        }

                        // val = String.Format("{0:C}", Convert.ToDouble(val));
                        // val = Convert.ToDouble(val).ToString("C" + number_of_decimals);
                    }
                    else
                    {
                        val = val.Replace("", "");
                        val = val.Replace(",", "");
                        string[] split = field_entity.format_option_currency_format.Split('.');
                        int number_of_digits = split[0].Length;
                        int number_of_decimals = 0;
                        if (split.Length > 1) number_of_decimals = split[1].Length;

                        string prefix_zeroes = "";
                        string suffix_zeroes = "";
                        for (int i = 0; i < number_of_digits; i++) { prefix_zeroes += "0"; }
                        for (int i = 0; i < number_of_decimals; i++) { suffix_zeroes += "0"; }

                        if (val == "") val = "0";

                        if (number_of_decimals > 0)
                        {
                            val = string.Format("{0:" + prefix_zeroes + "." + suffix_zeroes + "}", Double.Parse(val));
                        }
                        else
                        {
                            val = string.Format("{0:" + prefix_zeroes + "}", Double.Parse(val));
                        }

                        // val = String.Format("{0}", Convert.ToDouble(val));
                        // val = Convert.ToDouble(val).ToString("F" + number_of_decimals);
                    }
                }
                catch (Exception ee)
                {
                    val = "";
                }
            }

            return val;
        }

        private string ScrubFieldName(string field_name)
        {
            string[] split = field_name.Split(new string[] { ": " }, StringSplitOptions.None);
            return split[split.Length - 1].Replace("\n", "");
        }

        private void RenderEntity(CsvWriter csv, TextWriter tw, ExcelWorksheet ws, ref int row, ref int col, ref string html_content, string header, string footer, ReportEntity entity, IEnumerable<XElement> employees, ReportEntity currentLoop, List<ReportEntity> currentOperators, List<ReportEntity> currentConditions, List<ReportEntity> currentSections, XElement currentEmployee, Report report, ClarityContextContainer context, IEnumerable<XElement> overrideData, XElement employee_navigator, XElement currentEnrollment)
        {
            /*
            Need to Handle:
            [x] Calculated Fields
            [x] Mapped Fields
            [x] Field Formats
            [x] Custom Delimiters
            */

            if (header != null && header == "834")
            {
                // Generate unique batch number:
                report.report_834_options.isa13 = RandomDigits(9);
                report.report_834_options.gs06 = RandomDigits(9);
                report.report_834_options.st02 = RandomDigits(9);

                header = Render834Header(context, report);

                if (csv != null) csv.WriteField(header);
                if (csv != null) csv.NextRecord();
                if (tw != null) tw.WriteLine(header);

                if (ws != null)
                {
                    col = 1;
                    ws.Cells[this.colForInt(col) + row].Value = header;
                    row++;
                }

                html_content += "<tr><td>" + header + "</td></tr>";
            }
            else if (header != null && header != "")
            {
                header = header.Replace("XX", "{TOTAL LINE COUNT}");
                header = header.Replace("NN", "{TOTAL LINE COUNT}");

                if (csv != null) csv.WriteField(header);
                if (csv != null) csv.NextRecord();
                if (tw != null) tw.WriteLine(header);

                if (ws != null)
                {
                    col = 1;
                    ws.Cells[this.colForInt(col) + row].Value = header;
                    row++;
                }

                html_content += "<tr><td>" + header + "</td></tr>";
            }

            if (entity.available_entity.entity_type == "Loop")
            {
                // Loops should have one child.
                // If the child is an operator, then we need to process the operator and its child condition.
                // If the child is a section, then we need to itterate through its child fields and literals.

                ReportEntity child = entity.child_entities.First<ReportEntity>();

                if (child.available_entity.entity_type == "Section")
                {
                    RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, child, employees, entity, null, null, null, currentEmployee, report, context, null, employee_navigator, currentEnrollment);
                }
                else if (child.available_entity.entity_type == "Operator")
                {
                    // Loop through the various operators in case there are more than one!

                    List<ReportEntity> child_operators = entity.child_entities.ToList<ReportEntity>();
                    List<ReportEntity> child_conditions = new List<ReportEntity>();
                    List<ReportEntity> child_sections = new List<ReportEntity>();

                    foreach (ReportEntity child_operator in child_operators)
                    {
                        ReportEntity second_child = child_operator.child_entities.First<ReportEntity>();

                        if (second_child.available_entity.entity_type == "Condition")
                        {
                            child_conditions.Add(second_child);
                            ReportEntity third_child = second_child.child_entities.First<ReportEntity>();

                            if (third_child.available_entity.entity_type == "Section")
                            {
                                child_sections.Add(third_child);
                            }
                            else
                            {
                                child_sections.Add(null);
                            }
                        }
                        else if (second_child.available_entity.entity_type == "Section")
                        {
                            child_conditions.Add(null);
                            child_sections.Add(second_child);
                        }
                        else
                        {
                            child_conditions.Add(null);
                            child_sections.Add(null);
                        }
                    }

                    RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, child_sections.First<ReportEntity>(), employees, entity, child_operators, child_conditions, child_sections, currentEmployee, report, context, null, employee_navigator, currentEnrollment);
                }
            }
            else if (entity.available_entity.entity_type == "Section")
            {
                string current_loop_name = currentLoop.available_entity.name;

                IEnumerable<XElement> data = employees;
                
                if (overrideData != null)
                {
                    data = overrideData;
                }
                else if (current_loop_name == "Employee Loop")
                {
                    data = employees;
                }
                else if (current_loop_name == "Dependent Loop")
                {
                    data = from seg in currentEmployee.Descendants("Dependent") select seg;
                }
                else if (current_loop_name == "Enrollment Loop")
                {
                    data = from seg in currentEmployee.Descendants("Enrollment") select seg;
                }
                else if (current_loop_name == "Cobra Enrollments Loop")
                {
                    data = from seg in currentEmployee.Descendants("CobraEnrollment") select seg;
                }
                else if (current_loop_name == "Future Salaries Loop")
                {
                    data = from seg in currentEmployee.Descendants("FutureSalary") select seg;
                }
                else if (current_loop_name == "Employee Mapping Loop")
                {
                    data = from seg in currentEmployee.Descendants("EmployeeMapping") select seg;
                }
                else if (current_loop_name == "Dependent Enrollees Loop")
                {
                    if (currentEnrollment != null) data = from seg in currentEnrollment.Descendants("Enrollee") select seg;
                    else data = from seg in currentEmployee.Descendants("Enrollee") select seg;
                }
                else if (current_loop_name == "Benificiaries Loop")
                {
                    data = from seg in currentEmployee.Descendants("Benificiary") select seg;
                }
                else if (current_loop_name == "Plan Mapping Loop")
                {
                    data = from seg in currentEmployee.Descendants("PlanMapping") select seg;
                }
                else if (current_loop_name == "Enrollment Mapping Loop")
                {
                    data = from seg in currentEmployee.Descendants("EnrollmentMapping") select seg;
                }
                else if (current_loop_name == "Cafeteria Data Loop")
                {
                    data = from seg in currentEmployee.Descendants("CafeteriaData") select seg;
                }
                else if (current_loop_name == "HSA Data Loop")
                {
                    data = from seg in currentEmployee.Descendants("HSAData") select seg;
                }

                foreach (XElement employee in data)
                {
                    bool overall_include = false;
                    string change_type = "";
                    bool has_demographic_change = false;
                    bool has_dependent_change = false;
                    bool has_enrollment_change = false;
                    bool terminate_loop = false;

                    if (currentOperators != null && currentConditions != null)
                    {
                        int i = 0;
                        foreach (ReportEntity currentOperator in currentOperators)
                        {
                            bool include = true;
                            ReportEntity currentCondition = currentConditions.ElementAt<ReportEntity>(i);

                            if (currentOperator.available_entity.name == "Else")
                            {
                                if (overall_include == false)
                                {
                                    overall_include = true;
                                    entity = currentSections.ElementAt<ReportEntity>(i);
                                }
                                else
                                {
                                    include = false;
                                }
                            }
                            else
                            {
                                string val = (from seg in employee.Elements(this.ScrubFieldName(currentCondition.option)) select (string)seg).FirstOrDefault<string>();
                                List<string> vals = (from seg in employee.Elements(this.ScrubFieldName(currentCondition.option)) select (string)seg).ToList<string>();

                                if (currentCondition.available_entity.name == "=")
                                {
                                    include = false;

                                    foreach (string one_val in vals)
                                    {
                                        if (one_val == currentCondition.literal_value)
                                        {
                                            include = true;
                                        }
                                    }

                                    /*
                                    if (val != currentCondition.literal_value)
                                    {
                                        include = false;
                                    }
                                    */
                                }
                                else if (currentCondition.available_entity.name == "!=")
                                {
                                    include = false;

                                    foreach (string one_val in vals)
                                    {
                                        if (one_val != currentCondition.literal_value)
                                        {
                                            include = true;
                                        }
                                    }
                                }
                                else if (currentCondition.available_entity.name == "Contains")
                                {
                                    foreach (string one_val in vals)
                                    {
                                        if (one_val.Contains(currentCondition.literal_value))
                                        {
                                            include = true;
                                        }
                                    }
                                }
                                else if (currentCondition.available_entity.name == "Boolean Expression")
                                {
                                    string formula = currentCondition.literal_value;
                                    // {Employee: Gender} == 'M' || {Employee: LastName} == 'Mouse' && {Employee: FirstName} != 'Minnie'
                                    bool bool_res = false;

                                    string[] split_by_or = formula.Split(new string[] { " || " }, StringSplitOptions.None);

                                    foreach (string clause in split_by_or)
                                    {
                                        bool clause_res = true;
                                        string[] split_by_and = clause.Split(new string[] { " && " }, StringSplitOptions.None);

                                        foreach (string subclause in split_by_and)
                                        {
                                            if (subclause.Contains(" == "))
                                            {
                                                string[] split_by_equals = subclause.Split(new string[] { " == " }, StringSplitOptions.None);

                                                if (split_by_equals.Length > 1)
                                                {
                                                    List<string> all_vals;
                                                    if (split_by_equals[0].Contains("Employee: ") && currentEmployee != null) all_vals = (from seg in currentEmployee.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();
                                                    else if ((split_by_equals[0].Contains("Enrollment: ") || split_by_equals[0].Contains("Enrollee: ")) && currentEnrollment != null) all_vals = (from seg in currentEnrollment.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();
                                                    else all_vals = (from seg in employee.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();

                                                    bool clause_match = false;

                                                    foreach (string one_val in all_vals)
                                                    {
                                                        if (one_val == split_by_equals[1].Replace("'", "") || (one_val == "" && split_by_equals[1] == "null"))
                                                        {
                                                            clause_match = true;
                                                        }
                                                    }

                                                    if (!clause_match)
                                                    {
                                                        clause_res = false;
                                                    }
                                                }
                                                else
                                                {
                                                    clause_res = false;
                                                }
                                            }
                                            else if (subclause.Contains(" != "))
                                            {
                                                string[] split_by_equals = subclause.Split(new string[] { " != " }, StringSplitOptions.None);

                                                if (split_by_equals.Length > 1)
                                                {
                                                    List<string> all_vals;
                                                    if (split_by_equals[0].Contains("Employee: ") && currentEmployee != null) all_vals = (from seg in currentEmployee.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();
                                                    else if ((split_by_equals[0].Contains("Enrollment: ") || split_by_equals[0].Contains("Enrollee: ")) && currentEnrollment != null) all_vals = (from seg in currentEnrollment.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();
                                                    else all_vals = (from seg in employee.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();

                                                    bool clause_match = false;

                                                    foreach (string one_val in all_vals)
                                                    {
                                                        if ((one_val != split_by_equals[1].Replace("'", "") && split_by_equals[1] != "null") || (one_val != "" && split_by_equals[1] == "null"))
                                                        {
                                                            clause_match = true;
                                                        }
                                                    }

                                                    if (!clause_match)
                                                    {
                                                        clause_res = false;
                                                    }
                                                }
                                                else
                                                {
                                                    clause_res = false;
                                                }
                                            }
                                            else if (subclause.Contains(" > "))
                                            {
                                                string[] split_by_equals = subclause.Split(new string[] { " > " }, StringSplitOptions.None);

                                                if (split_by_equals.Length > 1)
                                                {
                                                    List<string> all_vals;
                                                    if (split_by_equals[0].Contains("Employee: ") && currentEmployee != null) all_vals = (from seg in currentEmployee.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();
                                                    else if ((split_by_equals[0].Contains("Enrollment: ") || split_by_equals[0].Contains("Enrollee: ")) && currentEnrollment != null) all_vals = (from seg in currentEnrollment.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();
                                                    else all_vals = (from seg in employee.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();

                                                    bool clause_match = false;

                                                    foreach (string one_val in all_vals)
                                                    {
                                                        // Convert each side to a DateTime:
                                                        try
                                                        {
                                                            DateTime one_val_dt = DateTime.Parse(one_val);

                                                            string second_val = "";
                                                            if (split_by_equals[1].Contains("Employee: ") && currentEmployee != null) second_val = (from seg in currentEmployee.Elements(this.ScrubFieldName(split_by_equals[1].Replace("{", "").Replace("}", ""))) select (string)seg).FirstOrDefault<string>();
                                                            else if ((split_by_equals[1].Contains("Enrollment: ") || split_by_equals[0].Contains("Enrollee: ")) && currentEnrollment != null) second_val = (from seg in currentEnrollment.Elements(this.ScrubFieldName(split_by_equals[1].Replace("{", "").Replace("}", ""))) select (string)seg).FirstOrDefault<string>();
                                                            else second_val = (from seg in employee.Elements(this.ScrubFieldName(split_by_equals[1].Replace("{", "").Replace("}", ""))) select (string)seg).FirstOrDefault<string>();

                                                            DateTime split_by_equals_dt = DateTime.Parse(second_val);

                                                            if (one_val_dt > split_by_equals_dt)
                                                            {
                                                                clause_match = true;
                                                            }
                                                        }
                                                        catch (Exception ee)
                                                        {
                                                        }
                                                    }

                                                    if (!clause_match)
                                                    {
                                                        clause_res = false;
                                                    }
                                                }
                                                else
                                                {
                                                    clause_res = false;
                                                }
                                            }
                                            else if (subclause.Contains(" < "))
                                            {
                                                string[] split_by_equals = subclause.Split(new string[] { " < " }, StringSplitOptions.None);

                                                if (split_by_equals.Length > 1)
                                                {
                                                    List<string> all_vals;
                                                    if (split_by_equals[0].Contains("Employee: ") && currentEmployee != null) all_vals = (from seg in currentEmployee.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();
                                                    else if ((split_by_equals[0].Contains("Enrollment: ") || split_by_equals[0].Contains("Enrollee: ")) && currentEnrollment != null) all_vals = (from seg in currentEnrollment.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();
                                                    else all_vals = (from seg in employee.Elements(this.ScrubFieldName(split_by_equals[0].Replace("{", "").Replace("}", ""))) select (string)seg).ToList<string>();

                                                    bool clause_match = false;

                                                    foreach (string one_val in all_vals)
                                                    {
                                                        try
                                                        {
                                                            // Convert each side to a DateTime:
                                                            DateTime one_val_dt = DateTime.Parse(one_val);

                                                            string second_val = "";
                                                            if (split_by_equals[1].Contains("Employee: ") && currentEmployee != null) second_val = (from seg in currentEmployee.Elements(this.ScrubFieldName(split_by_equals[1].Replace("{", "").Replace("}", ""))) select (string)seg).FirstOrDefault<string>();
                                                            else if ((split_by_equals[1].Contains("Enrollment: ") || split_by_equals[0].Contains("Enrollee: ")) && currentEnrollment != null) second_val = (from seg in currentEnrollment.Elements(this.ScrubFieldName(split_by_equals[1].Replace("{", "").Replace("}", ""))) select (string)seg).FirstOrDefault<string>();
                                                            else second_val = (from seg in employee.Elements(this.ScrubFieldName(split_by_equals[1].Replace("{", "").Replace("}", ""))) select (string)seg).FirstOrDefault<string>();

                                                            DateTime split_by_equals_dt = DateTime.Parse(second_val);

                                                            if (one_val_dt < split_by_equals_dt)
                                                            {
                                                                clause_match = true;
                                                            }
                                                        }
                                                        catch (Exception ee)
                                                        {
                                                        }
                                                    }

                                                    if (!clause_match)
                                                    {
                                                        clause_res = false;
                                                    }
                                                }
                                                else
                                                {
                                                    clause_res = false;
                                                }
                                            }
                                            else
                                            {
                                                clause_res = false;
                                            }
                                        }

                                        if (clause_res == true)
                                        {
                                            bool_res = true;
                                        }
                                    }

                                    if (bool_res == false)
                                    {
                                        include = false;
                                    }
                                }
                                else if (currentCondition.available_entity.name == "Field Change")
                                {
                                    string[] field_change_option_fields = currentCondition.option.Split('\n');
                                    string unique_identifier_expression = currentCondition.literal_value;
                                    bool found_any_field_change = false;

                                    List<AvailableEntity> entities = (from e in context.AvailableEntities where e.entity_type == "Field" orderby e.name select e).ToList<AvailableEntity>();

                                    foreach (AvailableEntity avail_entity in entities)
                                    {
                                        string replacement = (from seg in employee.Elements(this.ScrubFieldName(avail_entity.name)) select (string)seg).FirstOrDefault<string>();
                                        if (replacement == null) replacement = "";

                                        unique_identifier_expression = unique_identifier_expression.Replace("{" + avail_entity.name + "}", replacement);
                                    }

                                    foreach (string field_change_option_field in field_change_option_fields)
                                    {
                                        string change_val = (from seg in employee.Elements(this.ScrubFieldName(field_change_option_field)) select (string)seg).FirstOrDefault<string>();

                                        bool found_field_change = false;
                                        string unique_identifier = "";

                                        if (unique_identifier_expression != "")
                                        {
                                            unique_identifier = unique_identifier_expression;
                                        }
                                        else
                                        {
                                            if (field_change_option_field.Contains("Employee:"))
                                            {
                                                unique_identifier = (from seg in employee.Elements("SSN") select (string)seg).FirstOrDefault<string>();
                                            }
                                            else if (field_change_option_field.Contains("Dependent:"))
                                            {
                                                unique_identifier = (from seg in employee.Elements("SSN") select (string)seg).FirstOrDefault<string>();
                                            }
                                            else if (field_change_option_field.Contains("Enrollment:"))
                                            {
                                                unique_identifier = (from seg in employee.Elements("LastModified") select (string)seg).FirstOrDefault<string>();
                                            }
                                        }

                                        foreach (ReportFieldChange field_change in report.report_field_changes)
                                        {
                                            if (field_change_option_field.Contains("Employee:"))
                                            {
                                                if (field_change.field_name == field_change_option_field && field_change.record_type == "Employee" && field_change.unique_identifier == unique_identifier)
                                                {
                                                    found_field_change = true;

                                                    string original_value = field_change.field_value.ToString();
                                                    string new_value = change_val.ToString();

                                                    if (original_value.Equals(new_value))
                                                    {
                                                        // include = false;
                                                    }
                                                    else
                                                    {
                                                        field_change.field_value = change_val;
                                                        context.SaveChanges();
                                                        change_type = "Demographic";
                                                        has_demographic_change = true;
                                                        found_any_field_change = true;
                                                    }
                                                }
                                            }
                                            else if (field_change_option_field.Contains("Dependent:"))
                                            {
                                                if (field_change.field_name == field_change_option_field && field_change.record_type == "Dependent" && field_change.unique_identifier == unique_identifier)
                                                {
                                                    found_field_change = true;

                                                    if (field_change.field_value.ToString().Equals(change_val.ToString()))
                                                    {
                                                        // include = false;
                                                    }
                                                    else
                                                    {
                                                        field_change.field_value = change_val;
                                                        context.SaveChanges();
                                                        change_type = "Dependent";
                                                        has_dependent_change = true;
                                                        found_any_field_change = true;
                                                    }
                                                }
                                            }
                                            else if (field_change_option_field.Contains("Enrollment:"))
                                            {
                                                if (field_change.field_name == field_change_option_field && field_change.record_type == "Enrollment" && field_change.unique_identifier == unique_identifier)
                                                {
                                                    found_field_change = true;

                                                    if (field_change.field_value.ToString().Equals(change_val.ToString()))
                                                    {
                                                        // include = false;
                                                    }
                                                    else
                                                    {
                                                        field_change.field_value = change_val;
                                                        context.SaveChanges();
                                                        change_type = "Enrollment";
                                                        has_enrollment_change = true;
                                                        found_any_field_change = true;
                                                    }
                                                }
                                            }
                                        }

                                        if (!found_field_change)
                                        {
                                            ClarityContextContainer container2 = new ClarityContextContainer();

                                            Report report_to_save = (from r in container2.Reports where r.id == report.id select r).First<Report>();
                                            ReportFieldChange field_change = new ReportFieldChange();
                                            field_change.field_name = field_change_option_field;
                                            field_change.record_type = field_change_option_field.Split(':')[0];
                                            field_change.unique_identifier = unique_identifier;
                                            field_change.field_value = change_val;
                                            field_change.report = report_to_save;
                                            container2.ReportFieldChanges.Add(field_change);
                                            container2.SaveChanges();

                                            change_type = field_change.record_type;
                                            has_demographic_change = (field_change.record_type == "Employee");
                                            has_dependent_change = (field_change.record_type == "Dependent");
                                            has_enrollment_change = (field_change.record_type == "Enrollment");
                                            found_any_field_change = true;
                                        }
                                    }

                                    if (!found_any_field_change)
                                    {
                                        include = false;
                                    }
                                }
                            }

                            if (include)
                            {
                                overall_include = true;
                                entity = currentSections.ElementAt<ReportEntity>(i);
                            }

                            i++;
                        }
                    }
                    else
                    {
                        overall_include = true;
                    }

                    // Make sure this isn't a termination we have already sent to 834:
                    if (report.output_format == "834" && overall_include == true)
                    {
                        string key = "";

                        if (current_loop_name == "Enrollment Loop")
                        {
                            string endDate = (from seg in employee.Elements("EndDate") select (string)seg).FirstOrDefault<string>();

                            if (endDate != null && endDate != "")
                            {
                                key = "Enrollment_" + (from seg in currentEmployee.Elements("ExternalEmployeeId") select (string)seg).FirstOrDefault<string>() + "_" + (from seg in currentEmployee.Elements("SSN") select (string)seg).FirstOrDefault<string>() + "_" + (from seg in employee.Elements("PlanIdentifier") select (string)seg).FirstOrDefault<string>();
                            }
                        }
                        else if (current_loop_name == "Dependent Enrollees Loop")
                        {
                            string endDate = (from seg in employee.Elements("EndDate") select (string)seg).FirstOrDefault<string>();

                            if (endDate != null && endDate != "")
                            {
                                key = "Enrollment_" + (from seg in currentEmployee.Elements("ExternalEmployeeId") select (string)seg).FirstOrDefault<string>() + "_" + (from seg in currentEmployee.Elements("SSN") select (string)seg).FirstOrDefault<string>() + "_" + (from seg in employee.Elements("SequenceNumber") select (string)seg).FirstOrDefault<string>();
                            }
                        }

                        if (key != "")
                        {
                            foreach (Termination termination in report.terminations)
                            {
                                if (termination.employee_key == key)
                                {
                                    overall_include = false;
                                }
                            }

                            if (overall_include == true)
                            {
                                ClarityContextContainer container2 = new ClarityContextContainer();
                                Report report_to_save = (from r in container2.Reports where r.id == report.id select r).First<Report>();
                                Termination newTermination = new Termination();
                                newTermination.employee_key = key;
                                newTermination.report = report_to_save;
                                container2.Terminations.Add(newTermination);
                                container2.SaveChanges();
                            }
                        }
                    }

                    if (overall_include)
                    {
                        if (current_loop_name == "Enrollment Loop")
                        {
                            // this.total_employees = ((int)this.total_employees) + 1;
                            bool found = false;

                            foreach (string employee_id in this.unique_employees)
                            {
                                if (employee_id.Equals(employee.ToString())) found = true;
                            }

                            if (!found) this.unique_employees.Add(employee.ToString());
                        }
                        else if (current_loop_name == "Dependent Enrollees Loop")
                        {
                            // this.total_dependents = ((int)this.total_dependents) + 1;
                            bool found = false;

                            foreach (string employee_id in this.unique_dependents)
                            {
                                if (employee_id.Equals(employee.ToString())) found = true;
                            }

                            if (!found) this.unique_dependents.Add(employee.ToString());
                        }

                        if (entity.section_delimiter == "," && csv != null)
                        {
                            foreach (ReportEntity field_entity in entity.child_entities)
                            {
                                if (field_entity.available_entity.entity_type == "Operator")
                                {
                                    List<ReportEntity> child_operators = new List<ReportEntity>();
                                    List<ReportEntity> child_conditions = new List<ReportEntity>();
                                    List<ReportEntity> child_sections = new List<ReportEntity>();

                                    child_operators.Add(field_entity);

                                    // We need to find additional operators after this operator that are either else if or else operators:
                                    bool found_current_entity = false;

                                    foreach (ReportEntity sibling_entity in entity.child_entities)
                                    {
                                        if (sibling_entity.id == field_entity.id)
                                        {
                                            found_current_entity = true;
                                        }
                                        else if (found_current_entity && sibling_entity.available_entity.entity_type == "Operator" && (sibling_entity.available_entity.name == "Else If" || sibling_entity.available_entity.name == "Else"))
                                        {
                                            child_operators.Add(sibling_entity);
                                        }
                                        else if (found_current_entity)
                                        {
                                            break;
                                        }
                                    }

                                    foreach (ReportEntity child_operator in child_operators)
                                    {
                                        ReportEntity second_child = child_operator.child_entities.First<ReportEntity>();

                                        if (second_child.available_entity.entity_type == "Condition")
                                        {
                                            child_conditions.Add(second_child);
                                            ReportEntity third_child = second_child.child_entities.First<ReportEntity>();

                                            if (third_child.available_entity.entity_type == "Section")
                                            {
                                                child_sections.Add(third_child);
                                            }
                                            else
                                            {
                                                child_sections.Add(null);
                                            }
                                        }
                                        else if (second_child.available_entity.entity_type == "Section")
                                        {
                                            child_conditions.Add(null);
                                            child_sections.Add(second_child);
                                        }
                                        else
                                        {
                                            child_conditions.Add(null);
                                            child_sections.Add(null);
                                        }
                                    }

                                    List<XElement> individualRecord = new List<XElement>();
                                    individualRecord.Add(employee);

                                    RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, child_sections.First<ReportEntity>(), employees, currentLoop, child_operators, child_conditions, child_sections, currentEmployee, report, context, individualRecord, employee_navigator, currentEnrollment);
                                }
                                else if (field_entity.available_entity.entity_type == "Literal")
                                {
                                    if (csv != null) csv.WriteField(field_entity.literal_value);
                                }
                                else if (field_entity.available_entity.entity_type == "Change Type")
                                {
                                    string val = "";
                                    if (field_entity.available_entity.name == "[Change Type]") val = change_type;
                                    else if (field_entity.available_entity.name == "[Change Type: Demographic]" && has_demographic_change) val = "X";
                                    else if (field_entity.available_entity.name == "[Change Type: Dependent]" && has_dependent_change) val = "X";
                                    else if (field_entity.available_entity.name == "[Change Type: Enrollment]" && has_enrollment_change) val = "X";

                                    if (csv != null) csv.WriteField(val);
                                }
                                else if (field_entity.available_entity.entity_type == "Field")
                                {
                                    string val = "";

                                    if (current_loop_name == "Employee Loop")
                                    {
                                        val = (from seg in employee.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                    }
                                    else if (current_loop_name == "Dependent Enrollees Loop")
                                    {
                                        if (field_entity.available_entity.name.Contains("Employee: "))
                                        {
                                            val = (from seg in currentEmployee.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                        }
                                        else if (field_entity.available_entity.name.Contains("Enrollment: "))
                                        {
                                            val = (from seg in currentEnrollment.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                        }
                                        else if (field_entity.available_entity.name.Contains("Dependent: "))
                                        {
                                            // We need to find the dependent with the same sequence number as this dependent enrollee:
                                            string sequence_number = (from seg in employee.Elements("SequenceNumber") select (string)seg).FirstOrDefault<string>();
                                            IEnumerable<XElement> dependents = from seg in currentEmployee.Descendants("Dependent") select seg;

                                            foreach (XElement dependent in dependents)
                                            {
                                                string dependent_sequence_number = (from seg in dependent.Elements("SequenceNumber") select (string)seg).FirstOrDefault<string>();

                                                if (sequence_number.Equals(dependent_sequence_number))
                                                {
                                                    val = (from seg in dependent.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            val = (from seg in employee.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                        }
                                    }
                                    else
                                    {
                                        if (field_entity.available_entity.name.Contains("Employee: "))
                                        {
                                            val = (from seg in currentEmployee.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                        }
                                        else
                                        {
                                            val = (from seg in employee.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                        }
                                    }

                                    if (val == null) val = "";
                                    val = this.ProcessFieldOptions(field_entity, val);
                                    if (csv != null) csv.WriteField(val);
                                }
                                else if (field_entity.available_entity.entity_type == "Calculated Field")
                                {
                                    string val = this.ComputeCalculatedField(field_entity, employee);

                                    // val = this.ProcessFieldOptions(field_entity, val);
                                    if (csv != null) csv.WriteField(val);
                                }
                                else if (field_entity.available_entity.entity_type == "Mapped Field")
                                {
                                    string val = this.ComputeMappedField(field_entity, employee, currentEmployee, currentEnrollment, current_loop_name);
                                    // val = this.ProcessFieldOptions(field_entity, val);
                                    if (csv != null) csv.WriteField(val);
                                }
                                else if (field_entity.available_entity.entity_type == "Join Field")
                                {
                                    string val = this.ComputeJoinField(field_entity, employee, employee_navigator);
                                    // val = this.ProcessFieldOptions(field_entity, val);
                                    if (csv != null) csv.WriteField(val);
                                }
                                else if (field_entity.available_entity.entity_type == "New Line")
                                {
                                    // Ignore New Lines in CSV!
                                }
                                else if (field_entity.available_entity.entity_type == "Section")
                                {
                                    List<XElement> singleEmployee = new List<XElement>();
                                    singleEmployee.Add(employee);
                                    if (current_loop_name == "Employee Loop")
                                    {
                                        RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, currentLoop, null, null, null, employee, report, context, singleEmployee, employee_navigator, currentEnrollment);
                                    }
                                    else
                                    {
                                        RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, currentLoop, null, null, null, currentEmployee, report, context, singleEmployee, employee_navigator, currentEnrollment);
                                    }
                                }
                                else if (field_entity.available_entity.entity_type == "Loop")
                                {
                                    XElement currentEnrollmentToPass = null;
                                    if (current_loop_name == "Enrollment Loop") currentEnrollmentToPass = employee;
                                    else if (currentEnrollment != null) currentEnrollmentToPass = currentEnrollment;

                                    if (current_loop_name == field_entity.available_entity.name)
                                    {
                                        RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, entity, null, null, null, currentEmployee, report, context, null, employee_navigator, currentEnrollmentToPass);
                                    }
                                    else
                                    {
                                        if (field_entity.available_entity.name == "Cafeteria Data Loop")
                                        {
                                            RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, entity, null, null, null, employee, report, context, null, employee_navigator, currentEnrollmentToPass);
                                        }
                                        else if (currentEmployee == null)
                                        {
                                            RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, entity, null, null, null, employee, report, context, null, employee_navigator, currentEnrollmentToPass);
                                        }
                                        else
                                        {
                                            RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, entity, null, null, null, currentEmployee, report, context, null, employee_navigator, currentEnrollmentToPass);
                                        }
                                    }
                                }
                                else if (field_entity.available_entity.entity_type == "Terminate Loop")
                                {
                                    terminate_loop = true;
                                }
                            }
                        }
                        else
                        {
                            List<string> field_results = new List<string>();

                            foreach (ReportEntity field_entity in entity.child_entities)
                            {
                                if (field_entity.available_entity.entity_type == "Operator")
                                {
                                    if (field_entity.available_entity.name == "If")
                                    {
                                        List<ReportEntity> child_operators = new List<ReportEntity>();
                                        List<ReportEntity> child_conditions = new List<ReportEntity>();
                                        List<ReportEntity> child_sections = new List<ReportEntity>();

                                        child_operators.Add(field_entity);

                                        // We need to find additional operators after this operator that are either else if or else operators:
                                        bool found_current_entity = false;

                                        foreach (ReportEntity sibling_entity in entity.child_entities)
                                        {
                                            if (sibling_entity.id == field_entity.id)
                                            {
                                                found_current_entity = true;
                                            }
                                            else if (found_current_entity && sibling_entity.available_entity.entity_type == "Operator" && (sibling_entity.available_entity.name == "Else If" || sibling_entity.available_entity.name == "Else"))
                                            {
                                                child_operators.Add(sibling_entity);
                                            }
                                            else if (found_current_entity)
                                            {
                                                break;
                                            }
                                        }

                                        foreach (ReportEntity child_operator in child_operators)
                                        {
                                            ReportEntity second_child = child_operator.child_entities.First<ReportEntity>();

                                            if (second_child.available_entity.entity_type == "Condition")
                                            {
                                                child_conditions.Add(second_child);
                                                ReportEntity third_child = second_child.child_entities.First<ReportEntity>();

                                                if (third_child.available_entity.entity_type == "Section")
                                                {
                                                    child_sections.Add(third_child);
                                                }
                                                else
                                                {
                                                    child_sections.Add(null);
                                                }
                                            }
                                            else if (second_child.available_entity.entity_type == "Section")
                                            {
                                                child_conditions.Add(null);
                                                child_sections.Add(second_child);
                                            }
                                            else
                                            {
                                                child_conditions.Add(null);
                                                child_sections.Add(null);
                                            }
                                        }

                                        List<XElement> individualRecord = new List<XElement>();
                                        individualRecord.Add(employee);

                                        if (tw != null) tw.Write(String.Join(entity.section_delimiter, field_results));
                                        field_results = new List<string>();
                                        RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, child_sections.First<ReportEntity>(), employees, currentLoop, child_operators, child_conditions, child_sections, currentEmployee, report, context, individualRecord, employee_navigator, currentEnrollment);
                                    }
                                }
                                else if (field_entity.available_entity.entity_type == "Literal")
                                {
                                    field_results.Add(field_entity.literal_value);
                                }
                                else if (field_entity.available_entity.entity_type == "Change Type")
                                {
                                    string val = "";
                                    if (field_entity.available_entity.name == "[Change Type]") val = change_type;
                                    else if (field_entity.available_entity.name == "[Change Type: Demographic]" && has_demographic_change) val = "X";
                                    else if (field_entity.available_entity.name == "[Change Type: Dependent]" && has_dependent_change) val = "X";
                                    else if (field_entity.available_entity.name == "[Change Type: Enrollment]" && has_enrollment_change) val = "X";

                                    field_results.Add(val);
                                }
                                else if (field_entity.available_entity.entity_type == "Field")
                                {
                                    string val = "";

                                    if (current_loop_name == "Employee Loop")
                                    {
                                        val = (from seg in employee.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                    }
                                    else if (current_loop_name == "Dependent Enrollees Loop")
                                    {
                                        if (field_entity.available_entity.name.Contains("Employee: "))
                                        {
                                            val = (from seg in currentEmployee.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                        }
                                        else if (field_entity.available_entity.name.Contains("Enrollment: "))
                                        {
                                            val = (from seg in currentEnrollment.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                        }
                                        else if (field_entity.available_entity.name.Contains("Dependent: "))
                                        {
                                            // We need to find the dependent with the same sequence number as this dependent enrollee:
                                            string sequence_number = (from seg in employee.Elements("SequenceNumber") select (string)seg).FirstOrDefault<string>();
                                            IEnumerable<XElement> dependents = from seg in currentEmployee.Descendants("Dependent") select seg;
                                            
                                            foreach (XElement dependent in dependents)
                                            {
                                                string dependent_sequence_number = (from seg in dependent.Elements("SequenceNumber") select (string)seg).FirstOrDefault<string>();

                                                if (sequence_number.Equals(dependent_sequence_number))
                                                {
                                                    val = (from seg in dependent.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            val = (from seg in employee.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                        }
                                    }
                                    else
                                    {
                                        if (field_entity.available_entity.name.Contains("Employee: "))
                                        {
                                            val = (from seg in currentEmployee.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                        }
                                        else
                                        {
                                            val = (from seg in employee.Elements(this.ScrubFieldName(field_entity.available_entity.name)) select (string)seg).FirstOrDefault<string>();
                                        }
                                    }

                                    if (val == null) val = "";
                                    val = this.ProcessFieldOptions(field_entity, val);
                                    field_results.Add(val);
                                }
                                else if (field_entity.available_entity.entity_type == "Calculated Field")
                                {
                                    string val = this.ComputeCalculatedField(field_entity, employee);
                                    // val = this.ProcessFieldOptions(field_entity, val);
                                    field_results.Add(val);
                                }
                                else if (field_entity.available_entity.entity_type == "Mapped Field")
                                {
                                    string val = this.ComputeMappedField(field_entity, employee, currentEmployee, currentEnrollment, current_loop_name);
                                    // val = this.ProcessFieldOptions(field_entity, val);
                                    field_results.Add(val);
                                }
                                else if (field_entity.available_entity.entity_type == "Join Field")
                                {
                                    string val = this.ComputeJoinField(field_entity, employee, employee_navigator);
                                    // val = this.ProcessFieldOptions(field_entity, val);
                                    field_results.Add(val);
                                }
                                else if (field_entity.available_entity.entity_type == "New Line")
                                {
                                    // string val = "\r\n";
                                    // field_results.Add(val);

                                    if (tw != null) tw.Write(String.Join(entity.section_delimiter, field_results));
                                    if (tw != null) tw.Write("\r\n");
                                    field_results = new List<string>();
                                }
                                else if (field_entity.available_entity.entity_type == "Section")
                                {
                                    // Spit everything out before we render the loop. This will eventually need to be fixed for PDF and XLSX too:
                                    if (tw != null) tw.Write(String.Join(entity.section_delimiter, field_results));
                                    if (field_results.Count > 0) tw.Write(entity.section_delimiter);
                                    field_results = new List<string>();

                                    List<XElement> singleEmployee = new List<XElement>();
                                    singleEmployee.Add(employee);
                                    if (current_loop_name == "Employee Loop")
                                    {
                                        RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, currentLoop, null, null, null, employee, report, context, singleEmployee, employee_navigator, currentEnrollment);
                                    }
                                    else
                                    {
                                        RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, currentLoop, null, null, null, currentEmployee, report, context, singleEmployee, employee_navigator, currentEnrollment);
                                    }
                                }
                                else if (field_entity.available_entity.entity_type == "Loop")
                                {
                                    // Spit everything out before we render the loop. This will eventually need to be fixed for PDF and XLSX too:
                                    if (tw != null) tw.Write(String.Join(entity.section_delimiter, field_results));
                                    if (field_results.Count > 0) tw.Write(entity.section_delimiter);
                                    field_results = new List<string>();

                                    XElement currentEnrollmentToPass = null;
                                    if (current_loop_name == "Enrollment Loop") currentEnrollmentToPass = employee;
                                    else if (currentEnrollment != null) currentEnrollmentToPass = currentEnrollment;

                                    if (current_loop_name == field_entity.available_entity.name)
                                    {
                                        RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, entity, null, null, null, currentEmployee, report, context, null, employee_navigator, currentEnrollmentToPass);
                                    }
                                    else
                                    {
                                        if (field_entity.available_entity.name == "Cafeteria Data Loop")
                                        {
                                            RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, entity, null, null, null, employee, report, context, null, employee_navigator, currentEnrollmentToPass);
                                        }
                                        else if (currentEmployee == null)
                                        {
                                            RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, entity, null, null, null, employee, report, context, null, employee_navigator, currentEnrollmentToPass);
                                        }
                                        else
                                        {
                                            RenderEntity(csv, tw, ws, ref row, ref col, ref html_content, null, null, field_entity, employees, entity, null, null, null, currentEmployee, report, context, null, employee_navigator, currentEnrollmentToPass);
                                        }
                                    }
                                }
                                else if (field_entity.available_entity.entity_type == "Terminate Loop")
                                {
                                    terminate_loop = true;
                                }
                            }

                            // CSV:
                            if (csv != null) csv.WriteField(String.Join(entity.section_delimiter, field_results));

                            // TXT:
                            if (tw != null) tw.Write(String.Join(entity.section_delimiter, field_results));

                            // XSLX:
                            if (ws != null)
                            {
                                foreach (string res in field_results)
                                {
                                    ws.Cells[this.colForInt(col) + row].Value = res;
                                    col++;
                                }

                                row++;
                                col = 1;
                            }

                            // PDF:
                            html_content += "<tr><td>" + String.Join("</td><td>", field_results) + "</td></tr>";
                        }

                        if (csv != null) csv.NextRecord();
                    }

                    if (terminate_loop)
                    {
                        break;
                    }
                }
            }

            if (footer != null && footer == "834")
            {
                footer = Render834Footer(report);

                if (csv != null) csv.WriteField(footer);
                if (csv != null) csv.NextRecord();
                if (tw != null) tw.WriteLine(footer);

                if (ws != null)
                {
                    col = 1;
                    ws.Cells[this.colForInt(col) + row].Value = footer;
                    row++;
                }

                html_content += "<tr><td>" + footer + "</td></tr>";
            }
            else if (footer != null && footer != "")
            {
                footer = footer.Replace("XX", "{TOTAL LINE COUNT}");
                footer = footer.Replace("NN", "{TOTAL LINE COUNT}");

                if (csv != null) csv.WriteField(footer);
                if (csv != null) csv.NextRecord();
                if (tw != null) tw.WriteLine(footer);

                if (ws != null)
                {
                    col = 1;
                    ws.Cells[this.colForInt(col) + row].Value = footer;
                    row++;
                }

                html_content += "<tr><td>" + footer + "</td></tr>";
            }
        }

        private string RandomDigits(int length)
        {
            var random = new Random();
            string s = string.Empty;
            for (int i = 0; i < length; i++)
                s = String.Concat(s, random.Next(10).ToString());
            return s;
        }

        private string Render834Header(ClarityContextContainer context, Report report)
        {
            if (report.report_834_options.isa02 == "") report.report_834_options.isa02 = "..........";
            if (report.report_834_options.isa03 == "") report.report_834_options.isa03 = "00";
            if (report.report_834_options.isa04 == "") report.report_834_options.isa04 = "..........";
            if (report.report_834_options.isa05 == "") report.report_834_options.isa05 = "30";
            if (report.report_834_options.isa07 == "") report.report_834_options.isa07 = "30";
            if (report.report_834_options.isa08 == "") report.report_834_options.isa08 = "135123390";
            if (report.report_834_options.isa14 == "") report.report_834_options.isa14 = "1";
            if (report.report_834_options.isa15 == "") report.report_834_options.isa15 = "T";

            report.report_834_options.isa02 = report.report_834_options.isa02.PadLeft(10, '.');
            report.report_834_options.isa03 = report.report_834_options.isa03.PadLeft(2, '.');
            report.report_834_options.isa04 = report.report_834_options.isa04.PadLeft(10, '.');
            report.report_834_options.isa05 = report.report_834_options.isa05.PadLeft(2, '.');
            report.report_834_options.isa06 = report.report_834_options.isa06.PadLeft(9, '.');
            report.report_834_options.isa07 = report.report_834_options.isa07.PadLeft(2, '.');
            report.report_834_options.isa08 = report.report_834_options.isa08.PadLeft(9, '.');
            report.report_834_options.isa14 = report.report_834_options.isa14.PadLeft(1, '.');

            // DateTime timestamp = DateTime.Now;
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime timestamp = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

            string isa = "ISA*" + report.report_834_options.isa01 + "*" + report.report_834_options.isa02 + "*" +
                report.report_834_options.isa03 + "*" +
                report.report_834_options.isa04 + "*" +
                report.report_834_options.isa05 + "*" +
                report.report_834_options.isa06 + "*" +
                report.report_834_options.isa07 + "*" +
                report.report_834_options.isa08 + "*" +
                timestamp.ToString("yyMMdd") + "*" +
                timestamp.ToString("HHmm") + "*" +
                "^" + "*" +
                "00501" + "*" +
                report.report_834_options.isa13 + "*" +
                report.report_834_options.isa14 + "*" +
                report.report_834_options.isa15 + "*" +
                ":" + "~";

            if (report.report_834_options.gs03 == "") report.report_834_options.gs03 = "135123390";
            if (report.report_834_options.gs08 == "") report.report_834_options.gs08 = "005010X220A1";

            // report.report_834_options.gs02 = report.report_834_options.gs02.PadLeft(15, '.');
            // report.report_834_options.gs03 = report.report_834_options.gs03.PadLeft(15, '.');
            // report.report_834_options.gs08 = report.report_834_options.gs03.PadLeft(12, '.');

            string gs = "GS*BE*" + report.report_834_options.gs02 + "*" + report.report_834_options.gs03 + "*" +
                timestamp.ToString("yyyyMMdd") + "*" +
                timestamp.ToString("HHmm") + "*" +
                report.report_834_options.gs06 + "*X*" +
                report.report_834_options.gs08 + "~";

            string st = "ST*834*" + report.report_834_options.st02 + "*" + report.report_834_options.gs08 + "~";

            if (report.report_834_options.bgn08 == "") report.report_834_options.bgn08 = "2";

            // report.report_834_options.bgn02 = report.report_834_options.bgn02.PadLeft(50, '.');
            // report.report_834_options.bgn08 = report.report_834_options.bgn08.PadLeft(2, '.');

            string bgn = "BGN*00*" + report.report_834_options.bgn02 + "*" +
                timestamp.ToString("yyyyMMdd") + "*" +
                timestamp.ToString("HHmm") + "**" + report.report_834_options.bgn06 + "**" +
                report.report_834_options.bgn08 + "~";

            int n;
            bool isNumeric = int.TryParse(report.report_834_options.bgn02, out n);

            if (isNumeric)
            {
                n++;
                report.report_834_options.bgn02 = n.ToString().PadLeft(report.report_834_options.bgn02.Length, '0');
            }

            string refl = "REF*38*" + report.report_834_options.ref02 + "~";

            string dtp = "DTP*" + report.report_834_options.dtp01 + "*D8*" + timestamp.ToString("yyyyMMdd") + "~";
            string dtpb = "DTP*" + report.report_834_options.dtp01b + "*D8*" + timestamp.ToString("yyyyMMdd") + "~";

            string output_834 = isa + "\r\n" + gs + "\r\n" + st + "\r\n" + bgn + "\r\n" + refl;

            if (report.report_834_options.include_dtp_a == 1) output_834 += "\r\n" + dtp;
            if (report.report_834_options.include_dtp_b == 1) output_834 += "\r\n" + dtpb;

            string qtya = "QTY*ET*{TOTAL EMPLOYEE COUNT}~";
            string qtyb = "QTY*DT*{TOTAL DEPENDENT COUNT}~";
            string qtyc = "QTY*TO*{TOTAL EMPLOYEE PLUS DEPENDENT COUNT}~";

            if (report.report_834_options.include_qty_a == 1) output_834 += "\r\n" + qtya;
            if (report.report_834_options.include_qty_b == 1) output_834 += "\r\n" + qtyb;
            if (report.report_834_options.include_qty_c == 1) output_834 += "\r\n" + qtyc;

            string n1a = "N1*P5*" + report.report_834_options.n102a + "*" + report.report_834_options.n103a + "*" + report.report_834_options.n104a + "~";
            string n1b = "N1*IN*" + report.report_834_options.n102b + "*" + report.report_834_options.n103b + "*" + report.report_834_options.n104b + "~";
            string n1c = "N1*TV*" + report.report_834_options.n102c + "*" + report.report_834_options.n103c + "*" + report.report_834_options.n104c + "~";

            if (report.report_834_options.include_n1_a == 1) output_834 += "\r\n" + n1a;
            if (report.report_834_options.include_n1_b == 1) output_834 += "\r\n" + n1b;
            if (report.report_834_options.include_n1_c == 1) output_834 += "\r\n" + n1c;

            context.SaveChanges();

            Report report_to_update = (from r in context.Reports where r.id == report.id select r).First<Report>();
            report_to_update.report_834_options.bgn02 = report.report_834_options.bgn02;

            context.SaveChanges();

            return output_834;
        }

        private string Render834Footer(Report report)
        {
            string se = "SE*{TOTAL LINE COUNT 834}*" + report.report_834_options.st02 + "~";
            string ge = "GE*1*" + report.report_834_options.gs06 + "~";
            string iea = "IEA*1*" + report.report_834_options.isa13 + "~";

            return se + "\r\n" + ge + "\r\n" + iea;
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
    }
}