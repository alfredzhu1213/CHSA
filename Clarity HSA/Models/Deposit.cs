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
    
    public partial class Deposit
    {
        public int id { get; set; }
        public System.DateTime payroll_date { get; set; }
        public double employee_contribution { get; set; }
        public string employer_contribution { get; set; }
        public double loan_remaining_balance { get; set; }
        public string plan_type { get; set; }
        public string plan_begin { get; set; }
        public string plan_end { get; set; }
        public string deposit_type { get; set; }
        public string ii_posted { get; set; }
    
        public virtual Demographic employee { get; set; }
    }
}
