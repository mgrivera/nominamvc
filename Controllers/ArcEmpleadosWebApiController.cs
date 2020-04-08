using DocumentFormat.OpenXml.Packaging;
using MongoDB.Driver;
using NominaASP.Models;
using NominaASP.Models.MongoDB;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;

namespace NominaASP.Controllers
{
    public class ArcEmpleadosWebApiController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage LeerDatosIniciales()
        {
            if (!User.Identity.IsAuthenticated)
            {
                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Error: por favor haga un login a esta aplicación, y luego regrese a ejecutar esta función."
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }
 
            string contabm_mongodb_connection = System.Web.Configuration.WebConfigurationManager.AppSettings["contabm_mongodb_connectionString"];
            string contabm_mongodb_name = System.Web.Configuration.WebConfigurationManager.AppSettings["contabM_mongodb_name"];

            var client = new MongoClient(contabm_mongodb_connection);
            var mongoDataBase = client.GetDatabase(contabm_mongodb_name);

            var arcEmpleadosMongoCollection = mongoDataBase.GetCollection<ARCEmpleado>("ARCEmpleado");

            try
            {
                // --------------------------------------------------------------------------------------------------------------------------
                // solo para que ocura una exception si mongo no está iniciado ... nótese que antes, cuando establecemos mongo, no ocurre un 
                // exception si mongo no está iniciado ... 
                var builder = Builders<ARCEmpleado>.Filter;
                var filter = builder.Eq(x => x.Cia, -99999999);

                arcEmpleadosMongoCollection.DeleteManyAsync(filter);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "<br />" + ex.InnerException.Message;

                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Se ha producido un error al intentar ejecutar una operación en mongodb.<br />" +
                                   "El mensaje específico del error es:<br />" + message
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }

            try
            {
                dbNominaEntities context = new dbNominaEntities();
                string usuario = User.Identity.Name;

                Compania companiaSeleccionada = context.Companias.Where(c => c.tCiaSeleccionadas.Any(t => t.UsuarioLS == usuario)).FirstOrDefault();

                if (companiaSeleccionada == null)
                {
                    var errorResult = new
                    {
                        errorFlag = true,
                        resultMessage = "No hay una Cia Contab seleccionada; debe seleccionar una Cia Contab ..."
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, errorResult);
                }
                else
                {
                    var ciaContabSeleccionada = new
                    {
                        numero = companiaSeleccionada.Numero,
                        nombre = companiaSeleccionada.Nombre
                    };

                    //// ahora obtenemos una lista de años en mongodb 
                    //var builder = Builders<ARCEmpleado>.Filter;
                    //var filter = builder.Eq(x => x.Cia, ciaContabSeleccionada.numero);

                    //// var listaAnos = arcEmpleadosMongoCollection.Find(filter).Project(Builders<ARCEmpleado>.Projection.Include(a => a.Ano)).ToList();
                    //// var listaAnos = arcEmpleadosMongoCollection.Find(filter).Project(v => new { v.Ano }).Distinct().ToList();

                    var listaAnos = arcEmpleadosMongoCollection.AsQueryable<ARCEmpleado>()
                                                               .Where(e => e.Cia == ciaContabSeleccionada.numero)
                                                               .Select(e => e.Ano)
                                                               .Distinct()
                                                               .ToList();

                    var result = new
                    {
                        errorFlag = false,
                        resultMessage = "",
                        ciaContabSeleccionada = ciaContabSeleccionada,
                        listaAnos = listaAnos
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "<br />" + ex.InnerException.Message;

                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Error: se ha producido un error al intentar efectuar una operación en la base de datos.<br />" +
                                   "El mensaje específico de error es: <br />" + message
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }
        }

        [HttpPost]
        [Route("api/ArcEmpleadosWebApi/ConstruirArcEmpleadosParaUnAno")]
        public HttpResponseMessage ConstruirArcEmpleadosParaUnAno(int ano, bool agregarMontoUtilidades, int ciaContabSeleccionada)
        {
            if (!User.Identity.IsAuthenticated)
            {
                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Error: por favor haga un login a esta aplicación, y luego regrese a ejecutar esta función."
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }

            string contabm_mongodb_connection = System.Web.Configuration.WebConfigurationManager.AppSettings["contabm_mongodb_connectionString"];
            string contabm_mongodb_name = System.Web.Configuration.WebConfigurationManager.AppSettings["contabM_mongodb_name"];

            var client = new MongoClient(contabm_mongodb_connection);
            var mongoDataBase = client.GetDatabase(contabm_mongodb_name);

            try
            {
                var arcEmpleadosMongoCollection = mongoDataBase.GetCollection<ARCEmpleado>("ARCEmpleado");

                try
                {
                    // --------------------------------------------------------------------------------------------------------------------------
                    // solo para que ocura una exception si mongo no está iniciado ... nótese que antes, cuando establecemos mongo, no ocurre un 
                    // exception si mongo no está iniciado ...  
                    var builder = Builders<ARCEmpleado>.Filter;
                    var filter = builder.Eq(x => x.Cia, -99999999);

                    arcEmpleadosMongoCollection.DeleteManyAsync(filter);
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    if (ex.InnerException != null)
                        message += "<br />" + ex.InnerException.Message;

                    var errorResult = new
                    {
                        errorFlag = true,
                        resultMessage = "Se producido un error al intentar ejecutar una operación en mongodb.<br />" +
                                       "El mensaje específico del error es:<br />" + message
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, errorResult);
                }



                dbNominaEntities context = new dbNominaEntities();
                string usuario = User.Identity.Name;

                DateTime desde = new DateTime(ano, 1, 1);
                DateTime hasta = new DateTime(ano, 12, 31);

                int cantidadArcEmpleadosAgregados = 0;
                int cantidadArcEmpleadosEliminados = 0;
                int cantidadMontosUtilidades = 0; 

                // leemos todos los empleados de la compañía seleccionada 
                var query = context.tEmpleados.Where(t => t.Cia == ciaContabSeleccionada);

                foreach (var empleado in query)
                {
                    // nótese como hacemos un semi crunchin-crunchin, pues debemos separar los montos en rubros de la nómina por mes ... 

                    // leemos todas las nóminas para el empleado leído y año indicado 

                    // *solo* si el usuario indica que desea incluir las utilidades, agregamos el tipo de rubro 9 ... 

                    var nominas = context.tNominas.Where(n => n.Empleado == empleado.Empleado).
                                                  Where(n => n.tNominaHeader.FechaNomina >= desde && n.tNominaHeader.FechaNomina <= hasta).
                                                  Where(n => n.tNominaHeader.tGruposEmpleado.Cia == ciaContabSeleccionada).
                        // nótese como seleccionamos: salario, sso (5), islr (8)
                                                  Where(n => (n.SueldoFlag || n.SalarioFlag) || (n.tMaestraRubro.TipoRubro == 5 || n.tMaestraRubro.TipoRubro == 8 || n.tMaestraRubro.TipoRubro == 9));

                    ARCEmpleado arcEmpleado = null;

                    foreach (var nomina in nominas)
                    {
                        if (arcEmpleado == null)
                        {
                            arcEmpleado = new ARCEmpleado()
                            {
                                Empleado = empleado.Empleado,
                                Nombre = empleado.Nombre,
                                Ano = ano,
                                Cia = ciaContabSeleccionada
                            };
                        }

                        int mesRubro = nomina.tNominaHeader.FechaNomina.Month;

                        // ahora vamos a registrar los montos del mes; si no existen aún, agregamos antes el mes al empleado 
                        var montosMensuales = arcEmpleado.MontosMensuales.Where(m => m.Mes == mesRubro).FirstOrDefault();

                        if (montosMensuales == null)
                        {
                            // si no existe un registro de montos para el empleado y mes, lo agregamos ahora ... 
                            montosMensuales = new ARCEmpleado_MontosMes() { Mes = mesRubro, Remuneracion = 0, Islr = 0, Sso = 0 };
                            arcEmpleado.MontosMensuales.Add(montosMensuales);
                        }

                        if (nomina.SalarioFlag || nomina.SueldoFlag)
                            montosMensuales.Remuneracion += nomina.Monto;
                        else if (nomina.tMaestraRubro.TipoRubro == 5)
                            montosMensuales.Sso += nomina.Monto;
                        else if (nomina.tMaestraRubro.TipoRubro == 8)
                            montosMensuales.Islr += nomina.Monto;
                        else if (nomina.tMaestraRubro.TipoRubro == 9)
                        {
                            // monto de utilidades; lo agregamos solo si el usuario lo indicó 
                            if (agregarMontoUtilidades)
                            {
                                montosMensuales.Remuneracion += nomina.Monto;
                                cantidadMontosUtilidades++;
                            }
                        }
                    }

                    if (arcEmpleado != null)
                    {
                        // arcEmpleado == null: un empleado que no tuvo nominas en el año ... 

                        // primero buscamos el registro en mongo; si existe, lo eliminamos ... 
                        var builder = Builders<ARCEmpleado>.Filter;
                        var filter = builder.And(
                                builder.Eq(x => x.Empleado, empleado.Empleado),
                                builder.Eq(x => x.Ano, ano),
                                builder.Eq(x => x.Cia, ciaContabSeleccionada)
                            );


                        ARCEmpleado arcAnteriorEmpleado = arcEmpleadosMongoCollection.Find(filter).FirstOrDefault(); 

                        if (arcAnteriorEmpleado != null)
                        {
                            arcEmpleadosMongoCollection.DeleteOne(filter);
                            cantidadArcEmpleadosEliminados++;
                        }


                        arcEmpleado.Ingreso = DateTime.Now;
                        arcEmpleado.UltAct = DateTime.Now;
                        arcEmpleado.Usuario = User.Identity.Name;

                        // finalmente, agregamos el registro a mongo ... 
                        arcEmpleadosMongoCollection.InsertOne(arcEmpleado);

                        cantidadArcEmpleadosAgregados++;
                    }
                }

                var result = new
                {
                    errorFlag = false,
                    resultMessage = "",
                    cantidadArcEmpleadosAgregados = cantidadArcEmpleadosAgregados,
                    cantidadArcEmpleadosEliminados = cantidadArcEmpleadosEliminados,
                    cantidadMontosUtilidades = cantidadMontosUtilidades, 
                    agregarMontoUtilidades = agregarMontoUtilidades
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "<br />" + ex.InnerException.Message;

                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Error: se ha producido un error al intentar efectuar una operación en la base de datos.<br />" +
                                   "El mensaje específico de error es: <br />" + message
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }
        }

        [HttpGet]
        [Route("api/ArcEmpleadosWebApi/ConsultarArcEmpleadosParaUnAno")]
        public HttpResponseMessage ConsultarArcEmpleadosParaUnAno(int ano, int ciaContabSeleccionada)
        {
            if (!User.Identity.IsAuthenticated)
            {
                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Error: por favor haga un login a esta aplicación, y luego regrese a ejecutar esta función."
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }

            string contabm_mongodb_connection = System.Web.Configuration.WebConfigurationManager.AppSettings["contabm_mongodb_connectionString"];
            string contabm_mongodb_name = System.Web.Configuration.WebConfigurationManager.AppSettings["contabM_mongodb_name"];

            var client = new MongoClient(contabm_mongodb_connection);
            var mongoDataBase = client.GetDatabase(contabm_mongodb_name);

            var arcEmpleadosMongoCollection = mongoDataBase.GetCollection<ARCEmpleado>("ARCEmpleado");

            try
            {
                // --------------------------------------------------------------------------------------------------------------------------
                // solo para que ocura una exception si mongo no está iniciado ... nótese que antes, cuando establecemos mongo, no ocurre un 
                // exception si mongo no está iniciado ...  
                var builder0 = Builders<ARCEmpleado>.Filter;
                var filter0 = builder0.Eq(x => x.Cia, -99999999);

                arcEmpleadosMongoCollection.DeleteManyAsync(filter0);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "<br />" + ex.InnerException.Message;

                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Se producido un error al intentar ejecutar una operación en mongodb.<br />" +
                                    "El mensaje específico del error es:<br />" + message
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }

            var builder = Builders<ARCEmpleado>.Filter;
            var filter = builder.And(
                            builder.Eq(x => x.Ano, ano),
                            builder.Eq(x => x.Cia, ciaContabSeleccionada)
                );
         
            try
            {
                var listaArcEmpleados = arcEmpleadosMongoCollection.Find(filter).ToList(); 

                var result = new
                {
                    errorFlag = false,
                    resultMessage = "",
                    arcEmpleados = listaArcEmpleados
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "<br />" + ex.InnerException.Message;

                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Error: se ha producido un error al intentar efectuar una operación en la base de datos.<br />" +
                                   "El mensaje específico de error es: <br />" + message
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }
        }

        [HttpGet]
        [Route("api/ArcEmpleadosWebApi/GetListaPlantillasWord")]
        public HttpResponseMessage GetListaPlantillasWord()
        {
            if (!User.Identity.IsAuthenticated)
            {
                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Error: por favor haga un login a esta aplicación, y luego regrese a ejecutar esta función."
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }

            try
            {
                // leemos los archivos de tipo doc y docs en el directorio /Word/ArcEmpleados/ 

                string rootPath = HostingEnvironment.ApplicationPhysicalPath;

                string path = rootPath + "Word\\ArcEmpleados\\plantillas";

                if (!Directory.Exists(path))
                {
                    var result2 = new
                    {
                        errorFlag = true,
                        resultMessage = "Error: aparentemente, el directorio <em>" + path + "</em> no existe en el servidor; debe existir " +
                                        "y contener, al menos, la plantilla <em>base</em> para la construcción de los documentos (Word) que se desea obtener."
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, result2);
                }

                var files = Directory.EnumerateFiles(@path, "*.*")
                                     .Where(s => s.EndsWith(".doc") || s.EndsWith(".docx"))
                                     .ToArray();

                // para obtener solo el nombre del file y omitir el path 
                List<string> fileNames = new List<string>();

                foreach (string file in files)
                    fileNames.Add(Path.GetFileName(file)); 


                var result = new
                {
                    errorFlag = false,
                    resultMessage = "",
                    plantillasWord = fileNames
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "<br />" + ex.InnerException.Message;

                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Error: se ha producido un error al intentar efectuar una operación en la base de datos.<br />" +
                                   "El mensaje específico de error es: <br />" + message
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }
        }



        [HttpPost]
        [Route("api/ArcEmpleadosWebApi/ConvertirWord")]
        public HttpResponseMessage ConvertirWord([FromUri] string plantillaWord, [FromBody] List<ARCEmpleado> arcEmpleados_lista)
        {
            if (!User.Identity.IsAuthenticated)
            {
                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Error: por favor haga un login a esta aplicación, y luego regrese a ejecutar esta función."
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }


            if (arcEmpleados_lista == null || arcEmpleados_lista.Count() == 0)
            {
                string message = "Aparentemente, no existe información para construir el documento que Ud. ha requerido. " +
                        "<br /><br /> Probablemente Ud. no ha aplicado un filtro y seleccionado información aún." +
                        "<br /><br /> Si Ud. no ha aplicado un filtro para consultar registros, hágalo ahora y luego intente nuevamente esta función.";

                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = message
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }



            try
            {
                string rootPath = HostingEnvironment.ApplicationPhysicalPath;
                string path = rootPath + "Word\\ArcEmpleados";

                if (!File.Exists(path + "\\plantillas\\" + plantillaWord))
                {
                    string message = "Error: no hemos encontrado el documento Word (plantilla) necesario para ejecutar esta función.";

                    var errorResult = new
                    {
                        errorFlag = true,
                        resultMessage = message
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, errorResult);
                }

                // eliminamos los archivos que pudieron haber sido agregados, cuando este proceso fue ejecutado antes ... 
                string userName = User.Identity.Name.Replace("@", "_").Replace(".", "_");

                string[] filePaths = Directory.GetFiles(@path, "*_" + userName + ".docx");
                foreach (string filePath in filePaths)
                    File.Delete(filePath);


                dbNominaEntities context = new dbNominaEntities();

                DateTime desde = new DateTime(arcEmpleados_lista.First().Ano, 1, 1);
                DateTime hasta = new DateTime(arcEmpleados_lista.First().Ano, 12, 31);



                int cantidadRegistrosLeidos = 0;
                WordprocessingDocument doc = null;

                foreach (var arc in arcEmpleados_lista.OrderBy(a => a.Nombre))
                {
                    // éste es el nombre del nuevo archivo; nótese como usamos el nombre del empleado, para que luego el resultado final esté ordenado 
                    // por nombre ... 

                    string newFile = @path + "\\" + 
                                     System.IO.Path.GetFileNameWithoutExtension(plantillaWord) + 
                                     "_" +
                                     arc.Nombre.Replace(" ", "-").Replace(".", "-").Replace(",", "-") + 
                                     "_" + 
                                     userName + 
                                     ".docx";

                    File.Copy(path + "\\plantillas\\" + plantillaWord, newFile);            // nótese que las plantillas están en el directorio 'plantillas' 

                    doc = WordprocessingDocument.Open(newFile, true);

                    tEmpleado empleado = context.tEmpleados.Where(e => e.Empleado == arc.Empleado).FirstOrDefault();

                    if (empleado == null)
                    {
                        string message = "Error: no hemos podido leer el empleado <em>" + arc.Nombre + "</em> (" + arc.Empleado.ToString() + ") en la base de datos.";

                        var errorResult = new
                        {
                            errorFlag = true,
                            resultMessage = message
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, errorResult);
                    }

                    TextReplacer.SearchAndReplace(doc, "<<desde>>", desde.ToString("dd-MM-yyyy"), false);
                    TextReplacer.SearchAndReplace(doc, "<<hasta>>", hasta.ToString("dd-MM-yyyy"), false);

                    TextReplacer.SearchAndReplace(doc, "<<nombre>>", empleado.Nombre, false);
                    TextReplacer.SearchAndReplace(doc, "<<cedula>>", empleado.Cedula == null ? "indefinida" : empleado.Cedula, false);
                    TextReplacer.SearchAndReplace(doc, "<<telefono>>", empleado.Telefono1 == null ? "indefinido" : empleado.Telefono1, false);

                    decimal salarioAcumulado = 0;
                    decimal impuestoAcumulado = 0;

                    // para registrar los meses *sin* movimientos de nómina y eliminar los 'campos de combinación' del documento Word para los mismos ... 
                    var mesesProcesados = new List<int>(); 

                    foreach (ARCEmpleado_MontosMes monto in arc.MontosMensuales.OrderBy(m => m.Mes))
                    {
                        salarioAcumulado += monto.Remuneracion;
                        impuestoAcumulado += monto.Islr;

                        mesesProcesados.Add(monto.Mes);

                        switch (monto.Mes)
                        {
                            case 1:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioEne>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetEne>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetEne>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumEne>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumEne>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }

                            case 2:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioFeb>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetFeb>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetFeb>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumFeb>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumFeb>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }

                            case 3:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioMar>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetMar>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetMar>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumMar>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumMar>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }

                            case 4:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioAbr>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetAbr>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetAbr>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumAbr>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumAbr>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }

                            case 5:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioMay>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetMay>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetMay>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumMay>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumMay>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }

                            case 6:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioJun>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetJun>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetJun>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumJun>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumJun>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }

                            case 7:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioJul>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetJul>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetJul>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumJul>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumJul>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }

                            case 8:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioAgo>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetAgo>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetAgo>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumAgo>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumAgo>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }

                            case 9:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioSep>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetSep>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetSep>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumSep>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumSep>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }

                            case 10:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioOct>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetOct>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetOct>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumOct>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumOct>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }

                            case 11:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioNov>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetNov>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetNov>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumNov>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumNov>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }

                            case 12:
                                {
                                    decimal porcIslr = monto.Islr == 0 || monto.Remuneracion == 0 ? 0 : (monto.Islr * 100 / monto.Remuneracion);

                                    TextReplacer.SearchAndReplace(doc, "<<salarioDic>>", Math.Abs(monto.Remuneracion).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetDic>>", Math.Abs(porcIslr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetDic>>", Math.Abs(monto.Islr).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumDic>>", Math.Abs(salarioAcumulado).ToString("N2"), false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumDic>>", Math.Abs(impuestoAcumulado).ToString("N2"), false);

                                    break;
                                }
                        }
                    }

                    // ---------------------------------------------------------------------------------------------------------------------------
                    // si el empleado no tiene movimientos de nómina para algunos meses, reemplazamos los campos de combinación por espacios en el 
                    // documento Word 

                    for (int i = 1; i <= 12; i++ )
                    {
                        if (mesesProcesados.Exists(mes => mes == i))
                            continue; 

                        // Ok, el mes no existe (no tuvo movimientos de nómina; reemplazamos sus campos de combinación por espacios  
                        // (para que no se vean en el documento Word final) 

                        switch (i)
                        {
                            case 1:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioEne>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetEne>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetEne>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumEne>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumEne>>", " ", false);

                                    break;
                                }

                            case 2:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioFeb>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetFeb>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetFeb>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumFeb>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumFeb>>", " ", false);

                                    break;
                                }

                            case 3:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioMar>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetMar>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetMar>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumMar>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumMar>>", " ", false);

                                    break;
                                }

                            case 4:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioAbr>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetAbr>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetAbr>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumAbr>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumAbr>>", " ", false);

                                    break;
                                }

                            case 5:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioMay>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetMay>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetMay>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumMay>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumMay>>", " ", false);

                                    break;
                                }

                            case 6:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioJun>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetJun>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetJun>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumJun>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumJun>>", " ", false);

                                    break;
                                }

                            case 7:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioJul>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetJul>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetJul>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumJul>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumJul>>", " ", false);

                                    break;
                                }

                            case 8:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioAgo>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetAgo>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetAgo>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumAgo>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumAgo>>", " ", false);

                                    break;
                                }

                            case 9:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioSep>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetSep>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetSep>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumSep>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumSep>>", " ", false);

                                    break;
                                }

                            case 10:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioOct>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetOct>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetOct>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumOct>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumOct>>", " ", false);

                                    break;
                                }

                            case 11:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioNov>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetNov>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetNov>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumNov>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumNov>>", " ", false);

                                    break;
                                }

                            case 12:
                                {
                                    TextReplacer.SearchAndReplace(doc, "<<salarioDic>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<porcRetDic>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impRetDic>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<salAcumDic>>", " ", false);
                                    TextReplacer.SearchAndReplace(doc, "<<impAcumDic>>", " ", false);

                                    break;
                                }
                        }
                    }


                    doc.Close();
                    cantidadRegistrosLeidos++;
                }

                if (cantidadRegistrosLeidos == 0)
                {
                    string message = "Aparentemente, no existe información para construir el documento que Ud. ha requerido. " +
                        "<br /><br /> Probablemente Ud. no ha aplicado un filtro y seleccionado información aún.";

                    var errorResult = new
                    {
                        errorFlag = true,
                        resultMessage = message
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, errorResult);
                }

                context = null;
                doc.Dispose();

                string newFileName = "";

                try
                {
                    // combinamos todos los documentos individuales en uno solo

                    List<OpenXmlPowerTools.Source> sources = new List<OpenXmlPowerTools.Source>();

                    filePaths = Directory.GetFiles(@path, System.IO.Path.GetFileNameWithoutExtension(plantillaWord) + "*_" + userName + ".docx");

                    foreach (string filePath in filePaths)
                        sources.Add(new OpenXmlPowerTools.Source(new WmlDocument(filePath), true));

                    newFileName = path + "\\" + System.IO.Path.GetFileNameWithoutExtension(plantillaWord) + "_" + userName + ".docx";

                    // intentamos eliminar antes el archivo ... 
                    if (File.Exists(newFileName))
                        File.Delete(newFileName);

                    DocumentBuilder.BuildDocument(sources, newFileName);

                    // eliminamos los archivos temporales que fueron agregados 
                    // TODO: cómo eliminar solo los archivos temporales y no el resultado (ie: acuse recibo geh_manuel.docx ??? 

                    filePaths = Directory.GetFiles(@path, "*_" + userName + ".docx");
                    foreach (string filePath in filePaths)
                        if (filePath != newFileName)            // el archivo resultado debe permanecer ... 
                            File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    string message = "Error: hemos obtenido un error al intentar consolidar los documentos (Word) individuales (por factura) en uno solo.<br />" +
                        "El mensaje específico del error es: " + ex.Message;
                    if (ex.InnerException != null)
                        message += "<br />" + ex.InnerException.Message;

                    var errorResult = new
                    {
                        errorFlag = true,
                        resultMessage = message
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, errorResult);
                }

                var result = new
                {
                    errorFlag = false,
                    resultMessage = ""
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "<br />" + ex.InnerException.Message;

                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Error: se ha producido un error al intentar efectuar una operación en la base de datos.<br />" +
                                   "El mensaje específico de error es: <br />" + message
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }
        }


        [HttpGet]
        [Route("api/ArcEmpleadosWebApi/DownloadDocumentoWord")]
        public HttpResponseMessage DownloadDocumentoWord([FromUri] string plantillaWord)
        {
            if (!User.Identity.IsAuthenticated)
            {
                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = "Error: por favor haga un login a esta aplicación, y luego regrese a ejecutar esta función."
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }


            try
            {
                string rootPath = HostingEnvironment.ApplicationPhysicalPath;
                string path = rootPath + "Word\\ArcEmpleados";

                string userName = User.Identity.Name.Replace("@", "_").Replace(".", "_");
                string fileToDownload = path + "\\" + System.IO.Path.GetFileNameWithoutExtension(plantillaWord) + "_" + userName + ".docx";

                // el archivo debe existir ... 
                if (!File.Exists(fileToDownload))
                {
                    string message = "Error: no hemos encontrado el documento Word que Ud. desea descargar (download).<br />" + 
                        "Es probable que Ud. no lo haya construído aún; debe ejecutar antes el paso que permite obtenerlo.";

                    var errorResult = new
                    {
                        errorFlag = true,
                        resultMessage = message
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, errorResult);
                }

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

                var stream = new FileStream(fileToDownload, FileMode.Open);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = fileToDownload;

                return result;
               
            }
            catch (Exception ex)
            {
                string message = "Error: hemos obtenido un error al intentar consolidar los documentos (Word) individuales (por factura) en uno solo.<br />" +
                    "El mensaje específico del error es: " + ex.Message;
                if (ex.InnerException != null)
                    message += "<br />" + ex.InnerException.Message;

                var errorResult = new
                {
                    errorFlag = true,
                    resultMessage = message
                };

                return Request.CreateResponse(HttpStatusCode.OK, errorResult);
            }
        }
    }
}