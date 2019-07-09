using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NominaASP.Empleados.Consulta
{
    public class Nomina_Report_ConsultaEmpleados
    {
        public int Empleado { get; set; }
        public string CiaContab { get; set; }
        public string Departamento { get; set; }
        public string Nombre { get; set; }
        public string Cedula { get; set; }
        public string Cargo { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string SituacionActual { get; set; }
        public Nullable<decimal> SueldoBasico { get; set; }
        public Nullable<decimal> MontoCestaTickets { get; set; } 
        public Nullable<DateTime> FechaRetiro { get; set; }

        public List<Nomina_Report_ConsultaEmpleados> GetNomina_Report_ConsultaEmpleados()
        {
            List<Nomina_Report_ConsultaEmpleados> list = new List<Nomina_Report_ConsultaEmpleados>();
            return list;
        }
    }

    public class ConsultaEmpleados_CriteriosFiltro
    {
        public int? CiaContab { get; set; }
        public int? Empleado { get; set; }
        public int? Departamento { get; set; }
        public int? Cargo { get; set; }
        public string Status { get; set; }
        public string SituacionActual { get; set; }
    }
}