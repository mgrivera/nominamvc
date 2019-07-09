using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NominaASP.ViewModels.WebApi
{
    public class VacacionConsulta
    {
        public int Empleado { get; set; }
        public string Nombre { get; set; }
        public DateTime Salida { get; set; }
        public DateTime Regreso { get; set; }
        public int DiasHabiles { get; set; }
        public int DiasFeriados { get; set; }
        public int DiasDisfrutados { get; set; }
        public string ListaDiasFeriados { get; set; }
    }
}