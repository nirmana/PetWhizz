//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PetWhizz.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class paymentPlanType
    {
        public paymentPlanType()
        {
            this.paymentSchemes = new HashSet<paymentScheme>();
        }
    
        public int id { get; set; }
        public string planType { get; set; }
        public Nullable<bool> isActive { get; set; }
    
        public virtual ICollection<paymentScheme> paymentSchemes { get; set; }
    }
}