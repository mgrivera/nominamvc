using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NominaASP.Models;
using NominaASP.Models.MongoDB;
using NominaASP.ViewModels.WebApi;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NominaASP.Controllers
{
    public class VacacionesConsultaWebApiController : ApiController
    {
        // GET api/<controller>
        [HttpGet]
        //[ActionName("GetVacaciones")]
        public IEnumerable<VacacionConsulta> GetVacaciones(int empleado, DateTime desde, DateTime hasta)
        {
            // --------------------------------------------------------------------------------------------------------------------------
            // establecemos una conexión a mongodb; específicamente, a la base de datos del programa contabM; allí se registrará 
            // todo en un futuro; además, ahora ya están registradas las vacaciones ... 
            string contabM_mongodb_name = System.Web.Configuration.WebConfigurationManager.AppSettings["contabM_mongodb_name"];

            var client = new MongoClient("mongodb://localhost");
            var server = client.GetServer();
            // nótese como el nombre de la base de datos mongo (de contabM) está en el archivo webAppSettings.config; 
            // en este db se registran las vacaciones 
            var mongoDataBase = server.GetDatabase(contabM_mongodb_name);

            var vacaciones_mongoCollection = mongoDataBase.GetCollection<vacacion>("vacaciones");

            try
            {
                // --------------------------------------------------------------------------------------------------------------------------
                // solo para que ocura una exception si mongo no está iniciado ... nótese que antes, cuando establecemos mongo, no ocurre un 
                // exception si mongo no está iniciado ...  

                var queryDeleteDocs = Query<vacacion>.EQ(x => x.cia, -9999999);
                vacaciones_mongoCollection.Remove(queryDeleteDocs);
            }
            catch (Exception ex)
            {
                string message = "Error al intentar establecer una conexión a la base de datos (mongo) de 'contabM'; el mensaje de error es: " + ex.Message;
                //errorMessage = message;
                //return false;
            }
            // --------------------------------------------------------------------------------------------------------------------------

            List<VacacionConsulta> vacaciones = new List<VacacionConsulta>();
            VacacionConsulta vacacion;

            dbNominaEntities context = new dbNominaEntities();

            // obtenemos el ObjectContext para este context, pues más abajo lo usamos para hacer un ExcecuteStoreCommand ... 
            //var nominaDbObjectContext = (context as IObjectContextAdapter).ObjectContext;

            var empleadoItem = context.tEmpleados.Where(e => e.Empleado == empleado).Select(e => new { e.Empleado, e.Nombre }).FirstOrDefault();

            //query = query.Where(v => v.Salida != null && v.Salida >= desde);
            //query = query.Where(v => v.Salida != null && v.Salida <= hasta);
            //query = query.OrderBy(v => v.Salida);

            var mongoQuery = Query.And(
                            Query<vacacion>.EQ(x => x.empleado, empleado),
                            Query<vacacion>.GTE(x => x.salida, desde),
                            Query<vacacion>.LTE(x => x.salida, hasta)
                        );

            vacaciones_mongoCollection = null;
            vacaciones_mongoCollection = mongoDataBase.GetCollection<vacacion>("vacaciones");

            var mongoCursor = vacaciones_mongoCollection.Find(mongoQuery).Select(x => new { x.salida, x.regreso });

            foreach (var v in mongoCursor.OrderBy(x => x.salida))
            {
                vacacion = new VacacionConsulta()
                {
                    Empleado = empleadoItem.Empleado,
                    Nombre = empleadoItem.Nombre,
                    Salida = v.salida,
                    Regreso = v.regreso
                };

                vacacion.DiasHabiles = Convert.ToInt32(vacacion.Regreso.Subtract(vacacion.Salida).TotalDays);
                vacacion.DiasHabiles++;         // siempre sumamos 1 día al cálculo de días anterior ... 

                // leemos lod  días feriados para el período de vacaciones ... 
                var query2 = context.DiasFeriados.Where(f => f.Fecha >= vacacion.Salida && f.Fecha <= vacacion.Regreso);
                query2 = query2.OrderBy(f => f.Fecha);

                int cantDiasDiasFeriados = 0; 

                foreach (var f in query2)
                {
                    if (string.IsNullOrEmpty(vacacion.ListaDiasFeriados))
                        vacacion.ListaDiasFeriados += f.Fecha.ToString("d-M");
                    else
                        vacacion.ListaDiasFeriados += ", " + f.Fecha.ToString("d-M");
                    
                    cantDiasDiasFeriados++; 
                }

                if (!string.IsNullOrEmpty(vacacion.ListaDiasFeriados))
                    vacacion.ListaDiasFeriados += "."; 

                vacacion.DiasFeriados = cantDiasDiasFeriados;
                vacacion.DiasDisfrutados = vacacion.DiasHabiles - vacacion.DiasFeriados;

                vacaciones.Add(vacacion); 
            }

            return vacaciones;
        }
    }
}