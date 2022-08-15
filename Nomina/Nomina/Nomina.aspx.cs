using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
//using NominaASP.Models.Nomina;
using NominaASP.Models;

namespace NominaASP.Nomina.Nomina
{
    public partial class Nomina : System.Web.UI.Page
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
                Session["FiltroConsultaNomina"] = null;
            }
        }

        public IQueryable<NominaASP.Models.tNominaHeader> Nomina_GridView_GetData()
        {
            if (Session["CiaContabSeleccionada"] == null)
                Session["CiaContabSeleccionada"] = -999; 

            int ciaContabSeleccionada = Convert.ToInt32(Session["CiaContabSeleccionada"]); 

            dbNominaEntities context = new dbNominaEntities();

            var query = context.tNominaHeaders.Where(n => n.tGruposEmpleado.Cia == ciaContabSeleccionada); 

            if (query.Count() == 0)
            {
                string errorMessage = "No se han seleccionado registros que mostrar; probablemente no se ha indicado un filtro correcto a esta página.";

                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;

                return null;
            }

            query = query.OrderByDescending(n => n.FechaNomina).ThenByDescending(n => n.ID); 

            return query;
        }

        protected void Nomina_GridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }

            // cuando se selecciona un item en el parent, debemos seleccionar y mostrar sus children ... 

            this.NominaDetalles_GridView.DataBind();
            this.SalarioIntegral_ListView.DataBind(); 
            this.NominaHeaders_FormView.DataBind(); 

            // asignamos el datasource de los combos, para que muestren empleados y rubros que correspondan solo a la 
            // nómina seleccionada; de esa forma, será más fácil para el usuario hacer sus selecciones ... 

            if (Nomina_GridView.SelectedDataKey == null)
                return;

            int nominaHeader = Convert.ToInt32(Nomina_GridView.SelectedDataKey.Value.ToString());

            // -----------------------------------------------------------------------------------------------------------------------
            // cargamos los combos con los items que permiten al usuario filtrar la lista de rubros ... 
            // 
            // limpiamos los combos pues tienen la propiedad AppendDataBoundItems en true y duplicarían los items cada vez ... 

            this.Empleados_DropDownList.Items.Clear();
            this.Rubros_DropDownList.Items.Clear(); 
            
            Empleados_SqlDataSource.SelectParameters["HeaderID"].DefaultValue = nominaHeader.ToString();
            Rubros_SqlDataSource.SelectParameters["HeaderID"].DefaultValue = nominaHeader.ToString();

            ListItem item = new ListItem(" ", "-999");

            this.Empleados_DropDownList.Items.Add(item);
            this.Rubros_DropDownList.Items.Add(item); 

            this.Empleados_DropDownList.DataBind();
            this.Rubros_DropDownList.DataBind();
            // -----------------------------------------------------------------------------------------------------------------------

            // mostramos siempre la 1ra. página de los rubros cuando el usuario selecciona otra nómina (header) 
            this.NominaDetalles_GridView.PageIndex = 0; 

            // ---------------------------------------------------------------------------------------------------------------------------
            // establecemos el href de links que construyen un reporte para el registro seleccionado ... 


            string filter = "(it.HeaderID == " + nominaHeader.ToString() + ")";
            Session["FiltroConsultaNomina"] = filter; 

            HtmlAnchor link = this.Report_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('" + "OpcionesReporte.aspx', 1000, 680)";

            link = this.Report2_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('" + "OpcionesReporte.aspx', 1000, 680)";

            // links que permiten construír el archivo de texto para el mail merge y obtener los recibos de pago ... 

            link = this.ContruirArchivosMailMerge1_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('Nomina_ConstruccionTextFiles.aspx?headerID=" + nominaHeader.ToString() + "', 1000, 680)";

            link = this.ContruirArchivosMailMerge2_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('Nomina_ConstruccionTextFiles.aspx?headerID=" + nominaHeader.ToString() + "', 1000, 680)";
        }

        protected void Nomina_GridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView gv = (GridView)sender;
            gv.SelectedIndex = -1;
        }

        public IQueryable<NominaASP.Models.tNomina> NominaDetalles_GridView_GetData()
        {
            if (Nomina_GridView.SelectedDataKey == null)
                return null; 

            int? nominaHeader = Convert.ToInt32(Nomina_GridView.SelectedDataKey.Value.ToString());

            dbNominaEntities context = new dbNominaEntities();

            var query = context.tNominas.Where(n => n.HeaderID == nominaHeader);

            if (this.Empleados_DropDownList.SelectedValue != "-999")
            {
                int empleado = Convert.ToInt32(this.Empleados_DropDownList.SelectedValue);
                query = query.Where(n => n.Empleado == empleado);
            }

            if (this.Rubros_DropDownList.SelectedValue != "-999")
            {
                int rubro = Convert.ToInt32(this.Rubros_DropDownList.SelectedValue);
                query = query.Where(n => n.Rubro == rubro);
            }

            if (this.Tipos_DropDownList.SelectedValue != "")
                query = query.Where(n => n.Tipo == this.Tipos_DropDownList.SelectedValue);


            if (query.Count() == 0)
                return null;


            query = query.OrderBy(n => n.tEmpleado.Nombre).ThenBy(n => n.Tipo).ThenByDescending(n => Math.Abs(n.Monto)); 

            return query;
        }

        protected void NominaDetalles_GridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // intentamos seleccionar la compañia que el usuario selecciona en la lista ... 
        }

        protected void NominaDetalles_GridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView gv = (GridView)sender;
            gv.SelectedIndex = -1;
        }

        protected void btnOk_Click(object sender, EventArgs e)
        {
            // cuando el usuario confirma la ejecución de la nómina, regresamos a este método ... 
            int nominaHeaderID = 0; 

            if (this.Nomina_GridView.SelectedDataKey != null)
                nominaHeaderID = Convert.ToInt32(this.Nomina_GridView.SelectedDataKey.Value);

            tNominaHeader nominaHeader;
            int cantidadRegistrosNomina = 0; 

            using (dbNominaEntities context = new dbNominaEntities()) 
            {
                nominaHeader = context.tNominaHeaders.Where(h => h.ID == nominaHeaderID).FirstOrDefault();

                if (nominaHeader != null) 
                    cantidadRegistrosNomina = nominaHeader.tNominas.Count(); 
            } 

            
            // -------------------------------------------------------------------------------------------------------------------------
            // revisamos a ver si la nómina ya fue ejecutada antes ... 
            // de ser así, notificamos y preguntamos al usuario ... 

            if (cantidadRegistrosNomina > 0) 
                if (Session["ConfirmarNominaEjecutadaAntesFlag"] != null && !Convert.ToBoolean(Session["ConfirmarNominaEjecutadaAntesFlag"]))
                {
                    this.ModalPopupTitle_span.InnerHtml = "Nómina ya ejecutada antes ...";
                    this.ModalPopupBody_span.InnerHtml = "Esta nómina <b><em>ya fue ejecutada antes</em></b><br /><br >" + 
                                                         "¿Desea continuar y ejecutarla nuevamente?";

                    this.btnOk.Visible = true;
                    this.btnOk.Text = "Ejecutar nómina";
                    this.btnCancel.Text = "Cancelar";

                    this.ModalPopupExtender1.Show();

                    Session["ConfirmarNominaEjecutadaAntesFlag"] = true;
                    return; 
                }

            string errorMessage = "";

            NominaASP.Code.Nomina nomina = new NominaASP.Code.Nomina(Convert.ToInt32(this.Nomina_GridView.SelectedDataKey.Value), out errorMessage);

            if (errorMessage != "")
            {
                this.ModalPopupTitle_span.InnerHtml = "Nómina - Ejecución - Error al intentar ejecutar la nómina seleccionada";
                this.ModalPopupBody_span.InnerHtml = errorMessage;
            }


            string resultadoEjecucionMessage = "";

            nomina.Ejecutar(out errorMessage, out resultadoEjecucionMessage);

            if (errorMessage != "")
            {
                this.ModalPopupTitle_span.InnerHtml = "Nómina - Ejecución - Error al intentar ejecutar la nómina seleccionada";
                this.ModalPopupBody_span.InnerHtml = errorMessage;
            }
            else
            {
                this.ModalPopupTitle_span.InnerHtml = "Nómina - Ejecución";
                this.ModalPopupBody_span.InnerHtml = resultadoEjecucionMessage;

                this.NominaDetalles_GridView.DataBind();
                this.SalarioIntegral_ListView.DataBind(); 

                // -----------------------------------------------------------------------------------------------------------------------
                // cargamos los combos con los items que permiten al usuario filtrar la lista de rubros ... 
                // 
                // limpiamos los combos pues tienen la propiedad AppendDataBoundItems en true y duplicarían los items cada vez ... 

                this.Empleados_DropDownList.Items.Clear();
                this.Rubros_DropDownList.Items.Clear(); 

                nominaHeaderID = Convert.ToInt32(this.Nomina_GridView.SelectedDataKey.Value);

                Empleados_SqlDataSource.SelectParameters["HeaderID"].DefaultValue = nominaHeaderID.ToString();
                Rubros_SqlDataSource.SelectParameters["HeaderID"].DefaultValue = nominaHeaderID.ToString();

                ListItem item = new ListItem(" ", "-999");

                this.Empleados_DropDownList.Items.Add(item);
                this.Rubros_DropDownList.Items.Add(item); 

                this.Empleados_DropDownList.DataBind();
                this.Rubros_DropDownList.DataBind();
                // -----------------------------------------------------------------------------------------------------------------------
            }

            this.btnOk.Visible = false;
            this.btnCancel.Text = "Ok";

            this.ModalPopupExtender1.Show();
        }

        protected void NominaDetalles_GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //fetch grid control using findcontrol method
                Label montoLabel = e.Row.FindControl("monto_Label") as Label;

                if (montoLabel != null)
                {
                    decimal monto;
                    if (Decimal.TryParse(montoLabel.Text, out monto))
                        if (monto < 0)
                        {
                            montoLabel.ForeColor = Color.Red;
                            montoLabel.Text = monto.ToString("#,###.00##").Replace("-", ""); 
                        }
                        else
                        {
                            montoLabel.ForeColor = Color.Blue;
                            montoLabel.Text = monto.ToString("#,###.00##"); 
                        }
                }
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                // obtenemos una sumarización de la nómina para mostrarla en el footer row ... 

                if (Nomina_GridView.SelectedIndex == -1)
                    return;

                int? nominaHeader = Convert.ToInt32(Nomina_GridView.SelectedDataKey.Value.ToString());

                if (nominaHeader == null)
                    return;

                dbNominaEntities context = new dbNominaEntities();

                // nótese la solución (inelegante!) que usamos para sumarizar *varias* propiedades en una sola linq instruction ... 
                // (nótese como intentamos aplicar los criterios que ha indicado el usuario: por empleado, rubro, etc.) 

                var query = from s in context.tNominas where s.HeaderID == nominaHeader select s; 

                if (this.Empleados_DropDownList.SelectedValue != "-999")
                {
                    int empleado = Convert.ToInt32(this.Empleados_DropDownList.SelectedValue);
                    query = query.Where(n => n.Empleado == empleado);
                }

                if (this.Rubros_DropDownList.SelectedValue != "-999")
                {
                    int rubro = Convert.ToInt32(this.Rubros_DropDownList.SelectedValue);
                    query = query.Where(n => n.Rubro == rubro);
                }

                if (this.Tipos_DropDownList.SelectedValue != "")
                    query = query.Where(n => n.Tipo == this.Tipos_DropDownList.SelectedValue);

                decimal asignaciones = 0;
                decimal deducciones = 0;
                decimal saldo = 0; 

                foreach (tNomina n in query)
                {
                    asignaciones += n.Monto >= 0 ? n.Monto : 0;
                    deducciones += n.Monto < 0 ? n.Monto : 0;
                    saldo += n.Monto; 
                }

                // quitamos el signo negativo que pueda existir 

                
                

                e.Row.Cells[0].Text = "Totales.: ";
                e.Row.Cells[2].Text = "(Asignaciones - Deducciones - Saldo)";

                e.Row.Cells[4].ForeColor = Color.Blue;
                e.Row.Cells[4].Text = asignaciones.ToString("N2");

                deducciones = Math.Abs(deducciones);
                e.Row.Cells[5].Text = deducciones.ToString("N2");
                e.Row.Cells[5].ForeColor = Color.Red;

                // el total (asignaciones - deducciones) casi siempre es positivo, aunque no siempre ...
                if (saldo < 0)
                    e.Row.Cells[6].ForeColor = Color.Red;
                else
                    e.Row.Cells[6].ForeColor = Color.Blue;
                saldo = Math.Abs(saldo); 
                e.Row.Cells[6].Text = saldo.ToString("N2");
            }
        }

        protected void Empleados_DropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            NominaDetalles_GridView.DataBind(); 
        }

        protected void Rubros_DropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            NominaDetalles_GridView.DataBind(); 
        }

        protected void Tipos_DropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            NominaDetalles_GridView.DataBind(); 
        }

        public tNominaHeader Nomina_FormView_GetData()
        {
            if (this.Nomina_GridView.SelectedDataKey == null)
                return null;

            int nominaHeaderPK = Convert.ToInt32(this.Nomina_GridView.SelectedDataKey.Value); 

            tNominaHeader item = null;

            using (dbNominaEntities context = new dbNominaEntities())
            {
                item = context.tNominaHeaders.Include("tGruposEmpleado").Where(n => n.ID == nominaHeaderPK).FirstOrDefault();
            }

            return item;
        }

        public string DescripcionTipoNomina(string tipo)
        {
            switch (tipo)
            {
                case "M":
                    return "Mensual";
                case "N":
                    return "Normal";
                case "Q":
                    return "Quincenal";
                case "1Q":
                    return "1ra. quincena";
                case "2Q":
                    return "2da. quincena";
                case "V":
                    return "Vacaciones";
                case "E":
                    return "Especial";
                case "U":
                    return "Utilidades";
            }
            return "Indefinida (" + tipo + ")";
        }

        public void Nomina_FormView_UpdatItem(int id)
        {
            NominaASP.Models.tNominaHeader item = null;

            // Load the item here, e.g. item = MyDataLayer.Find(id);

            dbNominaEntities context = new dbNominaEntities(); 
            item = context.tNominaHeaders.Where(n => n.ID == id).FirstOrDefault(); 

            if (item == null)
            {
                // The item wasn't found
                ModelState.AddModelError("", String.Format("Error inesperado: el registro que se desea editar no fue encontrado.", id));
                return;
            }

            try
            {
                TryUpdateModel(item);

                if (item.Desde != null && item.Hasta != null && item.Desde > item.Hasta)
                    ModelState.AddModelError("Desde", "La fecha inicial del período de nómina debe ser *anterior* a la fecha final.");

                if (item.CantidadDias == null)
                    if (item.Desde != null && item.Hasta != null) 
                        item.CantidadDias = Convert.ToInt16((item.Hasta.Value - item.Desde.Value).TotalDays); 
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += "<br />" + ex.InnerException.Message;

                ModelState.AddModelError("", errorMessage);
                return;
            }


            if (ModelState.IsValid)
            {
                // Save changes here, e.g. MyDataLayer.SaveChanges();

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;
                    if (ex.InnerException != null)
                        errorMessage += ex.InnerException.Message;

                    CustomValidator1.ErrorMessage = errorMessage;
                    CustomValidator1.IsValid = false;
                }
            }
        }

        protected void cancelButton_Click(object sender, EventArgs e)
        {
            //Response.Redirect("~/Ventas/Lista/Ventas_List.aspx");
            NominaHeaders_FormView.ChangeMode(FormViewMode.ReadOnly); 
        }

        public IQueryable<tGruposEmpleado> Get_GrupoEmpleadosDropDownList_Items()
        {
            int ciaContabSeleccionada = 0; 

            if (Session["CiaContabSeleccionada"] != null)
                ciaContabSeleccionada = Convert.ToInt32(Session["CiaContabSeleccionada"].ToString());
            else
            {
                string errorMessage = "Aparentemente, no se ha seleccionado una Cia Contab. Ud. debe seleccionar una Cia Contab antes de continuar.";

                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;

                return null;
            }

            dbNominaEntities context = new dbNominaEntities();
            var items = context.tGruposEmpleados.Where(g => g.Cia == ciaContabSeleccionada).OrderBy(g => g.Descripcion);

            return items;
        }


        public void Nomina_FormView_InsertItem()
        {
            var item = new tNominaHeader(); 

            TryUpdateModel(item);

            if (item.Desde != null && item.Hasta != null && item.Desde > item.Hasta) 
                ModelState.AddModelError("Desde", "La fecha inicial del período de nómina debe ser *anterior* a la fecha final.");

            if (ModelState.IsValid)
            {
                if (item.CantidadDias == null)
                    if (item.Desde != null && item.Hasta != null)
                        item.CantidadDias = Convert.ToInt16((item.Hasta.Value - item.Desde.Value).TotalDays); 

                // Save changes here
                dbNominaEntities context = new dbNominaEntities();
                context.tNominaHeaders.Add(item); 

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;
                    if (ex.InnerException != null)
                        errorMessage += ex.InnerException.Message;

                    CustomValidator1.IsValid = false;
                    CustomValidator1.ErrorMessage = errorMessage;
                }
            }
        }



        protected void NominaHeaders_FormView_ItemInserted(object sender, FormViewInsertedEventArgs e)
        {
            if (e.Exception == null && ModelState.IsValid && this.CustomValidator1.IsValid)
            {
                // queremos que el usuario se quede en el tab ... 
                //TabContainer1.ActiveTabIndex = 0;

                // si refrescamos la lista y seleccionamos su 1er item, este se mostrará como current item 
                // en el DataForm, pues éste simpre muestra el item seleccionado en la lista ... 

                this.Nomina_GridView.DataBind();
                this.Nomina_GridView.PageIndex = 0; 
                this.Nomina_GridView.SelectedIndex = 0; 
            }
            else
            {
                e.ExceptionHandled = true;
                e.KeepInInsertMode = true;
            }
        }

        protected void NominaHeaders_FormView_ItemDeleted(object sender, FormViewDeletedEventArgs e)
        {
            if (e.Exception == null && ModelState.IsValid && this.CustomValidator1.IsValid)
            {
                // queremos que el usuario se quede en el tab ... 
                //TabContainer1.ActiveTabIndex = 0;

                // aunque no lo hacemos ahora, aquí podemos refrescar la lista (GridView) para que muestre 
                // el cambio hecho a los datos ... 

                //this.NominaHeaders_FormView.DataBind(); 
            }
            else
            {
                e.ExceptionHandled = true;
            }
        }

        protected void NominaHeaders_FormView_ItemUpdated(object sender, FormViewUpdatedEventArgs e)
        {
            if (e.Exception == null && ModelState.IsValid && this.CustomValidator1.IsValid)
            {
                // queremos que el usuario se quede en el tab ... 
                //TabContainer1.ActiveTabIndex = 0;

                // aunque no lo hacemos ahora, aquí podemos refrescar la lista (GridView) para que muestre 
                // el cambio hecho a los datos ... 

                //this.NominaHeaders_FormView.DataBind(); 
            }
            else
            {
                e.ExceptionHandled = true;
                e.KeepInEditMode = true;
            }
        }


        public void Nomina_FormView_DeleteItem(int id)
        {
            NominaASP.Models.tNominaHeader item = null;

            dbNominaEntities context = new dbNominaEntities();
            item = context.tNominaHeaders.FirstOrDefault(n => n.ID == id);

            if (item == null)
            {
                // The item wasn't found
                ModelState.AddModelError("", String.Format("Error inesperado: el item a eliminar (id {0}) no pudo ser encontrado.", id));
                return;
            }

            if (ModelState.IsValid)
            {
                // Save changes here, e.g. MyDataLayer.SaveChanges();
                context.tNominaHeaders.Remove(item);
                context.SaveChanges();

                //ConciliacionesBancarias_GridView.DataBind();                // para 'refrescar' el GridView con los cambios efectuados ... 
                //TabContainer1.ActiveTabIndex = 0;
            }
        }

        protected void EjecutarNomina_ImageButton_Click(object sender, ImageClickEventArgs e)
        {
            // para saber si el usuario confirmó ya la ejecución de la nómina si ya fue ejecutada antes ... 
            Session["ConfirmarNominaEjecutadaAntesFlag"] = false; 

            ConfirmarEjecucionNomina(); 
        }

        protected void EjecutarNomina_LinkButton_Click(object sender, EventArgs e)
        {
            // para saber si el usuario confirmó ya la ejecución de la nómina si ya fue ejecutada antes ... 
            Session["ConfirmarNominaEjecutadaAntesFlag"] = false; 

            ConfirmarEjecucionNomina(); 
        }

        private void ConfirmarEjecucionNomina()
        {
            if (this.Nomina_GridView.SelectedIndex == -1)
            {
                this.ModalPopupTitle_span.InnerHtml = "Nómina - Ejecución";
                this.ModalPopupBody_span.InnerHtml = "Ud. debe seleccionar el registro en la lista que corresponda a la nómina que desea ejecutar.";

                // usamos btnOk solo si queremos un postback (a btnOk.Click); por ejemplo, si mostramos un mensaje al usuario pero él puede 
                // decidir continuar (o cancelar); si continúa, hacemos el postback y continuamos en el servidor. 
                // En este caso, solo queremos que el usuario lea el mensaje y cierre el diálogo. Por eso, no mostramos btnOk y usamos Cancel como Ok ... 

                this.btnOk.Visible = false;
                this.btnCancel.Text = "Ok";

                this.ModalPopupExtender1.Show();
                return;
            }

            string ajaxPopupMessage;
            int nominaID = Convert.ToInt32(this.Nomina_GridView.SelectedDataKey.Value);

            using (dbNominaEntities context = new dbNominaEntities())
            {
                tNominaHeader nominaHeader = context.tNominaHeaders.Where(n => n.ID == nominaID).First();

                ajaxPopupMessage = "Desea ejecutar la nómina, de tipo: <b><em>'" + nominaHeader.Tipo + "'</em></b>, para el grupo de nómina: <b><em>'" + 
                                    nominaHeader.tGruposEmpleado.Descripcion + "'</em></b>" + 
                                    " y el período: <b><em>'" +
                                    (nominaHeader.Desde == null ? "(período no definido)" : nominaHeader.Desde.Value.ToString("d-MMM-yyyy")) + "'</em></b>" + 
                                    " a <b><em>'" +
                                    (nominaHeader.Hasta == null ? "(período no definido)" : nominaHeader.Hasta.Value.ToString("d-MMM-yyyy")) + "'</em></b>";
            }

            // ahora pedimos una confirmación al usuario; nótese que para hacerlo, usamos el ajax popup, pero si el usuario confirma se ejecuta el 
            // evento OnClick del butón ... 

            this.ModalPopupTitle_span.InnerHtml = "Nómina - Ejecución";
            this.ModalPopupBody_span.InnerHtml = ajaxPopupMessage;

            // usamos btnOk solo si queremos un postback (a btnOk.Click); por ejemplo, si mostramos un mensaje al usuario pero él puede 
            // decidir continuar (o cancelar); si continúa, hacemos el postback y continuamos en el servidor. 
            // En este caso, solo queremos que el usuario lea el mensaje y cierre el diálogo. Por eso, no mostramos btnOk y usamos Cancel como Ok ... 

            this.btnOk.Visible = true;
            this.btnOk.Text = "Ejecutar nómina";
            this.btnCancel.Text = "Cancelar";

            this.ModalPopupExtender1.Show();
        }

        protected void ObtenerReporte_LinkButton_PreRender(object sender, EventArgs e)
        {
        }

        protected void ObtenerReporte_ImageButton_PreRender(object sender, EventArgs e)
        {
            ImageButton lb = (ImageButton)sender;

            string URL = "../../ReportViewer.aspx?rpt=consultaNomina";

            int nominaID = -999;

            //Session["headerID"] = null;
            if (this.Nomina_GridView.SelectedDataKey != null)
            {
                nominaID = Convert.ToInt32(this.Nomina_GridView.SelectedDataKey.Value);
                URL += "&headerID=" + nominaID.ToString();

                // TODO: reemplazar por parametro cuando tengamos una respuesta a este issue ... 
                //Session["headerID"] = Convert.ToInt32(this.Nomina_GridView.SelectedDataKey.Value);
            }

            //lb.OnClientClick = @"javascript:PopupWin('" + URL + "', 1000, 680)";
            //string url = @"../../ReportViewer.aspx?rpt=consultaNomina&headerID=410";

            lb.OnClientClick = "PopupWin('" + URL + "', 1000, 680)";
        }

        // The id parameter name should match the DataKeyNames value set on the control
        public void Nomina_GridView_DeleteItem(int id)
        {
            using (dbNominaEntities context = new dbNominaEntities())
            {
                tNominaHeader nomina = context.tNominaHeaders.Where(n => n.ID == id).FirstOrDefault();

                if (nomina != null)
                {
                    context.tNominaHeaders.Remove(nomina);

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ex.Message;
                        if (ex.InnerException != null)
                            errorMessage += ex.InnerException.Message;

                        CustomValidator1.IsValid = false;
                        CustomValidator1.ErrorMessage = errorMessage;
                    }
                }
            }
        }

        protected void SalarioIntegral_ListView_PagePropertiesChanged(object sender, EventArgs e)
        {
            // cuando el usuario cambia de página, intentamos quitar la selección a algún item que lo tenga 
            ListView lv = sender as ListView;
            lv.SelectedIndex = -1;
        }

        protected void SalarioIntegral_ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        // The return type can be changed to IEnumerable, however to support
        // paging and sorting, the following parameters must be added:
        //     int maximumRows
        //     int startRowIndex
        //     out int totalRowCount
        //     string sortByExpression
        public IQueryable<NominaASP.Models.tNomina_SalarioIntegral> SalarioIntegral_ListView_GetData()
        {
            if (Nomina_GridView.SelectedDataKey == null)
                return null;

            int? nominaHeader = Convert.ToInt32(Nomina_GridView.SelectedDataKey.Value.ToString());

            if (nominaHeader == null)
                return null;

            dbNominaEntities context = new dbNominaEntities();

            var query = context.tNomina_SalarioIntegral.Include("tEmpleado").Where(n => n.HeaderID == nominaHeader.Value);

            //if (query.Count() == 0)
            //    return null;

            query = query.OrderBy(n => n.tEmpleado.Nombre);

            return query;
        }

        protected void SalarioIntegral_ListView_LayoutCreated(object sender, EventArgs e)
        {
            ListView lv = sender as ListView;

            if (Nomina_GridView.SelectedIndex == -1) 
                return;

            int? nominaHeader = Convert.ToInt32(Nomina_GridView.SelectedDataKey.Value.ToString());

            if (nominaHeader == null)
                return;

            dbNominaEntities context = new dbNominaEntities();

            // nótese la solución (inelegante!) que usamos para sumarizar *varias* propiedades en una sola linq instruction ... 

            var sumOfSalarioIntegral = from s in context.tNomina_SalarioIntegral
                                       where s.HeaderID == nominaHeader
                                       group s by 1 into g
                                       select new
                                       {
                                           SueldoBasicoMensual = g.Sum(x => x.SueldoBasico_Mensual),
                                           SueldoBasicoDiario = g.Sum(x => x.SueldoBasico_Diario),
                                           BonoVacacional = g.Sum(x => x.BonoVacacional_Monto),
                                           BonoVacacionalDiario = g.Sum(x => x.BonoVacacional_Diario),
                                           Utilidades = g.Sum(x => x.Utilidades_Monto),
                                           UtilidadesDiarias = g.Sum(x => x.Utilidades_Diario),
                                           SalarioIntegral = g.Sum(x => x.SalarioIntegral_Monto),
                                           SalarioIntegralDiario = g.Sum(x => x.SalarioIntegral_Diario)
                                       };

            if (sumOfSalarioIntegral.Count() == 0)
                return; 

            Label lblSum = (Label)lv.FindControl("SueldoMensual_Label");
            lblSum.Text = sumOfSalarioIntegral.First().SueldoBasicoMensual.ToString("N2"); 

            lblSum = (Label)lv.FindControl("SueldoDiario_Label");
            lblSum.Text = sumOfSalarioIntegral.First().SueldoBasicoDiario.ToString("N2"); 

            lblSum = (Label)lv.FindControl("BonoVac_Label");
            lblSum.Text = sumOfSalarioIntegral.First().BonoVacacional.ToString("N2");  

            lblSum = (Label)lv.FindControl("BonoVacDiario_Label");
            lblSum.Text = sumOfSalarioIntegral.First().BonoVacacionalDiario.ToString("N2"); 

            lblSum = (Label)lv.FindControl("Utilidades_Label");
            lblSum.Text = sumOfSalarioIntegral.First().Utilidades.ToString("N2");  

            lblSum = (Label)lv.FindControl("UtilidadesDiarias_Label");
            lblSum.Text = sumOfSalarioIntegral.First().UtilidadesDiarias.ToString("N2");  

            lblSum = (Label)lv.FindControl("SalarioIntegralDiario_Label");
            lblSum.Text = sumOfSalarioIntegral.First().SalarioIntegralDiario.ToString("N2");  

            lblSum = (Label)lv.FindControl("SalarioIntegralMensual_Label");
            lblSum.Text = sumOfSalarioIntegral.First().SalarioIntegral.ToString("N2");   
        }

        protected void SalarioIntegral_ListView_ItemDataBound(object sender, ListViewItemEventArgs e)
        {

        }
    }
}