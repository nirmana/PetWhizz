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
    
    public partial class userDevice
    {
        public userDevice()
        {
            this.userTokens = new HashSet<userToken>();
        }
    
        public long id { get; set; }
        public int userId { get; set; }
        public string deviceId { get; set; }
        public string deviceName { get; set; }
        public Nullable<System.DateTime> lastLogin { get; set; }
    
        public virtual user user { get; set; }
        public virtual ICollection<userToken> userTokens { get; set; }
    }
}
