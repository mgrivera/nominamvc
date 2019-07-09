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

namespace NominaASP.Empleados.Vacaciones
{
    public partial class DiasVacaciones : System.Web.UI.Page
    {
        ConsultaCriteriosFiltro_ControlDiasVacaciones _consultaCriteriosFiltro; 

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }

            if (!Page.IsPostBack)
            {
                _consultaCriteriosFiltro = null;
                Session["ConsultaCriteriosFiltro"] = null; 

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
            }
            else
            {
                string errorMessage = "";

                if (!PrepararFilterObject(out errorMessage))
                {
                    CustomValidator1.IsValid = false;
                    CustomValidator1.ErrorMessage = errorMessage;

                    return;
                }
            }
        }

        public IList<NominaASP.Empleados.Vacaciones.DiasVacacionesConsulta> Empleados_GridView_GetData
            (int maximumRows, int startRowIndex, out int totalRowCount)
        {
            totalRowCount = 0; 

            if (_consultaCriteriosFiltro == null)
                return null; 

            dbNominaEntities context = new dbNominaEntities();

            var query = context.tEmpleados.Include("Compania").Include("tDepartamento").Where(e => e.Cia == _consultaCriteriosFiltro.CiaContab);

            if (!string.IsNullOrEmpty(_consultaCriteriosFiltro.Status))
                query = query.Where(e => e.Status == _consultaCriteriosFiltro.Status);

            if (!string.IsNullOrEmpty(_consultaCriteriosFiltro.SituacionActual))
                query = query.Where(e => e.SituacionActual == _consultaCriteriosFiltro.SituacionActual); 

            totalRowCount = query.Count();
            string errorMessage = ""; 

            if (totalRowCount == 0)
            {
                errorMessage = "No se han seleccionado registros que mostrar; probablemente no se ha indicado un filtro correcto a esta página.";

                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;

                return null;
            }

            query = query.OrderBy(e => e.Alias);

            List<DiasVacacionesConsulta> list = new List<DiasVacacionesConsulta>();
            DiasVacacionesConsulta item;

            Nomina_FuncionesGenericas funcionesNomina = new Nomina_FuncionesGenericas();

            DateTime primerDiaAnoConsulta = new DateTime(_consultaCriteriosFiltro.AnoConsulta.Value, 1, 1);
            DateTime ultimoDiaAnoConsulta = new DateTime(_consultaCriteriosFiltro.AnoConsulta.Value, 12, 31);

            foreach (tEmpleado e in query)
            {
                item = new DiasVacacionesConsulta();

                item.CiaContab = e.Compania.Nombre;
                item.Departamento = e.tDepartamento.Descripcion;

                item.ID = e.Empleado;
                item.Nombre = e.Nombre;
                item.Alias = e.Alias;
                item.FechaIngreso = e.FechaIngreso;

                // -----------------------------------------------------------------------------------------------------------
                // 1) determinamos, según el año de la consulta (ej: 2013), el año de vacaciones del empleado; por ejemplo: un empleado 
                //    puede tener 15 años en la empresa y su año de vacaciones (con respecto al 2.013) podría ser: 1/7/2012 al 30/6/2013. 

                DateTime anoEmpleadoDesde = e.FechaIngreso;
                DateTime anoEmpleadoHasta = e.FechaIngreso.AddYears(1).AddDays(-1);
                int anoEmpleadoEnEmpresa = 0; 

                while (anoEmpleadoHasta < ultimoDiaAnoConsulta.AddDays(-1)) 
                {
                    item.AnoVacaciones_Ano = anoEmpleadoEnEmpresa + 1;
                    item.AnoVacaciones_Desde = anoEmpleadoDesde;
                    item.AnoVacaciones_Hasta = anoEmpleadoHasta;

                    anoEmpleadoDesde = anoEmpleadoDesde.AddYears(1);
                    anoEmpleadoHasta = anoEmpleadoDesde.AddYears(1).AddDays(-1);

                    anoEmpleadoEnEmpresa++; 
                }

                // -----------------------------------------------------------------------------------------------------------
                // 2) ahora leemos la vacación más reciente, siempre con respecto al año de la consulta ... 

                Vacacione vacacion = context.Vacaciones.Where(v => v.AnoVacaciones <= anoEmpleadoEnEmpresa && v.Empleado == e.Empleado).
                                                        OrderByDescending(v => v.Salida).
                                                        FirstOrDefault(); 

                if (vacacion != null) 
                {
                    item.VacacionMasReciente_Desde = vacacion.Salida; 
                    item.VacacionMasReciente_Hasta = vacacion.Regreso; 
                    item.VacacionMasReciente_AnoVacaciones = vacacion.AnoVacaciones; 
                    item.VacacionMasReciente_DiasPendAnosAnteriores = vacacion.CantDiasVacPendAnosAnteriores; 
                    item.VacacionMasReciente_DiasSegunTabla = vacacion.CantDiasVacSegunTabla; 

                    item.VacacionMasReciente_DiasDisfrutados_Antes = vacacion.CantDiasVacDisfrutadosAntes; 
                    item.VacacionMasReciente_DiasDisfrutados_Ahora = vacacion.CantDiasVacDisfrutadosAhora; 

                    item.VacacionMasReciente_DiasPendientes = vacacion.CantDiasVacPendientes; 
                }

                // -----------------------------------------------------------------------------------------------------------
                // 3) intentamos determinar si existen vacaciones pendientes (entre la última tomada y el año actual) 

                item.VacacionesPendientes_Cantidad = 0; 
                item.VacacionesPendientes_DiasSegunTabla = 0; 

                for (int ano = item.VacacionMasReciente_AnoVacaciones; ano < item.AnoVacaciones_Ano; ano++) 
                {
                    // buscamos la cantidad de días según tabla, para el año pendiente específico ... 

                    int cantidadDiasVacaciones = 0;
                    int cantidadDiasBono = 0; 

                    if (!funcionesNomina.DeterminarDiasVacacionesParaEmpleado(context,
                                                                              e.Empleado,
                                                                              ano,
                                                                              out cantidadDiasVacaciones,
                                                                              out cantidadDiasBono,
                                                                              out errorMessage))
                    {
                        this.CustomValidator1.ErrorMessage = errorMessage;
                        this.CustomValidator1.IsValid = false;

                        return null;
                    }


                    item.VacacionesPendientes_DiasSegunTabla += (short)cantidadDiasVacaciones; 
                    item.VacacionesPendientes_Cantidad++; 
                }


                // -----------------------------------------------------------------------------------------------------------
                // 4) finalmente, determinamos la cantidad de días de vacaciones pendientes ... 

                item.TotalDiasPendientes = 0;

                if (item.VacacionMasReciente_DiasPendientes != null)
                    item.TotalDiasPendientes = item.VacacionMasReciente_DiasPendientes.Value; 

                item.TotalDiasPendientes += item.VacacionesPendientes_DiasSegunTabla; 

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

            this.DiasVacaciones_ListView.DataBind();
            TabContainer1.ActiveTabIndex = 0; 
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

            _consultaCriteriosFiltro = new ConsultaCriteriosFiltro_ControlDiasVacaciones();

            if (Session["CiaContabSeleccionada"] != null)
                _consultaCriteriosFiltro.CiaContab = Convert.ToInt32(Session["CiaContabSeleccionada"]);

            _consultaCriteriosFiltro.AnoConsulta = Convert.ToInt32(this.AnoConsulta_TextBox.Text);

            if (this.Estados_DropDownList.SelectedValue != "-999")
                _consultaCriteriosFiltro.Status = this.Estados_DropDownList.SelectedValue;

            if (this.SituacionActual_DropDownList.SelectedValue != "-999")
                _consultaCriteriosFiltro.SituacionActual = this.SituacionActual_DropDownList.SelectedValue;

            // guardamos el filtro aquí, pues es necesario cuando el usuario va a otra página, como cuando quiere imprimir la consulta 
            Session["ConsultaCriteriosFiltro"] = _consultaCriteriosFiltro; 

            return true; 
        }

        protected void Facturas_ListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void DiasVacaciones_ListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void DiasVacaciones_ListView_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            this.DiasVacaciones_ListView.SelectedIndex = -1; 
        }
    }
}