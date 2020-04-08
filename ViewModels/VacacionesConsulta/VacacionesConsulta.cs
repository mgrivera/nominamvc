using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

//using NominaASP.Models.Nomina;
using NominaASP.Models;
using NominaASP.Models.LocalDb;
using System.Web.Providers.Entities;
using System.Data.Entity.Infrastructure;
using NominaASP.Code;
using System.ComponentModel;
using PagedList;
using ClosedXML.Excel;
using MongoDB.Driver;
using NominaASP.Models.MongoDB;

namespace NominaASP.ViewModels.VacacionesConsulta
{
    public class VacacionesConsulta
    {
        public DateTime? FechaConsulta { get; set; }
        public int? Departamento { get; set; }
        public int? Empleado { get; set; }
        public string  EstadoEmpleado { get; set; }
        public bool MostrarUltimoItemCadaEmpleado { get; set; }
        public int? CiaContabSeleccionada { get; set; }
        public string CiaContabSeleccionada_Nombre { get; set; }
        public int? Page { get; set; }

        public string Message { get; set; }
        public bool Error { get; set; }

        //public PagedList<VacacionConsulta> VacacionesDisfrutadas { get; set; }
        public StaticPagedList<VacacionConsulta> VacacionesDisfrutadas { get; set; }

        private IMongoDatabase _mongoDataBase = null;

        public bool CalcularVacaciones(string userName, out string errorMessage)
        {
            string contabm_mongodb_connection = System.Web.Configuration.WebConfigurationManager.AppSettings["contabm_mongodb_connectionString"];
            string contabm_mongodb_name = System.Web.Configuration.WebConfigurationManager.AppSettings["contabM_mongodb_name"];

            var client = new MongoClient(contabm_mongodb_connection);
            _mongoDataBase = client.GetDatabase(contabm_mongodb_name);

            var vacaciones_mongoCollection = _mongoDataBase.GetCollection<vacacion>("vacaciones");

            try
            {
                // --------------------------------------------------------------------------------------------------------------------------
                // solo para que ocura una exception si mongo no está iniciado ... nótese que antes, cuando establecemos mongo, no ocurre un 
                // exception si mongo no está iniciado ...  
                var builder = Builders<vacacion>.Filter;
                var filter = builder.Eq(x => x.cia, -99999999);

                vacaciones_mongoCollection.DeleteMany(filter);
            }
            catch (Exception ex)
            {
                string message = "Error al intentar establecer una conexión a la base de datos (mongo) de 'contabM'; el mensaje de error es: " + ex.Message;
                errorMessage = message;
                return false;
            }
            // --------------------------------------------------------------------------------------------------------------------------
            // determinamos las vacaciones para los empleados y sus días pendientes; luego grabamos en la base de 
            // datos ... 

            errorMessage = "";

            // la cia seleccionada nunca debe estar en nulls; sin embargo, ponemos cero por si acaso ... 
            if (this.CiaContabSeleccionada == null)
                this.CiaContabSeleccionada = 0; 

            dbNominaEntities context = new dbNominaEntities(); 
            localdbNominaEntities localdb = new localdbNominaEntities();

            // eliminamos los registros que puedan existir antes en la tabla específica en localdb 

            // nótese como obtenemos el objectcontext que corresponde al dbcontext ... 
            var localdbObjectContext = (localdb as IObjectContextAdapter).ObjectContext;

            localdbObjectContext.ExecuteStoreCommand("Delete From Vacaciones_DiasPendientes Where Usuario = {0}", userName); 

            // leemos los empleados para la compañía seleccionada; para cada empleado, determinamos *todos* sus años de vacaciones, 
            // hasta la fecha de la consulta. Nótese que cada año de vacacion, comienza con su fecha de ingreso y termina un año después. 
            // la exepción es el último año, que siempre termina con la fecha de la consulta, pues vamos a determinar hasta allí ... 

            var query = context.tEmpleados.Where(e => e.Cia == this.CiaContabSeleccionada.Value);

            if (!string.IsNullOrEmpty(this.EstadoEmpleado))
                query = query.Where(e => e.Status == this.EstadoEmpleado); 

            if (this.Departamento != null)
                query = query.Where(e => e.Departamento == this.Departamento); 

            if (this.Empleado != null)
                query = query.Where(e => e.Empleado == this.Empleado);

            Vacaciones_DiasPendientes vacacion;
            DateTime fechaRegistro = DateTime.Now; 

            Nomina_FuncionesGenericas nominaFuncionesGenericas = new Nomina_FuncionesGenericas();

            // obtenemos el ObjectContext para este context, pues más abajo lo usamos para hacer un ExcecuteStoreCommand ... 
            var nominaDbObjectContext = (context as IObjectContextAdapter).ObjectContext;

            foreach (tEmpleado empleado in query)
            {
                DateTime fechaInicial = empleado.FechaIngreso;
                int anoVacaciones = 0;

                int diasPendientesAnoAnterior = 0; 

                for (DateTime fechaInicioAnoVacaciones = fechaInicial; fechaInicioAnoVacaciones < this.FechaConsulta; fechaInicioAnoVacaciones = fechaInicioAnoVacaciones.AddYears(1))
                {
                    // para el último año, determinamos el año hasta la fecha de la consulta 
                    bool anoCompleto = true; 

                    DateTime fechaFinAnoVacaciones = fechaInicioAnoVacaciones.AddYears(1).AddDays(-1);

                    if (empleado.FechaRetiro != null && empleado.FechaRetiro.Value < fechaFinAnoVacaciones && empleado.FechaRetiro.Value < FechaConsulta)
                    {
                        // el empleado se retira antes del fin del año de vacaciones determinado ... 
                        fechaFinAnoVacaciones = empleado.FechaRetiro.Value;
                        anoCompleto = false;
                    }
                    else
                    {
                        if (fechaFinAnoVacaciones > FechaConsulta)
                        {
                            // el año de vacaciones termina después de la fecha de consulta; determinamos el fin del año 
                            // como la fecha de consulta ... 
                            fechaFinAnoVacaciones = FechaConsulta.Value;
                            anoCompleto = false;
                        }
                    }
                    

                    anoVacaciones++; 

                    vacacion = new Vacaciones_DiasPendientes()
                    {
                        NombreCiaContab = this.CiaContabSeleccionada_Nombre,
                        NombreDepartamento = empleado.tDepartamento.Descripcion,
                        Empleado = empleado.Empleado,
                        // TODO: agregar alias para mostrar más corto en lista ... 
                        NombreEmpleado = empleado.Nombre,
                        AliasEmpleado = empleado.Alias, 
                        NumeroVacacion = anoVacaciones,
                        AnoVacacionDesde = fechaInicioAnoVacaciones,
                        AnoVacacionHasta = fechaFinAnoVacaciones
                    };

                    // TODO: determinar la cantidad de días de vacaiones que corresponden a este año, según tabla de vacaciones. 
                    // Nota: hay una función que, justamente, determina este valor ... 

                    int cantidadDiasVacaciones; 
                    int cantidadDiasBonoVacacional;         // no lo usamos en esta función ... 
                    string message; 

                    if (!nominaFuncionesGenericas.DeterminarDiasVacacionesParaEmpleado(context, 
                                                                                       empleado.Empleado, 
                                                                                       anoVacaciones, 
                                                                                       out cantidadDiasVacaciones, 
                                                                                       out cantidadDiasBonoVacacional, 
                                                                                       out message))
                    {
                        errorMessage = message;
                        return false; 
                    }

                    vacacion.CantidadDiasVacacionesSegunTabla = cantidadDiasVacaciones;   

                    // determinamos la cantidad de días del año (ej: 360); casí siempre serán 360, pero en el último año, deteminamos la cantidad de 
                    // días en el período, para luego obtener factor prorrata (ej: 270 / 360) 

                    if (anoCompleto)
                    {
                        vacacion.CantidadDiasAnoParaCalculoProrrata = 360;      // el año es completo; asignamos 360 días 
                        vacacion.FactorProrrata = 1;                            // no se aplica un factor prorrata; asignamos 1 
                    }
                    else
                    {
                        // el último año determinado de vacaciones nunca será completo; por eso, determinamos la cantidad de días que contiene este 
                        // último año incompleto (ej: 260); luego, el factor prorrata será, simplemente: cant días año / 360 ... 

                        vacacion.CantidadDiasAnoParaCalculoProrrata = Convert.ToInt32(fechaFinAnoVacaciones.Subtract(fechaInicioAnoVacaciones).TotalDays);
                        vacacion.FactorProrrata = Math.Round(Convert.ToDecimal(vacacion.CantidadDiasAnoParaCalculoProrrata / 360D), 4);
                    }

                    vacacion.CantidadDiasVacacionesSegunTablaProrrata = Convert.ToInt32(vacacion.CantidadDiasVacacionesSegunTabla * vacacion.FactorProrrata);

                    // ahora leemos las vacaciones que ha tomado el empleado en el período específico ... 

                    // ****************************************************************************
                    // TODO: aquí debemos a leer las vacaciones desde mongo ... 

                    //var queryVacaciones = context.Vacaciones.Where(v => v.Empleado == empleado.Empleado);
                    //queryVacaciones = queryVacaciones.Where(v => v.Salida >= vacacion.AnoVacacionDesde && v.Salida <= vacacion.AnoVacacionHasta);
                    //queryVacaciones = queryVacaciones.OrderBy(v => v.Salida);
                    // ****************************************************************************

                    // -----------------------------------------------------------------------------------------------
                    // construimos un cursor para leer las vacaciones en mongo 
                    var builder = Builders<vacacion>.Filter;
                    var filter = builder.And(builder.Eq(x => x.empleado, empleado.Empleado),
                                             builder.Gte(x => x.salida, vacacion.AnoVacacionDesde),
                                             builder.Lte(x => x.salida, vacacion.AnoVacacionHasta)
                                            );
                    var sort = Builders<vacacion>.Sort.Ascending(v => v.salida); 
 
                    vacaciones_mongoCollection = null;
                    vacaciones_mongoCollection = _mongoDataBase.GetCollection<vacacion>("vacaciones");

                    var mongoCursor = vacaciones_mongoCollection.Find(filter).Sort(sort).Project(x => new { x.salida, x.regreso });    

                    DateTime? fechaRegresoUltimaVacacion = null;

                    foreach (var vacacionEmpleado in mongoCursor.ToEnumerable())
                    {
                        if (vacacion.VacacionesDisfrutadas_Desde1raVacacion == null)
                            vacacion.VacacionesDisfrutadas_Desde1raVacacion = vacacionEmpleado.salida;

                        if (vacacionEmpleado.salida != null && vacacionEmpleado.regreso != null)
                        {
                            vacacion.VacacionesDisfrutadas_TotalDias += Convert.ToInt32(vacacionEmpleado.regreso.Subtract(vacacionEmpleado.salida).TotalDays);
                            vacacion.VacacionesDisfrutadas_TotalDias++;         // siempre sumamos 1 día al cálculo de días anterior ... 

                            int cantDiasDiasFeriados = nominaDbObjectContext.ExecuteStoreQuery<int>("Select Count(*) From DiasFeriados Where Fecha Between {0} And {1}",
                                vacacionEmpleado.salida.ToString("yyyy-MM-dd"), vacacionEmpleado.regreso.ToString("yyyy-MM-dd")).FirstOrDefault();

                            vacacion.VacacionesDisfrutadas_TotalDiasFeriados += cantDiasDiasFeriados;

                            vacacion.VacacionesDisfrutadas_Cantidad++; 
                        }

                        fechaRegresoUltimaVacacion = vacacionEmpleado.regreso;
                    }

                    vacacion.VacacionesDisfrutadas_HastaUltimaVacacion = fechaRegresoUltimaVacacion;
                    vacacion.VacacionesDisfrutadas_TotalDiasDisfrutados = vacacion.VacacionesDisfrutadas_TotalDias - vacacion.VacacionesDisfrutadas_TotalDiasFeriados;

                    // determinamos la cantidad de días pendientes para el año de vacaciones del empleado; nótese que la cantidad de días pendientes 
                    // es guardada en una variable, para que esté pendiente para el registro que viene, como días pendientes del año anterior ... 
                    vacacion.DiasPendientes_AnosAnteriores = diasPendientesAnoAnterior;
                    vacacion.DiasPendientes_EsteAno = vacacion.CantidadDiasVacacionesSegunTablaProrrata - vacacion.VacacionesDisfrutadas_TotalDiasDisfrutados;
                    vacacion.DiasPendientes_Total = vacacion.DiasPendientes_AnosAnteriores + vacacion.DiasPendientes_EsteAno; 

                    diasPendientesAnoAnterior = diasPendientesAnoAnterior + vacacion.DiasPendientes_EsteAno; 
                    
                    // finalmente, grabamos el registro a la tabla ... 

                    vacacion.FRegistro = fechaRegistro; 
                    vacacion.Usuario = userName;

                    localdb.Vacaciones_DiasPendientes.Add(vacacion); 
                }
            }


            try
            {
                localdb.SaveChanges();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += " - " + ex.InnerException.Message;

                errorMessage = message;
                return false;
            }

            // el usuario puede indicar que traiga solo el último item para cada empleado; este item, muestra el año de vacaciones más reciente, 
            // con respecto a la fecha indicada para la consulta ... 

            if (this.MostrarUltimoItemCadaEmpleado)
            {
                var queryUltItem = localdb.Vacaciones_DiasPendientes.OrderBy(v => v.Empleado).ThenBy(v => v.NumeroVacacion);

                int ultimoEmpleado = -99999;
                Vacaciones_DiasPendientes ultimoRegistroLeido = null; 

                foreach (Vacaciones_DiasPendientes vac in queryUltItem)
                {
                    if (vac.Empleado == ultimoEmpleado)
                        localdb.Vacaciones_DiasPendientes.Remove(ultimoRegistroLeido);

                    ultimoRegistroLeido = vac; 
                    ultimoEmpleado = vac.Empleado; 
                }


                try
                {
                    localdb.SaveChanges();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    if (ex.InnerException != null)
                        message += " - " + ex.InnerException.Message;

                    errorMessage = message;
                    return false;
                }
            }

            return true;
        }

        public void LeerPáginaVacaciones(int page, int pageSize, string userName)
        {
            // en el método anterior, determinamos el estado de las vacaciones para la fecha indicada; 
            // en este método, simplemente leemos la página indicada desde la base de datos y la regresamos 
            // al controller ... 

            localdbNominaEntities localdb = new localdbNominaEntities();

            List<VacacionConsulta> vacacionesDisfrutadas_List = new List<VacacionConsulta>();
            VacacionConsulta vacacion;

            // nótese como obtenemos solo una página de items ... 

            int skip = (page - 1) * pageSize;

            foreach (Vacaciones_DiasPendientes v in localdb.Vacaciones_DiasPendientes.Where(x => x.Usuario == userName).
                                                                                      OrderBy(x => x.NombreEmpleado).
                                                                                      ThenBy(x => x.NumeroVacacion).
                                                                                      Skip(skip).
                                                                                      Take(pageSize)
                                                                                      )
            {
                vacacion = new VacacionConsulta()
                {
                    NombreCia = v.NombreCiaContab,
                    NombreDepartamento = v.NombreDepartamento,
                    Empleado = v.Empleado,
                    NombreEmpleado = v.NombreEmpleado,
                    AliasEmpleado = v.AliasEmpleado,
                    NumeroVacacion = v.NumeroVacacion,
                    VacacionAnoDesde = v.AnoVacacionDesde,
                    VacacionAnoHasta = v.AnoVacacionHasta,
                    CantidadDiasVacacionSegunTabla = v.CantidadDiasVacacionesSegunTabla,
                    CantidadDiasAnoParaCalculoProrrata = v.CantidadDiasAnoParaCalculoProrrata,
                    FactorProrrata = v.FactorProrrata,
                    CantidadDiasVacacionSegunTablaProrrata = v.CantidadDiasVacacionesSegunTablaProrrata,

                    VacacionesDisfrutadas_Cantidad = v.VacacionesDisfrutadas_Cantidad,
                    VacacionesDisfrutadas_Desde1raVacacion = v.VacacionesDisfrutadas_Desde1raVacacion,
                    VacacionesDisfrutadas_HastaUltimaVacacion = v.VacacionesDisfrutadas_HastaUltimaVacacion,
                    VacacionesDisfrutadas_TotalDias = v.VacacionesDisfrutadas_TotalDias,
                    VacacionesDisfrutadas_TotalDiasFeriados = v.VacacionesDisfrutadas_TotalDiasFeriados,
                    VacacionesDisfrutadas_TotalDiasDisfrutados = v.VacacionesDisfrutadas_TotalDiasDisfrutados,

                    DiasPendientes_AnosAnteriores = v.DiasPendientes_AnosAnteriores,
                    DiasPendientes_EsteAno = v.DiasPendientes_EsteAno,
                    DiasPendientes_Total = v.DiasPendientes_Total
                };

                vacacionesDisfrutadas_List.Add(vacacion);
            }

            //this.VacacionesDisfrutadas = new PagedList<VacacionConsulta>(vacacionesDisfrutadas_List.OrderBy(v => v.AliasEmpleado).ThenBy(v => v.NumeroVacacion), page, pageSize);
            //var usersAsIPagedList = new StaticPagedList<MembershipUser>(users, pageIndex + 1, pageSize, totalUserCount);

            // nótese el uso de StaticPagedList; la idea es poder páginar, pero cuando la página de items se obtiene separadamente, en forma anticipada. 
            // Recuérdese que PagedList recibe *toda* la lista (o mejor el IQueryable) y la clase efectúa la páginación ... 

            int itemCount = localdb.Vacaciones_DiasPendientes.Where(x => x.Usuario == userName).Count();

            this.VacacionesDisfrutadas = new StaticPagedList<VacacionConsulta>(vacacionesDisfrutadas_List.OrderBy(v => v.AliasEmpleado).
                                                                                                          ThenBy(v => v.NumeroVacacion),
                                                                                                          page,
                                                                                                          pageSize,
                                                                                                          itemCount);
        }


        public bool CrearDocumentoExcel(string userName, out string excelFileName, out string excelFilePath, out string resultMessage)
        {
            // las vacaciones están calculadas y en el 'localdb'; simplemente, leemos las que corresponden 
            // al usuario y regresamos el documento a descargar al usuario (download) 

            excelFileName = "";
            excelFilePath = ""; 
            resultMessage = "";

            localdbNominaEntities localdb = new localdbNominaEntities();

            //List<VacacionConsulta> vacacionesDisfrutadas_List = new List<VacacionConsulta>();
            //VacacionConsulta vacacion;

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Vacaciones - Días pendientes");

            int row = 2; 

            // agregamos el encabezado de la página 

            // header del encabezado 'superior'; propiedades comunes ... 
            worksheet.Range("B2:R3").Style.Font.Bold = false;
            worksheet.Range("B2:R3").Style.Font.FontColor = XLColor.White;
            worksheet.Range("B2:R3").Style.Font.FontSize = 11;
            worksheet.Range("B2:R3").Style.Font.FontName = "Arial";
            worksheet.Range("B2:R3").Style.Fill.BackgroundColor = XLColor.FromArgb(0x376091);

            // header: años de vacaciones
            worksheet.Range("C2:I2").Merge();
            worksheet.Cell("C2").Value = "Años de vacaciones";
            worksheet.Cell("C2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range("C2:I2").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range("C2:I2").Style.Border.OutsideBorderColor = XLColor.LightGray; 

            // header: Vacaciones tomadas
            worksheet.Range("J2:O2").Merge();
            worksheet.Cell("J2").Value = "Vacaciones tomadas";
            worksheet.Cell("J2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range("J2:O2").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range("J2:O2").Style.Border.OutsideBorderColor = XLColor.LightGray; 

            // header: Días pendientes
            worksheet.Range("P2:R2").Merge();
            worksheet.Cell("P2").Value = "Días pendientes";
            worksheet.Cell("P2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range("P2:R2").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range("P2:R2").Style.Border.OutsideBorderColor = XLColor.LightGray; 

            row++;

            // header: Días vacaciones
            worksheet.Range("F3:I3").Merge();
            worksheet.Cell("F3").Value = "Días vacaciones";
            worksheet.Cell("F3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range("F3:I3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range("F3:I3").Style.Border.OutsideBorderColor = XLColor.LightGray; 
            
            // header: Días disfrutados
            worksheet.Range("M3:O3").Merge();
            worksheet.Cell("M3").Value = "Días disfrutados";
            worksheet.Cell("M3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range("M3:O3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range("M3:O3").Style.Border.OutsideBorderColor = XLColor.LightGray; 

            row++;

            // header: header principal para todas las columnas 
            worksheet.Cell("B" + row.ToString()).Value = "Empleado";
            worksheet.Cell("C" + row.ToString()).Value = "#";
            worksheet.Cell("D" + row.ToString()).Value = "Desde";
            worksheet.Cell("E" + row.ToString()).Value = "Hasta";
            worksheet.Cell("F" + row.ToString()).Value = "Tabla";
            worksheet.Cell("G" + row.ToString()).Value = "Año";
            worksheet.Cell("H" + row.ToString()).Value = "%";
            worksheet.Cell("I" + row.ToString()).Value = "Vac";
            worksheet.Cell("J" + row.ToString()).Value = "Cant";
            worksheet.Cell("K" + row.ToString()).Value = "Desde 1ra vac";
            worksheet.Cell("L" + row.ToString()).Value = "Hasta ult vac";
            worksheet.Cell("M" + row.ToString()).Value = "Total";
            worksheet.Cell("N" + row.ToString()).Value = "Fer";
            worksheet.Cell("O" + row.ToString()).Value = "Háb";
            worksheet.Cell("P" + row.ToString()).Value = "Años ant";
            worksheet.Cell("Q" + row.ToString()).Value = "Este año";
            worksheet.Cell("R" + row.ToString()).Value = "Total";

            worksheet.Range("B4:R4").Style.Font.Bold = false;
            worksheet.Range("B4:R4").Style.Font.FontColor = XLColor.White;
            worksheet.Range("B4:R4").Style.Font.FontSize = 10;
            worksheet.Range("B4:R4").Style.Font.FontName = "Arial";
            worksheet.Range("B4:R4").Style.Fill.BackgroundColor = XLColor.FromArgb(0x95B3D7);
            worksheet.Range("B4:R4").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range("B4:R4").Style.Border.OutsideBorderColor = XLColor.FromArgb(0x236DBC);
            

            worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell("K" + row.ToString()).Style.Alignment.WrapText = true;
            worksheet.Cell("L" + row.ToString()).Style.Alignment.WrapText = true;
            worksheet.Cell("P" + row.ToString()).Style.Alignment.WrapText = true;
            worksheet.Cell("Q" + row.ToString()).Style.Alignment.WrapText = true;

            foreach (Vacaciones_DiasPendientes v in localdb.Vacaciones_DiasPendientes.Where(x => x.Usuario == userName).
                                                                                      OrderBy(x => x.NombreEmpleado).
                                                                                      ThenBy(x => x.NumeroVacacion)
                                                                                      )
            {
                row++;

                worksheet.Cell("B" + row.ToString()).Value = v.AliasEmpleado;
                worksheet.Cell("C" + row.ToString()).Value = v.NumeroVacacion.ToString();
                worksheet.Cell("D" + row.ToString()).Value = v.AnoVacacionDesde.ToString("dd-MM-yyyy");
                worksheet.Cell("E" + row.ToString()).Value = v.AnoVacacionHasta.ToString("dd-MM-yyyy");
                worksheet.Cell("F" + row.ToString()).Value = v.CantidadDiasVacacionesSegunTabla;
                worksheet.Cell("G" + row.ToString()).Value = v.CantidadDiasAnoParaCalculoProrrata;
                worksheet.Cell("H" + row.ToString()).Value = v.FactorProrrata;
                worksheet.Cell("I" + row.ToString()).Value = v.CantidadDiasVacacionesSegunTablaProrrata;
                worksheet.Cell("J" + row.ToString()).Value = v.VacacionesDisfrutadas_Cantidad;
                worksheet.Cell("K" + row.ToString()).Value = v.VacacionesDisfrutadas_Desde1raVacacion != null ? 
                                                             v.VacacionesDisfrutadas_Desde1raVacacion.Value.ToString("dd-MM-yyyy") : 
                                                             "";
                worksheet.Cell("L" + row.ToString()).Value = v.VacacionesDisfrutadas_HastaUltimaVacacion != null ? 
                                                             v.VacacionesDisfrutadas_HastaUltimaVacacion.Value.ToString("dd-MM-yyyy") : 
                                                             "";
                worksheet.Cell("M" + row.ToString()).Value = v.VacacionesDisfrutadas_TotalDias;
                worksheet.Cell("N" + row.ToString()).Value = v.VacacionesDisfrutadas_TotalDiasFeriados;
                worksheet.Cell("O" + row.ToString()).Value = v.VacacionesDisfrutadas_TotalDiasDisfrutados;
                worksheet.Cell("P" + row.ToString()).Value = v.DiasPendientes_AnosAnteriores; 
                worksheet.Cell("Q" + row.ToString()).Value = v.DiasPendientes_EsteAno;
                worksheet.Cell("R" + row.ToString()).Value = v.DiasPendientes_Total;
            }

            worksheet.Columns().AdjustToContents();

            // guardamos el documento Excel en el servidor 

            excelFileName = @"VacacionesDiasPendientes_" + userName + ".xlsx";
            excelFilePath = HttpContext.Current.Server.MapPath("~/Temp/" + excelFileName);

            try
            {
                workbook.SaveAs(excelFilePath);
            }
            catch(Exception ex)
            {
                resultMessage = ex.Message;
                if (ex.InnerException != null)
                    resultMessage += ex.InnerException.Message;

                return false; 
            }

            resultMessage = "Ok, el documento Excel ha sido generado. Haga un click en <em>download</em> para copiarlo a su PC."; 
            return true; 
        }
    }

    public class VacacionConsulta
    {
        public string NombreCia { get; set; }
        public int Empleado { get; set; }
        public string NombreEmpleado { get; set; }
        [DisplayName("Empleado")]
        public string AliasEmpleado { get; set; }
        public string NombreDepartamento { get; set; }
        [DisplayName("#")]
        public int NumeroVacacion { get; set; }
        [DisplayName("Desde")]
        [DataType(DataType.Date)]
        public DateTime VacacionAnoDesde { get; set; }
        [DisplayName("Hasta")]
        [DataType(DataType.Date)]
        public DateTime VacacionAnoHasta { get; set; }
        [DisplayName("Tabla")]
        public int CantidadDiasVacacionSegunTabla { get; set; }
        [DisplayName("Año")]
        public int CantidadDiasAnoParaCalculoProrrata { get; set; }
        [DisplayName("%")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:0.000}")]
        public decimal FactorProrrata { get; set; }
        [DisplayName("Vac")]
        public int CantidadDiasVacacionSegunTablaProrrata { get; set; }

        [DisplayName("Cant")]
        public int VacacionesDisfrutadas_Cantidad { get; set; }
        [DisplayName("Desde<br />1ra vac")]
        [DataType(DataType.Date)]
        public DateTime? VacacionesDisfrutadas_Desde1raVacacion { get; set; }
        [DisplayName("Hasta<br />Ult vac")]
        [DataType(DataType.Date)]
        public DateTime? VacacionesDisfrutadas_HastaUltimaVacacion { get; set; }
        [DisplayName("Total")]
        public int VacacionesDisfrutadas_TotalDias { get; set; }
        [DisplayName("Feriados")]
        public int VacacionesDisfrutadas_TotalDiasFeriados { get; set; }
        [DisplayName("Hábiles")]
        public int VacacionesDisfrutadas_TotalDiasDisfrutados { get; set; }

        [DisplayName("Año<br />ant")]
        public int DiasPendientes_AnosAnteriores { get; set; }
        [DisplayName("Este<br />año")]
        public int DiasPendientes_EsteAno { get; set; }
        [DisplayName("Total")]
        public int DiasPendientes_Total { get; set; }
    }

    public class Filtro
    {
        [Required(ErrorMessage="Por favor, indique la fecha para la cual desea determinar los días pendientes de vacaciones")]
        [DisplayName("Fecha de la consulta")]
        [DataType(DataType.Date)]
        public DateTime? FechaConsulta { get; set; }
        public int? Departamento { get; set; }
        public int? Empleado { get; set; }
        [DisplayName("Estado")]
        public string EstadoEmpleado { get; set; }
        public bool MostrarUltimoItemCadaEmpleado { get; set; }

        public SelectList Departamentos { get; set; }
        public SelectList Empleados { get; set; }
        public SelectList EstadosEmpleados { get; set; }

        public Filtro()
        {
            this.FillDepartamentos();
            this.FillEmpleados();
            this.FillEstadosEmpleados(); 
        }

        public void FillEstadosEmpleados()
        {
            List<SelectListItem> list = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "", Text = "Todos" }, 
                new SelectListItem() { Value = "A", Text = "Activos" }, 
                new SelectListItem() { Value = "S", Text = "Suspendidos" }
            };

            this.EstadosEmpleados = new SelectList(list, "Value", "Text", "A"); 
        }

        public void FillDepartamentos()
        {
            dbNominaEntities context = new dbNominaEntities();
            var list = (context.tDepartamentos.OrderBy(d => d.Descripcion).Select(d => new { ID = d.Departamento, Nombre = d.Descripcion })).ToList();
            this.Departamentos = new SelectList(list, "ID", "Nombre"); 
        }

        public void FillEmpleados()
        {
            dbNominaEntities context = new dbNominaEntities();
            var list = (context.tEmpleados.OrderBy(e => e.Nombre).Select(e => new { ID = e.Empleado, Nombre = e.Nombre })).ToList();
            this.Empleados = new SelectList(list, "ID", "Nombre"); 
        }
    }
}