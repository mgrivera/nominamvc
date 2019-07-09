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

namespace NominaASP.Nomina.AsientosContables
{
    public partial class AsientosContables_page : System.Web.UI.Page
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

                SqlDataSource1.SelectParameters["numeroCiaContab"].DefaultValue = companiaSeleccionada.Numero.ToString();
            }
        }
        
        protected void Nomina_GridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }

            if (this.Nomina_GridView.SelectedDataKey == null)
                return;

            int nominaHeader = Convert.ToInt32(Nomina_GridView.SelectedDataKey.Value.ToString());

            // ---------------------------------------------------------------------------------------------------------------------------
            // establecemos el href de links que construyen un reporte para el registro seleccionado ... 

            HtmlAnchor link = this.ConstruirAsientoContable_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('AsientosContables_ConstruccionAsientoContable.aspx?headerID=" + nominaHeader.ToString() + "', 1000, 680)";

            link = this.ConstruirAsientoContable2_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('AsientosContables_ConstruccionAsientoContable.aspx?headerID=" + nominaHeader.ToString() + "', 1000, 680)";

            link = this.ConsultarAsientoContable_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('AsientoContable_Consulta.aspx', 1000, 680)";

            link = this.ConsultarAsientoContable2_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('AsientoContable_Consulta.aspx', 1000, 680)";

            dbNominaEntities context = new dbNominaEntities();
            tNominaHeader header = context.tNominaHeaders.Where(h => h.ID == nominaHeader).FirstOrDefault();

            if (header != null)
            {
                link = this.ConsultarAsientoContable_HtmlAnchor as HtmlAnchor;
                link.HRef = "javascript:PopupWin('AsientoContable_Consulta.aspx?AsientoContableID=" + header.AsientoContableID.ToString() + "', 1000, 680)";

                link = this.ConsultarAsientoContable2_HtmlAnchor as HtmlAnchor;
                link.HRef = "javascript:PopupWin('AsientoContable_Consulta.aspx?AsientoContableID=" + header.AsientoContableID.ToString() + "', 1000, 680)";
            }
        }

        protected void Nomina_GridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView gv = (GridView)sender;
            gv.SelectedIndex = -1;
        }

        //protected void btnOk_Click(object sender, EventArgs e)
        //{
        //    // cuando el usuario confirma la ejecución de la nómina, regresamos a este método ... 

        //    //NominaASP.Code.Nomina nomina = new NominaASP.Code.Nomina(Convert.ToInt32(this.Nomina_GridView.SelectedDataKey.Value));

        //    //string errorMessage = "";
        //    //string resultadoEjecucionMessage = "";

        //    //nomina.Ejecutar(out errorMessage, out resultadoEjecucionMessage);

        //    //if (errorMessage != "")
        //    //{
        //    //    this.ModalPopupTitle_span.InnerHtml = "Nómina - Ejecución - Error al intentar ejecutar la nómina seleccionada";
        //    //    this.ModalPopupBody_span.InnerHtml = errorMessage;
        //    //}
        //    //else
        //    //{
        //    //    this.ModalPopupTitle_span.InnerHtml = "Nómina - Ejecución";
        //    //    this.ModalPopupBody_span.InnerHtml = resultadoEjecucionMessage;
        //    //}

        //    //this.btnOk.Visible = false;
        //    //this.btnCancel.Text = "Ok";

        //    //this.ModalPopupExtender1.Show();
        //}

        //public tNominaHeader Nomina_FormView_GetData()
        //{
        //    if (this.Nomina_GridView.SelectedDataKey == null)
        //        return null;

        //    int nominaHeaderPK = Convert.ToInt32(this.Nomina_GridView.SelectedDataKey.Value); 

        //    tNominaHeader item = null;

        //    using (dbNominaEntities context = new dbNominaEntities())
        //    {
        //        item = context.tNominaHeaders.Include("tGruposEmpleado").Where(n => n.ID == nominaHeaderPK).FirstOrDefault();
        //    }

        //    return item;
        //}

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

       

        //protected void cancelButton_Click(object sender, EventArgs e)
        //{
        //    //Response.Redirect("~/Ventas/Lista/Ventas_List.aspx");
        //}

        //protected void EjecutarNomina_ImageButton_Click(object sender, ImageClickEventArgs e)
        //{
        //    ConfirmarEjecucionNomina(); 
        //}

        //protected void EjecutarNomina_LinkButton_Click(object sender, EventArgs e)
        //{
        //    ConfirmarEjecucionNomina(); 
        //}

        //private void ConfirmarEjecucionNomina()
        //{
        //    if (this.Nomina_GridView.SelectedIndex == -1)
        //    {
        //        this.ModalPopupTitle_span.InnerHtml = "Nómina - Ejecución";
        //        this.ModalPopupBody_span.InnerHtml = "Ud. debe seleccionar el registro en la lista que corresponda a la nómina que desea ejecutar.";

        //        // usamos btnOk solo si queremos un postback (a btnOk.Click); por ejemplo, si mostramos un mensaje al usuario pero él puede 
        //        // decidir continuar (o cancelar); si continúa, hacemos el postback y continuamos en el servidor. 
        //        // En este caso, solo queremos que el usuario lea el mensaje y cierre el diálogo. Por eso, no mostramos btnOk y usamos Cancel como Ok ... 

        //        this.btnOk.Visible = false;
        //        this.btnCancel.Text = "Ok";

        //        this.ModalPopupExtender1.Show();
        //        return;
        //    }

        //    string ajaxPopupMessage;
        //    int nominaID = Convert.ToInt32(this.Nomina_GridView.SelectedDataKey.Value);

        //    using (dbNominaEntities context = new dbNominaEntities())
        //    {
        //        tNominaHeader nominaHeader = context.tNominaHeaders.Where(n => n.ID == nominaID).First();

        //        ajaxPopupMessage = "Desea ejecutar la nómina para el grupo de nómina: " + nominaHeader.tGruposEmpleado.Descripcion +
        //        " y el período: " +
        //        (nominaHeader.Desde == null ? "(período no definido)" : nominaHeader.Desde.Value.ToString("d-MMM-yyyy")) +
        //        " a " +
        //        (nominaHeader.Hasta == null ? "(período no definido)" : nominaHeader.Hasta.Value.ToString("d-MMM-yyyy"));
        //    }

        //    // ahora pedimos una confirmación al usuario; nótese que para hacerlo, usamos el ajax popup, pero si el usuario confirma se ejecuta el 
        //    // evento OnClick del butón ... 

        //    this.ModalPopupTitle_span.InnerHtml = "Nómina - Ejecución";
        //    this.ModalPopupBody_span.InnerHtml = ajaxPopupMessage;

        //    // usamos btnOk solo si queremos un postback (a btnOk.Click); por ejemplo, si mostramos un mensaje al usuario pero él puede 
        //    // decidir continuar (o cancelar); si continúa, hacemos el postback y continuamos en el servidor. 
        //    // En este caso, solo queremos que el usuario lea el mensaje y cierre el diálogo. Por eso, no mostramos btnOk y usamos Cancel como Ok ... 

        //    this.btnOk.Visible = true;
        //    this.btnOk.Text = "Ejecutar nómina";
        //    this.btnCancel.Text = "Cancelar";

        //    this.ModalPopupExtender1.Show();
        //}

        //protected void ConstruirAsientoContable_Button_Click(object sender, EventArgs e)
        //{

        //}

    }
}