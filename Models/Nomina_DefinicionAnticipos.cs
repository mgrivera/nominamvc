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
    
    public partial class Nomina_DefinicionAnticipos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Nomina_DefinicionAnticipos()
        {
            this.Nomina_DefinicionAnticipos_Empleados = new HashSet<Nomina_DefinicionAnticipos_Empleados>();
        }
    
        public int ID { get; set; }
        public int GrupoNomina { get; set; }
        public System.DateTime Desde { get; set; }
        public bool Suspendido { get; set; }
        public Nullable<decimal> PrimQuincPorc { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Nomina_DefinicionAnticipos_Empleados> Nomina_DefinicionAnticipos_Empleados { get; set; }
        public virtual tGruposEmpleado tGruposEmpleado { get; set; }
    }
}
