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
    
    public partial class animalBreed
    {
        public animalBreed()
        {
            this.pets = new HashSet<pet>();
        }
    
        public int id { get; set; }
        public Nullable<int> animalId { get; set; }
        public string breedName { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<System.DateTime> entryDate { get; set; }
    
        public virtual animal animal { get; set; }
        public virtual ICollection<pet> pets { get; set; }
    }
}
