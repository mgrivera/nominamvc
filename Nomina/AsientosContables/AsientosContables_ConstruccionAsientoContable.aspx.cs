using System;
using System.Web.Security;
using System.Linq;
using NominaASP.Code;
using System.IO;
using System.Web;
using System.Text;
using System.Collections.Generic;
//using NominaASP.Models.Nomina;
using NominaASP.Models;

namespace NominaASP.Nomina.AsientosContables
{
    public partial class AsientosContables_ConstruccionAsientoContable : System.Web.UI.Page
    {
        int _headerID;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }

            // -------------------------------------------------------------------------------------------------------------------
            // el usuario debe haber seleccionado un 'headerID' (id de prestaciones sociales) antes de intentar ejecutar esta función 

            if (this.Request.QueryString["headerID"] == null || !Int32.TryParse(this.Request.QueryString["headerID"].ToString(), out _headerID))
            {
                string errorMessage = "Aparentemente, Ud. no ha seleccionado un nómina de la lista;<br /> " +
                    "debe hacerlo, antes de intentar ejecutar esta función.";
                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;
                return;
            }

            _headerID = Convert.ToInt32(this.Request.QueryString["headerID"]); 

            if (!Page.IsPostBack)
            {
                //// -------------------------------------------------------------------------------------------------------------------
                ////  intentamos recuperar el state de esta página; en general, lo intentamos con popups filtros 

                //if (!(Membership.GetUser().UserName == null))
                //{
                //    KeepPageState MyKeepPageState = new KeepPageState(Membership.GetUser().UserName, this.GetType().Name.ToString());
                //    MyKeepPageState.ReadStateFromFile(this, this.Controls);
                //    MyKeepPageState = null;
                //}


                dbNominaEntities context = new dbNominaEntities();
                tNominaHeader nominaHeader = context.tNominaHeaders.Where(h => h.ID == _headerID).FirstOrDefault();

                if (nominaHeader == null)
                    return;                 // ésto nunca debería pasar ... 

                if (nominaHeader.Desde == null || nominaHeader.Hasta == null)
                {
                    string errorMessage = "Aparentemente, no se ha definido un período (desde/hasta) para la nómina seleccionada;<br /> " +
                                            "por favor cierre esta página y defina y registre un período (desde/hasta) para la nómina seleccionada<br />" +
                                            "antes de intentar ejecutar esta funcion.";
                    CustomValidator1.IsValid = false;
                    CustomValidator1.ErrorMessage = errorMessage;
                    return;
                }

                CambiosMoneda cambiosMoneda = context.CambiosMonedas.Where(c => c.Fecha <= nominaHeader.FechaNomina).OrderByDescending(c => c.Fecha).FirstOrDefault();

                if (cambiosMoneda != null)
                    this.FactorCambio_TextBox.Text = cambiosMoneda.Cambio.ToString("N2");

                this.Fecha_TextBox.Text = nominaHeader.FechaNomina.ToString("dd-MM-yyyy");

                string descripcionAsiento = "Asiento contable de nómina que corresponde al período: " +
                    nominaHeader.Desde.Value.ToString("dd-MMM-yyyy") + " a " +
                    nominaHeader.Hasta.Value.ToString("dd-MMM-yyyy") + ".";

                this.Descripcion_TextBox.Text = descripcionAsiento;

                _headerID = nominaHeader.ID;

                this.OrderByEmpleado_RadioButton.Checked = true;
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            // -------------------------------------------------------------------------------------------------------------------
            // el usuario debe haber seleccionado un 'headerID' (id de prestaciones sociales) antes de intentar ejecutar esta función 
            
            string errorMessage = "";
            int headerID; 

            if (this.Request.QueryString["headerID"] == null || !Int32.TryParse(this.Request.QueryString["headerID"].ToString(), out headerID))
                errorMessage = "Aparentemente, Ud. no ha seleccionado un nómina de la lista;<br /> " +
                    "debe hacerlo, antes de intentar ejecutar esta función.";

            if (string.IsNullOrEmpty(this.Descripcion_TextBox.Text))
                errorMessage = "Ud. debe indicar una descripción para el asiento contable<br /> " +
                    "(nota: si deja este campo en blanco y cierra y abre esta página, un<br /> " + 
                    "valor 'por defecto' será mostrado).";

            if (string.IsNullOrEmpty(this.Fecha_TextBox.Text))
                errorMessage = "Ud. debe indicar una fecha válida.";

            if (string.IsNullOrEmpty(this.FactorCambio_TextBox.Text))
                errorMessage = "Ud. debe indicar un factor de cambio válido a esta página (ej: 6,30).";

            DateTime fecha;
            decimal factorCambio;

            if (!Decimal.TryParse(this.FactorCambio_TextBox.Text, out factorCambio))
                errorMessage = "Aparentemente, el factor de cambio indicado no es un valor válido (ej: 6,30).";

            if (!DateTime.TryParse(this.Fecha_TextBox.Text, out fecha))
                errorMessage = "Ud. debe indicar una fecha válida.";

            if (errorMessage != "")
            {
                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;
                return;
            }


            // ----------------------------------------------------------------------------------------------------------------------
            // la siguiente función construye el asiento contable para la nómina seleccionada en la lista y pasada a esta página ... 

            Nomina_AsientoContable nomina_AsientoContable = new Nomina_AsientoContable(_headerID);

            string orderBy = "";

            if (this.OrderByEmpleado_RadioButton.Checked)
                orderBy = "empleado";

            if (this.OrderByRubro_RadioButton.Checked)
                orderBy = "rubro"; 

            string resultMessage = "";

            if (nomina_AsientoContable.ConstruirAsientoContable(this.Descripcion_TextBox.Text, fecha, factorCambio, orderBy, User.Identity.Name, out resultMessage))
                this.ModalPopupTitle_span.InnerHtml = "Nómina - Construcción del asiento contable de nómina";
            else
                this.ModalPopupTitle_span.InnerHtml = "Error al intentar construir el asiento contable";

            this.ModalPopupBody_span.InnerHtml = resultMessage;

            this.btnOk.Visible = false;
            this.btnCancel.Text = "Ok";

            this.ModalPopupExtender1.Show();

            // -------------------------------------------------------------------------------------------
            // para guardar el contenido de los controles de la página para recuperar el state cuando 
            // se abra la proxima vez 

            //KeepPageState MyKeepPageState = new KeepPageState(Membership.GetUser().UserName, this.GetType().Name.ToString());
            //MyKeepPageState.SavePageStateInFile(this.Controls);
            //MyKeepPageState = null;
            // ---------------------------------------------------------------------------------------------
        }

        protected void btnOk_Click(object sender, EventArgs e)
        {

        }
    }
}
