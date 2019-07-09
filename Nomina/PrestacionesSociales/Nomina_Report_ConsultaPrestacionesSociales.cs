using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NominaASP.Nomina.PrestacionesSociales
{
    public class Nomina_Report_ConsultaPrestacionesSociales
    {
        public int HeaderID { get; set; }

        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }

        public string CiaContab { get; set; }
        public string Empleado { get; set; }
        public DateTime FechaIngreso { get; set; }
        public short AnosServicio { get; set; }
        public short AnosServicioLey { get; set; }
        public short? CantDias1erMes { get; set; }

        public decimal SalarioPeriodo { get; set; }
        public decimal SalarioMensual { get; set; }
        public decimal SalarioDiario { get; set; }

        public short BonoVacCantDias { get; set; }
        public decimal BonoVacMonto { get; set; }
        public decimal BonoVacDiario { get; set; }

        public short? UtilidadesCantDias { get; set; }
        public decimal UtilidadesMonto { get; set; }
        public decimal UtilidadesDiario { get; set; }

        public decimal SalarioTotalDiario { get; set; }

        public short PrestacionesCantDias { get; set; }
        public decimal PrestacionesMonto { get; set; }

        public bool PrestacionesDiasAdicAnoCumplidoFlag { get; set; }
        public short? PrestacionesDiasAdicAnoCumplidoCantDias { get; set; }
        public decimal? PrestacionesDiasAdicAnoCumplidoMonto { get; set; }

        public decimal PrestacionesTotalMonto { get; set; }

        public List<Nomina_Report_ConsultaPrestacionesSociales> GetNomina_Report_ConsultaPrestacionesSociales()
        {
            List<Nomina_Report_ConsultaPrestacionesSociales> list = new List<Nomina_Report_ConsultaPrestacionesSociales>();
            return list;
        }
    }
}