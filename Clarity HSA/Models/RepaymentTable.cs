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
    
    public partial class RepaymentTable
    {
        public int id { get; set; }
        public double loan_amount_min { get; set; }
        public double loan_amount_max { get; set; }
        public double repaymant_amount { get; set; }
        public string division { get; set; }
        public string frequency { get; set; }
        public string defaultVal { get; set; }
    
        public virtual OrganizationSettings organization_settings { get; set; }
    }
}
