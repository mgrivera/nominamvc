using System;
using System.Collections.Generic;
using System.Linq;
using NominaASP.Models;
using System.Data.Entity.Infrastructure;
using MongoDB.Driver;
using NominaASP.Models.MongoDB;

namespace NominaASP.Code
{
    public class Nomina
    {
        private int nominaHeaderID = 0;
        private IMongoDatabase _mongoDataBase = null; 

        public Nomina(int nominaHeaderID_Param, out string errorMessage)
        {
            errorMessage = ""; 
            nominaHeaderID = nominaHeaderID_Param;


            // establecemos una conexión a mongodb; específicamente, a la base de datos del programa contabM; allí se registrará 
            // todo en un futuro; además, ahora ya están registradas las vacaciones ... 
            string contabm_mongodb_connection = System.Web.Configuration.WebConfigurationManager.AppSettings["contabm_mongodb_connectionString"];
            string contabm_mongodb_name = System.Web.Configuration.WebConfigurationManager.AppSettings["contabM_mongodb_name"];

            // --------------------------------------------------------------------------------------------------------------------------
            // establecemos una conexión a mongodb 
            var client = new MongoClient(contabm_mongodb_connection);
            // nótese como el nombre de la base de datos mongo (de contabM) está en el archivo webAppSettings.config; en este db se registran las vacaciones 
            _mongoDataBase = client.GetDatabase(contabm_mongodb_name);
            // --------------------------------------------------------------------------------------------------------------------------

            var vacacionesMongoCollection = _mongoDataBase.GetCollection<vacacion>("vacaciones");

            try
            {
                // --------------------------------------------------------------------------------------------------------------------------
                // solo para que ocura una exception si mongo no está iniciado ... nótese que antes, cuando establecemos mongo, no ocurre un 
                // exception si mongo no está iniciado ...  
                var builder = Builders<vacacion>.Filter;
                var filter = builder.Eq(x => x.cia, -99999999);

                vacacionesMongoCollection.DeleteManyAsync(filter);
            }
            catch (Exception ex)
            {
                errorMessage = "Error al intentar establecer una conexión a la base de datos (mongo) de 'contabM'; el mensaje de error es: " + 
                               ex.Message;
                return;
            };
        }
        
        public void Ejecutar(out string errorMessage, out string resultadoEjecucionMessage)
        {
            errorMessage = "";
            resultadoEjecucionMessage = ""; 

            NominaASP.Models.tNominaHeader nominaHeader = null;
            ParametrosNomina parametrosNomina; 

            // lo primero que hacemos es leer el header ... 

            dbNominaEntities context = new dbNominaEntities(); 

            nominaHeader = context.tNominaHeaders.Include("tGruposEmpleado").Where(n => n.ID == nominaHeaderID).FirstOrDefault();
            parametrosNomina = context.ParametrosNominas.Where(p => p.Cia == nominaHeader.tGruposEmpleado.Cia).FirstOrDefault();
            
            if (nominaHeader == null)
            {
                errorMessage = "Error inesperado: no hemos podido leer la nómina que corresponde al ID indicado (" + nominaHeaderID.ToString() + ").";
                return; 
            }

            if (nominaHeader.Desde == null || nominaHeader.Hasta == null)
            {
                errorMessage = "Ud. no puede ejecutar una nómina que no tenga un período de pago definido.";
                return;
            }

            if (nominaHeader.CantidadDias == null || nominaHeader.CantidadDias == 0)
            {
                errorMessage = "La nómina a ejecutar debe tener definida una cantidad de días, que corresponda a las cantidad de días del período de pago.";
                return;
            }

            if (parametrosNomina == null)
            {
                errorMessage = "Error inesperado: no hemos podido leer la tabla Parámetros para la Cia Contab que corresponde a la nómina ejecutada.";
                return;
            }

            if (context.tMaestraRubros.Where(r => r.TipoRubro == 1).Count() == 0)
            {
                errorMessage = "No se ha definido cual es el rubro que corresponde al sueldo de empleados, en la tabla Parámetros.";
                return;
            }

            // el usuario debe indicar el valor para el sueldo mínimo, solo si se usa en las deducciones registradas ... 

            DeduccionesNomina deduccion = context.DeduccionesNominas.Where(d => d.TopeBase == "SalMin").FirstOrDefault();

            Parametros_Nomina_SalarioMinimo parametroNominaSalarioMinimo = null; 

            if (deduccion != null)
            {
                // aunque leemos el salario mínimo que corresonde aquí, para hacerlo una sola vez, validamos (not null) solo cuando lo 
                // necesitemos más abajo en este código. Aunque alguna deducción lo pueda necesitar, no necesariamente es una que corresponda 
                // a esta nómina en particular ... 

                parametroNominaSalarioMinimo = context.Parametros_Nomina_SalarioMinimo.Where(p => p.Desde <= nominaHeader.Desde.Value).
                                                                                       OrderByDescending(p => p.Desde).
                                                                                       FirstOrDefault();
            }



            // si podemos leer alguna cuota de préstamo a aplicar, debe existir un rubro definido para hacerlo en Parametros ... 

            tCuotasPrestamo cuota = context.tCuotasPrestamos.Where(p => p.FechaCuota >= nominaHeader.Desde.Value && p.FechaCuota <= nominaHeader.Hasta.Value).
                                                             Where(p => p.PagarPorNominaFlag != null && p.PagarPorNominaFlag.Value).
                                                             FirstOrDefault();

            if (cuota != null)
            {
                if (context.tMaestraRubros.Where(r => r.TipoRubro == 2).Count() == 0 || context.tMaestraRubros.Where(r => r.TipoRubro == 3).Count() == 0)
                {
                    errorMessage = "Ud. debe definir en la tabla de rubros (Catálogos) cuales son los rubros que " + 
                                   "corresponden a pago de cuotas de préstamo y sus intereses.";
                    return;
                }
            }

            // nótese como la ejecutación de la nómina depende, básicamente, del tipo; aunque todas las nóminas, independientemente de su tipo, 
            // se ejecutan básicamente en la misma forma, el tipo determinará que empleados, período, etc., usar ... 

            Nomina_FuncionesGenericas funcionesGenericasNomina = new Nomina_FuncionesGenericas();

            switch (nominaHeader.Tipo)
            {
                case "M":           // mensual 
                case "N":           // normal                   - no se debería usar más ... 
                case "1Q":          // quincenal 
                case "2Q":          // quincenal 
                //case "Q":           // quincenal                - no se debería usar más ...
                case "E":           // especial 
                    {
                        EjecutarNominaNormal(context, nominaHeader, parametrosNomina, parametroNominaSalarioMinimo, out errorMessage, out resultadoEjecucionMessage);
                        break;
                    }
                case "V":           // vacaciones 
                    {
                        EjecutarNominaVacaciones(context, _mongoDataBase, nominaHeader, parametrosNomina, parametroNominaSalarioMinimo, out errorMessage, out resultadoEjecucionMessage);
                        break; 
                    }
                case "U":           // utilidades 
                    {
                        // debe existir un registro de utilidades al cual corresponda esta nómina 

                        Utilidade utilidades = context.Utilidades.Where(u => u.ID == nominaHeader.ProvieneDe_ID).FirstOrDefault();
                        if (utilidades == null)
                        {
                            errorMessage = "Error inesperado: no hemos podido leer el registro de definición de utilidades que corresponde a esta nómina. " +
                                           "Para cada nómina de utilidades que Ud. ejecute, debe existir un registro de definición de utilidades correspondiente.";
                            return;
                        }

                        if (utilidades.CantidadDiasUtilidades == 0 || utilidades.CantidadDiasPeriodoPago == 0)
                        {
                            errorMessage = "Error: la cantidad de días del período de pago y la cantidad de días de pago de " +
                                "utilidades deben ser, ambos, diferentes a cero. ";
                            return;
                        }

                        if (utilidades.AplicarInce && utilidades.IncePorc == 0)
                        {
                            errorMessage = "Error: aunque Ud. indicó en la definición del pago de utilidades que se debía aplicar el Ince,  " +
                                "el porcentaje indicado para el mismo es cero. ";
                            return;
                        }

                        int rubroUtilidades;

                        if (!funcionesGenericasNomina.ObtenerRubro(context, 9, out rubroUtilidades))
                        {
                            errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde a la asignación de utilidades." +
                                           "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                            return;
                        }

                        int rubroInce;

                        if (!funcionesGenericasNomina.ObtenerRubro(context, 10, out rubroInce))
                        {
                            errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde a la deducción por aporte Ince." +
                                           "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                            return;
                        }

                        EjecutarNominaUtilidades(context, 
                                                 _mongoDataBase, 
                                                 nominaHeader, 
                                                 parametrosNomina, 
                                                 parametroNominaSalarioMinimo, 
                                                 utilidades,
                                                 rubroUtilidades,
                                                 rubroInce,
                                                 out errorMessage, 
                                                 out resultadoEjecucionMessage);

                        break;
                    }
                default:            // error (???!!!) 
                    {
                        errorMessage = "Error inesperado: tipo de nómina no defido (" + nominaHeader.Tipo + "). " + 
                            "Debe ser: Mensual, Quincenal (1ra. o 2da.), Especial, Vacaciones, Utilidades.";
                        return; 
                    }
            }
        }

        private void EjecutarNominaNormal(dbNominaEntities context, 
                                          tNominaHeader nominaHeader, 
                                          ParametrosNomina parametrosNomina, 
                                          Parametros_Nomina_SalarioMinimo parametroNominaSalarioMinimo, 
                                          out string errorMessage, 
                                          out string resultadoEjecucionMessage)
        {
            errorMessage = "";
            resultadoEjecucionMessage = ""; 

            // determinamos la cantidad de días feriados; luego lo usamos para separar el sueldo en estos tipos de fechas 
            int cantDiasHabiles = 0; 
            int cantDiasFeriados = 0; 
            int cantDiasFinSemana = 0; 
            int cantDiasBancarios = 0;

            DeterminarCantidadDiasFeriadosEnPeriodo(context, 
                                                    nominaHeader.Desde.Value, 
                                                    nominaHeader.Hasta.Value, 
                                                    nominaHeader.CantidadDias.Value, 
                                                    out cantDiasHabiles, 
                                                    out cantDiasFinSemana, 
                                                    out cantDiasFeriados, 
                                                    out cantDiasBancarios); 

            List<NominaItem> Nomina_List = new List<NominaItem>(); 

            // leemos los empleados del grupo de nómina; vamos a ejecutar la nómina para cada uno de ellos ... 

            // el grupo ya viene con el header 
            tGruposEmpleado grupoNomina = nominaHeader.tGruposEmpleado; 

            if (grupoNomina == null) 
            {
                errorMessage = "Error inesperado: no hemos podido leer el grupo de empleados que se ha indicado para este nómina (" 
                    + nominaHeader.GrupoNomina.ToString() + "). ";
                return; 
            }

            int cantidadEmpleadosEnVacaciones = 0;
            int cantidadCuotasPrestamoAplicadas = 0; 
            int cantidadEmpleados = 0;
            int cantidadFaltasAplicadas = 0;
            int cantidadEmpleadosRegresanVacaciones = 0; 

            Nomina_FuncionesGenericas funcionesGenericasNomina = new Nomina_FuncionesGenericas(); 

            foreach (NominaASP.Models.tEmpleado empleado in grupoNomina.tdGruposEmpleados.Where(e => !e.SuspendidoFlag && e.tEmpleado.Status == "A").
                                                                                          Select(e => e.tEmpleado))
            {
                cantidadEmpleados++;

                // la función que sigue determina y graba un registro con el salario integral (y su descomposición) para el empleado ... 
                if (!funcionesGenericasNomina.DeterminarYRegistrarSalarioIntegralEmpleado(context, nominaHeader, empleado, out errorMessage))
                    return; 

                // omitir si está en vacaciones 
                if (EmpleadoEnVacaciones(context, _mongoDataBase, nominaHeader, empleado, out errorMessage))
                {
                    cantidadEmpleadosEnVacaciones++; 
                    continue;
                }

                // TODO: aplicar salario y faltas 
                // nótese como aquí vamos a agregar días de sueldo, pero también deducir días de faltas

                int cantFaltasAplicadasEmpleado = 0;
                bool empleadoRegresaVacacionesFlag = false; 

                // para saber si el empleado regresa de vacaciones y se aplicó en esta nómina deducción por pago de días anticipados (en el pago de vacaciones) 
                bool vacacaciones_SeAplicoDeduccionPorDiasAdelantados = false; 

                AgregarSueldo(context, 
                              nominaHeader, 
                              parametrosNomina, 
                              empleado, 
                              Nomina_List, 
                              cantDiasHabiles, 
                              cantDiasFeriados, 
                              cantDiasFinSemana, 
                              cantDiasBancarios, 
                              out cantFaltasAplicadasEmpleado, 
                              out empleadoRegresaVacacionesFlag, 
                              out vacacaciones_SeAplicoDeduccionPorDiasAdelantados,  
                              out errorMessage);

                if (errorMessage != "")
                    return;


                cantidadFaltasAplicadas += cantFaltasAplicadasEmpleado; 

                if (empleadoRegresaVacacionesFlag)
                    cantidadEmpleadosRegresanVacaciones++; 


                // solo si la nómina es del tipo Quincenal, revisamos si debemos aplicar anticipo (1q) y deducción por anticipo (2q) 
                if (nominaHeader.Tipo == "Q" || nominaHeader.Tipo == "1Q" || nominaHeader.Tipo == "2Q")
                    if (!vacacaciones_SeAplicoDeduccionPorDiasAdelantados)
                        // si un empleado regresa de vacaciones *y* en esta nómina se aplica una deducción por días de sueldo pagados por anticipado, 
                        // no se aplica esta deducción, pues se considera que se puede corresponder a la anterior ... 
                        DeterminarYAplicarAnticiposDeSueldo(context, 
                                                            nominaHeader, 
                                                            parametrosNomina, 
                                                            empleado, 
                                                            Nomina_List, 
                                                            cantDiasHabiles, 
                                                            cantDiasFeriados, 
                                                            cantDiasFinSemana, 
                                                            cantDiasBancarios, 
                                                            out errorMessage);

                if (errorMessage != "")
                    return; 


                // aplicamos las cuotas de préstamo registradas cuotas de préstamo 
                int cantidadCuotasPrestamoAplicadasEmpleado = 0; 

                AgregarCuotasPrestamo(context,
                                      nominaHeader,
                                      parametrosNomina,
                                      empleado,
                                      Nomina_List,
                                      out cantidadCuotasPrestamoAplicadasEmpleado,
                                      out  errorMessage);

                if (errorMessage != "")
                    return;

                cantidadCuotasPrestamoAplicadas += cantidadCuotasPrestamoAplicadasEmpleado; 

                //  otros montos en rubros asignados 
                AgregarRubrosAsignados(context,
                                       nominaHeader,
                                       parametrosNomina,
                                       empleado,
                                       Nomina_List,
                                       out errorMessage);

                if (errorMessage != "")
                    return;

                // aplicamos las deducciones obligatorias, solo si el usuario lo indica en el registro de nómina (header) 
                if (nominaHeader.AgregarDeduccionesObligatorias) 
                    DeduccionesObligatorias(context,
                                            _mongoDataBase, 
                                            nominaHeader,
                                            empleado,
                                            parametroNominaSalarioMinimo,
                                            Nomina_List,
                                            out errorMessage); 

                // aplicar ISLR 
                ISLR(context,
                     nominaHeader,
                     empleado,
                     Nomina_List,
                     out errorMessage); 
            }

            funcionesGenericasNomina = null; 

            // eliminamos la nómina anterior ... 

            // nótese como obtenemos el objectcontext que corresponde al dbcontext ... 
            var objectContext = (context as IObjectContextAdapter).ObjectContext;

            string sqlTransactCommand = "Delete From tNomina Where HeaderID = {0}";
            objectContext.ExecuteStoreCommand(sqlTransactCommand, new object[] { nominaHeader.ID }); 

            // TODO: agregamos ésta a la tabla tNomina y grabamos los cambios ... 

            tNomina nominaRecord;

            foreach (NominaItem n in Nomina_List)
            {
                nominaRecord = new tNomina();

                nominaRecord.HeaderID = nominaHeader.ID;
                nominaRecord.Empleado = n.EmpleadoID;
                nominaRecord.Rubro = n.RubroID;
                nominaRecord.Tipo = n.Tipo;
                nominaRecord.Descripcion = n.Descripcion;
                nominaRecord.Monto = n.Monto;
                nominaRecord.MontoBase = n.Base;
                nominaRecord.CantDias = Convert.ToByte(n.CantDias);
                nominaRecord.Fraccion = n.Fraccion;
                nominaRecord.Detalles = n.Detalles; 
                nominaRecord.SalarioFlag = n.SalarioFlag;
                nominaRecord.SueldoFlag = n.SueldoFlag;

                context.tNominas.Add(nominaRecord); 
            }

            try
            {
                nominaHeader.FechaEjecucion = DateTime.Now; 
                context.SaveChanges();

                // preparamos el mensaje que el programa mostrará al usuario 

                resultadoEjecucionMessage = "Ok, el proceso de nómina fue ejecutado en forma exitosa.<br /><br /> " +
                    cantidadEmpleados.ToString() + " empleados corresponden al grupo de nómina y fueron procesados;<br /> " +
                    cantidadEmpleadosEnVacaciones.ToString() + " empleados están ahora de vacaciones y fueron obviados;<br /> " +
                    cantidadEmpleadosRegresanVacaciones.ToString() + " empleados regresan de vacaciones en esta nómina;<br /> " +
                    cantidadFaltasAplicadas.ToString() + " faltas de empleados fueron aplicadas;<br /> " +
                    cantidadCuotasPrestamoAplicadas.ToString() + " cuotas de préstamo se han aplicado en esta nómina."; 
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += "<br /><br />" + ex.InnerException.Message; 
            }
            finally
            {
                context = null; 
            }
        }

        private void EjecutarNominaVacaciones(dbNominaEntities context,
                                              IMongoDatabase mongoDataBase, 
                                              NominaASP.Models.tNominaHeader nominaHeader,
                                              ParametrosNomina parametrosNomina,
                                              Parametros_Nomina_SalarioMinimo parametroNominaSalarioMinimo,
                                              out string errorMessage,
                                              out string resultadoEjecucionMessage)
        {
            errorMessage = "";
            resultadoEjecucionMessage = "";

            List<NominaItem> Nomina_List = new List<NominaItem>();

            // el registro de vacaciones debe existir en la tabla Vacaciones 
            // jun/16: ahora las vacaciones están en mongo ... 

            // Vacacione vacacion = context.Vacaciones.Where(v => v.ClaveUnica == nominaHeader.ProvieneDe_ID).FirstOrDefault();

            //var queryARCEmpleado = Query.And(
            //                Query.EQ("Ano", ano),
            //                Query.EQ("Cia", ciaContabSeleccionada));

            var vacaciones = mongoDataBase.GetCollection<vacacion>("vacaciones");

            var builder = Builders<vacacion>.Filter;
            var filter = builder.Eq(x => x.claveUnicaContab, nominaHeader.ProvieneDe_ID);

            var vacacion = vacaciones.Find<vacacion>(filter).FirstOrDefault(); 

            if (vacacion == null)
            {
                errorMessage = "Error inesperado: Ud. está ejecutando una nómina de tipo Vacaciones; sin embargo, no hemos podido encontrar " + 
                    "el registro de vacaciones que corresponde a esta nómina. ";
                return;
            }

            // ambos grupos de nómina deben ser iguales ... 

            if (vacacion.grupoNomina != nominaHeader.GrupoNomina)
            {
                errorMessage = "Error inesperado: el grupo de nómina en el registro de nómina no es el mismo que el indicado en el registro de vacaciones.";
                return;
            }

            // si el registro de vacaciones indica que se debe aplicar el bono, debe haber un bono específicado 
            // aplicamos un monto de bono que pueda existir y mostramos la cantidad de días que corresponde según el registro 
            // de la nomina (vacacion.CantDiasPago_Bono y vacacion.MontoBono)

            //if (vacacion.BonoVacacionalFlag != null && vacacion.BonoVacacionalFlag.Value) 
            //    if (vacacion.MontoBono == null || vacacion.MontoBono.Value == 0)
            //    {
            //        errorMessage = "Error: aunque se indica en el registro de vacaciones que se debe aplicar el bono vacacional, " + 
            //            "no hay un monto registrado para el mismo.";
            //        return;
            //    }

            // validamos que el empleado corresponda al grupo de nómina en el registro de vacaciones (nota: ésto ya fue hecho al 
            // momento de registrar la vacación) 

            tdGruposEmpleado grupoNominaEmpleado = context.tdGruposEmpleados.Where(d => d.Grupo == vacacion.grupoNomina && d.Empleado == vacacion.empleado).
                                                                             FirstOrDefault();

            if (grupoNominaEmpleado == null)
            {
                errorMessage = "Error: el empleado indicado en el registro de vacaciones no existe en el grupo de nómina en el mismo registro.";
                return;
            }

            // muchas propiedades pueden venir en nulls y deben traer un valor ... 

            if (vacacion.periodoPagoDesde == null || vacacion.periodoPagoHasta == null || vacacion.sueldo == null)
            {
                errorMessage = "Error: algunos campos en el registro de vacaciones deben contener un valor y están vacíos.<br />" + 
                    "Por favor, revise el registro de vacaciones y asigne valores a todos los campos que lo requieran.";
                return;
            }

            // NOTESE COMO EN LO SUCESIVO, copiamos el código escrito para una nómina normal y lo usamos para la ejecución de esta nómina de vacaciones 

            // el grupo ya viene con el header 
            tGruposEmpleado grupoNomina = nominaHeader.tGruposEmpleado;

            if (grupoNomina == null)
            {
                errorMessage = "Error inesperado: no hemos podido leer el grupo de empleados que se ha indicado para este nómina ("
                    + nominaHeader.GrupoNomina.ToString() + "). ";
                return;
            }

            int cantidadCuotasPrestamoAplicadas = 0;
            NominaASP.Models.tEmpleado empleadoVacaciones = null;                   // recuérdese que, al menos por ahora, la nómina de vacaciones es para un solo empleado ... 

            Nomina_FuncionesGenericas funcionesGenericasNomina = new Nomina_FuncionesGenericas(); 

            foreach (NominaASP.Models.tEmpleado empleado in grupoNomina.tdGruposEmpleados.Where(e => !e.SuspendidoFlag && e.tEmpleado.Status == "A").
                                                                         Where(e => e.Empleado == vacacion.empleado).
                                                                         Select(e => e.tEmpleado))
            {
                // la función que sigue determina y graba un registro con el salario integral (y su descomposición) para el empleado ... 
                if (!funcionesGenericasNomina.DeterminarYRegistrarSalarioIntegralEmpleado(context, nominaHeader, empleado, out errorMessage))
                    return; 

                decimal sueldoEmpleado = 0;
                empleadoVacaciones = empleado; 

                AgregarSueldoEmpleadoVacaciones(context, nominaHeader, parametrosNomina, vacacion, empleado, Nomina_List, out sueldoEmpleado, out  errorMessage);

                if (errorMessage != "")
                    return;

                Vacaciones_AgregarBono(context,
                                       nominaHeader,
                                       parametrosNomina,
                                       vacacion,
                                       Nomina_List,
                                       out errorMessage); 

                if (errorMessage != "")
                    return;

                // TODO: aplicar cuotas de préstamo 

                int cantidadCuotasPrestamoAplicadasEmpleado = 0;

                AgregarCuotasPrestamo(context,
                                      nominaHeader,
                                      parametrosNomina,
                                      empleado,
                                      Nomina_List,
                                      out cantidadCuotasPrestamoAplicadasEmpleado,
                                      out  errorMessage);

                if (errorMessage != "")
                    return;

                cantidadCuotasPrestamoAplicadas += cantidadCuotasPrestamoAplicadasEmpleado;

                //  otros montos en rubros asignados 

                AgregarRubrosAsignados(context,
                                       nominaHeader,
                                       parametrosNomina,
                                       empleado,
                                       Nomina_List,
                                       out errorMessage);

                if (errorMessage != "")
                    return;


                // TODO: aplicar deducciones obligatorias 

                if (vacacion.aplicarDeduccionesFlag.HasValue && vacacion.aplicarDeduccionesFlag.Value)
                    DeduccionesObligatorias(context,
                                            mongoDataBase, 
                                            nominaHeader,
                                            empleado,
                                            parametroNominaSalarioMinimo,
                                            Nomina_List,
                                            out errorMessage);

                // aplicar ISLR 

                ISLR(context,
                     nominaHeader,
                     empleado,
                     Nomina_List,
                     out errorMessage);
            }

            funcionesGenericasNomina = null; 

            // eliminamos la nómina anterior ... 

            string sqlTransactCommand = "Delete From tNomina Where HeaderID = {0}";

            // nótese como obtenemos el objectcontext que corresponde al dbcontext ... 
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.ExecuteStoreCommand(sqlTransactCommand, new object[] { nominaHeader.ID });

            // TODO: agregamos ésta a la tabla tNomina y grabamos los cambios ... 

            tNomina nominaRecord;

            foreach (NominaItem n in Nomina_List)
            {
                nominaRecord = new tNomina();

                nominaRecord.HeaderID = nominaHeader.ID;
                nominaRecord.Empleado = n.EmpleadoID;
                nominaRecord.Rubro = n.RubroID;
                nominaRecord.Tipo = n.Tipo;
                nominaRecord.Descripcion = n.Descripcion;
                nominaRecord.Monto = n.Monto;
                nominaRecord.MontoBase = n.Base;
                nominaRecord.CantDias = Convert.ToByte(n.CantDias);
                nominaRecord.Fraccion = n.Fraccion;
                nominaRecord.Detalles = n.Detalles; 
                nominaRecord.SalarioFlag = n.SalarioFlag;
                nominaRecord.SueldoFlag = n.SueldoFlag;

                context.tNominas.Add(nominaRecord);
            }

            try
            {
                nominaHeader.FechaEjecucion = DateTime.Now;
                context.SaveChanges();

                // preparamos el mensaje que el programa mostrará al usuario 

                resultadoEjecucionMessage = "Ok, el proceso de nómina fue ejecutado en forma exitosa.<br /> " +
                    "La nómina de vacaciones para " + empleadoVacaciones.Nombre + " ha sido registrada.<br /><br />" + 
                    cantidadCuotasPrestamoAplicadas.ToString() + " cuotas de préstamo se han aplicado en esta nómina.";
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += "<br /><br />" + ex.InnerException.Message;
            }
            finally
            {
                context = null;
            }
        }





        private void EjecutarNominaUtilidades(dbNominaEntities context,
                                              IMongoDatabase mongoDatabase, 
                                              tNominaHeader nominaHeader,
                                              ParametrosNomina parametrosNomina,
                                              Parametros_Nomina_SalarioMinimo parametroNominaSalarioMinimo, 
                                              Utilidade utilidades, 
                                              int rubroUtilidades, 
                                              int rubroInce, 
                                              out string errorMessage,
                                              out string resultadoEjecucionMessage)
        {
            errorMessage = "";
            resultadoEjecucionMessage = "";

            List<NominaItem> Nomina_List = new List<NominaItem>();

            // leemos los empleados del grupo de nómina; vamos a ejecutar la nómina para cada uno de ellos ... 

            // el grupo ya viene con el header 
            tGruposEmpleado grupoNomina = nominaHeader.tGruposEmpleado;

            if (grupoNomina == null)
            {
                errorMessage = "Error inesperado: no hemos podido leer el grupo de empleados que se ha indicado para este nómina ("
                    + nominaHeader.GrupoNomina.ToString() + "). ";
                return;
            }

            // al menos por ahora, incluímos en esta nómina todos los empleados del grupo, aunque estén en vacaciones ... 

            //int cantidadEmpleadosEnVacaciones = 0;
            int cantidadCuotasPrestamoAplicadas = 0;
            int cantidadEmpleados = 0;

            Nomina_FuncionesGenericas funcionesGenericasNomina = new Nomina_FuncionesGenericas(); 

            foreach (NominaASP.Models.tEmpleado empleado in grupoNomina.tdGruposEmpleados.Where(e => !e.SuspendidoFlag && e.tEmpleado.Status == "A").Select(e => e.tEmpleado))
            {
                cantidadEmpleados++;

                // la función que sigue determina y graba un registro con el salario integral (y su descomposición) para el empleado ... 
                if (!funcionesGenericasNomina.DeterminarYRegistrarSalarioIntegralEmpleado(context, nominaHeader, empleado, out errorMessage))
                    return; 

                // omitir si está en vacaciones 
                //short cantidadDiasVacaciones;
                //if (EmpleadoEnVacaciones(context, nominaHeader, empleado, out cantidadDiasVacaciones))
                //{
                //    cantidadEmpleadosEnVacaciones++;
                //    continue;
                //}

                // TODO: aplicar salario y faltas 
                // nótese como aquí vamos a agregar días de sueldo, pero también deducir días de faltas y vacaciones (cuando existan) ... 

                //AgregarSueldo(context, nominaHeader, parametrosNomina, empleado, cantidadDiasVacaciones, Nomina_List, out cantFaltasAplicadasEmpleado, out  errorMessage);

                // con esta función, determinamos el monto 'base' de salario o sueldo para el calculo de las utilidades; normalmente, esta 
                // base corresponde al salario ganado por el empleado en el período base de aplicación de las utilidades 

                Nomina_FuncionesGenericas funcionesNomina = new Nomina_FuncionesGenericas();

                string sueldoSalarioFlag = utilidades.BaseAplicacion == 1 ? "Sueldo" : "Salario";

                decimal montoBaseUtilidades = 0; 

                if (!funcionesNomina.DeterminarMontoSueldoSalarioNominasRelacionadas(context,
                                                                                     nominaHeader,
                                                                                     empleado.Empleado,
                                                                                     sueldoSalarioFlag,
                                                                                     out montoBaseUtilidades,
                                                                                     out errorMessage))
                {
                    return;
                }

                decimal montoUtilidades = Math.Round((montoBaseUtilidades / utilidades.CantidadDiasPeriodoPago) * utilidades.CantidadDiasUtilidades, 2);
                decimal montoInce = utilidades.AplicarInce ? Math.Round(montoUtilidades * utilidades.IncePorc, 2) : 0;
                montoInce *= -1; 

                NominaItem nominaItem;

                if (montoUtilidades != 0)
                {
                    nominaItem = new NominaItem()
                    {
                        EmpleadoID = empleado.Empleado,
                        RubroID = rubroUtilidades,
                        Tipo = "A",
                        Descripcion = "Utilidades correspondientes al período " +
                                      nominaHeader.Desde.Value.ToString("d-MM-yy") +
                                      " a " +
                                      nominaHeader.Hasta.Value.ToString("d-MM-yy") + ".",
                        Monto = Math.Round(montoUtilidades, 2),       // cantdias nunca es cero; lo validamos antes ... 
                        Base = montoBaseUtilidades,
                        CantDias = utilidades.CantidadDiasUtilidades,
                        Fraccion = Math.Round((Convert.ToDecimal(utilidades.CantidadDiasUtilidades) / Convert.ToDecimal(utilidades.CantidadDiasPeriodoPago)), 3),
                        SueldoFlag = false,
                        SalarioFlag = false             // nótese que no marcamos el rubro como salario; de otra forma, puede aplicarse en otros cálculos de 
                                                        // nóminas del mes, etc. Las utilidades luego no deben intervenir en deducciones legales, etc. 
                    };

                    Nomina_List.Add(nominaItem);
                }


                if (montoInce != 0)
                {
                    nominaItem = new NominaItem()
                    {
                        EmpleadoID = empleado.Empleado,
                        RubroID = rubroInce,
                        Tipo = "D",
                        Descripcion = "Deducción por aporte Ince.",
                        Monto = Math.Round(montoInce, 2),
                        Base = Math.Round(montoUtilidades, 2),
                        CantDias = 0,
                        Fraccion = utilidades.IncePorc * 100,                       // para convertir .01 a 1% 
                        SueldoFlag = false,
                        SalarioFlag = false
                    };

                    Nomina_List.Add(nominaItem);
                }


                // TODO: aplicar cuotas de préstamo 

                int cantidadCuotasPrestamoAplicadasEmpleado = 0;

                AgregarCuotasPrestamo(context,
                                      nominaHeader,
                                      parametrosNomina,
                                      empleado,
                                      Nomina_List,
                                      out cantidadCuotasPrestamoAplicadasEmpleado,
                                      out  errorMessage);

                if (errorMessage != "")
                    return;

                cantidadCuotasPrestamoAplicadas += cantidadCuotasPrestamoAplicadasEmpleado;

                //  otros montos en rubros asignados 

                AgregarRubrosAsignados(context,
                                       nominaHeader,
                                       parametrosNomina,
                                       empleado,
                                       Nomina_List,
                                       out errorMessage);

                if (errorMessage != "")
                    return;

                // TODO: aplicar deducciones obligatorias 

                // nota: en una nómina de utilidades, es improbable que el usuario desee aplicar deducciones obligatorias (sso, pf, lph, etc.), pues 
                // las utilidades no generan estas deducciones; sin embargo, basta con que el usuario ponga en tipo de nómina 'U', para que estas 
                // deducciones sean aplicadas ... 

                if (nominaHeader.AgregarDeduccionesObligatorias)
                    DeduccionesObligatorias(context,
                                            mongoDatabase, 
                                            nominaHeader,
                                            empleado,
                                            parametroNominaSalarioMinimo,
                                            Nomina_List,
                                            out errorMessage);

                // aplicar ISLR 

                // nótese como se indica al cálculo del islr que la nómina es del tipo Utilidades; para este tipo de nómina 
                // esta función lee el monto de utilidades, *aunque* no esté marcado como salario o sueldo ... 

                ISLR(context,
                     nominaHeader,
                     empleado,
                     Nomina_List,
                     out errorMessage, 
                     true, 
                     rubroUtilidades);

                if (!string.IsNullOrEmpty(errorMessage))
                    return; 
            }

            funcionesGenericasNomina = null; 

            // eliminamos la nómina anterior ... 

            string sqlTransactCommand = "Delete From tNomina Where HeaderID = {0}";

            // nótese como obtenemos el objectcontext que corresponde al dbcontext ... 
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.ExecuteStoreCommand(sqlTransactCommand, new object[] { nominaHeader.ID });

            // TODO: agregamos ésta a la tabla tNomina y grabamos los cambios ... 

            tNomina nominaRecord;

            foreach (NominaItem n in Nomina_List)
            {
                nominaRecord = new tNomina();

                nominaRecord.HeaderID = nominaHeader.ID;
                nominaRecord.Empleado = n.EmpleadoID;
                nominaRecord.Rubro = n.RubroID;
                nominaRecord.Tipo = n.Tipo;
                nominaRecord.Descripcion = n.Descripcion;
                nominaRecord.Monto = n.Monto;
                nominaRecord.MontoBase = n.Base;
                nominaRecord.CantDias = Convert.ToByte(n.CantDias);
                nominaRecord.Fraccion = n.Fraccion;
                nominaRecord.Detalles = n.Detalles; 
                nominaRecord.SalarioFlag = n.SalarioFlag;
                nominaRecord.SueldoFlag = n.SueldoFlag;

                context.tNominas.Add(nominaRecord);
            }

            try
            {
                nominaHeader.FechaEjecucion = DateTime.Now;
                context.SaveChanges();

                // preparamos el mensaje que el programa mostrará al usuario 

                resultadoEjecucionMessage = "Ok, el proceso de nómina fue ejecutado en forma exitosa.<br /><br /> " +
                    cantidadEmpleados.ToString() + " empleados corresponden al grupo de nómina y fueron procesados;<br /> " +
                    cantidadCuotasPrestamoAplicadas.ToString() + " cuotas de préstamo se han aplicado en esta nómina.";
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += "<br /><br />" + ex.InnerException.Message;
            }
            finally
            {
                context = null;
            }
        }


        private bool EmpleadoEnVacaciones(dbNominaEntities context,             // ***VACACIONES***
                                          IMongoDatabase mongoDataBase, 
                                          tNominaHeader nominaHeader, 
                                          tEmpleado empleado, 
                                          out string errorMessage) 
        {
            // determinamos si un empleado está de vacaciones en el período de la nómina 
            errorMessage = "";

            var vacacionMongoCollection = mongoDataBase.GetCollection<vacacion>("vacaciones");

            var vacaciones = mongoDataBase.GetCollection<vacacion>("vacaciones");

            var builder = Builders<vacacion>.Filter;
            var filter = builder.And(
                builder.Eq(x => x.empleado, empleado.Empleado),
                builder.Eq(x => x.obviarEnLaNominaFlag, true),
                builder.Lte(x => x.desactivarNominaDesde, nominaHeader.FechaNomina.ToUniversalTime()),
                builder.Gte(x => x.desactivarNominaHasta, nominaHeader.FechaNomina.ToUniversalTime()),
                builder.Eq(x => x.cia, empleado.Cia)
                ); 

            var vacacion = vacaciones.Find<vacacion>(filter).FirstOrDefault();

            if (vacacion != null)
                return true; 

            return false; 
        }

        public void AgregarSueldo(dbNominaEntities context,
                                   tNominaHeader nominaHeader,
                                   ParametrosNomina parametrosNomina,
                                   tEmpleado empleado,
                                   List<NominaItem> Nomina_List,
                                   int cantDiasHabiles,
                                   int cantDiasFeriados,
                                   int cantDiasFinSemana,
                                   int cantDiasBancarios,
                                   out int cantidadFaltasAplicadas,
                                   out bool empleadoRegresaVacacionesFlag,
                                   out bool vacacaciones_SeAplicoDeduccionPorDiasAdelantados, 
                                   out string errorMessage)
        {
            // leemos y aplicamos el sueldo del empleado, si se indica así en nominaHeader; 
            // además, aplicamos una deducción por la cantidad de días de vacaciones ... nota: ésto ocurre cuando el empleado regresa de vacaciones 
            // y se le adelantó alguna cantidad de días de esta nómina, se debe aplicar esta deducción en esta nómina ... 

            errorMessage = "";
            cantidadFaltasAplicadas = 0;
            empleadoRegresaVacacionesFlag = false;
            vacacaciones_SeAplicoDeduccionPorDiasAdelantados = false; 

            DateTime periodoPagoDesde = nominaHeader.Desde.Value;
            DateTime periodoPagoHasta = nominaHeader.Hasta.Value;

            Nomina_FuncionesGenericas funcionesGenericasNomina = new Nomina_FuncionesGenericas();

            NominaItem nominaItem;

            // buscamos el sueldo más reciente para el período de pago 
            // nótese que lo leemos y determinamos aunque no lo agreguemos, para usarlo en registro de faltas y vacaciones ... 

            decimal sueldoEmpleado;
            sueldoEmpleado = DeterminarSueldoEmpleado(context, empleado.Empleado, periodoPagoDesde); 

            int rubroSueldoEmpleados;

            if (!funcionesGenericasNomina.ObtenerRubro(context, 1, out rubroSueldoEmpleados))
            {
                errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde " +
                                "al sueldo básico de los empleados." +
                                "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                return;
            }

            if (nominaHeader.AgregarSueldo)
            {
                // agregamos un item a la nómina (lista) con el sueldo del empleado 
                // feb/2017: separamos el sueldo del empleado en la cantidad de 'tipos' de días: hábiles, feriados, sab/dom y bancarios ... 

                int cantidadDiasTotales = cantDiasHabiles + cantDiasBancarios + cantDiasFeriados + cantDiasFinSemana; 

                string descripcionRubroSbas = string.IsNullOrEmpty(nominaHeader.DescripcionRubroSueldo) ? "Sueldo básico" : nominaHeader.DescripcionRubroSueldo; 

                nominaItem = new NominaItem()
                {
                    EmpleadoID = empleado.Empleado,
                    RubroID = rubroSueldoEmpleados,
                    Tipo = "A",
                    Descripcion = descripcionRubroSbas + " - (" + cantDiasHabiles.ToString() + " días hábiles)",
                    Monto = Math.Round(sueldoEmpleado / cantidadDiasTotales * cantDiasHabiles, 2),       
                    Base = sueldoEmpleado,
                    CantDias = Convert.ToInt16(cantDiasHabiles),
                    SueldoFlag = true,
                    SalarioFlag = false
                };
                Nomina_List.Add(nominaItem);

                nominaItem = new NominaItem()
                {
                    EmpleadoID = empleado.Empleado,
                    RubroID = rubroSueldoEmpleados,
                    Tipo = "A",
                    Descripcion = descripcionRubroSbas + " - (" + cantDiasFinSemana.ToString() + " días fines semana)",
                    Monto = Math.Round(sueldoEmpleado / cantidadDiasTotales * cantDiasFinSemana, 2),       
                    Base = sueldoEmpleado,
                    CantDias = Convert.ToInt16(cantDiasFinSemana),
                    SueldoFlag = true,
                    SalarioFlag = false
                };
                Nomina_List.Add(nominaItem);

                nominaItem = new NominaItem()
                {
                    EmpleadoID = empleado.Empleado,
                    RubroID = rubroSueldoEmpleados,
                    Tipo = "A",
                    Descripcion = descripcionRubroSbas + " - (" + cantDiasFeriados.ToString() + " días feriados)",
                    Monto = Math.Round(sueldoEmpleado / cantidadDiasTotales * cantDiasFeriados, 2),       
                    Base = sueldoEmpleado,
                    CantDias = Convert.ToInt16(cantDiasFeriados),
                    SueldoFlag = true,
                    SalarioFlag = false
                };
                Nomina_List.Add(nominaItem);

                // los días bancarios los mostramos solo si existen 
                if (cantDiasBancarios > 0)
                {
                    nominaItem = new NominaItem()
                    {
                        EmpleadoID = empleado.Empleado,
                        RubroID = rubroSueldoEmpleados,
                        Tipo = "A",
                        Descripcion = descripcionRubroSbas + " - (" + cantDiasBancarios.ToString() + " días bancarios)",
                        Monto = Math.Round(sueldoEmpleado / cantidadDiasTotales * cantDiasBancarios, 2),       
                        Base = sueldoEmpleado,
                        CantDias = Convert.ToInt16(cantDiasBancarios),
                        SueldoFlag = true,
                        SalarioFlag = false
                    };
                    Nomina_List.Add(nominaItem);
                }
            }


            // ---------------------------------------------------------------------------------------------------------------------
            // FALTAS: 

            // si el empleado tiene faltas registradas en el período de pago, las agregamos a la nómina 

            // si la nómina corresonde a la 2da. quincena, ajustamos el inicio del período; recuérdese que el período de una 2q
            // para nóminas con anticipo es todo el mes !!! 

            DateTime faltaDesdePeriodo = periodoPagoDesde;
            DateTime faltaHastaPeriodo = periodoPagoHasta;

            if (nominaHeader.Tipo == "2Q")
                if (periodoPagoDesde.Day < 15)
                    faltaDesdePeriodo = new DateTime(periodoPagoDesde.Year, periodoPagoDesde.Month, 16);

            var query = context.Empleados_Faltas.
                Where(f => f.Empleado == empleado.Empleado && f.Descontar).
                Where(f => f.Descontar_FechaNomina != null && f.Descontar_GrupoNomina != null).
                Where(f => f.Descontar_FechaNomina == nominaHeader.FechaNomina && f.Descontar_GrupoNomina == nominaHeader.GrupoNomina).           
                OrderBy(f => f.Desde);

            foreach (Empleados_Faltas falta in query)
            {
                if (falta.CantHoras != null)
                {
                    // nótese como, aunque la falta viene con una cantidad de dáis hábiles calculada, la calculamos nuevamente, pues el período 
                    // puede no estar todo dentro del período de pago ... 

                    decimal montoFalta = sueldoEmpleado / 30;         // obtenemos el monto de un día de sueldo 
                    montoFalta = montoFalta / 8;                            // obtenemos el monto de una hora de sueldo 
                    montoFalta = montoFalta * falta.CantHoras.Value;        // multiplicamos por la cantidad de horas 

                    nominaItem = new NominaItem()
                    {
                        EmpleadoID = empleado.Empleado,
                        RubroID = rubroSueldoEmpleados,
                        Tipo = "D",
                        Descripcion = !string.IsNullOrEmpty(falta.DescripcionRubroNomina) ? falta.DescripcionRubroNomina :
                                      ("Deducción por falta, ocurrida en el período de pago, desde " +
                                      falta.Desde.ToString("d-MMM-yy") + " a " +
                                      falta.Hasta.ToString("d-MMM-yy") + " ("
                                      + falta.CantHoras.Value + " horas)"),
                        Monto = Math.Round((montoFalta) * -1, 2),
                        Base = sueldoEmpleado,
                        CantDias = 0, 
                        SueldoFlag = !string.IsNullOrEmpty(falta.Base) ? (falta.Base == "Sueldo" ? true : false) : false,
                        SalarioFlag = !string.IsNullOrEmpty(falta.Base) ? (falta.Base == "Salario" ? true : false) : false
                    };

                    Nomina_List.Add(nominaItem);
                    cantidadFaltasAplicadas++;
                }
                else
                {
                    nominaItem = new NominaItem()
                    {
                        EmpleadoID = empleado.Empleado,
                        RubroID = rubroSueldoEmpleados,
                        Tipo = "D",
                        Descripcion = !string.IsNullOrEmpty(falta.DescripcionRubroNomina) ? falta.DescripcionRubroNomina :
                                      ("Deducción por falta, ocurrida en el período de pago, desde " +
                                      falta.Desde.ToString("d-MMM-yy") + " a " +
                                      falta.Hasta.ToString("d-MMM-yy") + " ("
                                      + falta.CantDiasHabiles + " días)"),
                        Monto = Math.Round((sueldoEmpleado / 30 * falta.CantDiasHabiles) * -1, 2),
                        Base = sueldoEmpleado,
                        CantDias = falta.CantDiasHabiles,
                        SueldoFlag = !string.IsNullOrEmpty(falta.Base) ? (falta.Base == "Sueldo" ? true : false) : false,
                        SalarioFlag = !string.IsNullOrEmpty(falta.Base) ? (falta.Base == "Salario" ? true : false) : false
                    };

                    Nomina_List.Add(nominaItem);
                    cantidadFaltasAplicadas++;
                }
            }

            // regreso de vacaciones: si un empleado regresa de vacaciones, puede (o no) aplicarse un descuento por días de sueldo anticipados en 
            // esas vacaciones ... 
            NominaASP.Models.MongoDB.vacacion vacacion = null;          // ***VACACIONES***
          
            if (EmpleadoRegresaDeVacaciones(context,
                                            _mongoDataBase, 
                                            nominaHeader,
                                            empleado,
                                            out vacacion, 
                                            out errorMessage))
            {
                empleadoRegresaVacacionesFlag = true;

                if (vacacion.proximaNomina_AplicarDeduccionPorAnticipo != null &&
                    vacacion.proximaNomina_AplicarDeduccionPorAnticipo.Value &&
                    vacacion.proximaNomina_AplicarDeduccionPorAnticipo_CantDias != null &&
                    vacacion.proximaNomina_AplicarDeduccionPorAnticipo_CantDias.Value > 0)
                {
                    nominaItem = new NominaItem()
                    {
                        EmpleadoID = empleado.Empleado,
                        RubroID = rubroSueldoEmpleados,
                        Tipo = "D",
                        Descripcion = "Días de sueldo adelantados en período de vacaciones (" +
                                      vacacion.proximaNomina_AplicarDeduccionPorAnticipo_CantDias.Value.ToString() + " días)",
                        Monto = Math.Round((sueldoEmpleado / 30 * vacacion.proximaNomina_AplicarDeduccionPorAnticipo_CantDias.Value) * -1, 2),
                        Base = sueldoEmpleado,
                        CantDias = Convert.ToInt16(vacacion.proximaNomina_AplicarDeduccionPorAnticipo_CantDias.Value),
                        SueldoFlag = true,
                        SalarioFlag = false
                    };

                    Nomina_List.Add(nominaItem);

                    // para luego saber que se aplicó esta deducción por días pagados por enticipado en el pago de las vacaciones ... 
                    vacacaciones_SeAplicoDeduccionPorDiasAdelantados = true;          
                }
            }
            // ---------------------------------------------------------------------------------------------------------------------
        }

        public decimal DeterminarSueldoEmpleado(dbNominaEntities context, int empleadoID, DateTime fecha)
        {
            decimal? sueldoEmpleado = context.Empleados_Sueldo.Where(s => s.Empleado == empleadoID && s.Desde <= fecha).
                                                               OrderByDescending(s => s.Desde).
                                                               Select(s => s.Sueldo).
                                                               FirstOrDefault();

            return sueldoEmpleado != null ? sueldoEmpleado.Value : 0; 
        }

        // ***VACACIONES***
        private bool EmpleadoRegresaDeVacaciones(dbNominaEntities context,
                                                 IMongoDatabase mongoDataBase, 
                                                 tNominaHeader nominaHeader,
                                                 tEmpleado empleado,
                                                 out NominaASP.Models.MongoDB.vacacion vacacion, 
                                                 out string errorMessage)
        {
            errorMessage = "";
            vacacion = null; 

            // si un empleado regresa de vacaciones, debemos aplicar un descuento (si corresponde) por días de sueldo adelantado en vacaciones ... 
            // además, en la nómina de vacaciones se indica la cantidad de días que se debe aplicar para el cálculo de las deducciones legales ... 

            var vacacionesMongoCollection = mongoDataBase.GetCollection<vacacion>("vacaciones");

            var builder = Builders<vacacion>.Filter;
            var filter = builder.And(
                builder.Eq(x => x.grupoNomina, nominaHeader.tGruposEmpleado.Grupo),
                builder.Eq(x => x.empleado, empleado.Empleado),
                builder.Eq(x => x.proximaNomina_FechaNomina, nominaHeader.FechaNomina)
                );

            vacacion = vacacionesMongoCollection.Find<vacacion>(filter).FirstOrDefault();

            if (vacacion == null)
                return false;

            return true; 
        }

        private void Vacaciones_AgregarBono(dbNominaEntities context,
                                            tNominaHeader nominaHeader,
                                            ParametrosNomina parametrosNomina,
                                            NominaASP.Models.MongoDB.vacacion vacacion,
                                            List<NominaItem> Nomina_List,
                                            out string errorMessage)
        {
            errorMessage = "";
            Nomina_FuncionesGenericas funcionesGenericasNomina = new Nomina_FuncionesGenericas();

            if (!(vacacion.montoBono != null && vacacion.montoBono.Value != 0))
                return;

            int rubro;

            if (!funcionesGenericasNomina.ObtenerRubro(context, 4, out rubro))
            {
                errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde " +
                                "a la asignación por bono vacacional." +
                                "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                return;
            }

            // la función anterior, regresa solo el id del rubro; leemos el rubro que corresponde al bono vacacional, para poder 
            // saber si correponde a: sueldo, salario, ambos 

            tMaestraRubro rubroBonoVacacional = context.tMaestraRubros.Where(r => r.Rubro == rubro).First(); 

            // NOTA: aunque podríamos validar que el monto de bono corresponda a la cantidad de días de sueldo que se indica, 
            // no lo hacemos; en un principio, pasamos a esta función el monto de sueldo del empleado para hacerlo; claro que, además, 
            // el sueldo básico es necesario para ponerlo como 'base' del rubro ... 

            NominaItem nominaItem;
            nominaItem = new NominaItem()
            {
                EmpleadoID = vacacion.empleado,
                RubroID = rubro,
                Tipo = "A",
                Descripcion = "Bono vacacional (" + vacacion.cantDiasPago_Bono.Value.ToString() + " días)",
                Monto = Math.Round(vacacion.montoBono.Value, 2),      
                Base = vacacion.sueldo.Value,
                CantDias = Convert.ToInt16(vacacion.cantDiasPago_Bono.Value),
                SueldoFlag = (rubroBonoVacacional.SueldoFlag.HasValue ? rubroBonoVacacional.SueldoFlag.Value : false),
                SalarioFlag = (rubroBonoVacacional.SalarioFlag.HasValue ? rubroBonoVacacional.SalarioFlag.Value : false)
            };

            Nomina_List.Add(nominaItem);
        }

        private void AgregarSueldoEmpleadoVacaciones(dbNominaEntities context,
                                                     tNominaHeader nominaHeader,
                                                     ParametrosNomina parametrosNomina,
                                                     NominaASP.Models.MongoDB.vacacion vacacion, 
                                                     tEmpleado empleado,
                                                     List<NominaItem> Nomina_List,
                                                     out decimal sueldoEmpleado, 
                                                     out string errorMessage)
        {
            // en el caso de las vacaciones, aplicar el sueldo es muy fácil, pues viene en el registro de vacaciones ya descompuesto en días: 
            // ya devengados, feriados, hábiles

            Nomina_FuncionesGenericas funcionesGenericasNomina = new Nomina_FuncionesGenericas(); 

            errorMessage = "";
            sueldoEmpleado = 0; 

            int rubroSueldoEmpleados;

            if (!funcionesGenericasNomina.ObtenerRubro(context, 1, out rubroSueldoEmpleados))
            {
                errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde " +
                                "al sueldo básico de los empleados." +
                                "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                return;
            }

            DateTime periodoPagoDesde = vacacion.periodoPagoDesde.Value;
            DateTime periodoPagoHasta = vacacion.periodoPagoHasta.Value;

            NominaItem nominaItem;

            sueldoEmpleado = vacacion.sueldo.Value;

            int cantidadDiasYaDevengados = vacacion.cantDiasPago_YaTrabajados != null ? vacacion.cantDiasPago_YaTrabajados.Value : 0;
            int cantidadDiasHabiles = vacacion.cantDiasPago_Habiles != null ? vacacion.cantDiasPago_Habiles.Value : 0;
            int cantidadDiasFinesSemana = vacacion.cantDiasPago_SabDom != null ? vacacion.cantDiasPago_SabDom.Value : 0;
            int cantidadDiasFeriados = vacacion.cantDiasPago_Feriados != null ? vacacion.cantDiasPago_Feriados.Value : 0; 

            // días devengados pendientes de pago; éstos son los días que ocurren antes del inicio de vacaciones; 
            // el empleado los trabajó y están pendientes ... 

            nominaItem = new NominaItem()
            {
                EmpleadoID = empleado.Empleado,
                RubroID = rubroSueldoEmpleados,
                Tipo = "A",
                Descripcion = "Días de sueldo ya devengado y pendiente - (" + cantidadDiasYaDevengados.ToString() + " días)",
                Monto = Math.Round(sueldoEmpleado / 30 * cantidadDiasYaDevengados, 2),       
                Base = sueldoEmpleado,
                CantDias = Convert.ToInt16(cantidadDiasYaDevengados),
                SueldoFlag = true,
                SalarioFlag = false
            };

            Nomina_List.Add(nominaItem);

            nominaItem = new NominaItem()
            {
                EmpleadoID = empleado.Empleado,
                RubroID = rubroSueldoEmpleados,
                Tipo = "A",
                Descripcion = "Período de vacaciones - Días hábiles - (" + cantidadDiasHabiles.ToString() + " días)",
                Monto = Math.Round(sueldoEmpleado / 30 * cantidadDiasHabiles, 2),       // cantdias nunca es cero; lo validamos antes ... 
                Base = sueldoEmpleado,
                CantDias = Convert.ToInt16(cantidadDiasHabiles),
                SueldoFlag = true,
                SalarioFlag = false
            };

            Nomina_List.Add(nominaItem);

            nominaItem = new NominaItem()
            {
                EmpleadoID = empleado.Empleado,
                RubroID = rubroSueldoEmpleados,
                Tipo = "A",
                Descripcion = "Período de vacaciones - Sábados y domingos - (" + cantidadDiasFinesSemana.ToString() + " días)",
                Monto = Math.Round(sueldoEmpleado / 30 * cantidadDiasFinesSemana, 2),       // cantdias nunca es cero; lo validamos antes ... 
                Base = sueldoEmpleado,
                CantDias = Convert.ToInt16(cantidadDiasFinesSemana),
                SueldoFlag = true,
                SalarioFlag = false
            };

            Nomina_List.Add(nominaItem);

            nominaItem = new NominaItem()
            {
                EmpleadoID = empleado.Empleado,
                RubroID = rubroSueldoEmpleados,
                Tipo = "A",
                Descripcion = "Período de vacaciones - Feriados - (" + cantidadDiasFeriados.ToString() + " días)",
                Monto = Math.Round(sueldoEmpleado / 30 * cantidadDiasFeriados, 2),       // cantdias nunca es cero; lo validamos antes ... 
                Base = sueldoEmpleado,
                CantDias = Convert.ToInt16(cantidadDiasFeriados),
                SueldoFlag = true,
                SalarioFlag = false
            };

            Nomina_List.Add(nominaItem);
        }


        private void AgregarCuotasPrestamo(dbNominaEntities context,
                                           tNominaHeader nominaHeader,
                                           ParametrosNomina parametrosNomina,
                                           tEmpleado empleado, 
                                           List<NominaItem> Nomina_List,
                                           out int cantidadCuotasPrestamoAplicadas,
                                           out string errorMessage)
        {
            // leemos y aplicamos a la nómina las cuotas de préstamo que puedan ser adecuadas ... 

            errorMessage = "";
            cantidadCuotasPrestamoAplicadas = 0; 

            DateTime periodoPagoDesde = nominaHeader.Desde.Value;
            DateTime periodoPagoHasta = nominaHeader.Hasta.Value;

            Nomina_FuncionesGenericas funcionesGenericasNomina = new Nomina_FuncionesGenericas();

            NominaItem nominaItem;

            // leemos las cuotas de préstamo que puedan existir para el empleado y el período de nómina ... 
            // IMPORTANTE: las cuotas de préstamo deben ser registradas de acuerdo al tipo de nómina en la cual se 
            // desea aplicar; por ejemplo: en nóminas de vacaciones, las cuotas de préstamo que se deseen aplicar deben 
            // tener tipo de nómina V; igual para Utilidades, etc. 

            var query = context.tCuotasPrestamos.Include("tPrestamo").
                                                    Where(p => p.tPrestamo.Empleado == empleado.Empleado).
                                                    Where(p => p.FechaCuota >= periodoPagoDesde && p.FechaCuota <= periodoPagoHasta).
                                                    Where(p => p.PagarPorNominaFlag != null && p.PagarPorNominaFlag.Value).
                                                    Where(p => (p.TipoNomina.Contains(nominaHeader.Tipo)) || (p.TipoNomina.Contains("Q") && nominaHeader.Tipo.Contains("Q"))).
                                                    Where(p => p.tPrestamo.Situacion != "SO" && p.tPrestamo.Situacion != "AN");

            foreach (tCuotasPrestamo cuota in query)
            {
                // si el tipo de la nómina ejecutada es quincenal, nos aseguramos que la cuota corresponda a la quincena específica (1ra o 2da); 
                // la razón de ésto es que el período de la nómina de una 2da quincena es, normalmente, todo el mes ... además, el tipo de la cuota 
                // (a diferencia de la nómina) no es 1Q o 2Q en forma específica, sino Q ... 

                if (nominaHeader.Tipo == "1Q")
                    if (cuota.FechaCuota.Day > 15)
                        continue;

                if (nominaHeader.Tipo == "2Q")
                    if (cuota.FechaCuota.Day <= 15)
                        continue; 
                

                decimal montoIntereses = cuota.MontoIntereses != null ? cuota.MontoIntereses.Value : 0;
                decimal montoCuota = cuota.Monto;

                // agregamos un item a la nómina (lista) 

                int rubro;

                if (!funcionesGenericasNomina.ObtenerRubro(context, 2, out rubro))
                {
                    errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde " +
                                    "a la deducción por aplicación de cuotas de préstamo." +
                                    "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                    return;
                }

                // ahora actualizamos la cuota y el prestamo 

                cuota.Cancelada = true;

                decimal montoTotalPagado = 0;
                decimal? montoTotalPagadoIntereses = 0;

                tPrestamo prestamo = cuota.tPrestamo;

                // leemos y sumarizamos las cuotas *anteriores y canceladas* a la cuota actual ... 
                int cantidadCuotas = prestamo.tCuotasPrestamos.Count();
                int numeroCuotaActual = prestamo.tCuotasPrestamos.Where(c => c.FechaCuota <= cuota.FechaCuota).Count();

                montoTotalPagado = prestamo.tCuotasPrestamos.Where(c => c.FechaCuota < cuota.FechaCuota && (c.Cancelada != null && c.Cancelada.Value)).Sum(c => c.Monto);
                montoTotalPagadoIntereses = prestamo.tCuotasPrestamos.Where(c => c.FechaCuota <= cuota.FechaCuota && (c.Cancelada != null && c.Cancelada.Value)).Sum(c => (decimal?)c.MontoIntereses); 

                montoTotalPagado += montoTotalPagadoIntereses != null ? montoTotalPagadoIntereses.Value : 0;

                // al monto de cuotas anteriores, debemos sumar el de la cuota que se está tratando ... 
                montoTotalPagado += montoCuota + montoIntereses; 

                prestamo.MontoCancelado = montoTotalPagado;
                if (prestamo.MontoCancelado != null)
                    prestamo.MontoCancelado = Math.Round(prestamo.MontoCancelado.Value, 2); 

                if (prestamo.MontoCancelado >= prestamo.MontoOtorgado)
                {
                    prestamo.Saldo = 0;
                    prestamo.Situacion = "CA";
                    prestamo.FechaCancelado = nominaHeader.FechaNomina;
                }
                else
                {
                    prestamo.Saldo = prestamo.MontoOtorgado - prestamo.MontoCancelado;
                    if (prestamo.Saldo != null)
                        prestamo.Saldo = Math.Round(prestamo.Saldo.Value, 2); 

                    prestamo.Situacion = "OT";
                    prestamo.FechaCancelado = null;
                }

                nominaItem = new NominaItem()
                {
                    EmpleadoID = empleado.Empleado,
                    RubroID = rubro,
                    Tipo = "D",
                    Descripcion = "Préstamo #: " + cuota.tPrestamo.Numero.ToString() + "; cuota: " + 
                                  numeroCuotaActual.ToString() + "/" + 
                                  cantidadCuotas.ToString() + 
                                  "; saldo: " + 
                                  prestamo.Saldo.Value.ToString("N2") + 
                                  ".",
                    Monto = Math.Round(montoCuota * -1, 2),
                    Base = 0,
                    CantDias = 0,
                    SueldoFlag = false,
                    SalarioFlag = false
                };

                Nomina_List.Add(nominaItem);

                if (montoIntereses != 0)
                {
                    if (!funcionesGenericasNomina.ObtenerRubro(context, 3, out rubro))
                    {
                        errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde " +
                                        "a la deducción por aplicación de interéses sobre préstamos otorgados." +
                                        "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                        return;
                    }

                    nominaItem = new NominaItem()
                    {
                        EmpleadoID = empleado.Empleado,
                        RubroID = rubro,
                        Tipo = "D",
                        Descripcion = "Cuota de préstamo - Intereses - Prestamo número " + cuota.tPrestamo.Numero.ToString(),
                        Monto = Math.Round(montoIntereses * -1, 2),
                        Base = 0,
                        CantDias = 0,
                        SueldoFlag = false,
                        SalarioFlag = false
                    };

                    Nomina_List.Add(nominaItem);
                }


                cantidadCuotasPrestamoAplicadas++; 
            }
        }


        private void AgregarRubrosAsignados(dbNominaEntities context,
                                            tNominaHeader nominaHeader,
                                            ParametrosNomina parametrosNomina,
                                            tEmpleado empleado,
                                            List<NominaItem> Nomina_List,
                                            out string errorMessage)
        {
            // leemos y aplicamos a la nómina los rubros asignados para el empleado (en la tabla tRubrosAsignados) ... 

            errorMessage = "";

            DateTime periodoPagoDesde = nominaHeader.Desde.Value;
            DateTime periodoPagoHasta = nominaHeader.Hasta.Value;

            NominaItem nominaItem;

            // leemos los rubros que apliquen a esta nómina y al empleado ... 

            // nótese como, al menos por ahora, solo seleccionamos rubros que tengan un monto; por ahora no permitimos rubros con formulas, etc. 
            // nótese como cada rubro asignado a seleccionar *debe* corresponder al tipo de la nómina (V, E, U, ...); la excepción es para nóminas 
            // del tipo 1Q y 2Q; en estos casos, el tipo no es idéntico pues, para el rubro asignado, el tipo es Q pero para la nómina, el tipo es 
            // 1Q o 2Q. Sin embargo, cuando la nómina es del tipo 1Q o 2Q, rubros de tipo Q son seleccionados ... 

            var query = context.tRubrosAsignados.Where(r => r.Empleado == empleado.Empleado && !r.SuspendidoFlag).
                                                 Where(r => r.TipoNomina.Contains(nominaHeader.Tipo) || 
                                                           (r.TipoNomina.Contains("Q") && (nominaHeader.Tipo == "1Q" || nominaHeader.Tipo == "2Q"))).
                                                 Where(r => r.MontoAAplicar != null && r.MontoAAplicar != 0);

            foreach (tRubrosAsignado rubro in query)
            {
                bool rubroAplicaEnNominaDePago = ValidarPeriodoAplicacionRubro(rubro.RubroAsignado,
                                                                               periodoPagoDesde,
                                                                               periodoPagoHasta,
                                                                               rubro.Siempre,
                                                                               rubro.Desde,
                                                                               rubro.Hasta,
                                                                               out errorMessage);

                if (!rubroAplicaEnNominaDePago)
                {
                    if (!string.IsNullOrEmpty(errorMessage))
                        return;
                    else
                        continue;
                }

                // el período de aplicación del rubro asignado es válido (ejemplo: siempre, o el período de aplicación corresonde a la nómina) 
                // ahora revisamos su periodicidad (1q, 2q, siempre) 

                bool rubroPeriodicidad = ValidarPeriodicidadRubro(nominaHeader.Tipo,
                                                                  rubro.Periodicidad,
                                                                  out  errorMessage);


                if (!rubroPeriodicidad)
                {
                    if (!string.IsNullOrEmpty(errorMessage))
                        return;
                    else
                        continue;
                }

                // Ok, el rubro asignado aplica; lo aplicamos a la nómina ... 
                nominaItem = new NominaItem()
                {
                    EmpleadoID = empleado.Empleado,
                    RubroID = rubro.Rubro,
                    Tipo = rubro.Tipo,
                    Descripcion = rubro.Descripcion,
                    Monto = rubro.Tipo == "A" ? rubro.MontoAAplicar : -rubro.MontoAAplicar,
                    Base = 0,
                    CantDias = 0,
                    SueldoFlag = false,
                    SalarioFlag = rubro.SalarioFlag != null ? rubro.SalarioFlag.Value : false,
                    Fraccion = null 
                };

                Nomina_List.Add(nominaItem);
            }
        }

        private void DeterminarYAplicarAnticiposDeSueldo(dbNominaEntities context,
                                                         tNominaHeader nominaHeader,
                                                         ParametrosNomina parametrosNomina,
                                                         tEmpleado empleado,
                                                         List<NominaItem> Nomina_List,
                                                         int cantDiasHabiles,
                                                         int cantDiasFeriados,
                                                         int cantDiasFinSemana,
                                                         int cantDiasBancarios,
                                                         out string errorMessage)
        {
            // aplicamos un 'anticipo' de sueldo, si así se ha parametrizado en la nómina 
            errorMessage = "";

            int rubroSueldoEmpleados;
            Nomina_FuncionesGenericas funcionesGenericasNomina = new Nomina_FuncionesGenericas();

            if (!funcionesGenericasNomina.ObtenerRubro(context, 1, out rubroSueldoEmpleados))
            {
                errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde " +
                                "a la deducción por aplicación de cuotas de préstamo." +
                                "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                return;
            }

            DateTime periodoPagoDesde = nominaHeader.Desde.Value;
            DateTime periodoPagoHasta = nominaHeader.Hasta.Value;

            NominaItem nominaItem;

            // solo aplicamos un anticipo si el grupo de nómina para el empleado tiene un registro en la tabla Definición de Anticipos ... 
            Nomina_DefinicionAnticipos definicionAnticipo = context.Nomina_DefinicionAnticipos.Where(a => a.GrupoNomina == nominaHeader.tGruposEmpleado.Grupo).
                                                                                               Where(a => a.Desde <= periodoPagoDesde && !a.Suspendido).
                                                                                               OrderByDescending(a => a.Desde).FirstOrDefault(); 

            if (definicionAnticipo == null) 
                return; 


            decimal porcentajeAnticipo1Q = 0; 

            if (definicionAnticipo.PrimQuincPorc != null) 
                porcentajeAnticipo1Q = definicionAnticipo.PrimQuincPorc.Value; 

            // existe un registro de definción de anticipos; determinamos si existe, además, un item para el empleado específico 
            Nomina_DefinicionAnticipos_Empleados definicionAnticipoEmpleado = 
                definicionAnticipo.Nomina_DefinicionAnticipos_Empleados.Where(a => a.Empleado == empleado.Empleado).
                                                                        Where(a => !a.Suspendido && a.PrimQuincPorc != null).FirstOrDefault(); 

            if (definicionAnticipoEmpleado != null) 
                porcentajeAnticipo1Q = definicionAnticipoEmpleado.PrimQuincPorc.Value; 

            if (porcentajeAnticipo1Q == 0) 
                return; 

            // buscamos el sueldo más reciente para el período de pago 
            decimal? sueldoEmpleado = context.Empleados_Sueldo.Where(s => s.Empleado == empleado.Empleado && s.Desde <= periodoPagoDesde).
                                                               OrderByDescending(s => s.Desde).
                                                               Select(s => s.Sueldo).
                                                               FirstOrDefault();

            if (sueldoEmpleado == null)
                sueldoEmpleado = 0;

            // aplicamos el rubro como un anticipo (asignación) o deducción por anticipo (deducción) de acuerdo a la quincena (1ra o 2da) ... 
            if (nominaHeader.Tipo == "1Q") 
            {
                // y usuario decide, y lo registra en parámetros, cual es el porcentaje a anticipar en la 1Q
                decimal sueldoAnticipo = sueldoEmpleado.Value * porcentajeAnticipo1Q; 

                // 1ra. quincena; agregamos una deducción por anticipo de sueldo 
                //nominaItem = new NominaItem()
                //{
                //    EmpleadoID = empleado.Empleado,
                //    RubroID = rubroSueldoEmpleados,
                //    Tipo = "A",
                //    Descripcion = "Anticipo de sueldo - 1ra. quincena.",
                //    Monto = Math.Round(sueldoEmpleado.Value * porcentajeAnticipo1Q, 2),
                //    Base = sueldoEmpleado.Value,
                //    CantDias = 0,
                //    Fraccion = porcentajeAnticipo1Q * 100,                  // para expresar .0575 como 5.75%
                //    SueldoFlag = true,
                //    SalarioFlag = false
                //};

                //Nomina_List.Add(nominaItem);

                string descripcionRubroSbas = string.IsNullOrEmpty(nominaHeader.DescripcionRubroSueldo) ? "Anticipo de sueldo 1Q" : nominaHeader.DescripcionRubroSueldo; 

                nominaItem = new NominaItem()
                {
                    EmpleadoID = empleado.Empleado,
                    RubroID = rubroSueldoEmpleados,
                    Tipo = "A",
                    Descripcion = descripcionRubroSbas + " - (" + cantDiasHabiles.ToString() + " días hábiles)",
                    Monto = Math.Round(sueldoAnticipo / nominaHeader.CantidadDias.Value * cantDiasHabiles, 2),       
                    Base = sueldoEmpleado.Value,
                    CantDias = Convert.ToInt16(cantDiasHabiles),
                    Fraccion = porcentajeAnticipo1Q * 100,                  // para expresar .0575 como 5.75%
                    SueldoFlag = true,
                    SalarioFlag = false
                };
                Nomina_List.Add(nominaItem);

                nominaItem = new NominaItem()
                {
                    EmpleadoID = empleado.Empleado,
                    RubroID = rubroSueldoEmpleados,
                    Tipo = "A",
                    Descripcion = descripcionRubroSbas + " - (" + cantDiasFinSemana.ToString() + " días fines semana)",
                    Monto = Math.Round(sueldoAnticipo / nominaHeader.CantidadDias.Value * cantDiasFinSemana, 2),        
                    Base = sueldoEmpleado.Value,
                    CantDias = Convert.ToInt16(cantDiasFinSemana),
                    Fraccion = porcentajeAnticipo1Q * 100,                  // para expresar .0575 como 5.75%
                    SueldoFlag = true,
                    SalarioFlag = false
                };
                Nomina_List.Add(nominaItem);

                nominaItem = new NominaItem()
                {
                    EmpleadoID = empleado.Empleado,
                    RubroID = rubroSueldoEmpleados,
                    Tipo = "A",
                    Descripcion = descripcionRubroSbas + " - (" + cantDiasFeriados.ToString() + " días feriados)",
                    Monto = Math.Round(sueldoAnticipo / nominaHeader.CantidadDias.Value * cantDiasFeriados, 2),    
                    Base = sueldoEmpleado.Value,
                    CantDias = Convert.ToInt16(cantDiasFeriados),
                    Fraccion = porcentajeAnticipo1Q * 100,                  // para expresar .0575 como 5.75%
                    SueldoFlag = true,
                    SalarioFlag = false
                };
                Nomina_List.Add(nominaItem);

                // los días bancarios los mostramos solo si existen 
                if (cantDiasBancarios > 0)
                {
                    nominaItem = new NominaItem()
                    {
                        EmpleadoID = empleado.Empleado,
                        RubroID = rubroSueldoEmpleados,
                        Tipo = "A",
                        Descripcion = descripcionRubroSbas + " - (" + cantDiasBancarios.ToString() + " días bancarios)",
                        Monto = Math.Round(sueldoAnticipo / nominaHeader.CantidadDias.Value * cantDiasBancarios, 2),
                        Base = sueldoEmpleado.Value,
                        CantDias = Convert.ToInt16(cantDiasBancarios),
                        Fraccion = porcentajeAnticipo1Q * 100,                  // para expresar .0575 como 5.75%
                        SueldoFlag = true,
                        SalarioFlag = false
                    };
                    Nomina_List.Add(nominaItem);
                }
            }
            else 
            {
                // no es la 1Q; en la 2Q, agregamos la deducción por el anticipo que se pagó en la 1Q ... 
                nominaItem = new NominaItem()
                {
                    EmpleadoID = empleado.Empleado,
                    RubroID = rubroSueldoEmpleados,
                    Tipo = "D",
                    Descripcion = "Deducción por anticipo de sueldo en 1ra. quincena.",
                    Monto = Math.Round((sueldoEmpleado.Value * porcentajeAnticipo1Q) * -1, 2),
                    Base = sueldoEmpleado.Value,
                    CantDias = 0,
                    Fraccion = porcentajeAnticipo1Q * 100,              // para expresar .0575 como 5.75%
                    SueldoFlag = true,
                    SalarioFlag = false
                };

                Nomina_List.Add(nominaItem);
            }
        }

        private bool ValidarPeriodoAplicacionRubro(int rubroAsignadoID,
                                                   DateTime periodoPagoDesde,
                                                   DateTime periodoPagoHasta,
                                                   bool? periodicidadSiempre,
                                                   DateTime? periodicidadDesde,
                                                   DateTime? periodicidadHasta,
                                                   out string errorMessage)
        {
            errorMessage = "";

            if (periodicidadSiempre == null && (periodicidadDesde == null || periodicidadHasta == null))
            {
                errorMessage = "El rubro número " + rubroAsignadoID.ToString() +
                    " no tiene su 'período de aplicación' correctamente establecido. Por favor revise y corrija esta situación.";
                return false;
            }

            if (((periodicidadSiempre == null) || (periodicidadSiempre != null && !periodicidadSiempre.Value)) &&
                (periodicidadDesde == null || periodicidadHasta == null))
            {
                errorMessage = "El rubro número " + rubroAsignadoID.ToString() +
                    " no tiene su 'período de aplicación' correctamente establicido. Por favor revise y corrija esta situación.";
                return false;
            }


            if (periodicidadSiempre != null && periodicidadSiempre.Value)
                return true;


            // debe venir un período, pues lo validamos arriba ... 

            if (periodicidadDesde <= periodoPagoDesde && periodicidadHasta >= periodoPagoHasta)
                // el período de aplicación del rubro es más amplio que el periodo de pago de la nómina 
                return true;

            if (periodicidadDesde >= periodoPagoDesde && periodicidadDesde <= periodoPagoHasta)
                // el comienzo del período de aplicación del rubro corresponde al período de pago 
                return true;

            if (periodicidadHasta >= periodoPagoDesde && periodicidadHasta <= periodoPagoHasta)
                // el final del período de aplicación del rubro corresponde al período de pago 
                return true;

            return false;
        }

        private bool ValidarPeriodicidadRubro(string tipoNomina, 
                                              string periodicidadRubro, 
                                              out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrEmpty(periodicidadRubro))
                // si la periodicidad es null, asumimos que no debe aplicar; el usuario debe usar Siempre ... 
                return true; 

            if (periodicidadRubro == "S")
                return true;

            if (periodicidadRubro == "1Q" && tipoNomina == "2Q")
                return false;

            if (periodicidadRubro == "2Q" && tipoNomina == "1Q")
                // el rubro se aplica en 2das quincenas, pero la períodicidad de la nómina corresponde a la 1ra; obviamos el rubro 
                return false;

            return true;
        }



        private void DeduccionesObligatorias(dbNominaEntities context,
                                             IMongoDatabase mongoDatabase, 
                                             tNominaHeader nominaHeader,
                                             tEmpleado empleado, 
                                             Parametros_Nomina_SalarioMinimo parametroNominaSalarioMinimo, 
                                             List<NominaItem> Nomina_List,
                                             out string errorMessage)
        {
            errorMessage = ""; 

            DateTime periodoPagoDesde = nominaHeader.Desde.Value;
            DateTime periodoPagoHasta = nominaHeader.Hasta.Value;

            Nomina_FuncionesGenericas funcionesGenericasNomina = new Nomina_FuncionesGenericas();

            // leemos y aplicamos las deducciones obligatorias en las cuales participe el empleado ... 

            // primero leemos los registros en DeduccionesNomina en los cuales participe el empleado 

            var query = context.DeduccionesNominas.Where(d => !d.SuspendidoFlag).
                                                   Where(d => d.Desde <= periodoPagoDesde).
                                                   Where(d => d.Cia == null || (d.Cia != null && d.Cia.Value == empleado.Cia)).
                                                   Where(d => d.Empleado == null || (d.Empleado != null && d.Empleado.Value == empleado.Empleado)).
                                                   Where(d => d.GrupoNomina == null || (d.GrupoNomina != null && d.tGruposEmpleado.tdGruposEmpleados.Any(e => e.Empleado == empleado.Empleado && !e.SuspendidoFlag))).
                                                   Where(d => d.GrupoEmpleados == null || (d.GrupoEmpleados != null && d.tGruposEmpleado1.tdGruposEmpleados.Any(e => e.Empleado == empleado.Empleado && !e.SuspendidoFlag))).
                                                   OrderByDescending(d => d.Desde);

            NominaItem nominaItem = null; 

            foreach (DeduccionesNomina deduccion in query)
            {
                nominaItem = null;
                decimal baseDeduccion = 0;
                short? cantidadDiasBaseDeduccion = null; 

                if (!DeterminarBaseDeduccion(context,
                                             mongoDatabase, 
                                             nominaHeader,
                                             empleado,
                                             deduccion, 
                                             Nomina_List,
                                             out baseDeduccion, 
                                             out cantidadDiasBaseDeduccion, 
                                             out errorMessage)) 
                {
                    return; 
                }

                switch (deduccion.Tipo)
                {
                    case "PF":          // el pf y sso se calculan igual, aunque sus variables varían sus valores ... 
                    case "SSO":
                        {
                            // aplicamos un tope si se ha definido para la deducción 
                            if (deduccion.Tope != null && deduccion.Tope.Value != 0)
                            {
                                if (string.IsNullOrEmpty(deduccion.TopeBase))
                                {
                                    errorMessage = "Error: aunque se ha definido un tope para una deducción de nómina, " + 
                                        "no se ha definido su 'base' (monto, salario mínimo, etc.). " + 
                                        "Por favor revise y corrija esta situación.";
                                    return;
                                }

                                switch (deduccion.TopeBase)
                                {
                                    case "Monto":
                                        {
                                            if (baseDeduccion > deduccion.Tope.Value)
                                                baseDeduccion = deduccion.Tope.Value; 

                                            break;
                                        }
                                    case "SalMin":
                                        {
                                            if (parametroNominaSalarioMinimo == null)
                                            {
                                                // aunque el parámetro que corresponde al salario mínimo fue leído, su existencia no fue validada; lo hacemos aquí  ... 
                                         
                                                errorMessage = "Error: aunque se ha incluído el 'salario mínimo' en la definición de algunas deducciones de nómina, " +
                                                    "no se ha registrado su valor en la tabla de parámetros que corresponde. " +
                                                    "Por favor revise y corrija esta situación.";
                                                return;
                                            }

                                            decimal tope = deduccion.Tope.Value * parametroNominaSalarioMinimo.Monto;

                                            if (baseDeduccion > tope)
                                                baseDeduccion = tope; 

                                            break;
                                        }
                                }
                            }

                            int cantidadLunesPeriodoMes = CantidadLunesEnMes(nominaHeader.Desde.Value, nominaHeader.Hasta.Value);
                                
                            decimal montoDeduccion = (baseDeduccion * 12 / 52) * deduccion.AporteEmpleado * cantidadLunesPeriodoMes;

                            montoDeduccion = Math.Round(montoDeduccion, 2);

                            string descripcionRubro = deduccion.Tipo == "SSO" ?
                                        "Seguro Social que corresponde al período de pago" :
                                        "Paro Forzoso que corresponde al período de pago";

                            descripcionRubro += " (" + cantidadLunesPeriodoMes.ToString() + " lunes)";

                            int rubro = 0;

                            if (deduccion.Tipo == "PF")
                            {
                                if (!funcionesGenericasNomina.ObtenerRubro(context, 7, out rubro))
                                {
                                    errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde " +
                                                    "a la deducción por paro forzoso." +
                                                    "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                                    return;
                                }
                            }
                            else if (deduccion.Tipo == "SSO")
                            {
                                if (!funcionesGenericasNomina.ObtenerRubro(context, 5, out rubro))
                                {
                                    errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde " +
                                                    "a la deducción por seguro social obligatorio." +
                                                    "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                                    return;
                                }
                            }

                            nominaItem = new NominaItem()
                            {
                                EmpleadoID = empleado.Empleado,
                                RubroID = rubro,
                                Tipo = "D",
                                Descripcion = descripcionRubro,
                                Monto = -montoDeduccion,
                                Base = baseDeduccion,
                                CantDias = cantidadDiasBaseDeduccion != null ? cantidadDiasBaseDeduccion.Value : nominaHeader.CantidadDias.Value,
                                Fraccion = deduccion.AporteEmpleado * 100,      // para expresar .0575 como 5.75%
                                Detalles = cantidadLunesPeriodoMes.ToString() + "L", 
                                SueldoFlag = false,
                                SalarioFlag = false
                            };

                            Nomina_List.Add(nominaItem);

                            break;
                        }
                    
                    case "LPH":
                        {
                            // aplicamos un tope si se ha definido para la deducción 
                            if (deduccion.Tope != null && deduccion.Tope.Value != 0)
                            {
                                if (string.IsNullOrEmpty(deduccion.TopeBase))
                                {
                                    errorMessage = "Error: aunque se ha definido un tope para una deducción de nómina, " +
                                        "no se ha definido su 'base' (monto, salario mínimo, etc.). " +
                                        "Por favor revise y corrija esta situación.";
                                    return;
                                }

                                switch (deduccion.TopeBase)
                                {
                                    case "Monto":
                                        {
                                            if (baseDeduccion > deduccion.Tope.Value)
                                                baseDeduccion = deduccion.Tope.Value;

                                            break;
                                        }
                                    case "SalMin":
                                        {
                                            if (parametroNominaSalarioMinimo == null)
                                            {
                                                // aunque el parámetro que corresponde al salario mínimo fue leído, su existencia no fue validada; lo hacemos aquí  ... 

                                                errorMessage = "Error: aunque se ha incluído el 'salario mínimo' en la definición de algunas deducciones de nómina, " +
                                                    "no se ha registrado su valor en la tabla de parámetros que corresponde." +
                                                    "Por favor revise y corrija esta situación.";
                                                return;
                                            }

                                            decimal tope = deduccion.Tope.Value * parametroNominaSalarioMinimo.Monto;

                                            if (baseDeduccion > tope)
                                                baseDeduccion = tope;

                                            break;
                                        }
                                }
                            }

                            // nótese como aplicamos la cantidad de días del período para corregir, por ejemplo, un período de solo 15 días 

                            short cantidadDiasNomina = nominaHeader.CantidadDias.Value; 
                            // corregimos la cantidad de días para nóminas del tipo mensual; la idea es siempre asumir 30 días y no 28 para febrero 
                            // o 31 para diciembre o marzo. En nóminas quincenales, la cantidad de días, sin embargo, será siempre 15 ... 
                            switch (nominaHeader.Tipo)
                            {
                                case "M":           // mensual 
                                    cantidadDiasNomina = 30;
                                    break;
                                case "1Q":          // quincenal 
                                case "2Q":          // quincenal 
                                    cantidadDiasNomina = 15;
                                    break;
                                default:
                                    break;
                            }

                            baseDeduccion = baseDeduccion / 30 * cantidadDiasNomina; 

                            // la aplicación del lph es mucho más simple ... 
                            decimal montoDeduccion = baseDeduccion  * deduccion.AporteEmpleado;
                            montoDeduccion = Math.Round(montoDeduccion, 2); 

                            string descripcionRubro = "Política Habitacional que corresponde al período de pago";

                            int rubro;

                            if (!funcionesGenericasNomina.ObtenerRubro(context, 6, out rubro))
                            {
                                errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde " +
                                                "a la deducción por ley de política habitacional." +
                                                "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                                return;
                            }
                                 
                            nominaItem = new NominaItem()
                            {
                                EmpleadoID = empleado.Empleado,
                                RubroID = rubro,
                                Tipo = "D",
                                Descripcion = descripcionRubro,
                                Monto = -montoDeduccion,
                                Base = baseDeduccion,
                                CantDias = cantidadDiasBaseDeduccion != null ? cantidadDiasBaseDeduccion.Value : nominaHeader.CantidadDias.Value,
                                Fraccion = deduccion.AporteEmpleado * 100,          // para expresar .0575 como 5.75%
                                SueldoFlag = false,
                                SalarioFlag = false
                            };

                            Nomina_List.Add(nominaItem);

                            break;
                        }
                }
            }
        }

        private bool DeterminarBaseDeduccion(dbNominaEntities context,
                                             IMongoDatabase mongoDataBase, 
                                             tNominaHeader nominaHeader,
                                             tEmpleado empleado,
                                             DeduccionesNomina deduccion, 
                                             List<NominaItem> Nomina_List,
                                             out decimal baseDeduccion, 
                                             out short? cantidadDiasDeduccion, 
                                             out string errorMessage)
        {
            cantidadDiasDeduccion = null; 
            baseDeduccion = 0;
            errorMessage = ""; 

            // si el empleado está de vacaciones, la variable que sigue tendrá un valor en esta función ... 
            NominaASP.Models.MongoDB.vacacion vacacion = null;

            if (nominaHeader.Tipo == "V")
            {
                // para tener el registro de vacaciones, si la nómina es de este tipo ... 
                var vacaciones = mongoDataBase.GetCollection<vacacion>("vacaciones");

                var builder = Builders<vacacion>.Filter;
                var filter = builder.Eq(x => x.claveUnicaContab, nominaHeader.ProvieneDe_ID);

                vacacion = vacaciones.Find<vacacion>(filter).FirstOrDefault();
            }
                


            bool empleadoRegresaDeVacacionesFlag = false;

            if (nominaHeader.Tipo != "V")
                if (EmpleadoRegresaDeVacaciones(context,
                                                mongoDataBase, 
                                                nominaHeader,
                                                empleado,
                                                out vacacion, 
                                                out errorMessage))
                {
                    empleadoRegresaDeVacacionesFlag = true;
                }

            if (nominaHeader.Tipo == "V")
            {
                // para las nóminas de vacaciones, leemos si se deben aplicar las deducciones obligatorias; además, 
                // la cantidad de días de sueldo ... todos los valores necesarios están en el registro de las vacaciones 

                if (vacacion == null)
                {
                    errorMessage = "Error inesperado: aunque la nómina corresponde al tipo 'Vacaciones', no hemos podido encontrar " + 
                        "un registro de vacaciones para el empleado '" + empleado.Nombre +
                        "', y las condiciones indicadas a esta nómina de pago.";
                    return false;
                }

                if (!(vacacion.aplicarDeduccionesFlag != null && vacacion.aplicarDeduccionesFlag.Value))
                    return true;


                if (!(vacacion.cantDiasDeduccion != null && vacacion.cantDiasDeduccion.Value != 0))
                    return true; 

                
            }

            // el sueldo o salario ya está en la lista de rubros; determinamos este monto dependiendo del parámetro en la deducción 
            if (deduccion.Base == "Sueldo")
            {
                if (nominaHeader.Tipo == "V")
                {
                    // nómina de vacaciones 
                    baseDeduccion = vacacion.sueldo.Value / 30 * vacacion.cantDiasDeduccion.Value;
                    cantidadDiasDeduccion = Convert.ToInt16(vacacion.cantDiasDeduccion.Value); 
                }
                else if (empleadoRegresaDeVacacionesFlag)
                {
                    // el empleado regresa de vacaciones 
                    baseDeduccion = vacacion.sueldo.Value / 30 *
                                    (vacacion.proximaNomina_AplicarDeduccionesLegales_CantDias != null ?
                                    vacacion.proximaNomina_AplicarDeduccionesLegales_CantDias.Value : 0);
                    cantidadDiasDeduccion = Convert.ToInt16(vacacion.proximaNomina_AplicarDeduccionesLegales_CantDias.Value); 
                }
                else
                {
                    // buscamos el sueldo más reciente para el período de pago 
                    baseDeduccion = DeterminarSueldoEmpleado(context, empleado.Empleado, nominaHeader.Desde.Value); 
                }
            }
            else if (deduccion.Base == "Salario")
            {
                // nótese como el salario incluye al sueldo ... 
                baseDeduccion = Nomina_List.Where(n => n.EmpleadoID == empleado.Empleado).
                                            Where(n => n.SueldoFlag || n.SalarioFlag).
                                            Sum(n => n.Monto);
            }
            else if (deduccion.Base == "SalInt")
            {
                // buscamos el salario integral para este empleado en esta nómina; el salario integral se calcula y registra 
                // como primer paso en la nómina y debe existir para cada empleado de la nómina 
                decimal? baseDeduccionNullable;

                baseDeduccionNullable = context.tNomina_SalarioIntegral.Where(s => s.HeaderID == nominaHeader.ID && s.Empleado == empleado.Empleado)
                                                                        .Select(s => (decimal?)s.SalarioIntegral_Monto)
                                                                        .FirstOrDefault();

                if (baseDeduccionNullable == null)
                {
                    errorMessage = "Error inesperado: no hemos podido leer el salario integral para el empleado '" + empleado.Nombre +
                        "', en esta nómina de pago. Cada empleado debe tener un salario integral calculado en cada nómina de pago. " +
                        "Por favor revise y corrija esta situación.";
                    return false;
                }

                if (nominaHeader.Tipo == "V")
                {
                    // nómina de vacaciones 
                    baseDeduccion = baseDeduccionNullable.Value / 30 * vacacion.cantDiasDeduccion.Value;
                    cantidadDiasDeduccion = Convert.ToInt16(vacacion.cantDiasDeduccion.Value); 
                }
                else if (empleadoRegresaDeVacacionesFlag)
                {
                    // el empleado regresa de vacaciones 
                    baseDeduccion = baseDeduccionNullable.Value / 30 *
                                    (vacacion.proximaNomina_AplicarDeduccionesLegales_CantDias != null ?
                                    vacacion.proximaNomina_AplicarDeduccionesLegales_CantDias.Value : 0);
                    cantidadDiasDeduccion = Convert.ToInt16(vacacion.proximaNomina_AplicarDeduccionesLegales_CantDias.Value); 
                }
                else
                {
                    baseDeduccion = baseDeduccionNullable.Value;
                }
            }
            else
            {
                errorMessage = "Error: alguna de las deducciones definidas para el empleado '" + empleado.Nombre +
                        "', tiene un valor indifinido en su campo Base. " +
                        "Por favor revise y corrija esta situación.";
                return false;
            }


            // lo que sigue determina el monto en sueldo o salario que se ha pagado en nóminas *diferentes* a la actual, 
            // *pero* que corresponden al período de pago. Consideramos que este paso es solo válido en nóminas de 2da. 
            // quincena, cuando se usa el mecanismo de anticipo en la 1ra. 

            // no ejecutamos este código en otros tipos de nómina, para hacer todo el proceso más simple. Otros tipos de 
            // nómina pueden ser: vacaciones, utilidades, etc. 

            // nota: si la base de cálculo de la deducción es el salario integral, ya este monto está calculado y no contempla 
            // un 'historico' de lo que ha ganado el empleado; solo un calculo en base a su sueldo, días de vacaciones y de 
            // utilidades; es decir, no tiene que ver con lo que el empleado ha ganado efectivamente en un mes, etc., sino de 
            // acuerdo a su monto de sueldo en la maestra y la definición de sus utlidades y vacaciones ... 

            // cuando el empleado regresa de vacaciones, tanto el sueldo como la cantidad de días a deducir en deducciones 'legales', 
            // se registran en la vacación ...

            decimal montoSueldoSalarioNominasAnteriores = 0;

            if (nominaHeader.Tipo.Contains("Q") && deduccion.Base != "SalInt" && deduccion.Base != "Sueldo" && !empleadoRegresaDeVacacionesFlag)
            {
                // nótese que ésto nunca aplica para una nómina de vacaciones, pues su tipo de nómina es V ... 

                Nomina_FuncionesGenericas funcionesNomina = new Nomina_FuncionesGenericas();
                if (!funcionesNomina.DeterminarMontoSueldoSalarioNominasRelacionadas(context,
                                                                                     nominaHeader,
                                                                                     empleado.Empleado,
                                                                                     deduccion.Base,
                                                                                     out montoSueldoSalarioNominasAnteriores,
                                                                                     out errorMessage))
                {
                    return false;
                }
            }

            baseDeduccion += montoSueldoSalarioNominasAnteriores;

            return true; 
        }

        private int CantidadLunesEnMes(DateTime desde, DateTime hasta)
        {
            DateTime fecha = desde;
            int cantidadLunes = 0; 

            while (fecha <= hasta)
            {
                if (fecha.DayOfWeek == DayOfWeek.Monday) 
                    cantidadLunes++; 

                fecha = fecha.AddDays(1); 
            }

            return cantidadLunes; 
        }

        private int CantidadMesesDiferentes(DateTime desde, DateTime hasta)
        {
            // determinamos la cantidad de meses que hay en el período de pago ... 

            int cantidadMeses = 1;

            int mes = desde.Month;

            while (desde < hasta)
            {
                desde = desde.AddDays(1);
                if (desde.Month != mes)
                {
                    mes = desde.Month; 
                    cantidadMeses++;
                }
            }

            return cantidadMeses; 
        }


    private void ISLR(dbNominaEntities context,
                      NominaASP.Models.tNominaHeader nominaHeader,
                      NominaASP.Models.tEmpleado empleado, 
                      List<NominaItem> Nomina_List,
                      out string errorMessage, 
                      bool? nominaTipoUtilidades = null, 
                      int? rubroUtilidades = null)
        {
            // para nóminas de Utilidades, leemos el monto que corresponde a las mismas y lo agregamos a la base para el cálculo del impuesto; la razón es que 
            // el monto de utilidades no es marcado como sueldo ni salario, lo cual hace que no sea leído al determinar el islr ... 
            if (nominaTipoUtilidades != null && nominaTipoUtilidades.Value)
            {
                if (rubroUtilidades == null)
                {
                    errorMessage = "Error: cálculo del islr: aunque se indicó a esta función que la nómina es del tipo 'Utilidades', " + 
                        "no se indicó el rubro que corresponden a las mismas (ej: UTIL).";
                    return;
                }
            }

            errorMessage = "";

            DateTime periodoPagoDesde = nominaHeader.Desde.Value;
            DateTime periodoPagoHasta = nominaHeader.Hasta.Value;

            Nomina_FuncionesGenericas funcionesGenericasNomina = new Nomina_FuncionesGenericas(); 

            // leemos y aplicamos las deducciones obligatorias en las cuales participe el empleado ... 

            // primero leemos los registros en DeduccionesNomina en los cuales participe el empleado 
            var query = context.DeduccionesISLRs.Where(d => !d.SuspendidoFlag).
                                                 Where(d => d.Desde <= periodoPagoDesde).
                                                 Where(d => d.TipoNomina.Contains(nominaHeader.Tipo) || (d.TipoNomina.Contains("Q") && (nominaHeader.Tipo == "1Q" || nominaHeader.Tipo == "2Q"))). 
                                                 Where(d => d.Empleado == null || (d.Empleado != null && d.Empleado.Value == empleado.Empleado)).
                                                 Where(d => d.GrupoNomina == null || (d.GrupoNomina != null && d.tGruposEmpleado.tdGruposEmpleados.Any(e => e.Empleado == empleado.Empleado && !e.SuspendidoFlag)));

            NominaItem nominaItem = null;

            foreach (DeduccionesISLR deduccion in query)
            {
                // solo para nóminas de tipo quincenal, verificamos la periodicidad ... 

                bool periodicidadOk = ValidarPeriodicidadRubro(nominaHeader.Tipo, deduccion.Periodicidad, out errorMessage);

                if (!periodicidadOk)
                    continue; 

                nominaItem = null;

                decimal baseDeduccion = 0;
                decimal montoSueldoSalarioNominasAnteriores = 0; 

                // el sueldo o salario ya está en la lista de rubros; determinamos este monto dependiendo del parámetro en la deducción 
                if (deduccion.Base == "Sueldo")
                {
                    baseDeduccion = Nomina_List.Where(n => n.EmpleadoID == empleado.Empleado).
                                                Where(n => n.SueldoFlag). 
                                                Sum(n => n.Monto);

                    if (nominaTipoUtilidades != null && nominaTipoUtilidades.Value)
                    {
                        baseDeduccion += Nomina_List.Where(n => n.EmpleadoID == empleado.Empleado).
                                                Where(n => n.RubroID == rubroUtilidades.Value).
                                                Sum(n => n.Monto);
                    }
                }
                else if (deduccion.Base == "Salario") 
                {
                    // nótese como el salario incluye al sueldo ... 
                    baseDeduccion = Nomina_List.Where(n => n.EmpleadoID == empleado.Empleado).
                                                Where(n => n.SueldoFlag || n.SalarioFlag).
                                                Sum(n => n.Monto);

                    if (nominaTipoUtilidades != null && nominaTipoUtilidades.Value)
                    {
                        baseDeduccion += Nomina_List.Where(n => n.EmpleadoID == empleado.Empleado).
                                                Where(n => n.RubroID == rubroUtilidades.Value).
                                                Sum(n => n.Monto);
                    }
                }
                else if (deduccion.Base == "SalInt") 
                {
                    // buscamos el salario integral para este empleado en esta nómina; el salario integral se calcula y registra 
                    // como primer paso en la nómina y debe existir para cada empleado de la nómina 
                    decimal? baseDeduccionNullable;

                    baseDeduccionNullable  = context.tNomina_SalarioIntegral.Where(s => s.HeaderID == nominaHeader.ID && s.Empleado == empleado.Empleado)
                                                                            .Select(s => (decimal?)s.SalarioIntegral_Monto)
                                                                            .FirstOrDefault();

                    if (baseDeduccionNullable == null)
                    {
                        errorMessage = "Error inesperado: no hemos podido leer el salario integral para el empleado '" + empleado.Nombre +
                            "', en esta nómina de pago. Cada empleado debe tener un salario integral calculado en cada nómina de pago. " +
                            "Por favor revise y corrija esta situación.";
                        return;
                    }

                    baseDeduccion = baseDeduccionNullable.Value; 
                }
                else
                {
                    errorMessage = "Error: alguna de las deducciones definidas para el empleado '" + empleado.Nombre +
                            "', tiene un valor indifinido en su campo Base. " +
                            "Por favor revise y corrija esta situación.";
                    return;
                }

                if (nominaHeader.Tipo.Contains("Q") && deduccion.Base != "SalInt")
                {
                    Nomina_FuncionesGenericas funcionesNomina = new Nomina_FuncionesGenericas();
                    if (!funcionesNomina.DeterminarMontoSueldoSalarioNominasRelacionadas(context,
                                                                                         nominaHeader,
                                                                                         empleado.Empleado,
                                                                                         deduccion.Base,
                                                                                         out montoSueldoSalarioNominasAnteriores,
                                                                                         out errorMessage))
                    {
                        return;
                    }
                }


                baseDeduccion += montoSueldoSalarioNominasAnteriores; 


                decimal montoDeduccion = baseDeduccion * deduccion.Porcentaje; 
                montoDeduccion = Math.Round(montoDeduccion, 2);

                string descripcionRubro = "Impuesto Sobre la Renta que corresponde al período de pago";

                int rubroISLR;

                if (!funcionesGenericasNomina.ObtenerRubro(context, 8, out rubroISLR))
                {
                    errorMessage = "Error: aparentemente, no se ha definido el rubro que corresponde a la deducción por impuesto sobre la renta." +
                                   "Ud. debe registrar cual rubro corresponde a este concepto en la maestra de rubros.";
                    return;
                }

                nominaItem = new NominaItem()
                {
                    EmpleadoID = empleado.Empleado,
                    RubroID = rubroISLR,
                    Tipo = "D",
                    Descripcion = descripcionRubro,
                    Monto = -montoDeduccion,
                    Base = baseDeduccion,
                    CantDias = nominaHeader.CantidadDias.Value,
                    Fraccion = deduccion.Porcentaje * 100,                  // para expresar .0550 como 5.50%
                    SueldoFlag = false,
                    SalarioFlag = false
                };

                Nomina_List.Add(nominaItem);
            }
        }



















    public void DeterminarCantidadDiasFeriadosEnPeriodo(dbNominaEntities context,
                                                         DateTime desde, 
                                                         DateTime hasta, 
                                                         int cantDiasPeriodoNomina, 
                                                         out int habiles,
                                                         out int finesSemana,
                                                         out int feriados,
                                                         out int bancarios
                                                         )
    {
        // leemos la tabla de días feriados para determinar la cantidad de días: fines de semana, feriados, bancarios ... 

        habiles = cantDiasPeriodoNomina; 
        finesSemana = 0; 
        feriados = 0;
        bancarios = 0; 

        var query = context.DiasFeriados.Where(d => d.Fecha >= desde && d.Fecha <= hasta);

        foreach (DiasFeriado feriado in query)
        {
            switch (feriado.Tipo) {
                case 0:
                case 1:
                    finesSemana++;
                    break;
                case 2:
                case 3:
                    feriados++;
                    break;
                case 4:
                    bancarios++;
                    break; 
            }
        }

        // finalmente, calculamos los hábiles como los días en el perído menos los feriados 
        habiles = habiles - finesSemana - feriados - bancarios; 
    }

        public class NominaItem
        {
            public int EmpleadoID { get; set; }
            public int RubroID { get; set; }
            public string Tipo { get; set; }
            public string Descripcion { get; set; }
            public decimal Monto { get; set; }
            public decimal Base { get; set; }
            public short CantDias { get; set; }
            public decimal? Fraccion { get; set; }
            public string Detalles { get; set; }
            public bool SueldoFlag { get; set; }
            public bool SalarioFlag { get; set; }
        }
    }
}