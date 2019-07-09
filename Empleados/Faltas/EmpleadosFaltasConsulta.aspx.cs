using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using NominaASP.Code;
//using NominaASP.Models.Nomina;
using NominaASP.Models;

namespace NominaASP.Empleados.Faltas
{
    public partial class EmpleadosFaltasConsulta : System.Web.UI.Page
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

                string errorMessage = ""; 

                if (companiaSeleccionada == null)
                {
                    errorMessage = "No hay una Cia Contab seleccionada; debe seleccionar una Cia Contab ...";

                    //CustomValidator1.IsValid = false;
                    //CustomValidator1.ErrorMessage = errorMessage;

                    this.CiaContabSeleccionada_span.InnerHtml = errorMessage; 

                    return;
                }

                this.CiaContabSeleccionada_span.InnerHtml = companiaSeleccionada.Nombre; 

                // para guardar la cia contab seleccionada 

                Session["CiaContabSeleccionada"] = companiaSeleccionada.Numero;

                // -------------------------------------------------------------------------------------------------------------------
                //  intentamos recuperar el state de esta página; en general, lo intentamos con popups filtros 

                if (!(Membership.GetUser().UserName == null))
                {
                    KeepPageState MyKeepPageState = new KeepPageState(Membership.GetUser().UserName, this.GetType().Name.ToString());
                    MyKeepPageState.ReadStateFromFile(this, this.Filtro_Div.Controls);
                    MyKeepPageState = null;
                }

                this.Empleados_DropDownList.DataBind();
                this.Departamentos_DropDownList.DataBind();
                this.Estados_DropDownList.DataBind();
                this.SituacionActual_DropDownList.DataBind(); 

                // -------------------------------------------------------------------------------------------------------------------
                // como el filtro se reestablece, intentamos recuperarlo en un objeto que sirve para aplicar el filtro en el SelectMethod del GridView ... 

                if (!PrepararFilterObject(out errorMessage))
                {
                    CustomValidator1.IsValid = false;
                    CustomValidator1.ErrorMessage = errorMessage;

                    return;
                }
            }
        }

        public IList<NominaASP.Empleados.Faltas.EmpleadoFaltaConsulta> Empleados_GridView_GetData
            (int maximumRows, int startRowIndex, out int totalRowCount)
        {
            // nótese como el filtro indicado por el usuario es guardado en un objeto ... 

            ConsultaEmpleadosFaltas_CriteriosFiltro criterioFiltro;

            if (Session["ConsultaEmpleadosFaltas_CriteriosFiltro"] != null)
                criterioFiltro = Session["ConsultaEmpleadosFaltas_CriteriosFiltro"] as ConsultaEmpleadosFaltas_CriteriosFiltro; 
            else
            {
                criterioFiltro = new ConsultaEmpleadosFaltas_CriteriosFiltro(); 
            }

            if (Session["CiaContabSeleccionada"] == null)
                criterioFiltro.CiaContab = -999; 
            else
                criterioFiltro.CiaContab = Convert.ToInt32(Session["CiaContabSeleccionada"]); 

            dbNominaEntities context = new dbNominaEntities();

            var query = context.Empleados_Faltas.Include("tEmpleado").
                                                 Include("tEmpleado.tDepartamento").
                                                 Include("tEmpleado.Compania").
                                                 Where(e => e.tEmpleado.Cia == criterioFiltro.CiaContab);


            if (criterioFiltro.Empleado != null)
                query = query.Where(e => e.Empleado == criterioFiltro.Empleado);


            if (criterioFiltro.Departamento != null)
                query = query.Where(e => e.tEmpleado.Departamento == criterioFiltro.Departamento);


            if (criterioFiltro.Desde != null && criterioFiltro.Hasta != null)
            {
                query = query.Where(e => (e.Desde >= criterioFiltro.Desde && e.Desde <= criterioFiltro.Hasta) ||    // el período cubre el inicio de la falta 
                                         (e.Hasta >= criterioFiltro.Desde && e.Hasta <= criterioFiltro.Hasta) ||    // el período cubro el final de la falta 
                                         (e.Desde < criterioFiltro.Desde && e.Hasta > criterioFiltro.Hasta)         // el periodo esta *dentro* de la falta 
                    
                    );
            }

            if (criterioFiltro.Descontar != null && criterioFiltro.Descontar.Value)
                query = query.Where(e => e.Descontar);

            if (!string.IsNullOrEmpty(criterioFiltro.Status))
                query = query.Where(e => e.tEmpleado.Status == criterioFiltro.Status);

            if (!string.IsNullOrEmpty(criterioFiltro.SituacionActual))
                query = query.Where(e => e.tEmpleado.SituacionActual == criterioFiltro.SituacionActual); 


            totalRowCount = query.Count();

            if (totalRowCount == 0)
            {
                string errorMessage = "No se han seleccionado registros que mostrar; probablemente no se ha indicado un filtro correcto a esta página.";

                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;

                return null;
            }

            query = query.OrderBy(e => e.tEmpleado.Nombre).ThenBy(e => e.Desde);

            List<EmpleadoFaltaConsulta> list = new List<EmpleadoFaltaConsulta>();
            EmpleadoFaltaConsulta item;

            foreach (Empleados_Faltas falta in query)
            {
                item = new EmpleadoFaltaConsulta();

                item.CiaContab = falta.tEmpleado.Compania.Abreviatura;
                item.Departamento = falta.tEmpleado.tDepartamento.Descripcion;
                item.Empleado = falta.tEmpleado.Nombre;
                item.Descontar = falta.Descontar;
                item.Desde = falta.Desde;
                item.Hasta = falta.Hasta; 
                item.TotalDias = falta.CantDias;
                item.SabYDom = falta.CantDiasSabDom;
                item.Feriados = falta.CantDiasFeriados;
                item.Faltas = falta.CantDiasHabiles;
                item.Observaciones = falta.Observaciones;
                item.CantHoras = falta.CantHoras;
                item.FechaNomina = falta.Descontar_FechaNomina; 

                list.Add(item); 
            }

            // cómo regresamos un IEnumerable y no un IQuerable, debemos hacer el paging aquí; nótese como el GridView regresa 
            // la página y cantidad de registros por página ... 

            var pagedList = list.Skip(startRowIndex).Take(maximumRows).ToList();

            return pagedList;
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
        }

        protected void Filter_Ok_Button_Click(object sender, EventArgs e)
        {
            // mantenemos el criterio indicado en un objeto en session, para usarlo al generar el reporte (y también para aplicar un filtro al grid) ... 

            string errorMessage = "";

            if (!PrepararFilterObject(out errorMessage))
            {
                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;

                return; 
            }

            // -------------------------------------------------------------------------------------------
            // para guardar el contenido de los controles de la página para recuperar el state cuando 
            // se abra la proxima vez 

            KeepPageState MyKeepPageState = new KeepPageState(Membership.GetUser().UserName, this.GetType().Name.ToString());
            MyKeepPageState.SavePageStateInFile(this.Filtro_Div.Controls);         // nótese como guardamos solo los controles en el tab que contiene el filtro al usuario ... 
            MyKeepPageState = null;
            // ---------------------------------------------------------------------------------------------

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

        protected void LimpiarFiltro_Button_Click(object sender, EventArgs e)
        {
            LimpiarFiltro MyLimpiarFiltro = new LimpiarFiltro(this.Filtro_Div.Controls);
            MyLimpiarFiltro.LimpiarControlesPagina();
            MyLimpiarFiltro = null;
        }

        private bool PrepararFilterObject(out string errorMessage)
        {
            errorMessage = "";

            Session["ConsultaEmpleadosFaltas_CriteriosFiltro"] = null;
            ConsultaEmpleadosFaltas_CriteriosFiltro criterioFiltro = new ConsultaEmpleadosFaltas_CriteriosFiltro();

            if (Session["CiaContabSeleccionada"] != null)
                criterioFiltro.CiaContab = Convert.ToInt32(Session["CiaContabSeleccionada"]);

            if (this.Empleados_DropDownList.SelectedValue != "-999")
                criterioFiltro.Empleado = Convert.ToInt32(this.Empleados_DropDownList.SelectedValue);

            if (this.Departamentos_DropDownList.SelectedValue != "-999")
                criterioFiltro.Departamento = Convert.ToInt32(this.Departamentos_DropDownList.SelectedValue);

            if (this.Estados_DropDownList.SelectedValue != "-999")
                criterioFiltro.Status = this.Estados_DropDownList.SelectedValue;

            if (this.SituacionActual_DropDownList.SelectedValue != "-999")
                criterioFiltro.SituacionActual = this.SituacionActual_DropDownList.SelectedValue;

            if (!string.IsNullOrEmpty(this.Desde_TextBox.Text))
            {
                DateTime date;
                if (!DateTime.TryParse(this.Desde_TextBox.Text, out date))
                {
                    errorMessage = "Aparentemente, el valor indicado para la fecha no es correcto.";
                    return false;
                }
                criterioFiltro.Desde = date;
            }

            if (!string.IsNullOrEmpty(this.Hasta_TextBox2.Text))
            {
                DateTime date;
                if (!DateTime.TryParse(this.Hasta_TextBox2.Text, out date))
                {
                    errorMessage = "Aparentemente, el valor indicado para la fecha no es correcto.";
                    return false;
                }
                criterioFiltro.Hasta = date;
            }

            if (this.Descontar_CheckBox.Checked)
                criterioFiltro.Descontar = true;

            Session["ConsultaEmpleadosFaltas_CriteriosFiltro"] = criterioFiltro;

            return true; 
        }
    }
}