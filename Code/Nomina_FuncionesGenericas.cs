using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using NominaASP.Models.Nomina;
using NominaASP.Models;
using System.Data.Entity.Infrastructure;

namespace NominaASP.Code
{
    public class Nomina_FuncionesGenericas
    {
        public bool DeterminarSalarioEmpleadoEnPeriodo(dbNominaEntities dbContext,
                                                        int numeroEmpleado,
                                                        DateTime desde,
                                                        DateTime hasta,
                                                        out decimal salarioEmpleado,
                                                        out string errorMessage,
                                                        bool incluirMontoCestaTicketsEnMaestraEmpleados = false, 
                                                        bool incluirMontoBonoVacacionalSiExisteEnPeriodo = true)
        {
            salarioEmpleado = 0;
            errorMessage = "";
            int rubroBonoVacacional;

            if (!ObtenerRubro(dbContext, 4, out rubroBonoVacacional))
            {
                errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde " +
                                "a la asignación por bono vacacional." +
                                "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                return false;
            }

            decimal? monto; 

            // leemos y sumamos en la nómina (tNomina) todos los rubros que corresopndan a Salario en el período indicado ... 

            monto = dbContext.tNominas.Where(n => n.Empleado == numeroEmpleado && 
                                                  n.tNominaHeader.FechaNomina >= desde && 
                                                  n.tNominaHeader.FechaNomina <= hasta && 
                                                  (n.SalarioFlag || n.SueldoFlag)).
                                                  Sum(n => (decimal?) n.Monto);


            // nótese el parámetro que indica si se debe o no incluir el monto del bono (de existir en el período) en el salario ... 

            if (!incluirMontoBonoVacacionalSiExisteEnPeriodo)
            {
                decimal? montoBonoVacacional;

                // nótese como *solo* restamos el bono vacacional si está marcado como sueldo o salario; de no ser así, igual nunca sería leído 
                // por el query anterior ... 

                montoBonoVacacional = dbContext.tNominas.Where(n => n.Empleado == numeroEmpleado &&
                                                               n.tNominaHeader.FechaNomina >= desde &&
                                                               n.tNominaHeader.FechaNomina <= hasta &&
                                                               n.Rubro == rubroBonoVacacional && 
                                                               (n.SalarioFlag || n.SueldoFlag)).
                                                               Sum(n => (decimal?)n.Monto);

                if (montoBonoVacacional != null)
                    monto -= montoBonoVacacional.Value; 
            }

            if (monto != null)
                salarioEmpleado = monto.Value; 

            if (incluirMontoCestaTicketsEnMaestraEmpleados)
            {
                decimal? montoCestaTickets = dbContext.tEmpleados.Where(e => e.Empleado == numeroEmpleado).Select(e => e.MontoCestaTickets).FirstOrDefault();

                if (montoCestaTickets != null)
                    salarioEmpleado += montoCestaTickets.Value; 
            }

            return true;
        }

        public bool DeterminarMontoBaseCalculoBonoVacacional(dbNominaEntities dbContext,
                                                             int numeroEmpleado,
                                                             DateTime desde,
                                                             DateTime hasta,
                                                             out decimal montoBaseCalculoBonoVacacional,
                                                             out string errorMessasge)
        {
            errorMessasge = ""; 

            // esta función determina la base que debe ser usada para determinar el bono vacacional de un empleado; nótese que esta función es 
            // usada por el cálculo de prestaciones sociales; allí se usa esta base calculada en forma estándard, de acuerdo a algunos parámetros 
            // que indica el usuario en la tabla de empleados ... 

            // ---------------------------------------------------------------------------------------------------------------------
            // leemos los items necesarios para calcular la base del calculo del bono vacacional en la tabla
            // de empleados

            decimal nMontoCestaTickets = 0;
            bool bBonoVacAgregarMontoCestaTicketsFlag = false;
            bool bBonoVacAgregarSueldoFlag = false;
            decimal nBonoVacacionalMontoAdicional = 0;
            bool bBonoVacAgregarMontoAdicionalFlag = false;

            var empleado = (from e in dbContext.tEmpleados
                            where e.Empleado == numeroEmpleado
                            select new
                            {
                                MontoCestaTickets = e.MontoCestaTickets,
                                BonoVacAgregarSueldoFlag = e.BonoVacAgregarSueldoFlag,
                                BonoVacAgregarMontoCestaTicketsFlag = e.BonoVacAgregarMontoCestaTicketsFlag,
                                BonoVacAgregarMontoAdicionalFlag = e.BonoVacAgregarMontoAdicionalFlag,
                                BonoVacacionalMontoAdicional = e.BonoVacacionalMontoAdicional
                            }).FirstOrDefault();

            if (empleado != null)
            {
                nMontoCestaTickets = empleado.MontoCestaTickets != null ? empleado.MontoCestaTickets.Value : 0;
                bBonoVacAgregarMontoCestaTicketsFlag = empleado.BonoVacAgregarMontoCestaTicketsFlag != null ? empleado.BonoVacAgregarMontoCestaTicketsFlag.Value : false;
                bBonoVacAgregarSueldoFlag = empleado.BonoVacAgregarSueldoFlag != null ? empleado.BonoVacAgregarSueldoFlag.Value : false;
                bBonoVacAgregarMontoAdicionalFlag = empleado.BonoVacAgregarMontoAdicionalFlag != null ? empleado.BonoVacAgregarMontoAdicionalFlag.Value : false;
                nBonoVacacionalMontoAdicional = empleado.BonoVacacionalMontoAdicional != null ? empleado.BonoVacacionalMontoAdicional.Value : 0;
            }

            montoBaseCalculoBonoVacacional = 0;

            if (bBonoVacAgregarSueldoFlag)
            {
                // buscamos el sueldo más reciente para el período indicado  
                // (nótese que en un período, puede existir más de un sueldo para un empleado; sin embargo, al menos por ahora, tomamos el más reciente que 
                // corresponda al periodo indicado ... 

                decimal? wsalarioEmpleado2 = dbContext.Empleados_Sueldo.Where(s => s.Empleado == numeroEmpleado && s.Desde <= hasta).
                                                                   OrderByDescending(s => s.Desde).
                                                                   Select(s => s.Sueldo).
                                                                   FirstOrDefault();

                if (wsalarioEmpleado2 != null)
                    montoBaseCalculoBonoVacacional += wsalarioEmpleado2.Value;
            }

            if (bBonoVacAgregarMontoCestaTicketsFlag)
                // agregamos el monto en cesta tickets a la base del bono
                montoBaseCalculoBonoVacacional += nMontoCestaTickets;


            if (bBonoVacAgregarMontoAdicionalFlag)
                // agregamos el monto adicional a la base del bono
                montoBaseCalculoBonoVacacional += nBonoVacacionalMontoAdicional;


            return true;
        }

        public bool DeterminarDiasVacacionesParaEmpleado(
            dbNominaEntities dbContext,
            int empleado,
            int cantidadAnosServicio,
            out int cantidadDiasVacaciones,
            out int cantidadDiasBonoVacacional, 
            out string errorMessage)
        {
            cantidadDiasVacaciones = 0;
            cantidadDiasBonoVacacional = 0; 
            errorMessage = "";

            if (cantidadAnosServicio == 0)
                // si el empleado no ha cumplido un año, asumimos que sus vacaciones son iguales a los
                // que tienen un año
                cantidadAnosServicio = 1;

            VacacPorAnoParticulare vacacionParticular = (from v in dbContext.VacacPorAnoParticulares
                                                         where v.Empleado == empleado &&
                                                               v.Ano == cantidadAnosServicio
                                                         select v).FirstOrDefault();

            if (vacacionParticular != null)
                if (vacacionParticular.DiasBono != null)
                {
                    cantidadDiasVacaciones = vacacionParticular.Dias;
                    if (vacacionParticular.DiasAdicionales != null)
                        cantidadDiasVacaciones += vacacionParticular.DiasAdicionales.Value;

                    cantidadDiasBonoVacacional = vacacionParticular.DiasBono.Value; 

                    return true;
                }

            VacacPorAnoGenerica vacacionGenerica = (from v in dbContext.VacacPorAnoGenericas
                                                    where v.Ano == cantidadAnosServicio
                                                    select v).FirstOrDefault();

            if (vacacionGenerica != null)
                if (vacacionGenerica.DiasBono != null)
                {
                    cantidadDiasVacaciones = vacacionGenerica.Dias;
                    if (vacacionGenerica.DiasAdicionales != null)
                        cantidadDiasVacaciones += vacacionGenerica.DiasAdicionales.Value;

                    cantidadDiasBonoVacacional = vacacionGenerica.DiasBono.Value; 

                    return true;
                }



            // no encontramos un registro de definición de días de vacaciones para el empleado; regresamos con un error ... 

            errorMessage = "Error: no existe un registro en las tablas de 'días de vacaciones' para empleados con una cantidad de años igual a " +
                cantidadAnosServicio.ToString() + "." +
                "Ud. debe agregar un registro en esta tabla para la cantidad de años mencionada. ";

            return false;
        }

      
        public bool DeterminarMontoSueldoSalarioNominasRelacionadas(dbNominaEntities dbContext,
                                                                    NominaASP.Models.tNominaHeader nominaHeader, 
                                                                    int numeroEmpleado,
                                                                    string sueldoOSalario,                // Sueldo o Salario 
                                                                    out decimal montoSueldoOSalario,
                                                                    out string errorMessasge)
        {
            // para leer en nóminas anteriores montos de sueldo o salario para un empleado en particular ... 
            // este mecanismo es usado, normalmente, en el cálculo de deducciones, las cuales se aplican 
            // a bases de salario o sueldo que cubren más de la nómina que se está ejecutando (en realidad, 
            // normalmente esto ocurre solo en nóminas quincenales) 

            montoSueldoOSalario = 0;
            errorMessasge = "";

            if (sueldoOSalario != "Sueldo" && sueldoOSalario != "Salario")
            {
                errorMessasge = "Error: la base de aplicación pasada a esta función debe ser: Sueldo o Salario.";
                return false; 
            }

            decimal? montoAplicadoAntes; 

            // nótese como nos aseguramos de no leer nóminas de tipo Utilidades para calcular la base de aplicación que sigue; 
            // esta base de aplicación se usa para calcular deducciones legales e islr ... la idea es que una nómina de utilidades 
            // ya pagó islr; si se vuelve a leer este salario (utilidades) para el cálculo del islr en una 2da. quincena, por ejemplo, 
            // se aplicaría el porcentaje de islr dos veces a este monto ... 

            if (sueldoOSalario == "Sueldo") 
                montoAplicadoAntes = dbContext.tNominas.Where(n => n.tNominaHeader.FechaNomina >= nominaHeader.Desde 
                                                                && n.tNominaHeader.FechaNomina <= nominaHeader.Hasta).
                                                        Where(n => n.Empleado == numeroEmpleado).
                                                        Where(n => n.SueldoFlag).
                                                        Where(n => n.HeaderID != nominaHeader.ID).
                                                        Where(n => n.Tipo != "U").
                                                        Sum(n => (decimal?)n.Monto);
            else
                montoAplicadoAntes = dbContext.tNominas.Where(n => n.tNominaHeader.FechaNomina >= nominaHeader.Desde
                                                                && n.tNominaHeader.FechaNomina <= nominaHeader.Hasta).
                                                        Where(n => n.Empleado == numeroEmpleado).
                                                        Where(n => n.SueldoFlag || n.SalarioFlag).
                                                        Where(n => n.HeaderID != nominaHeader.ID).
                                                        Where(n => n.Tipo != "U").
                                                        Sum(n => (decimal?)n.Monto);

            if (montoAplicadoAntes == null)
                montoAplicadoAntes = 0;


            montoSueldoOSalario = montoAplicadoAntes.Value;
            return true; 
        }

        public bool DeterminarYRegistrarSalarioIntegralEmpleado(dbNominaEntities dbContext,
                                                                tNominaHeader nominaHeader, 
                                                                tEmpleado empleado,
                                                                out string errorMessage)
        {
            errorMessage = "";

            // determinamos el sueldo básico del empleado 

            // buscamos el sueldo más reciente para el período de pago 
            // nótese que lo leemos y determinamos aunque no lo agreguemos, para usarlo en registro de faltas y vacaciones ... 

            decimal? sueldoEmpleado = dbContext.Empleados_Sueldo.Where(s => s.Empleado == empleado.Empleado && s.Desde <= nominaHeader.Desde.Value).
                                                                 OrderByDescending(s => s.Desde).
                                                                 Select(s => s.Sueldo).
                                                                 FirstOrDefault();

            if (sueldoEmpleado == null)
                sueldoEmpleado = 0;

            // determinamos la cantidad de días de vacaciones del empleado, para calcular su bono vacacional 
            // nótese como determinamos el "año en curso" para el empleado; por ejemplo, un empleado que tiene 4 1/2 años 
            // en la empresa, consideramos que tiene 5 años, pues su año "en curso" es el 5to. 

            int cantidadAnosServicio = 0;
            DateTime fechaIngreso = empleado.FechaIngreso;

            while (fechaIngreso < nominaHeader.Hasta.Value)
            {
                cantidadAnosServicio++;
                fechaIngreso = fechaIngreso.AddYears(1); 
            }

            int cantidadDiasVacaciones = 0;
            int cantidadDiasBonoVacacional = 0; 

            if (!DeterminarDiasVacacionesParaEmpleado(dbContext,
                                                      empleado.Empleado,
                                                      cantidadAnosServicio,
                                                      out cantidadDiasVacaciones,
                                                      out cantidadDiasBonoVacacional, 
                                                      out errorMessage)) 
            {
                return false; 
            }

            // determinamos la cantidad de días de utilidades, para determinar su monto completo de utilidades 

            // nótese como debe existir un registro de definición de utilidades, para poder leer la cantidad de días que se aplican para la empresa 

            Utilidade utilidades = (dbContext.Utilidades.Where(u => u.tGruposEmpleado.Grupo == nominaHeader.tGruposEmpleado.Grupo).
                                                         Where(u => u.FechaNomina <= nominaHeader.FechaNomina).OrderByDescending(u => u.FechaNomina)).FirstOrDefault();

            if (utilidades == null)
            {
                errorMessage = "<b>Error:</b> no hemos podido leer un registro de <em>'definición de utilidades'</em> que aplique a esta nómina de pago.<br />" +
                    "Ud. debe registrar un registro de <em>'definición de utilidades'</em> que pueda ser usado por esta nómina de pago.";
                return false; 
            }

            // nótese que el usuario puede definir utilidades para períodos diferentes a un año (ejemplo: utilidades que se pagan cada 6 meses) 

            int cantidadDiasUtilidades = (360 * utilidades.CantidadDiasUtilidades) / utilidades.CantidadDiasPeriodoPago; 

            // grabamos un registro en la tabla de salario integral, para el empleado específico ... 

            // primero eliminamos un registro que pueda existir para el empleado y la nómina 

            // nótese como obtenemos el objectcontext que corresponde al dbcontext ... 
            var objectContext = (dbContext as IObjectContextAdapter).ObjectContext;
            objectContext.ExecuteStoreCommand("Delete From tNomina_SalarioIntegral Where HeaderID = {0} And Empleado = {1}",
                                          new Object[] { nominaHeader.ID, empleado.Empleado }); 


            tNomina_SalarioIntegral salarioIntegral = new tNomina_SalarioIntegral(); 
            
            salarioIntegral.HeaderID = nominaHeader.ID; 
            salarioIntegral.Empleado = empleado.Empleado; 

            salarioIntegral.SueldoBasico_Mensual = sueldoEmpleado.Value;
            salarioIntegral.SueldoBasico_Diario = Math.Round(sueldoEmpleado.Value / 30, 2);

            salarioIntegral.BonoVacacional_Dias = Convert.ToInt16(cantidadDiasBonoVacacional);
            salarioIntegral.BonoVacacional_Monto = Math.Round(sueldoEmpleado.Value / 30 * cantidadDiasBonoVacacional, 2);
            salarioIntegral.BonoVacacional_Diario = Math.Round(sueldoEmpleado.Value / 30 * cantidadDiasBonoVacacional / 30, 2);

            salarioIntegral.Utilidades_Dias = Convert.ToInt16(cantidadDiasUtilidades);
            salarioIntegral.Utilidades_Monto = Math.Round(sueldoEmpleado.Value / 30 * cantidadDiasUtilidades, 2);
            salarioIntegral.Utilidades_Diario = Math.Round(sueldoEmpleado.Value / 30 * cantidadDiasUtilidades / 30, 2);

            salarioIntegral.SalarioIntegral_Monto = salarioIntegral.SueldoBasico_Mensual +
                                                   (salarioIntegral.BonoVacacional_Monto / 12) +
                                                   (salarioIntegral.Utilidades_Monto / 12);
            salarioIntegral.SalarioIntegral_Diario = salarioIntegral.SalarioIntegral_Monto / 30;

            salarioIntegral.SalarioIntegral_Monto = Math.Round(salarioIntegral.SalarioIntegral_Monto, 2);
            salarioIntegral.SalarioIntegral_Diario = Math.Round(salarioIntegral.SalarioIntegral_Diario, 2);
            
            dbContext.tNomina_SalarioIntegral.Add(salarioIntegral);

            try
            {
                dbContext.SaveChanges(); 
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += "<br />" + ex.InnerException.Message;

                return false; 
            }

            return true; 
        }

        public int CantidadMesesEntreFechas(DateTime desde, DateTime hasta)
        {
            // nota: esta función determina la cantidad de meses entre 2 fechas. 
            // la función la tomamos de un post en StackOverflow ... (many thanks to the author!) 

            int MonthsElapsed = ((hasta.AddDays(1).Year - desde.AddDays(1).Year) * 12) +
                    (hasta.AddDays(1).Month - desde.AddDays(1).Month) -
                    (hasta.AddDays(1).Day < desde.AddDays(1).Day ? 1 : 0);

            return (MonthsElapsed + 1); 
        }

        public bool ObtenerRubro(dbNominaEntities context, int tipoRubro, out int rubroID)
        {
            // lee y regresa un rubro en la maestra de rubros, según su tipo específico; por ejemplo: 
            // el rubro 8 es bono vacacional, etc. 

            rubroID = 0;

            tMaestraRubro rubro = context.tMaestraRubros.Where(r => r.TipoRubro == tipoRubro).FirstOrDefault();

            if (rubro == null)
                return false;

            rubroID = rubro.Rubro;

            return true;
        }

    }
}