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
    
    public partial class vetClinicScheduleSlot
    {
        public vetClinicScheduleSlot()
        {
            this.clinicServiceRequests = new HashSet<clinicServiceRequest>();
        }
    
        public long id { get; set; }
        public Nullable<long> vetClinicScheduleId { get; set; }
        public Nullable<int> appointmentNumber { get; set; }
        public Nullable<System.TimeSpan> slotStartsAt { get; set; }
        public Nullable<System.TimeSpan> slotEndsAt { get; set; }
        public Nullable<bool> isReserved { get; set; }
        public Nullable<bool> isActive { get; set; }
    
        public virtual ICollection<clinicServiceRequest> clinicServiceRequests { get; set; }
        public virtual vetClinicSchedule vetClinicSchedule { get; set; }
    }
}