using System;
using System.Web.Security;
using System.Linq;
using NominaASP.Code;
using System.IO;
using System.Web;
using System.Text;
using System.Collections.Generic;
using NominaASP.Models.Nomina;
using NominaASP.Models; 

namespace NominaASP.Nomina.PrestacionesSociales
{
    public partial class PrestacionesSociales_ObtencionTxtFile : System.Web.UI.Page
    {
        int _headerID;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (!User.Identity.IsAuthenticated)
                {
                    FormsAuthentication.SignOut();
                    return;
                }

                // -------------------------------------------------------------------------------------------------------------------
                //  intentamos recuperar el state de esta página; en general, lo intentamos con popups filtros 

                if (!(Membership.GetUser().UserName == null))
                {
                    KeepPageState MyKeepPageState = new KeepPageState(Membership.GetUser().UserName, this.GetType().Name.ToString());
                    MyKeepPageState.ReadStateFromFile(this, this.Controls);
                    MyKeepPageState = null;
                }

                if (string.IsNullOrEmpty(this.Fecha_TextBox.Text))
                    this.Fecha_TextBox.Text = DateTime.Today.ToString("dd-MM-yyyy");

                if (string.IsNullOrEmpty(this.Oficina_TextBox.Text))
                    this.Oficina_TextBox.Text = "111";

                if (string.IsNullOrEmpty(this.CodigoArea_TextBox.Text))
                    this.CodigoArea_TextBox.Text = "0001";

                if (string.IsNullOrEmpty(this.CentroCosto_TextBox.Text))
                    this.CentroCosto_TextBox.Text = "100";
            }

            // -------------------------------------------------------------------------------------------------------------------
            // el usuario debe haber seleccionado un 'headerID' (id de prestaciones sociales) antes de intentar ejecutar esta función 

            if (this.Request.QueryString["headerID"] == null || !Int32.TryParse(this.Request.QueryString["headerID"].ToString(), out _headerID))
            {
                string errorMessage = "Aparentemente, Ud. no ha seleccionado un registro de prestaciones sociales;<br /> " +
                    "debe hacerlo, antes de intentar ejecutar esta función.";
                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;
                return;
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            // -------------------------------------------------------------------------------------------------------------------
            // el usuario debe haber seleccionado un 'headerID' (id de prestaciones sociales) antes de intentar ejecutar esta función 

            int headerID;
            
            string errorMessage = ""; 

            if (this.Request.QueryString["headerID"] == null || !Int32.TryParse(this.Request.QueryString["headerID"].ToString(), out headerID))
                errorMessage = "Aparentemente, Ud. no ha seleccionado un registro de prestaciones sociales;<br /> " +
                        "debe hacerlo, antes de intentar ejecutar esta función.";

            if (string.IsNullOrEmpty(this.CodigoContrato_TextBox.Text)) 
                errorMessage = "Ud. debe indicar valores para los campos en esta página.";

            if (string.IsNullOrEmpty(this.Fecha_TextBox.Text))
                errorMessage = "Ud. debe indicar valores para los campos en esta página.";

            if (string.IsNullOrEmpty(this.Oficina_TextBox.Text))
                errorMessage = "Ud. debe indicar valores para los campos en esta página.";

            if (string.IsNullOrEmpty(this.CodigoArea_TextBox.Text))
                errorMessage = "Ud. debe indicar valores para los campos en esta página.";

            if (string.IsNullOrEmpty(this.CentroCosto_TextBox.Text))
                errorMessage = "Ud. debe indicar valores para los campos en esta página.";

            DateTime fecha;
            int intValue;

            if (!Int32.TryParse(this.CodigoContrato_TextBox.Text, out intValue))
                errorMessage = "El código de contrato debe ser un valor numérico.";

            if (!Int32.TryParse(this.Oficina_TextBox.Text, out intValue))
                errorMessage = "El número de oficina debe ser un valor numérico.";

            if (!Int32.TryParse(this.CodigoArea_TextBox.Text, out intValue))
                errorMessage = "El código de area debe ser un valor numérico.";

            if (!Int32.TryParse(this.CentroCosto_TextBox.Text, out intValue))
                errorMessage = "El centro de costo debe ser un valor numérico.";

            if (!DateTime.TryParse(this.Fecha_TextBox.Text, out fecha))
                errorMessage = "Ud. debe indicar una fecha válida.";

            if (this.CodigoContrato_TextBox.Text.ToString().Length > 6)
                errorMessage = "El código de contrato no debe tener más de 6 dígitos.";

            if (this.Oficina_TextBox.Text.ToString().Length > 3)
                errorMessage = "El número de oficina no debe tener más de 3 dígitos.";

            if (this.CodigoArea_TextBox.Text.ToString().Length > 4)
                errorMessage = "El código de area no debe tener más de 4 dígitos.";

            if (this.CentroCosto_TextBox.Text.ToString().Length > 3)
                errorMessage = "El centro de costo no debe tener más de 3 dígitos.";

            if (errorMessage != "")
            {
                CustomValidator1.IsValid = false;
                CustomValidator1.ErrorMessage = errorMessage;
                return;
            }


            // ----------------------------------------------------------------------------------------
            // la siguiente función construye el archivo de texto ... 

            string popupMessage = "";
            string filePath; 

            if (PrestacionesSociales_ObtenerTxtFile(out filePath, out popupMessage))
            {
                // para que el usuario pueda copiar el archivo al disco duro de su PC 

                ObtencionArchivoRetencionesIva_DownloadFile_LinkButton.Visible = true;
                FileName_HiddenField.Value = filePath;
            }

            this.ModalPopupTitle_span.InnerHtml = "Nómina - Prestaciones sociales - Obtención del archivo de texto";
            this.ModalPopupBody_span.InnerHtml = popupMessage;

            this.btnOk.Visible = false;
            this.btnCancel.Text = "Ok";

            this.ModalPopupExtender1.Show();

            // -------------------------------------------------------------------------------------------
            // para guardar el contenido de los controles de la página para recuperar el state cuando 
            // se abra la proxima vez 

            KeepPageState MyKeepPageState = new KeepPageState(Membership.GetUser().UserName, this.GetType().Name.ToString());
            MyKeepPageState.SavePageStateInFile(this.Controls);
            MyKeepPageState = null;
            // ---------------------------------------------------------------------------------------------
        }

        private bool PrestacionesSociales_ObtenerTxtFile(out string filePath, out string popupMessage)
        {
            // Create the CSV file on the server 

            String fileName = @"PrestacionesSociales_" + User.Identity.Name + ".txt";
            filePath = HttpContext.Current.Server.MapPath("~/Temp/" + fileName);

            if (File.Exists(@filePath))
            {
                try
                {
                    File.Delete(@filePath);
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;
                    if (ex.InnerException != null)
                        errorMessage += ex.InnerException.Message;

                    popupMessage = "Ha ocurrido un error al intentar tener acceso al archivo requerido en el servidor (" + @filePath + "). <br /><br />" +
                        "Probablemente el archivo está 'tomado' por algún proceso (puede ser éste).<br />" +
                        "Por favor cierre y abra esta página para intentar liberar el archivo.<br /><br />" +
                        "El mensaje específico del error obtenido es: <br /><br />" + 
                        errorMessage;
                    return false;
                }
            }

            popupMessage = "";
            StreamWriter sw;

            try
            {
                sw = new StreamWriter(filePath, false);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += ex.InnerException.Message;

                popupMessage = "Error al intentar escribir al archivo.<br /><br />" + errorMessage;
                return false;
            }

            // obtenemos los registros de prestaciones que corresponden al HeaderID pasado a esta página, en una lista, para 
            // que se fácil su procesamiento ...

            List<PrestacionesSociale> registroPrestacionesList; 

            using (dbNominaEntities context = new dbNominaEntities())
            {
                registroPrestacionesList = context.PrestacionesSociales.Where(p => p.HeaderID == _headerID).ToList<PrestacionesSociale>();

                if (registroPrestacionesList.Count() == 0)
                {
                    popupMessage = "Error inesperado: aparentemente, el registro de prestaciones indicado no tiene información de empleados asociada; " + 
                        "por favor revise.";
                    return false;
                }
            }

            StringBuilder sb;

            int cantidadLineas = 0;
            string delimitador = "";

            if (this.AgregarDelimitadores_CheckBox.Checked)
                delimitador = "*"; 

            if (this.AgregarEncabezados_CheckBox.Checked)
            {
                // agregamos una linea de encabezado al archivo ... 

                sb = new StringBuilder();

                sb.Append("TR" + delimitador);
                sb.Append("CodCon" + delimitador);
                sb.Append("Fecha " + delimitador);
                sb.Append("CantP" + delimitador);
                sb.Append("Total prest " + delimitador);
                sb.Append("Ofi" + delimitador);
                sb.Append("Relleno");

                sw.Write(sb.ToString());
                sw.Write(sw.NewLine);

                cantidadLineas++;
            }

            decimal montoPrestacionesTotal = registroPrestacionesList.Sum(p => p.MontoPrestaciones);

            montoPrestacionesTotal += registroPrestacionesList.Select(item =>
                                      {
                                          if (item.MontoPrestacionesDiasAdicionales.HasValue == false)
                                              return 0;
                                          else
                                              return item.MontoPrestacionesDiasAdicionales.Value;
                                      }).Sum(item => item);

            sb = new StringBuilder();

            sb.Append("01" + delimitador);
            sb.Append(Convert.ToInt32(this.CodigoContrato_TextBox.Text).ToString("000000") + delimitador);
            sb.Append(Convert.ToDateTime(this.Fecha_TextBox.Text).ToString("ddMMyy") + delimitador);
            sb.Append(registroPrestacionesList.Count().ToString("00000") + delimitador);      // cant personas 
            sb.Append(montoPrestacionesTotal.ToString("0000000000.00").Replace(",", "").Replace(".", "") + delimitador);      // total prestaciones 
            sb.Append(Convert.ToInt32(this.Oficina_TextBox.Text).ToString("000") + delimitador);
            sb.Append("000000000000000000000000" + delimitador);

            sw.Write(sb.ToString());
            sw.Write(sw.NewLine);

            cantidadLineas++;

            // ya escribimos el 1er. registro al archivo; este registro es una especie de 'metadata', que sumariza el resto de los registros; 
            // los escribimos ahora 

            if (this.AgregarEncabezados_CheckBox.Checked)
            {
                // agregamos una linea de encabezado al archivo ... 

                sb = new StringBuilder();

                sb.Append("TR" + delimitador);
                sb.Append("N" + delimitador);
                sb.Append("Cedula    " + delimitador);
                sb.Append("TipoTr" + delimitador);
                sb.Append("Numero de cuenta ban" + delimitador);
                sb.Append("Monto presta" + delimitador);
                sb.Append("CAre" + delimitador);
                sb.Append("CCo");

                sw.Write(sb.ToString());
                sw.Write(sw.NewLine);

                cantidadLineas++;
            }


            dbNominaEntities db = new dbNominaEntities();

            var query = db.PrestacionesSociales.Where(p => p.HeaderID == _headerID);

            foreach (PrestacionesSociale p in query)
            {
                sb = new StringBuilder();

                string nacionalidad = p.tEmpleado.Cedula.Substring(0, 1);
                string cedula = p.tEmpleado.Cedula.Substring(1, p.tEmpleado.Cedula.Length - 1);
                cedula = cedula.Replace(".", "").Replace("-", "").Replace(" ", ""); 
                
                Int64 intCedula; 

                if (!Int64.TryParse(cedula, out intCedula)) 
                {
                    popupMessage = "Error: aparentemente, la cédula: " + cedula + 
                        " no corresponde a un valor correcto (ej: V13252342); por favor revise.";
                    return false;
                }

                string numeroCuentaBancos = p.tEmpleado.NumeroCuentaBancariaPrestacionesSociales;
                numeroCuentaBancos = numeroCuentaBancos.Replace(" ", ""); 

                Int64 intNumeroCuentaBancos;

                if (!Int64.TryParse(numeroCuentaBancos, out intNumeroCuentaBancos))
                {
                    popupMessage = "Error: aparentemente, la cuenta bancaria: " + numeroCuentaBancos + 
                        " no corresponde a un valor correcto (debe ser numérico y menor o igual a 20 dígitos); por favor revise.";
                    return false;
                }

                decimal montoPrestaciones = p.MontoPrestaciones;

                if (p.MontoPrestacionesDiasAdicionales != null)
                    montoPrestaciones += p.MontoPrestacionesDiasAdicionales.Value;

                string strMontoPrestaciones = montoPrestaciones.ToString("0000000000.00").Replace(",", "").Replace(".", ""); 

                sb.Append("02" + delimitador);                                                          // tipo de registro (2 para registros luego del 1ro.) 
                sb.Append(nacionalidad + delimitador);                                                  // nacionalidad  
                sb.Append(intCedula.ToString("0000000000") + delimitador);                              // cédula 

                // -------------------------------------------------------------------------------------------------------------
                // el archivo contiene un campo que indica si el empleado es nuevo o no en el registro de prestaciones ... 

                bool empleadoNuevoParaPrestaciones = false;
                int anoMesPrestacionesSociales = Convert.ToInt32(p.PrestacionesSocialesHeader.Ano.ToString("0000") + p.PrestacionesSocialesHeader.Mes.ToString("00"));

                if (p.tEmpleado.PrestacionesSociales.
                    Where(pr => Convert.ToInt32(pr.PrestacionesSocialesHeader.Ano.ToString("0000") + pr.PrestacionesSocialesHeader.Mes.ToString("00")) 
                        < anoMesPrestacionesSociales)
                    .Count() == 0)
                    // si el empleado no está en prestaciones anteriores, consideramos que es nuevo en este proceso .... 
                    empleadoNuevoParaPrestaciones = true;

                sb.Append((empleadoNuevoParaPrestaciones ? "001000" : "002000") + delimitador);             // tipo de registro 
                // -------------------------------------------------------------------------------------------------------------


                sb.Append(intNumeroCuentaBancos.ToString("00000000000000000000") + delimitador);        // número de cuenta bancaria 
                sb.Append(strMontoPrestaciones + delimitador);                                          // monto de prestaciones 
                sb.Append(this.CodigoArea_TextBox.Text + delimitador);                                  // código de area 
                sb.Append(this.CentroCosto_TextBox.Text);                                               // centro de costo 

                sw.Write(sb.ToString());
                sw.Write(sw.NewLine);

                cantidadLineas++;
            }



            try
            {
                // finally close the file 
                sw.Close();
                sw = null;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += ex.InnerException.Message;

                popupMessage = "Ha ocurrido un error al intentar grabar el archivo requerido en el servidor. <br /><br />" + errorMessage;
                return false;
            }

            popupMessage = "Ok, el archivo de texto ha sido generado en forma satisfactoria. <br />" +
                           "La cantidad de registros que se han grabado al archivo es: " + cantidadLineas.ToString() + ". <br /><br />" +
                           "El nombre del archivo en el servidor es: " + filePath + ".<br /><br />" +
                           "Cierre este diálogo y haga un click al link <span style='color: blue; font-style: italic; '>Copiar archivo al disco duro local</span>, " + 
                           "para copiar el archivo construido a su PC.";

            return true; 
        }

        protected void btnOk_Click(object sender, EventArgs e)
        {

        }

        protected void ObtencionArchivoRetencionesIva_DownloadFile_LinkButton_Click(object sender, EventArgs e)
        {
            // hacemos un download del archivo recién generado para que pueda ser copiado al disco duro 
            // local por del usuario 

            if (FileName_HiddenField.Value == null || FileName_HiddenField.Value == "")
            {
                string popupMessage = "No se ha podido obtener el nombre del archivo generado. <br /><br />" + 
                    "Genere el archivo nuevamente y luego intente copiarlo al disco de su PC usando esta función.";

                this.ModalPopupTitle_span.InnerHtml = "Nómina - Prestaciones sociales - Obtención del archivo de texto";
                this.ModalPopupBody_span.InnerHtml = popupMessage;

                this.btnOk.Visible = false;
                this.btnCancel.Text = "Ok";

                this.ModalPopupExtender1.Show();

                return;
            }


            FileStream liveStream = new FileStream(FileName_HiddenField.Value, FileMode.Open, FileAccess.Read);

            byte[] buffer = new byte[(int)liveStream.Length];
            liveStream.Read(buffer, 0, (int)liveStream.Length);
            liveStream.Close();

            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Length", buffer.Length.ToString());
            Response.AddHeader("Content-Disposition", "attachment; filename=" + FileName_HiddenField.Value);
            Response.BinaryWrite(buffer);
            Response.End();
        }

    }
}
