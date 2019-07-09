using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.IO;
using System.Text;
using System.Linq;
using System.Web.UI.WebControls;
using NominaASP.Code;
using NominaASP.Models.Nomina;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using DocumentFormat.OpenXml.Bibliography;
using System.Collections.Generic;
using Bullzip.PdfWriter;
using System.Web.UI.HtmlControls;

namespace NominaASP.Nomina.Nomina
{
    public partial class OpcionesReporte : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }

            // -----------------------------------------------------------------------------------------

            Master.Page.Title = "Nómina - Generación de archivo para 'combinación de correspondencia' en Microsoft Word";

            if (!Page.IsPostBack)
            {
                // -------------------------------------------------------------------------------------------------------------------
                //  intentamos recuperar el state de esta página; en general, lo intentamos con popups filtros 

                if (!(Membership.GetUser().UserName == null))
                {
                    KeepPageState MyKeepPageState = new KeepPageState(Membership.GetUser().UserName, this.GetType().Name.ToString());
                    MyKeepPageState.ReadStateFromFile(this, this.Controls);
                    MyKeepPageState = null;
                }

                string filter = "";

                // por alguna razón, la consulta pasa un filtro muy complejo y no pudimos abrir esta página usando ese filtro
                // la razón básica, es que el fitro usa comillas simples, como ' y entorpecía el window.open en javascript ... 
                if (Session["FiltroConsultaNomina"] != null)
                    filter = Session["FiltroConsultaNomina"].ToString();
                else
                {
                    this.CustomValidator1.ErrorMessage = "No se ha pasado un parámetro a esta página que corresponda al filtro de la consulta; " +
                        "Por favor intente nuevamente.";
                    this.CustomValidator1.IsValid = false;

                    return; 
                }

                string agruparPor = "empleado";

                if (this.AgruparPorRubro_RadioButton.Checked)
                    agruparPor = "rubro";

                HtmlAnchor link = this.ObtenerReporte_HtmlAnchor as HtmlAnchor;
                link.HRef = "javascript:PopupWin('" + "../../ReportViewer.aspx?rpt=consultaNomina&opcion=nomina&agrupar=" + agruparPor + "', 1000, 680)";
            }
                // -------------------------------------------------------------------------------------------------------------------    
        }

        protected void SetLinkAddress(object sender, EventArgs e)
        {
            string filter = "";

            // por alguna razón, la consulta pasa un filtro muy complejo y no pudimos abrir esta página usando ese filtro
            // la razón básica, es que el fitro usa comillas simples, como ' y entorpecía el window.open en javascript ... 
            if (Session["FiltroConsultaNomina"] != null)
                filter = Session["FiltroConsultaNomina"].ToString();
            else
            {
                this.CustomValidator1.ErrorMessage = "No se ha pasado un parámetro a esta página que corresponda al filtro de la consulta; " +
                    "Por favor intente nuevamente.";
                this.CustomValidator1.IsValid = false;

                return; 
            }

            string agruparPor = "empleado";

            if (this.AgruparPorRubro_RadioButton.Checked)
                agruparPor = "rubro";

            HtmlAnchor link = this.ObtenerReporte_HtmlAnchor as HtmlAnchor;
            link.HRef = "javascript:PopupWin('" + "../../ReportViewer.aspx?rpt=consultaNomina&opcion=nomina&agrupar=" + agruparPor + "', 1000, 680)";
            // -------------------------------------------------------------------------------------------------------------------    

            // -------------------------------------------------------------------------------------------
            // para guardar el contenido de los controles de la página para recuperar el state cuando 
            // se abra la proxima vez 

            KeepPageState MyKeepPageState = new KeepPageState(Membership.GetUser().UserName, this.GetType().Name.ToString());
            MyKeepPageState.SavePageStateInFile(this.Controls);
            MyKeepPageState = null;
            // ---------------------------------------------------------------------------------------------
        }

        protected void btnOk_Click(object sender, EventArgs e)
        {
        }
    }
}