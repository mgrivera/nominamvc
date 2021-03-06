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
    
    public partial class tNominaHeader
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tNominaHeader()
        {
            this.tNominas = new HashSet<tNomina>();
            this.tNomina_SalarioIntegral = new HashSet<tNomina_SalarioIntegral>();
        }
    
        public int ID { get; set; }
        public System.DateTime FechaNomina { get; set; }
        public System.DateTime FechaEjecucion { get; set; }
        public int GrupoNomina { get; set; }
        public Nullable<System.DateTime> Desde { get; set; }
        public Nullable<System.DateTime> Hasta { get; set; }
        public Nullable<System.DateTime> FechaPago { get; set; }
        public string DescripcionRubroSueldo { get; set; }
        public Nullable<short> CantidadDias { get; set; }
        public string Tipo { get; set; }
        public bool AgregarSueldo { get; set; }
        public bool AgregarDeduccionesObligatorias { get; set; }
        public string ProvieneDe { get; set; }
        public Nullable<int> ProvieneDe_ID { get; set; }
        public Nullable<int> AsientoContableID { get; set; }
    
        public virtual tGruposEmpleado tGruposEmpleado { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tNomina> tNominas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tNomina_SalarioIntegral> tNomina_SalarioIntegral { get; set; }
    }
}
