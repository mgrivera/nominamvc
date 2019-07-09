using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NominaASP.Empleados.Vacaciones
{
    public class ConsultaCriteriosFiltro_ControlDiasVacaciones
    {
        public int? CiaContab { get; set; }
        public int? AnoConsulta { get; set; }
        public string Status { get; set; }
        public string SituacionActual { get; set; }
    }

    public class DiasVacacionesConsulta
    {
        public string CiaContab { get; set; }
        public string Departamento { get; set; }
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Alias { get; set; }
        public DateTime FechaIngreso { get; set; }

        // año de vacaciones para el empleado; por ejemplo: 6 - 15/7/2012 a 14/7/2013   (6: año del empleado en la empresa) 

        public int AnoVacaciones_Ano { get; set; }
        public DateTime AnoVacaciones_Desde { get; set; }
        public DateTime AnoVacaciones_Hasta { get; set; }

        // vacación más reciente 

        public DateTime? VacacionMasReciente_Desde { get; set; }
        public DateTime? VacacionMasReciente_Hasta { get; set; }
        public int VacacionMasReciente_AnoVacaciones { get; set; }
        public short? VacacionMasReciente_DiasPendAnosAnteriores { get; set; }
        public short? VacacionMasReciente_DiasSegunTabla { get; set; }
        public short? VacacionMasReciente_DiasDisfrutados_Antes { get; set; }       // en vacaciones anteriores pero del mismo año 
        public short? VacacionMasReciente_DiasDisfrutados_Ahora { get; set; }       // en esta vacación específica 
        public short? VacacionMasReciente_DiasPendientes { get; set; }


        // vacacaiones pendientes - solo si el empleado tiene vacaciones pendientes posteriores a su vacación más reciente y para el año de la consulta 
        
        public short VacacionesPendientes_Cantidad { get; set; }
        public short VacacionesPendientes_DiasSegunTabla { get; set; }

        // Por último, total de días pendientes 

        public short TotalDiasPendientes { get; set; }
       
        public List<DiasVacacionesConsulta> GetDiasVacacionesConsulta()
        {
            List<DiasVacacionesConsulta> list = new List<DiasVacacionesConsulta>();
            return list;
        }
    }
}