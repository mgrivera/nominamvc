﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class dbContabUserEntities : DbContext
    {
        public dbContabUserEntities()
            : base("name=dbContabUserEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<aspnet_Users> aspnet_Users { get; set; }
        public virtual DbSet<Compania> Companias { get; set; }
        public virtual DbSet<CompaniasYUsuario> CompaniasYUsuarios { get; set; }
        public virtual DbSet<tCiaSeleccionada2> tCiaSeleccionada2 { get; set; }
    }
}
