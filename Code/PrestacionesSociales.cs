using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using NominaASP.Models.Nomina;
using NominaASP.Models;
using System.Data.Entity.Infrastructure;

namespace NominaASP.Code
{
    public class PrestacionesSociales_Funciones
    {
        dbNominaEntities _dbContext;

        public PrestacionesSociales_Funciones()
        {
            dbNominaEntities context = new dbNominaEntities();
            _dbContext = context; 
        }

        public bool PrestacionesSociales_CalculoYRegistro(int prestacionesSocialesHeaderID, 
                                                          string userName,
                                                          out int cantidadRegistros,
                                                          out string resultMessage)
        {
            resultMessage = "";
            cantidadRegistros = 0;

            // lo primero que hacemos es leer el 'header' para las prestaciones; allí están los parámetros de ejecución de las prestaciones 
            PrestacionesSocialesHeader header = _dbContext.PrestacionesSocialesHeaders.Where(h => h.ID == prestacionesSocialesHeaderID).FirstOrDefault();

            if (header == null)
            {
                resultMessage = "Error inesperado: no hemos podido leer el 'header' para las prestaciones (" + 
                    prestacionesSocialesHeaderID.ToString() + 
                    ") que se ha pasado como parámetro a esta función.";
                return false;
            }

            if (header.Desde == null || header.Hasta == null)
            {
                resultMessage = "Error: aparentemente, el período (desde, hasta) de prestaciones no está correctamente inicializado; " +
                    "por favor registre un período de prestaciones y, solo luego, ejecute nuevamente esta función.";
                return false;
            }

            // eliminamos los registros que puedan existir para el usuario antes ... 
            int mes = header.Mes;
            int ano = header.Ano;

            DateTime desde = header.Desde.Value;
            DateTime hasta = header.Hasta.Value;
            short cantidadDiasUtilidades = header.CantDiasUtilidades;

            // nótese que el usuario indica si el monto en cesta tickets para los empleados debe ser agregado para el cálculo del 
            // sueldo ... 
            bool agregarMontoCestaTickets = header.AgregarMontoCestaTickets; 

            int cia = header.Compania.Numero;

            // nótese como obtenemos el objectcontext que corresponde al dbcontext ... 
            var objectContext = (_dbContext as IObjectContextAdapter).ObjectContext;

            int rowsAffected = objectContext.ExecuteStoreCommand("Delete From PrestacionesSociales Where HeaderID = {0}", prestacionesSocialesHeaderID);

            int cantidadMesesPeriodoPrestaciones = Convert.ToInt32((hasta - desde).TotalDays / 30);     // cuando hay febrero, esto no funciona correctamente ... 

            Nomina_FuncionesGenericas funcionesNomina = new Nomina_FuncionesGenericas();
            cantidadMesesPeriodoPrestaciones = funcionesNomina.CantidadMesesEntreFechas(desde, hasta); 

            
            int cantidadMesesServicio;
            int cantidadAnosServicioParaPrestaciones = 0;
            int cantidadMesesTrabajadosPeriodo = 0;
            int cantidadDiasTrabajadosPeriodo = 0;

            // recorremos los empleados activos para la compañía Contab y calculamos un registro de prestaciones sociales para cada uno ... 
            var empleadosQuery = from e in _dbContext.tEmpleados
                                 where e.Status == "A" &&
                                 e.Cia == cia &&
                                 e.FechaIngreso <= hasta
                                 select new { e.Empleado, e.FechaIngreso, e.FechaRetiro, e.MontoCestaTickets, e.PrestacionesAgregarMontoCestaTicketsFlag };

            PrestacionesSociale prestacionSocial;

            foreach (var empleado in empleadosQuery)
            {
                cantidadMesesServicio = (hasta - empleado.FechaIngreso).Days / 30;
                cantidadMesesServicio = cantidadMesesServicio + 1;

                // la ley debe comenzar a aplicarse a partir del 19-jun-97
                if (empleado.FechaIngreso >= new DateTime(1997, 7, 19))

                    // al tomar la parte entera de la cantidad de días entre 360, obtenemos la cantidad
                    // de años completos (por alguna razón, DateDiff regresa 8 entre, por ejemplo,
                    // 7-19-1997 y 3-31-2005. La cantidad de años en verdad es: 7 y pico.
                    cantidadAnosServicioParaPrestaciones = (hasta - empleado.FechaIngreso).Days / 360;
                else
                    cantidadAnosServicioParaPrestaciones = (hasta - new DateTime(1997, 7, 19)).Days / 360;

                // si el empleado no tiene aún 4 meses de servicio, no cobra prestaciones
                // CAMBIA en la nueva ley!! ahora el empleado cobra a partir del 1er. mes ... 

                // vamos a determinar ahora la cantidad de meses trabajados por el empleado en el período de cálculo
                // el empleado pudo haber trabajado la cantidad completa de meses (caso usual), o pudo haber trabajado 
                // menos (caso inusual); en este último caso, determinamos la cantidad de meses y días trabajados 
                cantidadMesesTrabajadosPeriodo = 0;
                cantidadDiasTrabajadosPeriodo = 0;

                if (empleado.FechaRetiro == null)
                {
                    if (empleado.FechaIngreso <= desde)
                    {
                        // el empleado esta activo desde antes del período de prestaciones; consideramos que ha ganado todos sus días de 
                        // prestaciones ... 
                        cantidadMesesTrabajadosPeriodo = cantidadMesesPeriodoPrestaciones;
                    }
                    else
                    {
                        // el empleado ingresó *luego* del período de prestaciones ... 
                        if (empleado.FechaIngreso.Day == 1)
                        {
                            // asumimos 0 días y una cantidad completa de meses 
                            cantidadMesesTrabajadosPeriodo = (hasta - empleado.FechaIngreso).Days / 30;
                        }
                        else
                        {
                            // asumimos algunos meses (pueden ser 0) y una cantidad de días 
                            DateTime lastDayOfThisMonth = empleado.FechaIngreso.AddMonths(1).AddDays(-1);
                            cantidadDiasTrabajadosPeriodo = (lastDayOfThisMonth - empleado.FechaIngreso).Days + 1;
                            cantidadMesesTrabajadosPeriodo = (hasta - empleado.FechaIngreso).Days / 30;
                            cantidadMesesTrabajadosPeriodo--;

                        }
                    }
                }

                // -----------------------------------------------------------------------------------------------------------
                // ahora que tenemos la cantidad de meses y días trabajados por el empleado en el período de cálculo, 
                // determinamos sus prestaciones; nota: el caso común es 3 meses en el período de cálculo; el caso 
                // inusual es una cantidad menor de meses y algunos días. 
                int cantidadAnosServicio = ((empleado.FechaRetiro == null ? hasta : (empleado.FechaRetiro.Value < hasta ? empleado.FechaRetiro.Value : hasta)) -
                    empleado.FechaIngreso).Days / 360;

                // ----------------------------------------------------------------------------------------------------------
                // la función que sigue determina el salario para el empleado, para el mes *final* del período de calculo 
                // (recuérdese que el salario base a usar en el cálculo de las prestaciones, es el último del período indicado) 
                string errorMessage = "";
                decimal salarioEmpleado = 0;
                decimal montoCestaTickets = 0;
                decimal salarioPrestaciones = 0; 

                DateTime ultimoMesPeriodo_Desde = new DateTime(hasta.Year, hasta.Month, 1);
                DateTime ultimoMesPeriodo_Hasta = new DateTime(hasta.Year, hasta.Month, 1).AddMonths(1).AddDays(-1);

                // el último parámetro en la función es opcional, e indica si se debe o no incluir el monto de cesta tickets en el salario ... 
                // (nota: el monto de cestatickets se registra con el empleado en la maestra) 

                // aunque la función que sigue puede agregar el monto de cesta tickets al sueldo básico del empleado, lo hacemos después, pues debemos 
                // tener ambos montos separados pues se registran en la tabla de prestaciones. 

                if (!funcionesNomina.DeterminarSalarioEmpleadoEnPeriodo(_dbContext, 
                                                                        empleado.Empleado,
                                                                        ultimoMesPeriodo_Desde,
                                                                        ultimoMesPeriodo_Hasta, 
                                                                        out salarioEmpleado, 
                                                                        out errorMessage,
                                                                        false,              // incluir monto cesta tickets desde maestra de empleados 
                                                                        false               // incliuir monto de bono vacacional, si está marcado como sueldo o salario 
                                                                        ))
                {
                    resultMessage = errorMessage;
                    return false;
                }


                // si el usuario así lo indicó, agregamos el monto de cesta tickets al sueldo básico del empleado ... 

                // nótese que el empleado debe estar 'marcado' para que este monto deba ser agregado, además de haberse hacho 
                // también en el header de prestaciones ... 
                montoCestaTickets = empleado.MontoCestaTickets.HasValue ? empleado.MontoCestaTickets.Value : 0;

                salarioPrestaciones = salarioEmpleado;

                if (agregarMontoCestaTickets && empleado.PrestacionesAgregarMontoCestaTicketsFlag.HasValue && empleado.PrestacionesAgregarMontoCestaTicketsFlag.Value)
                {
                    salarioPrestaciones += montoCestaTickets; 
                }



                decimal montoBaseCalculoBonoVacacional = 0;

                // para calcular la base para el bono vacacional, usamos la misma función que determina el salario del empleado, pero 
                // agregamos el monto de cestatickets (cuando existe) al salario determinado ... 
                if (!funcionesNomina.DeterminarSalarioEmpleadoEnPeriodo(_dbContext,
                                                                        empleado.Empleado,
                                                                        ultimoMesPeriodo_Desde,
                                                                        ultimoMesPeriodo_Hasta,
                                                                        out montoBaseCalculoBonoVacacional,
                                                                        out errorMessage,
                                                                        true,                   // incluir monto cesta tickets desde maestra de empleados 
                                                                        false                   // incliuir monto de bono vacacional (si está marcado como sueldo o salario) 
                                                                        ))
                {
                    resultMessage = errorMessage;
                    return false;
                }

                // ----------------------------------------------------------------------------------------------------------
                // la función que sigue, determina la cantidad de días de vacaciones para el empleado ... 

                int cantidadDiasVacaciones = 0;
                int cantidadDiasBono = 0; 
                if (!funcionesNomina.DeterminarDiasVacacionesParaEmpleado(_dbContext, 
                                                                          empleado.Empleado, 
                                                                          cantidadAnosServicio, 
                                                                          out cantidadDiasVacaciones, 
                                                                          out cantidadDiasBono, 
                                                                          out errorMessage))

                {
                    resultMessage = errorMessage;
                    return false;
                }


                prestacionSocial = new PrestacionesSociale();

                prestacionSocial.HeaderID = header.ID; 
                prestacionSocial.Empleado = empleado.Empleado;
                
                prestacionSocial.FechaIngreso = empleado.FechaIngreso;
                prestacionSocial.AnosServicio = Convert.ToInt16(cantidadAnosServicio);
                prestacionSocial.AnosServicioPrestaciones = Convert.ToInt16(cantidadAnosServicioParaPrestaciones);

                prestacionSocial.PrimerMesPrestacionesFlag = false;

                if (empleado.FechaIngreso >= desde)
                {
                    // el empleado ingresó justo en el período de calculo de prestaciones 
                    prestacionSocial.PrimerMesPrestacionesFlag = true;
                    prestacionSocial.CantidadDiasTrabajadosPrimerMes = Convert.ToByte(cantidadDiasTrabajadosPeriodo);
                }

                prestacionSocial.SueldoBasico = salarioEmpleado;
                prestacionSocial.MontoCestaTickets = montoCestaTickets;
                prestacionSocial.SueldoBasicoPrestaciones = salarioPrestaciones;

                prestacionSocial.SueldoBasicoDiario = Math.Round(salarioPrestaciones / 30, 2);

                prestacionSocial.DiasVacaciones = Convert.ToInt16(cantidadDiasVacaciones);
                prestacionSocial.BonoVacacional = Math.Round(cantidadDiasVacaciones * (montoBaseCalculoBonoVacacional / 30), 2);
                prestacionSocial.BonoVacacionalDiario = Math.Round((cantidadDiasVacaciones * (montoBaseCalculoBonoVacacional / 30)) / 360, 2);

                // nótese como agregamos el monto del bono vacacional, calculado antes, para determinar el monto de utilidades; en resumen, para calcular 
                // el monto de utilidades, sumamos el bono vacacional al salario determinado para el empledo 

                decimal utilidades = (prestacionSocial.SueldoBasicoDiario + prestacionSocial.BonoVacacionalDiario) * cantidadDiasUtilidades; 

                prestacionSocial.Utilidades = Math.Round(utilidades, 2);
                prestacionSocial.UtilidadesDiarias = Math.Round(prestacionSocial.Utilidades / 360, 2);

                prestacionSocial.SueldoDiarioAumentado =
                    prestacionSocial.SueldoBasicoDiario +
                    prestacionSocial.BonoVacacionalDiario +
                    prestacionSocial.UtilidadesDiarias;

                int diasPrestaciones;       // normalmente 15 (tres meses por 5 días para cada uno) 

                diasPrestaciones = (cantidadMesesPeriodoPrestaciones * 5);
                prestacionSocial.DiasPrestaciones = Convert.ToInt16(diasPrestaciones);

                if (!prestacionSocial.PrimerMesPrestacionesFlag)
                    prestacionSocial.MontoPrestaciones = Math.Round(prestacionSocial.SueldoDiarioAumentado * diasPrestaciones, 2);
                else
                    prestacionSocial.MontoPrestaciones = Math.Round(prestacionSocial.SueldoDiarioAumentado * (diasPrestaciones + cantidadDiasTrabajadosPeriodo), 2);


                prestacionSocial.AnoCumplidoFlag = false;

                // --------------------------------------------------------------------------------------
                // determinamos si el empleado está cumpliendo un año en la compañía. Este proceso debe
                // agregar 2 días de prestaciones por cada año cumplido (incluyendo el 1ro) *hasta* 30 

                // NOTA IMPORTANTE: notese como restamos un año a la cantidad de años, pues el 1er. año
                // NO GENERA días adicionales

                // NOTA IMPORTANTE 2: (nov/10) revertimos la premisa anterior, para cancelar 2 días adicionales para cada
                // año cumplido A PARTIR (e incluyendo) del primero ...

                if (empleado.FechaIngreso >= new DateTime(1997, 7, 19))
                {
                    if ((empleado.FechaIngreso.Month >= desde.Month && empleado.FechaIngreso.Month <= hasta.Month)
                        && cantidadAnosServicioParaPrestaciones > 0)
                    {
                        prestacionSocial.AnoCumplidoFlag = true;
                        prestacionSocial.CantidadDiasAdicionales = Convert.ToByte(cantidadAnosServicioParaPrestaciones * 2);

                        if (prestacionSocial.CantidadDiasAdicionales > 30)
                            prestacionSocial.CantidadDiasAdicionales = 30;

                        prestacionSocial.MontoPrestacionesDiasAdicionales = 
                            Math.Round(prestacionSocial.SueldoDiarioAumentado * prestacionSocial.CantidadDiasAdicionales.Value, 2);
                    }
                }
                else
                {
                    if ((new DateTime(1997, 7, 19).Month >= desde.Month && new DateTime(1997, 7, 19).Month <= hasta.Month) &&
                        cantidadAnosServicioParaPrestaciones > 0)
                    {
                        prestacionSocial.AnoCumplidoFlag = true;
                        prestacionSocial.CantidadDiasAdicionales = Convert.ToByte(cantidadAnosServicioParaPrestaciones * 2);

                        if (prestacionSocial.CantidadDiasAdicionales > 30)
                            prestacionSocial.CantidadDiasAdicionales = 30;

                        prestacionSocial.MontoPrestacionesDiasAdicionales = 
                            Math.Round(prestacionSocial.SueldoDiarioAumentado * prestacionSocial.CantidadDiasAdicionales.Value, 2);
                    }
                }

                _dbContext.PrestacionesSociales.Add(prestacionSocial);

                cantidadRegistros++;
            }


            try
            {
                _dbContext.SaveChanges();
                funcionesNomina = null; 
            }
            catch (Exception ex)
            {
                // por algún motivo, falló el acceso al db 

                resultMessage = ex.Message;

                if (ex.InnerException != null)
                    resultMessage += "\n\n" + ex.InnerException.Message;

                return false;
            }


            resultMessage = "Ok, la ejecución del proceso ha sido existosa.";

            return true;
        }
    }
}