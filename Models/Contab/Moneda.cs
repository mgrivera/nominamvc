//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NominaASP.Models.Contab
{
    using System;
    using System.Collections.Generic;
    
    public partial class Moneda
    {
        public Moneda()
        {
            this.Asientos = new HashSet<Asiento>();
            this.Asientos1 = new HashSet<Asiento>();
            this.Companias = new HashSet<Compania>();
        }
    
        public int Moneda1 { get; set; }
        public string Descripcion { get; set; }
        public string Simbolo { get; set; }
        public bool NacionalFlag { get; set; }
        public bool DefaultFlag { get; set; }
    
        public virtual ICollection<Asiento> Asientos { get; set; }
        public virtual ICollection<Asiento> Asientos1 { get; set; }
        public virtual ICollection<Compania> Companias { get; set; }
    }
}
