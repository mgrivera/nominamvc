using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NominaASP.Models.Nomina
{
    [MetadataType(typeof(tNominaHeader.Metadata))]
    public partial class tNominaHeader
    {
        private abstract class Metadata
        {
            [Required(ErrorMessage = "Ud. debe indicar una fecha de nómina")]
            public object FechaNomina { get; set; }

            [Required(ErrorMessage = "Ud. debe indicar un grupo de nómina (o grupo de empleados)")]
            public object GrupoNomina { get; set; }

            [Required(ErrorMessage = "Ud. debe indicar una fecha inicial para el período de la nómina")]
            public object Desde { get; set; }

            [Required(ErrorMessage = "Ud. debe indicar una fecha final para el período de la nómina")]
            public object Hasta { get; set; }

            //[Required(ErrorMessage = "Ud. debe indicar la cantidad de días que corresponde al período de la nómina")]
            public object CantidadDias { get; set; }

            [Required(ErrorMessage = "Ud. debe indicar un tipo de nómina")]
            public object Tipo { get; set; }
        }
    }


    [MetadataType(typeof(tEmpleado.Metadata))]
    public partial class tEmpleado
    {
        private abstract class Metadata
        {
            public object Empleado { get; set; }
            public string Status { get; set; }
            public string SituacionActual { get; set; }
        }
    }

    [MetadataType(typeof(PrestacionesSocialesHeader.Metadata))]
    public partial class PrestacionesSocialesHeader
    {
        private abstract class Metadata
        {
            [Required(ErrorMessage = "Ud. debe indicar una fecha inicial del período de prestaciones")]
            public object Desde { get; set; }

            [Required(ErrorMessage = "Ud. debe indicar una fecha final del período de prestaciones")]
            public object Hasta { get; set; }

            [Required(ErrorMessage = "Ud. debe indicar la cantidad de días de utilidades que la empresa otorga a sus empleados")]
            public object CantDiasUtilidades { get; set; }

            [Required(ErrorMessage = "La definición de prestaciones sociales debe corresponder a una Cia Contab")]
            public object Cia { get; set; }
        }
    }
}