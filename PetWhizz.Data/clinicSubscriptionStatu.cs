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
    
    public partial class clinicSubscriptionStatu
    {
        public long id { get; set; }
        public Nullable<int> clinicId { get; set; }
        public Nullable<long> subscriptionId { get; set; }
        public Nullable<long> totalTransactionsAllowed { get; set; }
        public Nullable<long> currentTransactionsCount { get; set; }
        public Nullable<System.DateTime> createdDate { get; set; }
    
        public virtual clinic clinic { get; set; }
        public virtual clinicSubscription clinicSubscription { get; set; }
    }
}
