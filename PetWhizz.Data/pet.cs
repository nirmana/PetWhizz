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
    
    public partial class pet
    {
        public pet()
        {
            this.petOwners = new HashSet<petOwner>();
            this.petVetClinics = new HashSet<petVetClinic>();
        }
    
        public long id { get; set; }
        public Nullable<int> breedId { get; set; }
        public string petName { get; set; }
        public string sex { get; set; }
        public Nullable<System.DateTime> birthDay { get; set; }
        public string coverImage { get; set; }
        public string profileImage { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<bool> isDeleted { get; set; }
        public Nullable<System.DateTime> entryDate { get; set; }
        public Nullable<System.DateTime> lastUpdatedTime { get; set; }
        public string lastUpdatedBy { get; set; }
    
        public virtual animalBreed animalBreed { get; set; }
        public virtual ICollection<petOwner> petOwners { get; set; }
        public virtual ICollection<petVetClinic> petVetClinics { get; set; }
    }
}