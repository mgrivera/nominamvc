//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NominaASP.Models.Users
{
    using System;
    using System.Collections.Generic;
    
    public partial class aspnet_Users
    {
        public aspnet_Users()
        {
            this.CompaniasYUsuarios = new HashSet<CompaniasYUsuario>();
        }
    
        public System.Guid ApplicationId { get; set; }
        public System.Guid UserId { get; set; }
        public string UserName { get; set; }
        public string LoweredUserName { get; set; }
        public string MobileAlias { get; set; }
        public bool IsAnonymous { get; set; }
        public System.DateTime LastActivityDate { get; set; }
    
        public virtual ICollection<CompaniasYUsuario> CompaniasYUsuarios { get; set; }
    }
}
