//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Clarity_HSA.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class extra_report_fields
    {
        public int id { get; set; }
        public System.DateTime dob { get; set; }
        public string social_security_num { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string company_identifier { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public double balance { get; set; }
        public double monthly_payment { get; set; }
        public string social_security_last_four { get; set; }
        public bool terminated { get; set; }
        public string status { get; set; }
        public double maximum_advance_amount { get; set; }
        public double hsa_balance { get; set; }
        public string payroll_identifier { get; set; }
        public Nullable<System.DateTime> termination_date { get; set; }
        public Nullable<double> goal_amount { get; set; }
        public string company_payroll_id { get; set; }
    }
}