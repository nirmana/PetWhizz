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
    
    public partial class vetType
    {
        public vetType()
        {
            this.vets = new HashSet<vet>();
        }
    
        public int id { get; set; }
        public string vetTypeName { get; set; }
        public Nullable<bool> isActive { get; set; }
    
        public virtual ICollection<vet> vets { get; set; }
    }
}
