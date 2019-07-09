using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NominaASP.Models.MongoDB
{
    public class vacacion
    {
        //[BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }              // este id es un simple string en mongo ... 
        public int claveUnicaContab { get; set; }

        public int empleado { get; set; }
        public DateTime? fechaIngreso { get; set; } 
        public int grupoNomina { get; set; }   
        public decimal? sueldo { get; set; }

        public DateTime salida { get; set; }
        public DateTime regreso { get; set; }
        public DateTime fechaReintegro { get; set; }
        public int? cantDiasDisfrute_Feriados { get; set; }
        public int? cantDiasDisfrute_SabDom { get; set; }
        public int? cantDiasDisfrute_Habiles { get; set; }
        public int cantDiasDisfrute_Total { get; set; }

        // pago de las vacaciones
        public DateTime? periodoPagoDesde { get; set; }
        public DateTime? periodoPagoHasta { get; set; }
        public int? cantDiasPago_Feriados { get; set; }
        public int? cantDiasPago_SabDom { get; set; }
        public int? cantDiasPago_Habiles { get; set; }
        public int? cantDiasPago_YaTrabajados { get; set; }
        public int? cantDiasPago_Total { get; set; }
        public int? cantDiasPago_Bono { get; set; }
        public decimal? baseBonoVacacional { get; set; }

        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public decimal? montoBono { get; set; }

        public bool? aplicarDeduccionesFlag { get; set; }
        public int? cantDiasDeduccion { get; set; }

        // nómina en la cual se aplicará la vacación
        public DateTime? fechaNomina { get; set; }
        public bool? obviarEnLaNominaFlag { get; set; }
        public DateTime? desactivarNominaDesde { get; set; }
        public DateTime? desactivarNominaHasta { get; set; }

        // próxima nómima (normal) de pago (al regreso)
        public DateTime? proximaNomina_FechaNomina { get; set; }
        public bool? proximaNomina_AplicarDeduccionPorAnticipo { get; set; }
        public int? proximaNomina_AplicarDeduccionPorAnticipo_CantDias { get; set; }
        public bool? proximaNomina_AplicarDeduccionesLegales { get; set; }
        public int? proximaNomina_AplicarDeduccionesLegales_CantDias { get; set; }

        // datos del año de la vacación
        public int anoVacaciones { get; set; }
        public int numeroVacaciones { get; set; }
        public DateTime? anoVacacionesDesde { get; set; }
        public DateTime? anoVacacionesHasta { get; set; }

        // datos para el control de días pendientes
        public int? cantDiasVacPendAnosAnteriores { get; set; }
        public int? cantDiasVacSegunTabla { get; set; }
        public int? cantDiasVacDisfrutadosAntes { get; set; }
        public int? cantDiasVacDisfrutadosAhora { get; set; }
        public int? cantDiasVacPendientes { get; set; }

        public int cia { get; set; }
    }
}   