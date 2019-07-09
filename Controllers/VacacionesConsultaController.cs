using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using NominaASP.ViewModels.VacacionesConsulta;
using NominaASP.Models; 

namespace NominaASP.Controllers
{
    public class VacacionesConsultaController : Controller
    {
        public ActionResult Index(VacacionesConsulta model)
        {
            // lo primero que hacemos es leer la compañía Contab seleccinada .... 

            if (User.Identity == null || string.IsNullOrEmpty(User.Identity.Name))
            {
                // un timeout podría hacer que el usuario no esté authenticado ... 
                string errorMessage = "Ud. debe hacer un login en la aplicación antes de continuar. Por vaya a Home e intente hacer un login en la aplicación.";
                this.ModelState.AddModelError("", errorMessage);
                return View(model);
            }

            // este valor viene en session desde otras páginas ... 

            if (model.CiaContabSeleccionada == null)
            {
                dbNominaEntities context = new dbNominaEntities();
                string usuario = User.Identity.Name;

                Compania companiaSeleccionada = context.Companias.Where(c => c.tCiaSeleccionadas.Any(t => t.UsuarioLS == usuario)).FirstOrDefault();

                if (companiaSeleccionada == null)
                {
                    string errorMessage = "No hay una Cia Contab seleccionada; debe seleccionar una Cia Contab ...";
                    this.ModelState.AddModelError("", errorMessage);
                    return View(model);
                }

                model.CiaContabSeleccionada = companiaSeleccionada.Numero;
                model.CiaContabSeleccionada_Nombre = companiaSeleccionada.Nombre; 
            }

            // cuando model trae una fecha, es que venimos desde el filtro; 
            // cuando viene una página, es que el usuario ya filtro y los resultados son mostrados en la página ... 

            if (model.FechaConsulta != null && model.Page == null)
            {
                string errorMessage = "";
                if (!model.CalcularVacaciones(this.User.Identity.Name, out errorMessage))
                {
                    this.ModelState.AddModelError("", errorMessage);
                    //model.Error = true;
                    //model.Message = errorMessage; 
                }
                    

                //model.CrearDocumentoExcel(this.User.Identity.Name);

                model.Page = 1; 
            }

            if (model.Page != null)
                model.LeerPáginaVacaciones(model.Page.Value, 20, this.User.Identity.Name); 

            return View(model);
        }

        public ActionResult Filtro()
        {
            Filtro filtro = new Filtro();

            //filtro.FillDepartamentos();
            //filtro.FillEmpleados();

            return View(filtro);
        }

        //public ActionResult Download()
        //{
        //    if (User.Identity == null || string.IsNullOrEmpty(User.Identity.Name))
        //    {
        //        // un timeout podría hacer que el usuario no esté authenticado ... 
        //        string errorMessage = "Ud. debe hacer un login en la aplicación antes de continuar. Por vaya a Home e intente hacer un login en la aplicación.";
        //        this.ModelState.AddModelError("", errorMessage);

        //        VacacionesConsulta model = null; 
        //        return View(model);
        //    }


        //    // TODO: crear el documento Excel ... 


        //    var document = ""; 
        //    var cd = new System.Net.Mime.ContentDisposition
        //    {
        //        // for example foo.bak
        //        FileName = document.FileName, 

        //        // always prompt the user for downloading, set to true if you want 
        //        // the browser to try to show the file inline
        //        Inline = false, 
        //    };
        //    Response.AppendHeader("Content-Disposition", cd.ToString());
        //    return File(document.Data, document.ContentType);
        //}

        [HttpPost]
        public ActionResult Filtro(Filtro filtro)
        {
            if (this.ModelState.IsValid)
            {
                return RedirectToAction("Index", new 
                { 
                    FechaConsulta = filtro.FechaConsulta, 
                    EstadoEmpleado = filtro.EstadoEmpleado,
                    Departamento = filtro.Departamento, 
                    Empleado = filtro.Empleado,
                    MostrarUltimoItemCadaEmpleado = filtro.MostrarUltimoItemCadaEmpleado
                });
            }

            return View(filtro); 
        }

        public ActionResult ConstruirDocumentoExcel(VacacionesConsulta model)
        {
            string errorMessage = ""; 

            // lo primero que hacemos es leer la compañía Contab seleccinada .... 

            if (User.Identity == null || string.IsNullOrEmpty(User.Identity.Name))
            {
                // un timeout podría hacer que el usuario no esté authenticado ... 
                errorMessage = "Ud. debe hacer un login en la aplicación antes de continuar. Por vaya a Home e intente hacer un login en la aplicación.";
                model.Error = true;
                model.Message = errorMessage; 

                return View(model);
            }

            // este valor viene en session desde otras páginas ... 

            if (model.CiaContabSeleccionada == null)
            {
                errorMessage = "No hay una Cia Contab seleccionada; debe seleccionar una Cia Contab ...";
                model.Error = true;
                model.Message = errorMessage; 

                return View(model);
            }

            string excelFileName = "";
            string resultMessage = "";
            string excelFilePath = ""; 

            if (!model.CrearDocumentoExcel(this.User.Identity.Name, out excelFileName, out excelFilePath, out resultMessage))
            {
                model.Error = true;
                model.Message = errorMessage; 
            }
            else
            {
                // para indicar al view que muestre el mecanismo para el download ... 

                this.ViewBag.ShowDownloadButton = true;

                this.ViewBag.UriAddress = HttpContext.Request.Url.Scheme + 
                                          "://" + HttpContext.Request.Url.Authority +
                                          (HttpContext.Request.ApplicationPath.EndsWith("/") ? HttpContext.Request.ApplicationPath : HttpContext.Request.ApplicationPath + "/") 
                                          + 
                                          "Temp/" + 
                                          excelFileName; 

                model.Message = resultMessage; 
            }
                

            if (model.Page != null)
                model.LeerPáginaVacaciones(model.Page.Value, 20, this.User.Identity.Name);

            return View("Index", model);
        }
	}
}