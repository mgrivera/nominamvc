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
    
    public partial class UltimoMesCerradoContab
    {
        public byte Mes { get; set; }
        public short Ano { get; set; }
        public System.DateTime UltAct { get; set; }
        public string ManAuto { get; set; }
        public int Cia { get; set; }
        public string Usuario { get; set; }
    
        public virtual Compania Compania { get; set; }
    }
}
