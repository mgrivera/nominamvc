//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NominaASP.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Asiento
    {
        public int NumeroAutomatico { get; set; }
        public short Numero { get; set; }
        public byte Mes { get; set; }
        public short Ano { get; set; }
        public string Tipo { get; set; }
        public System.DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public int Moneda { get; set; }
        public int MonedaOriginal { get; set; }
        public Nullable<bool> ConvertirFlag { get; set; }
        public decimal FactorDeCambio { get; set; }
        public string ProvieneDe { get; set; }
        public Nullable<int> ProvieneDe_ID { get; set; }
        public System.DateTime Ingreso { get; set; }
        public System.DateTime UltAct { get; set; }
        public Nullable<bool> CopiableFlag { get; set; }
        public Nullable<bool> AsientoTipoCierreAnualFlag { get; set; }
        public short MesFiscal { get; set; }
        public short AnoFiscal { get; set; }
        public string Usuario { get; set; }
        public string Lote { get; set; }
        public int Cia { get; set; }
    
        public virtual Compania Compania { get; set; }
    }
}
