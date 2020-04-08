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
using MongoDB.Driver;

namespace NominaASP.Controllers
{
    public class VacacionesConsultaWebApiController : ApiController
    {
        private IMongoDatabase _mongoDataBase = null;

        [HttpGet]
        public IEnumerable<VacacionConsulta> GetVacaciones(int empleado, DateTime desde, DateTime hasta)
        {
            // establecemos una conexión a mongodb; específicamente, a la base de datos del programa contabM; allí se registrará 
            // todo en un futuro; además, ahora ya están registradas las vacaciones ... 
            string contabm_mongodb_connection = System.Web.Configuration.WebConfigurationManager.AppSettings["contabm_mongodb_connectionString"];
            string contabm_mongodb_name = System.Web.Configuration.WebConfigurationManager.AppSettings["contabM_mongodb_name"];

            var client = new MongoClient(contabm_mongodb_connection);
            _mongoDataBase = client.GetDatabase(contabm_mongodb_name);
            // --------------------------------------------------------------------------------------------------------------------------

            var vacaciones_mongoCollection = _mongoDataBase.GetCollection<vacacion>("vacaciones");

            try
            {
                // --------------------------------------------------------------------------------------------------------------------------
                // solo para que ocura una exception si mongo no está iniciado ... nótese que antes, cuando establecemos mongo, no ocurre un 
                // exception si mongo no está iniciado ...  
                var builder = Builders<vacacion>.Filter;
                var filter = builder.Eq(x => x.cia, -99999999);

                vacaciones_mongoCollection.DeleteManyAsync(filter);
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

            var empleadoItem = context.tEmpleados.Where(e => e.Empleado == empleado).Select(e => new { e.Empleado, e.Nombre }).FirstOrDefault();

            var builder2 = Builders<vacacion>.Filter;
            var filter2 = builder2.And(
                                builder2.Eq(x => x.empleado, empleado),
                                builder2.Gte(x => x.salida, desde),
                                builder2.Lte(x => x.salida, hasta)
                        );
            var sort = Builders<vacacion>.Sort.Ascending(v => v.salida);

            vacaciones_mongoCollection = null;
            vacaciones_mongoCollection = _mongoDataBase.GetCollection<vacacion>("vacaciones");

            var mongoCursor = vacaciones_mongoCollection.Find(filter2).Project(x => new { x.salida, x.regreso }).Sort(sort).ToCursor(); 

            foreach (var v in mongoCursor.ToEnumerable())
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