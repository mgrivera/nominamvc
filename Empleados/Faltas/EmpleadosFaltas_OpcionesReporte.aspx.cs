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

namespace NominaASP.Empleados.Faltas
{
    public partial class EmpleadosFaltas_OpcionesReporte : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }

            // -----------------------------------------------------------------------------------------

            Master.Page.Title = "Nómina - Empleados - Faltas - Consulta";

            if (Session["ConsultaEmpleadosFaltas_CriteriosFiltro"] == null)
            {
                string errorMessage = "Aparentemente, Ud. no ha aplicado un filtro que permita seleccionar registros. " +
                    "Por favor, cierre esta página y aplique un filtro para seleccionar registros.";

                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;

                return;
            }

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
            }
        }

        protected void ObtenerReporte_Button_Click(object sender, EventArgs e)
        {
            if (Session["ConsultaEmpleadosFaltas_CriteriosFiltro"] == null)
            {
                string errorMessage = "Aparentemente, Ud. no ha aplicado un filtro que permita seleccionar registros. " + 
                    "Por favor, cierre esta página y aplique un filtro para seleccionar registros.";

                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;

                return;
            }

            if (!this.NormalFormat_RadioButton.Checked && !this.PdfFormat_RadioButton.Checked)
            {
                string errorMessage = "Por seleccione una opción para el formato del archivo (normal, pdf).";

                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;

                return;
            }

            // -------------------------------------------------------------------------------------------
            // para guardar el contenido de los controles de la página para recuperar el state cuando 
            // se abra la proxima vez 

            KeepPageState MyKeepPageState = new KeepPageState(Membership.GetUser().UserName, this.GetType().Name.ToString());
            MyKeepPageState.SavePageStateInFile(this.Controls);   
            MyKeepPageState = null;
            // ---------------------------------------------------------------------------------------------

            if (this.NormalFormat_RadioButton.Checked)
                Response.Redirect("~/ReportViewer.aspx?rpt=faltasEmpleados&opc=normal");
            else
                Response.Redirect("~/ReportViewer.aspx?rpt=faltasEmpleados&opc=pdf");
        }
    }
}