using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
//using NominaASP.Models.Nomina;
using NominaASP.Models;

namespace NominaASP.Empleados.Consulta
{
    public partial class EmpleadosConsulta : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }

            if (!Page.IsPostBack)
            {
                dbNominaEntities context = new dbNominaEntities();
                string usuario = User.Identity.Name;

                Compania companiaSeleccionada = context.Companias.Where(c => c.tCiaSeleccionadas.Any(t => t.UsuarioLS == usuario)).FirstOrDefault(); 

                if (companiaSeleccionada == null)
                {
                    string errorMessage = "No hay una Cia Contab seleccionada; debe seleccionar una Cia Contab ...";

                    //CustomValidator1.IsValid = false;
                    //CustomValidator1.ErrorMessage = errorMessage;

                    this.CiaContabSeleccionada_span.InnerHtml = errorMessage; 

                    return;
                }

                this.CiaContabSeleccionada_span.InnerHtml = companiaSeleccionada.Nombre; 

                // para guardar la cia contab seleccionada 

                Session["CiaContabSeleccionada"] = companiaSeleccionada.Numero;


                // --------------------------------------------------------------------------------------------------------------------------
                // construimos el objeto que mantiene los criterios usados por el usuario para establecer un filtro y pasarlo al reporte ... 
                Session["ConsultaEmpleados_CriteriosFiltro"] = null;
                ConsultaEmpleados_CriteriosFiltro criterioFiltro = new ConsultaEmpleados_CriteriosFiltro();

                if (companiaSeleccionada != null)
                    criterioFiltro.CiaContab = companiaSeleccionada.Numero;

                Session["ConsultaEmpleados_CriteriosFiltro"] = criterioFiltro;
            }
        }

        public IQueryable<NominaASP.Models.tEmpleado> Empleados_GridView_GetData()
        {
            if (Session["CiaContabSeleccionada"] == null)
                Session["CiaContabSeleccionada"] = -999; 

            int ciaContabSeleccionada = Convert.ToInt32(Session["CiaContabSeleccionada"]); 

            dbNominaEntities context = new dbNominaEntities();

            var query = context.tEmpleados.Include("tDepartamento").Include("tCargo").Include("Compania").Where(e => e.Cia == ciaContabSeleccionada); 

            if (query.Count() == 0)
            {
                string errorMessage = "No se han seleccionado registros que mostrar; probablemente no se ha indicado un filtro correcto a esta página.";

                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;

                return null;
            }

            if (this.Empleados_DropDownList.SelectedValue != "-999")
            {
                int pk = Convert.ToInt32(this.Empleados_DropDownList.SelectedValue);
                query = query.Where(e => e.Empleado == pk);
            }

            if (this.Departamentos_DropDownList.SelectedValue != "-999")
            {
                int pk = Convert.ToInt32(this.Departamentos_DropDownList.SelectedValue);
                query = query.Where(e => e.Departamento == pk);
            }

            if (this.Cargos_DropDownList.SelectedValue != "-999")
            {
                int pk = Convert.ToInt32(this.Cargos_DropDownList.SelectedValue);
                query = query.Where(e => e.Cargo == pk);
            }

            if (this.Estados_DropDownList.SelectedValue != "-999")
                query = query.Where(e => e.Status == this.Estados_DropDownList.SelectedValue);

            if (this.SituacionActual_DropDownList.SelectedValue != "-999")
                query = query.Where(e => e.SituacionActual == this.SituacionActual_DropDownList.SelectedValue); 

            query = query.OrderBy(e => e.Nombre);

            return query;
        }

        protected void Empleados_GridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }
        }

        protected void Empleados_GridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView gv = (GridView)sender;
            gv.SelectedIndex = -1;

            //gv.PageIndex = e.NewPageIndex;
        }

        public string GetStatusEmpleado(string status)
        {
            switch (status)
            {
                case "A":
                    return "Activo";
                case "S":
                    return "Suspendido";
            }
            return "Indefinido";
        }

        public string GetSituacionActualEmpleado(string situacionActual)
        {
            switch (situacionActual)
            {
                case "NO":
                    return "Normal";
                case "VA":
                    return "Vacaciones";
                case "RE":
                    return "Reposo";
                case "LI":
                    return "Liquidado";
            }
            return "Indefinido";
        }

        public string GetSueldoEmpleado(int empleadoID)
        {
            decimal? sueldoEmpleado = null;

            using (dbNominaEntities context = new dbNominaEntities())
            {
                sueldoEmpleado = context.Empleados_Sueldo.Where(s => s.Empleado == empleadoID).
                                                          OrderByDescending(s => s.Desde).
                                                          Select(s => s.Sueldo).
                                                          FirstOrDefault();
            }

            return sueldoEmpleado == null ? "" : sueldoEmpleado.Value.ToString("N2");
        }

        public string GetMontoCestaTickets(decimal? montoCestaTickets)
        {
            return montoCestaTickets == null ? "" : montoCestaTickets.Value.ToString("N2");
        }

        protected void Filter_Ok_Button_Click(object sender, EventArgs e)
        {
            // mantenemos el criterio indicado en un objeto en session, para usarlo al generar el reporte ... 

            Session["ConsultaEmpleados_CriteriosFiltro"] = null; 
            ConsultaEmpleados_CriteriosFiltro criterioFiltro = new ConsultaEmpleados_CriteriosFiltro();

            if (Session["CiaContabSeleccionada"] != null)
                criterioFiltro.CiaContab = Convert.ToInt32(Session["CiaContabSeleccionada"]);

            if (this.Empleados_DropDownList.SelectedValue != "-999")
                criterioFiltro.Empleado = Convert.ToInt32(this.Empleados_DropDownList.SelectedValue);
              
            if (this.Departamentos_DropDownList.SelectedValue != "-999")
                criterioFiltro.Departamento = Convert.ToInt32(this.Departamentos_DropDownList.SelectedValue);

            if (this.Cargos_DropDownList.SelectedValue != "-999")
                criterioFiltro.Cargo = Convert.ToInt32(this.Cargos_DropDownList.SelectedValue);
                
            if (this.Estados_DropDownList.SelectedValue != "-999")
                criterioFiltro.Status = this.Estados_DropDownList.SelectedValue;

            if (this.SituacionActual_DropDownList.SelectedValue != "-999")
                criterioFiltro.SituacionActual = this.SituacionActual_DropDownList.SelectedValue;

            Session["ConsultaEmpleados_CriteriosFiltro"] = criterioFiltro; 

            Empleados_GridView.DataBind();
            this.Empleados_GridView.PageIndex = 0; 
            TabContainer1.ActiveTabIndex = 0; 
        }

        public IQueryable<NominaASP.Models.tEmpleado> Empleados_DropDownList_SelectMethod()
        {
            int ciaContabSeleccionada = -999;

            if (Session["CiaContabSeleccionada"] != null)
                ciaContabSeleccionada = Convert.ToInt32(Session["CiaContabSeleccionada"]);

            dbNominaEntities context = new dbNominaEntities();

            var query = context.tEmpleados.Where(e => e.Cia == ciaContabSeleccionada);

            query = query.OrderBy(e => e.Nombre);

            return query;
        }

        public IQueryable<NominaASP.Models.tCargo> Cargos_DropDownList_SelectMethod()
        {
            dbNominaEntities context = new dbNominaEntities();

            IOrderedQueryable<tCargo> query = context.tCargos;
            query = query.OrderBy(e => e.Descripcion);

            return query;
        }

        public IQueryable<NominaASP.Models.tDepartamento> Departamentos_DropDownList_SelectMethod()
        {
            dbNominaEntities context = new dbNominaEntities();

            IOrderedQueryable<tDepartamento> query = context.tDepartamentos;
            query = query.OrderBy(e => e.Descripcion);

            return query;
        }
    }
}