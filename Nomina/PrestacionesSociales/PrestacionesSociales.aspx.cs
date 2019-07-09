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
using NominaASP.Code;
//using NominaASP.Models.Nomina;
using NominaASP.Models;

namespace NominaASP.Nomina.PrestacionesSociales
{
    public partial class PrestacionesSociales : System.Web.UI.Page
    {
        int? ciaContabSeleccionada; 

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
            }

            if (Session["CiaContabSeleccionada"] != null)
                ciaContabSeleccionada = Convert.ToInt32(Session["CiaContabSeleccionada"]); 
        }

        protected void btnOk_Click(object sender, EventArgs e)
        {
            // cuando el usuario confirma la ejecución de las prestaciones sociales para un período (header), regresamos a este método ... 

            int prestacionesSocialesID = Convert.ToInt32(this.PrestacionesSocialesHeaders_GridView.SelectedDataKey.Value);

            PrestacionesSociales_Funciones prestacionesSocialesFunciones = new PrestacionesSociales_Funciones();

            int cantidadRegistros = 0;
            string resultadoEjecucionMessage = "";
            bool result = false;

            result = prestacionesSocialesFunciones.PrestacionesSociales_CalculoYRegistro(prestacionesSocialesID, 
                                                                                         User.Identity.Name, 
                                                                                         out cantidadRegistros, 
                                                                                         out resultadoEjecucionMessage);

            if (!result)
            {
                this.ModalPopupTitle_span.InnerHtml = "Nómina - Error al intentar ejecutar el cálculo de prestaciones";
                this.ModalPopupBody_span.InnerHtml = resultadoEjecucionMessage;
            }
            else
            {
                this.ModalPopupTitle_span.InnerHtml = "Nómina - Prestaciones sociales";
                this.ModalPopupBody_span.InnerHtml = resultadoEjecucionMessage;

                this.Detalles_ListView.DataBind();
            }

            this.btnOk.Visible = false;
            this.btnCancel.Text = "Ok";

            this.ModalPopupExtender1.Show();
        }

        public tNominaHeader Nomina_FormView_GetData()
        {
            if (this.PrestacionesSocialesHeaders_GridView.SelectedDataKey == null)
                return null;

            int nominaHeaderPK = Convert.ToInt32(this.PrestacionesSocialesHeaders_GridView.SelectedDataKey.Value); 

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
            tNominaHeader item = null;

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
            PrestacionesSocialesHeaders_FormView.ChangeMode(FormViewMode.ReadOnly); 
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

        // The return type can be changed to IEnumerable, however to support
        // paging and sorting, the following parameters must be added:
        //     int maximumRows
        //     int startRowIndex
        //     out int totalRowCount
        //     string sortByExpression

        public IQueryable<NominaASP.Models.PrestacionesSocialesHeader> PrestacionesSocialesHeaders_GridView_GetData()
        {
            if (Session["CiaContabSeleccionada"] == null)
                Session["CiaContabSeleccionada"] = -999;

            int ciaContabSeleccionada = Convert.ToInt32(Session["CiaContabSeleccionada"]);

            dbNominaEntities context = new dbNominaEntities();

            var query = context.PrestacionesSocialesHeaders.Where(p => p.Cia == ciaContabSeleccionada);

            if (query.Count() == 0)
            {
                string errorMessage = "No se han seleccionado registros que mostrar; probablemente no se ha indicado un filtro correcto a esta página.";

                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;

                return null;
            }

            query = query.OrderByDescending(p => p.ID);

            return query;
        }

        protected void PrestacionesSocialesHeaders_GridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }

            // cuando se selecciona un item en el parent, debemos seleccionar y mostrar sus children ... 

            this.Detalles_ListView.DataBind();
            this.PrestacionesSocialesHeaders_FormView.DataBind();

            // asignamos el datasource de los combos, para que muestren empleados y rubros que correspondan solo a la 
            // nómina seleccionada; de esa forma, será más fácil para el usuario hacer sus selecciones ... 

            if (this.PrestacionesSocialesHeaders_GridView.SelectedDataKey == null)
                return;

            int prestacionesSocialesHeader = Convert.ToInt32(PrestacionesSocialesHeaders_GridView.SelectedDataKey.Value.ToString());

            // mostramos siempre la 1ra. página de los rubros cuando el usuario selecciona otra nómina (header) 
            DataPager pager = this.Detalles_ListView.FindControl("Detalles_DataPager") as DataPager;
            if (pager != null)
                pager.SetPageProperties(0, pager.PageSize, true);

            // intentamos, también, eliminar cualquier selección que pueda existir en el ListView de detalles ... 
            ListView lv = this.Detalles_ListView;
            lv.SelectedIndex = -1;

            // ---------------------------------------------------------------------------------------------------------------------------
            // establecemos el href de links que construyen un reporte para el registro seleccionado ... 

            HtmlAnchor link = this.Report_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('" + "../../ReportViewer.aspx?rpt=consultaPrestaciones&headerID=" + prestacionesSocialesHeader.ToString() + "', 1150, 600)";

            link = this.Report2_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('" + "../../ReportViewer.aspx?rpt=consultaPrestaciones&headerID=" + prestacionesSocialesHeader.ToString() + "', 1150, 600)";

            // ---------------------------------------------------------------------------------------------------------------------------
            // establecemos el href de links que permiten construir un archivo de texto ... 

            link = this.ObtenerArchivoTxt1_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('" + "PrestacionesSociales_ObtencionTxtFile.aspx?headerID=" + prestacionesSocialesHeader.ToString() + "', 1000, 600)";

            link = this.ObtenerArchivoTxt2_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('" + "PrestacionesSociales_ObtencionTxtFile.aspx?headerID=" + prestacionesSocialesHeader.ToString() + "', 1000, 600)";
        }

        protected void PrestacionesSocialesHeaders_GridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView gv = (GridView)sender;
            gv.SelectedIndex = -1;
        }
        
        public IQueryable<NominaASP.Models.PrestacionesSociale> Detalles_ListView_GetData()
        {
            if (this.PrestacionesSocialesHeaders_GridView.SelectedDataKey == null)
                return null;

            int? header = Convert.ToInt32(PrestacionesSocialesHeaders_GridView.SelectedDataKey.Value.ToString());

            dbNominaEntities context = new dbNominaEntities();

            var query = context.PrestacionesSociales.Where(n => n.HeaderID == header);

            if (query.Count() == 0)
                return null;


            query = query.OrderBy(n => n.tEmpleado.Nombre);

            return query;
        }

        public string NombreMes(int mes)
        {
            switch (mes)
            {
                case 1:
                    return "Enero";
                case 2:
                    return "Febrero";
                case 3:
                    return "Marzo";
                case 4:
                    return "Abril";
                case 5:
                    return "Mayo";
                case 6:
                    return "Junio";
                case 7:
                    return "Julio";
                case 8:
                    return "Agosto";
                case 9:
                    return "Septiembre";
                case 10:
                    return "Octubre";
                case 11:
                    return "Noviembre";
                case 12:
                    return "Diciembre"; 
            }

            return "Indefinido"; 
        }

        // The id parameter should match the DataKeyNames value set on the control
        // or be decorated with a value provider attribute, e.g. [QueryString]int id
        public NominaASP.Models.PrestacionesSocialesHeader PrestacionesSocialesHeaders_FormView_GetItem()
        {
            if (this.PrestacionesSocialesHeaders_GridView.SelectedDataKey == null)
                return null;

            int prestacionesSocialesHeaderPK = Convert.ToInt32(this.PrestacionesSocialesHeaders_GridView.SelectedDataKey.Value);

            PrestacionesSocialesHeader item = null;

            using (dbNominaEntities context = new dbNominaEntities())
            {
                item = context.PrestacionesSocialesHeaders.Include("Compania").Where(n => n.ID == prestacionesSocialesHeaderPK).FirstOrDefault();
            }

            return item;
        }

        // The id parameter name should match the DataKeyNames value set on the control
        public void PrestacionesSocialesHeaders_FormView_UpdateItem(int id)
        {
            PrestacionesSocialesHeader item = null;

            // Load the item here, e.g. item = MyDataLayer.Find(id);

            dbNominaEntities context = new dbNominaEntities();
            item = context.PrestacionesSocialesHeaders.Where(n => n.ID == id).FirstOrDefault();

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

                if (item.Desde != null)
                {
                    item.Mes = Convert.ToInt16(item.Hasta.Value.Month);
                    item.Ano = Convert.ToInt16(item.Hasta.Value.Year);
                }
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

        public void PrestacionesSocialesHeaders_FormView_InsertItem()
        {
            var item = new PrestacionesSocialesHeader();

            TryUpdateModel(item);

            if (item.Desde != null && item.Hasta != null && item.Desde > item.Hasta)
                ModelState.AddModelError("Desde", "La fecha inicial del período de prestaciones debe ser *anterior* a la fecha final.");

            if (ModelState.IsValid)
            {
                item.Mes = Convert.ToInt16(item.Hasta.Value.Month);
                item.Ano = Convert.ToInt16(item.Hasta.Value.Year);

                if (this.ciaContabSeleccionada != null) 
                    item.Cia = this.ciaContabSeleccionada.Value; 

                // Save changes here
                dbNominaEntities context = new dbNominaEntities();
                context.PrestacionesSocialesHeaders.Add(item);

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

        // The id parameter name should match the DataKeyNames value set on the control
        public void PrestacionesSocialesHeaders_FormView_DeleteItem(int id)
        {
            PrestacionesSocialesHeader item = null;

            dbNominaEntities context = new dbNominaEntities();
            item = context.PrestacionesSocialesHeaders.FirstOrDefault(n => n.ID == id);

            if (item == null)
            {
                // The item wasn't found
                ModelState.AddModelError("", String.Format("Error inesperado: el item a eliminar (id {0}) no pudo ser encontrado.", id));
                return;
            }

            if (ModelState.IsValid)
            {
                // Save changes here, e.g. MyDataLayer.SaveChanges();
                context.PrestacionesSocialesHeaders.Remove(item);
                context.SaveChanges();

                //ConciliacionesBancarias_GridView.DataBind();                // para 'refrescar' el GridView con los cambios efectuados ... 
                //TabContainer1.ActiveTabIndex = 0;
            }
        }

        protected void PrestacionesSocialesHeaders_FormView_ItemInserted(object sender, FormViewInsertedEventArgs e)
        {
            if (e.Exception == null && ModelState.IsValid && this.CustomValidator1.IsValid)
            {
                // queremos que el usuario se quede en el tab ... 
                //TabContainer1.ActiveTabIndex = 0;

                // si refrescamos la lista y seleccionamos su 1er item, este se mostrará como current item 
                // en el DataForm, pues éste simpre muestra el item seleccionado en la lista ... 

                this.PrestacionesSocialesHeaders_GridView.DataBind();
                this.PrestacionesSocialesHeaders_GridView.PageIndex = 0;
                this.PrestacionesSocialesHeaders_GridView.SelectedIndex = 0;
            }
            else
            {
                e.ExceptionHandled = true;
                e.KeepInInsertMode = true;
            }
        }

        protected void PrestacionesSocialesHeaders_FormView_ItemDeleted(object sender, FormViewDeletedEventArgs e)
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

        protected void PrestacionesSocialesHeaders_FormView_ItemUpdated(object sender, FormViewUpdatedEventArgs e)
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

        protected void PrestacionesSocialesHeaders_FormView_ItemCreated(object sender, EventArgs e)
        {
            FormView fv = sender as FormView; 

            if (fv.CurrentMode == FormViewMode.Insert)
            {
                // nótese como usamos este método para establecer defaults en los controles del FormView 

                TextBox tb = (TextBox)fv.Row.FindControl("CantDiasUtilidades_TextBox");
                if (tb != null)
                    tb.Text = "60";
            }
        }

        protected void DeterminarPrestacionesSociales_ImageButton_Click(object sender, ImageClickEventArgs e)
        {
            ConfirmarEjecucionPrestacionesSociales(); 
        }

        protected void DeterminarPrestacionesSociales_LinkButton_Click(object sender, EventArgs e)
        {
            ConfirmarEjecucionPrestacionesSociales(); 
        }

        private void ConfirmarEjecucionPrestacionesSociales()
        {
            if (this.PrestacionesSocialesHeaders_GridView.SelectedIndex == -1)
            {
                this.ModalPopupTitle_span.InnerHtml = "Prestaciones sociales - Ejecución";
                this.ModalPopupBody_span.InnerHtml = "Ud. debe seleccionar el registro en la lista que corresponda a la definición que desea ejecutar.";

                // usamos btnOk solo si queremos un postback (a btnOk.Click); por ejemplo, si mostramos un mensaje al usuario pero él puede 
                // decidir continuar (o cancelar); si continúa, hacemos el postback y continuamos en el servidor. 
                // En este caso, solo queremos que el usuario lea el mensaje y cierre el diálogo. Por eso, no mostramos btnOk y usamos Cancel como Ok ... 

                this.btnOk.Visible = false;
                this.btnCancel.Text = "Ok";

                this.ModalPopupExtender1.Show();
                return;
            }

            string ajaxPopupMessage;
            int prestacionesSocialesID = Convert.ToInt32(this.PrestacionesSocialesHeaders_GridView.SelectedDataKey.Value);

            using (dbNominaEntities context = new dbNominaEntities())
            {
                PrestacionesSocialesHeader prestacionesSocialesHeader = context.PrestacionesSocialesHeaders.Where(n => n.ID == prestacionesSocialesID).First();

                ajaxPopupMessage = "Desea ejecutar el cálculo de prestaciones sociales para el período: " +
                (prestacionesSocialesHeader.Desde == null ? "(período no definido)" : prestacionesSocialesHeader.Desde.Value.ToString("d-MMM-yyyy")) +
                " a " +
                (prestacionesSocialesHeader.Hasta == null ? "(período no definido)" : prestacionesSocialesHeader.Hasta.Value.ToString("d-MMM-yyyy"));
            }

            // ahora pedimos una confirmación al usuario; nótese que para hacerlo, usamos el ajax popup, pero si el usuario confirma se ejecuta el 
            // evento OnClick del butón ... 

            this.ModalPopupTitle_span.InnerHtml = "Prestaciones sociales - Ejecución";
            this.ModalPopupBody_span.InnerHtml = ajaxPopupMessage;

            // usamos btnOk solo si queremos un postback (a btnOk.Click); por ejemplo, si mostramos un mensaje al usuario pero él puede 
            // decidir continuar (o cancelar); si continúa, hacemos el postback y continuamos en el servidor. 
            // En este caso, solo queremos que el usuario lea el mensaje y cierre el diálogo. Por eso, no mostramos btnOk y usamos Cancel como Ok ... 

            this.btnOk.Visible = true;
            this.btnOk.Text = "Determinar prestaciones sociales";
            this.btnCancel.Text = "Cancelar";

            this.ModalPopupExtender1.Show();
        }

        protected void Detalles_ListView_PagePropertiesChanged(object sender, EventArgs e)
        {
            // cuando el usuario cambia de página, intentamos quitar la selección a algún item que lo tenga 
            ListView lv = sender as ListView;
            lv.SelectedIndex = -1;
        }

        protected void Detalles_ListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}