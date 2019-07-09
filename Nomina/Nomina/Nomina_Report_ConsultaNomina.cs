using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NominaASP.Nomina.Nomina
{
    public class Nomina_Report_ConsultaNomina
    {
        public int HeaderID { get; set; }
        public string CiaContab { get; set; }
        public string GrupoEmpleados { get; set; }
        public DateTime FechaNomina { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string TipoNomina { get; set; }

        public string Departamento { get; set; }
        public string Empleado { get; set; }
        public string Rubro { get; set; }
        public string Descripcion { get; set; }

        public Nullable<decimal> Asignacion { get; set; }
        public Nullable<decimal> Deduccion { get; set; }
        public decimal Monto { get; set; }

        public Nullable<decimal> Base { get; set; }
        public Nullable<short> CantidadDias { get; set; }
        public Nullable<Decimal> Fraccion { get; set; }
        public string Detalles { get; set; }
        public Nullable<bool> SueldoFlag { get; set; }
        public Nullable<bool> SalarioFlag { get; set; }

        public List<Nomina_Report_ConsultaNomina> GetNomina_Report_ConsultaNomina()
        {
            List<Nomina_Report_ConsultaNomina> list = new List<Nomina_Report_ConsultaNomina>();
            return list;
        }
    }
}