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
    
    public partial class webUserType
    {
        public webUserType()
        {
            this.webUsers = new HashSet<webUser>();
        }
    
        public int id { get; set; }
        public string userTypeCode { get; set; }
        public string userType { get; set; }
    
        public virtual ICollection<webUser> webUsers { get; set; }
    }
}
