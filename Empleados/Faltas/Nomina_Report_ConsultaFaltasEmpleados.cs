using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NominaASP.Empleados.Faltas
{
    public class ConsultaEmpleadosFaltas_CriteriosFiltro
    {
        public int? CiaContab { get; set; }
        public int? Empleado { get; set; }
        public int? Departamento { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public bool? Descontar { get; set; }
        public string Status { get; set; }
        public string SituacionActual { get; set; }
    }

    public class EmpleadoFaltaConsulta
    {
        public string CiaContab { get; set; }
        public string Departamento { get; set; }
        public string Empleado { get; set; }
        public bool Descontar { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public short TotalDias { get; set; }
        public short SabYDom { get; set; }
        public short Feriados { get; set; }
        public short Faltas { get; set; }
        public short? CantHoras { get; set; }
        public DateTime? FechaNomina { get; set; }
        public string Observaciones { get; set; }

        public List<EmpleadoFaltaConsulta> GetEmpleadoFaltaConsulta()
        {
            List<EmpleadoFaltaConsulta> list = new List<EmpleadoFaltaConsulta>();
            return list;
        }
    }
}