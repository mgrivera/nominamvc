using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.IO;
using System.Text;
using System.Linq;
using System.Web.UI.WebControls;
using NominaASP.Code;
//using NominaASP.Models.Nomina;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using DocumentFormat.OpenXml.Bibliography;
using System.Collections.Generic;
using Bullzip.PdfWriter;
using NominaASP.Models;
using Spire.Doc;

namespace NominaASP.Nomina.Nomina
{
    public partial class Nomina_ConstruccionTextFiles : System.Web.UI.Page
    {
        int _headerID = 0;

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
                Session["FileToDownload"] = null; 

                // -------------------------------------------------------------------------------------------------------------------
                //  intentamos recuperar el state de esta página; en general, lo intentamos con popups filtros 

                if (!(Membership.GetUser().UserName == null))
                {
                    KeepPageState MyKeepPageState = new KeepPageState(Membership.GetUser().UserName, this.GetType().Name.ToString());
                    MyKeepPageState.ReadStateFromFile(this, this.Controls);
                    MyKeepPageState = null;
                }
                // -------------------------------------------------------------------------------------------------------------------

                this.OpcionesMailMerge_DropDownList.SelectedValue = "0";

                if (this.Request.QueryString["headerID"] == null || !Int32.TryParse(this.Request.QueryString["headerID"].ToString(), out _headerID))
                {
                    string errorMessage = "Aparentemente, Ud. no ha seleccionado un registro de nómina;<br /> " +
                        "debe hacerlo, antes de intentar ejecutar esta función.";

                    this.ModalPopupTitle_span.InnerHtml = "Nómina - Obtención del archivo de texto";
                    this.ModalPopupBody_span.InnerHtml = errorMessage;

                    this.btnOk.Visible = false;
                    this.btnCancel.Text = "Ok";

                    this.ModalPopupExtender1.Show();

                    return;
                }
            }
            else
                if (!(this.Request.QueryString["headerID"] == null)) 
                    Int32.TryParse(this.Request.QueryString["headerID"].ToString(), out _headerID); 
        }

        protected void GenerarMailMergeFile_Button_Click(object sender, EventArgs e)
        {
            string popupMessage = "";
            if (!GenerarArchivo_RecibosPago2(out popupMessage))
            {
                this.ModalPopupTitle_span.InnerHtml = "Error al intentar obtener los recibos de pago";
                //this.OpcionesFormato_RecibosPago_Div.Visible = false; 
            }
            else
            {
                this.ModalPopupTitle_span.InnerHtml = "Nómina - Obtención del archivo de texto";
                this.OpcionesFormato_RecibosPago_Div.Visible = true; 
            }

            this.ModalPopupBody_span.InnerHtml = popupMessage;
            btnOk.Visible = false;
            this.btnCancel.Text = "Ok";

            this.ModalPopupExtender1.Show();

            return;
        }

        private bool GenerarArchivo_RecibosPago2(out string popupMessage)
        {
            popupMessage = ""; 

            // leemos el header que corresponde a la nómina seleccionada 

            tNominaHeader header;
            dbNominaEntities context = new dbNominaEntities();

            header = context.tNominaHeaders.Include("tGruposEmpleado").Include("tGruposEmpleado.Compania").Where(h => h.ID == _headerID).FirstOrDefault(); 

            if (header == null) 
            {
                popupMessage = "Error inesperado: no hemos encontrado el registro (header) que corresponde a la nómina seleccionada.<br /> " +
                        "Cierre esta función y seleccione nuevamente una nómina antes de ejecutar, nuevamente, esta función.";

                context = null; 
                return false;     
            }


            // --------------------------------------------------------------------------------------------------------------
            // la clase Nomina contiene muchas funciones necesarias para el cálculo de la nómina 
            // usamos varios métodos de esta clase más abajo, para obtener el sueldo de cada empleado y la cantidad de días 
            // hábiles y feriados del período de pago ... 
            string errorMessage = "";
            NominaASP.Code.Nomina Nomina = new Code.Nomina(_headerID, out errorMessage);

            // determinamos la cantidad de días feriados; luego lo usamos para separar el sueldo en estos tipos de fechas 
            int cantDiasHabiles = 0;
            int cantDiasFeriados = 0;
            int cantDiasFinSemana = 0;
            int cantDiasBancarios = 0;

            Nomina.DeterminarCantidadDiasFeriadosEnPeriodo(context,
                                                    header.Desde.Value,
                                                    header.Hasta.Value,
                                                    header.CantidadDias.Value,
                                                    out cantDiasHabiles,
                                                    out cantDiasFinSemana,
                                                    out cantDiasFeriados,
                                                    out cantDiasBancarios);

            cantDiasFeriados += cantDiasFinSemana + cantDiasBancarios; 
            // --------------------------------------------------------------------------------------------------------------
            // verificamos que el directorio exista (si no, lo agregamos) 

            string userName = Membership.GetUser().UserName;

            // nótese como obtenemos el directorio de la aplicación y no el del dominio (c:\inetpub\wwwroot\) 
            // (en nuestro caso, casi nunca son los mismos ...) 
            string path = Server.MapPath("~/Temp/RecibosPago/" + header.tGruposEmpleado.Compania.Abreviatura + "/");

            try
            {
                System.IO.Directory.CreateDirectory(path);      // creamos el directorio si no existe ... 
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    errorMessage += "<br />" + ex.InnerException.Message;

                popupMessage = errorMessage;

                context = null;
                return false;     
            }

            // si la plantilla word no existe, lo notificamos al usuario 
            if (!File.Exists(path + "RecibosPago.docx"))
            {
                popupMessage = "Error: no hemos encontrado el documento Word (plantilla) necesario para ejecutar esta función.<br /> " +
                        "Ud. debe copiar la plantilla que corresponde al archivo '" + path + "RecibosPago.docx" + "'.";
                context = null;

                return false;     
            }

            // eliminamos los archivos que puedan existir ... 
            string[] filePaths = Directory.GetFiles(@path, "*_*_*_" + userName + ".docx");

            try
            {
                foreach (string filePath in filePaths)
                {
                    File.Delete(filePath);
                }
            }
            catch(Exception ex)
            {
                if (ex.InnerException != null)
                    errorMessage += "<br />" + ex.InnerException.Message;

                errorMessage += "<br />" + "Nota: para corregir esta situación, tal vez basta con cerrar y reejecutar este proceso.";

                popupMessage = errorMessage;

                context = null;
                return false;    
            }
            

            var query = from n in context.tNominas
                        where n.HeaderID == _headerID
                        group n by new
                        {
                            Empleado = n.Empleado,
                            NombreCiaContab = n.tNominaHeader.tGruposEmpleado.Compania.Nombre,
                            FechaNomina = n.tNominaHeader.FechaNomina,
                            NombreEmpleado = n.tEmpleado.Nombre,
                            NumeroCedula = n.tEmpleado.Cedula,
                            NombreDepartamento = n.tEmpleado.tDepartamento.Descripcion,
                            NombreCargo = n.tEmpleado.tCargo.Descripcion, 
                            CuentaBancos = n.tEmpleado.CuentaBancariaDepositosNomina,
                            CuentaBancosNombreBanco = n.tEmpleado.Banco.Nombre, 

                            FechaPago = n.tNominaHeader.FechaPago, 
                            Desde = n.tNominaHeader.Desde, 
                            Hasta = n.tNominaHeader.Hasta, 
                            CantidadDias = n.tNominaHeader.CantidadDias, 
                            TipoNomina = n.tNominaHeader.Tipo, 
                            FechaIngresoEmpleado = n.tEmpleado.FechaIngreso
                        } into g
                        select new
                        {
                            NombreCiaContab = g.Key.NombreCiaContab,
                            FechaNomina = g.Key.FechaNomina,

                            EmpleadoID = g.Key.Empleado, 
                            NombreEmpleado = g.Key.NombreEmpleado,
                            NumeroCedula = g.Key.NumeroCedula,
                            NombreDepartamento = g.Key.NombreDepartamento,
                            NombreCargo = g.Key.NombreCargo, 
                            CuentaBancos = g.Key.CuentaBancos,
                            CuentaBancosNombreBanco = g.Key.CuentaBancosNombreBanco,

                            FechaPago = g.Key.FechaPago, 
                            Desde = g.Key.Desde, 
                            Hasta = g.Key.Hasta, 
                            CantidadDias = g.Key.CantidadDias, 
                            TipoNomina = g.Key.TipoNomina, 
                            FechaIngresoEmpleado = g.Key.FechaIngresoEmpleado, 

                            SumOfAsignaciones = (decimal?)g.Where(s => s.Monto > 0).Sum(s => s.Monto),
                            SumOfDeducciones = (decimal?)g.Where(s => s.Monto < 0).Sum(s => s.Monto),
                            SumOfTotal = g.Sum(s => s.Monto), 
                            CantRubros = g.Count(), 

                            Items = g
                        };


            int cantidadRegistrosLeidos = 0;
            WordprocessingDocument doc = null; 

            foreach (var n in query.OrderBy(n => n.NombreEmpleado))
            {
                int linea = 1; 
                int pagina = 1; 

                int cantidadPaginas = 0;
                decimal p = n.CantRubros / 16M;             // son 16 rubros (max) en cada página ... 
                if (p == Math.Truncate(p))
                    cantidadPaginas = Convert.ToInt32(p);
                else
                    cantidadPaginas = Convert.ToInt32(Math.Truncate(p) + 1);

                // nótese como ordenamos primero por tipo, para que se muestren asig/deducc, y luego por número único, que es un identity (pk). Al 
                // hacerlo, leemos en el mismo orden que se registraron los rubros al ser calculada la nómina. Primero los sueldos (hab/fin semana/...), 
                // y, para deducciones, primero la deducción por anticipo en la 1ra. quincena, cuando existe 
                foreach (var rubro in n.Items.OrderBy(r => r.Tipo).OrderBy(r => r.NumeroUnico))
                {
                    if (linea == 1)
                    {
                        // primera linea; comenzamos una nueva página ... 

                        // copiamos el documento original en uno nuevo, para accederlo, cambiarlo y grabarlo ... 
                        string nombreEmpleado = n.NombreEmpleado.Replace(" ", "").Replace(",", "").Replace(".", "").Replace("_", "").Trim();
                        string newFile = @path + nombreEmpleado + "_" + pagina.ToString() + "_" + n.EmpleadoID + "_" + userName + ".docx";
                        File.Copy(path + "RecibosPago.docx", newFile);
                        doc = WordprocessingDocument.Open(newFile, true);

                        TextReplacer.SearchAndReplace(doc, "<pag>", pagina.ToString(), false);
                        TextReplacer.SearchAndReplace(doc, "<totalpags>", cantidadPaginas.ToString(), false);

                        TextReplacer.SearchAndReplace(doc, "<Compania>", n.NombreCiaContab, false);
                        TextReplacer.SearchAndReplace(doc, "<Nombre>", n.NombreEmpleado, false);
                        TextReplacer.SearchAndReplace(doc, "<Cedula>", n.NumeroCedula, false);
                        TextReplacer.SearchAndReplace(doc, "<Departamento>", n.NombreDepartamento, false);
                        TextReplacer.SearchAndReplace(doc, "<Cargo>", n.NombreCargo, false);
                        TextReplacer.SearchAndReplace(doc, "<Fecha>", n.FechaNomina.ToString("dd-MMM-yyyy"), false);

                        string cuentaBancaria = "";
                        string cuentaBancosNombreBanco = ""; 

                        if (n.CuentaBancos == null)
                        {
                            cuentaBancaria = "sin cuenta bancaria asignada";
                            cuentaBancosNombreBanco = "sin cuenta bancaria asignada"; 
                        }
                        else
                        {
                            cuentaBancaria = n.CuentaBancos;
                            cuentaBancosNombreBanco = n.CuentaBancosNombreBanco;  
                        }

                        TextReplacer.SearchAndReplace(doc, "<CuentaBancaria>", cuentaBancaria, false);
                        TextReplacer.SearchAndReplace(doc, "<Banco>", cuentaBancosNombreBanco, false);


                        if (n.FechaPago.HasValue)
                        {
                            TextReplacer.SearchAndReplace(doc, "<FechaPago>", n.FechaPago.Value.ToString("dd-MMM-yyyy"), false);
                        }

                        string tipoNominaDescripcion = ""; 

                        switch (n.TipoNomina)
                            {
                                case "M":
                                    tipoNominaDescripcion = "Mensual";
                                    break; 
                                case "N":
                                    tipoNominaDescripcion = "Normal";
                                    break; 
                                case "Q":
                                    tipoNominaDescripcion = "Quincenal";
                                    break; 
                                case "1Q":
                                    tipoNominaDescripcion = "1ra. quincena";
                                    break; 
                                case "2Q":
                                    tipoNominaDescripcion = "2da. quincena";
                                    break; 
                                case "V":
                                    tipoNominaDescripcion = "Vacaciones";
                                    break; 
                                case "E":
                                    tipoNominaDescripcion = "Especial";
                                    break; 
                                case "U":
                                    tipoNominaDescripcion = "Utilidades";
                                    break; 
                                default: 
                                    tipoNominaDescripcion = "Indefinida (" + n.TipoNomina + ")";
                                    break; 
                            }
                            
                        TextReplacer.SearchAndReplace(doc, "<Desde>", n.Desde.Value.ToString("dd-MMM-yyyy"), false);
                        TextReplacer.SearchAndReplace(doc, "<Hasta>", n.Hasta.Value.ToString("dd-MMM-yyyy"), false);
                        TextReplacer.SearchAndReplace(doc, "<CantidadDias>", n.CantidadDias.Value.ToString(), false);
                        TextReplacer.SearchAndReplace(doc, "<TipoNomina>", tipoNominaDescripcion, false);
                        TextReplacer.SearchAndReplace(doc, "<FechaIngresoEmpleado>", n.FechaIngresoEmpleado.ToString("dd-MMM-yyyy"), false);

                        
                        // la clase Nomina contiene muchas funciones necesarias para el cálculo de la nómina 
                        // usamos la siguiente función para obtener el sueldo del empleado 
                        decimal sueldoEmpleado = 0, sueldoDiarioEmpleado = 0; 
                        sueldoEmpleado = Nomina.DeterminarSueldoEmpleado(context, n.EmpleadoID, n.Desde.Value);

                        sueldoDiarioEmpleado = sueldoEmpleado / 30;

                        TextReplacer.SearchAndReplace(doc, "<salarioMensual>", sueldoEmpleado.ToString("N2"), false);
                        TextReplacer.SearchAndReplace(doc, "<salarioDiario>", sueldoDiarioEmpleado.ToString("N2"), false);

                        TextReplacer.SearchAndReplace(doc, "<cantidadDiasHabiles>", cantDiasHabiles.ToString(), false);
                        TextReplacer.SearchAndReplace(doc, "<cantidadDiasFeriados>", cantDiasFeriados.ToString(), false);

                        // cuando la nómina para un empleado no tiene deducciones o asignaciones, estos montos vienen en null ... 

                        TextReplacer.SearchAndReplace(doc, "<TotalAsignaciones>", n.SumOfAsignaciones != null ? n.SumOfAsignaciones.Value.ToString("N2") : "0,00", false);
                        TextReplacer.SearchAndReplace(doc, "<TotalDeducciones>", n.SumOfDeducciones != null ? (n.SumOfDeducciones * -1).Value.ToString("N2") : "0,00", false);

                        if (n.SumOfTotal >= 0)
                        {
                            TextReplacer.SearchAndReplace(doc, "<TotalPagar1>", n.SumOfTotal.ToString("N2"), false);
                            TextReplacer.SearchAndReplace(doc, "<TotalPagar2>", " ", false);
                        }
                        else
                        {
                            TextReplacer.SearchAndReplace(doc, "<TotalPagar2>", (n.SumOfTotal * -1).ToString("N2"), false);
                            TextReplacer.SearchAndReplace(doc, "<TotalPagar1>", " ", false);
                        }

                    }

                    // ---------------------------------------------------------------------------------------------------------------
                    // imprimimos la linea 

                    string asignacion = rubro.Monto >= 0 ? rubro.Monto.ToString("N2") : " ";
                    string deduccion = rubro.Monto < 0 ? rubro.Monto.ToString("N2").Replace("-", "") : " "; 

                    TextReplacer.SearchAndReplace(doc, "<DescripcionRubro" + linea.ToString() + ">", rubro.Descripcion, false);
                    TextReplacer.SearchAndReplace(doc, "<Asignacion" + linea.ToString() + ">", asignacion, false);
                    TextReplacer.SearchAndReplace(doc, "<Deduccion" + linea.ToString() + ">", deduccion, false);

                    linea++;

                    if (linea == 17)
                    {
                        // debemos agregar una nueva página, pues la cantidad de rubros exede la cantidad de lineas en una página (16) 
                        linea = 1;
                        pagina++;

                        doc.Close();
                    }
                }

                for (int i = linea; i <= 16; i++)
                {
                    TextReplacer.SearchAndReplace(doc, "<DescripcionRubro" + i.ToString() + ">", " ", false);
                    TextReplacer.SearchAndReplace(doc, "<Asignacion" + i.ToString() + ">", " ", false);
                    TextReplacer.SearchAndReplace(doc, "<Deduccion" + i.ToString() + ">", " ", false);
                }

                if (doc != null)
                {
                    doc.Close();
                    doc.Dispose(); 
                }

                cantidadRegistrosLeidos++;
            }

            if (cantidadRegistrosLeidos == 0)
            {
                popupMessage = "No existe información para construir el archivo que Ud. ha requerido. " +
                    "<br /><br /> Probablemente Ud. no ha aplicado un filtro y seleccionado información aún.";

                context = null;
                return false;     
            }

            context = null;

            string newFileName = ""; 

            try
            {
                // combinamos todos los documentos individuales en uno solo, que contendrá todos 
                // los documentos word individuales ... 

                List<OpenXmlPowerTools.Source> sources = new List<OpenXmlPowerTools.Source>();

                filePaths = Directory.GetFiles(@path, "*_*_*_" + userName + ".docx");

                foreach (string filePath in filePaths)
                    sources.Add(new OpenXmlPowerTools.Source(new WmlDocument(filePath), true));


                newFileName = path + "RecibosPago_" + userName + ".docx";

                // intentamos eliminar antes el archivo ... 
                if (File.Exists(newFileName))
                    File.Delete(newFileName);

                DocumentBuilder.BuildDocument(sources, newFileName);

                // guardamos el nombre del documento en session, para que el usuario puede, luego, hacer un download ... 

                Session["FileToDownload"] = newFileName; 

                // convertimos a pdf los recibos de los empleados; nota: aquí usamos una versión "free" de Spyre.Doc for .Net; 
                // si el documento Word es muy grande, creo que no abrirá en forma correcta; sin embargo, cada recibo de pago 
                // debe tener un máximo de 2 páginas; sería ya una excepción que alguno tuviera 3 páginas ... 

                try
                {
                    foreach (string filePath in filePaths)
                    {
                        Document document = new Document();
                        document.LoadFromFile(filePath);
                        document.SaveToFile(filePath.Replace(".docx", ".pdf"), FileFormat.PDF);
                    }
                }
                catch(Exception ex)
                {
                    errorMessage = "Error: hemos obtenido un error al intentar convertir alguno de los recibos de pago de algún empleado, " +
                         "desde formato Word (.docx) a formato pdf.<br />" +
                         "Una razón probable es que el recibo de pago sea muy grande (más de tres páginas?). Por favor revise.<br /><br />" + 
                         "Sin embargo, los recibos normales, en formato Word, " +
                         "se han generado en forma normal y Ud. los puede obtener si hace un click en <em>Download</em>.<br /><br />" +
                         "El mensaje de error específico es: " + ex.Message; 

                    if (ex.InnerException != null)
                        errorMessage += "<br />" + ex.InnerException.Message;

                    popupMessage = errorMessage;

                    context = null;
                    return false;     
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += "<br />" + ex.InnerException.Message;

                popupMessage = errorMessage;

                context = null;
                return false;     
            }



            // ----------------------------------------------------------------------------------------------------------------------------

            popupMessage = "Ok, los recibos de pago han sido generados en forma satisfactoria.<br />" +
                "En total, este proceso ha construido <b>" + cantidadRegistrosLeidos.ToString() + " recibos de pago</b>.<br /><br />" +
                "Ud. debe hacer un click en el <em>link</em> a la izquierda, para obtener (<em>download</em>) el documento.";
            return true; 
        }


        protected void btnOk_Click(object sender, EventArgs e)
        {
        }

        protected void OpcionesMailMerge_DropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: ACTIVAR ESTE CÓDIGO PARA QUE AL CAMBIAR DE OPCIÓN EN EL COMBO SE ESCONDA EL UI QUE PERMITE OBTENER LOS RECIBOS, ETC. 
            //this.OpcionesFormato_RecibosPago_Div.Visible = false; 
        }

        private void EnviarEmailsAEmpleados() 
        {
            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }

            string errorMessage = "";

            if (!this.Email_EnviarUsuario_CheckBox.Checked && !Email_EnviarEmpleado_CheckBox.Checked)
            {
                errorMessage = "Por favor indique a quien deben ser enviados los correos: empleados, usuario.<br /><br /> " +
                        "Ud. debe marcar al menos una de las opciones anteriores.";

                this.ModalPopupTitle_span.InnerHtml = "Nómina - Obtención de recibos de pago a empleados";
                this.ModalPopupBody_span.InnerHtml = errorMessage;

                this.btnOk.Visible = false;
                this.btnCancel.Text = "Ok";

                this.ModalPopupExtender1.Show();

                return;
            }

            dbNominaEntities context = new dbNominaEntities();

            // leemos el header que corresponde a la nómina seleccionada 

            tNominaHeader header;

            header = context.tNominaHeaders.Include("tGruposEmpleado").Include("tGruposEmpleado.Compania").Where(h => h.ID == _headerID).FirstOrDefault();

            if (header == null)
            {
                errorMessage = "No existe información para construir el archivo que Ud. ha requerido. " +
                        "<br /><br /> Probablemente Ud. no ha aplicado un filtro y seleccionado información aún.";

                this.ModalPopupTitle_span.InnerHtml = "Nómina - Obtención de recibos de pago a empleados";
                this.ModalPopupBody_span.InnerHtml = errorMessage;

                this.btnOk.Visible = false;
                this.btnCancel.Text = "Ok";

                this.ModalPopupExtender1.Show();

                context = null;
                return;
            }

            string resultMessage = "";

            if (EnviarEmails(header, context, out resultMessage))
                this.ModalPopupTitle_span.InnerHtml = "Nómina - Obtención de recibos de pago a empleados";
            else
                this.ModalPopupTitle_span.InnerHtml = "Hemos obtenido un error al intentar obtener los recibos de pago";

            this.ModalPopupBody_span.InnerHtml = resultMessage;

            this.btnOk.Visible = false;
            this.btnCancel.Text = "Ok";

            this.ModalPopupExtender1.Show();

            context = null;

            return;    
        }

        private bool EnviarEmails(NominaASP.Models.tNominaHeader header, dbNominaEntities context, out string resultMessage)
        {
            // ----------------------------------------------------------------------------------------
            // la compañía debe tener la configuración necesaria para enviar correos ... 

            int ciaContabID = header.tGruposEmpleado.Compania.Numero;

            Compania ciaContab = context.Companias.Where(c => c.Numero == ciaContabID).FirstOrDefault();

            if (ciaContab == null ||
                string.IsNullOrEmpty(ciaContab.EmailServerName) ||
                string.IsNullOrEmpty(ciaContab.EmailServerCredentialsUserName) ||
                string.IsNullOrEmpty(ciaContab.EmailServerCredentialsPassword))
            {
                resultMessage = "Aparentemente, no se ha registrado la configuración necesaria para enviar correos, " +
                    "para la compañía (CiaContab) usada al seleccionar los registros.<br /><br />" +
                    "Por favor revise el registro para la compañía (CiaContab) en la tabla Compañías y defina la " +
                    "configuración necesaria para enviar e-mails.";

                return false;
            }

            string host = ciaContab.EmailServerName;
            int? port = ciaContab.EmailServerPort;
            bool enableSSL = ciaContab.EmailServerSSLFlag != null ? ciaContab.EmailServerSSLFlag.Value : false;
            string emailCredentialsUserName = ciaContab.EmailServerCredentialsUserName;
            string emailCredentialsUserPassword = ciaContab.EmailServerCredentialsPassword;

            // ahora que tenemos la configuración para enviar los correos, vamos a inicializar una clase que permite 
            // hacerlo ... 

            // obtenemos el email del usuario, pues será usado como 'from address' ... 

            MembershipUser member = Membership.GetUser(User.Identity.Name);
            string usuarioEmail = member.Email;

            if (string.IsNullOrEmpty(usuarioEmail))
            {
                resultMessage = "El usuario '" + User.Identity.Name + "' no tiene una dirección de correo registrada.<br />" +
                    "Por favor registre una dirección de correo para este usuario.";

                return false;
            }

            SendEmail sendMail = new SendEmail(host, port, enableSSL, emailCredentialsUserName, emailCredentialsUserPassword);
            // -----------------------------------------------------------------------------------------

            // finalmente, recorremos los archivos construídos y generamos y enviamos un correo para cada uno de ellos; 
            // nótese que cada archivo contiene en su nombre el id del proveedor y de la cia contab ... 

            // --------------------------------------------------------------------------------------------------------------
            // nótese que los documentos pdf fueron creados antes, cuando el usuario pidió los recibos de pago ... 
            // puede haber, aunque es muy improbable, 

            string userName = Membership.GetUser().UserName;
            string path = Server.MapPath("~/Temp/RecibosPago/" + header.tGruposEmpleado.Compania.Abreviatura + "/");

            string[] filePaths = Directory.GetFiles(@path, "*_*_*_" + userName + ".pdf");

            int cantidadEmpleados = 0;
            int cantidadCorreos = 0;
            int cantidadEmpleadosSinCorreoRegistrado = 0; 

            foreach (string filePath in filePaths)
            {
                // Directory.GetFiles regresa el nombre full (dir+name) del file; abajo quitamos el directorio y obtenemos solo el file name 

                string justFileName = Path.GetFileName(filePath);

                string[] words = justFileName.Split('_');

                int numeroEmpleado;

                try
                {
                    numeroEmpleado = Convert.ToInt32(words[2]);
                }
                catch (Exception ex)
                {
                    resultMessage = ex.Message;
                    if (ex.InnerException != null)
                        resultMessage += ex.InnerException.Message;

                    resultMessage = "Hemos obtenido un error al intentar tratar el nombre de archivo '" + filePath + "';<br />" +
                                  "probablemente, el nombre de archivo no está bien formado para ser tratado por el programa.<br />" +
                                  "El mensaje específico de error es:<br />" +
                                  resultMessage; 
                    return false;
                }

                // leemos el e-mail del empleado
                tEmpleado empleado = context.tEmpleados.Where(x => x.Empleado == numeroEmpleado).FirstOrDefault();

                if (empleado == null)
                    continue;

                cantidadEmpleados++;

                if (string.IsNullOrEmpty(empleado.email))
                {
                    cantidadEmpleadosSinCorreoRegistrado++;
                    continue;
                }
                    
                sendMail.FromAddress = usuarioEmail;

                if (this.Email_EnviarEmpleado_CheckBox.Checked)
                    sendMail.ToAddress = empleado.email;
                else
                    sendMail.ToAddress = usuarioEmail;

                if (this.Email_EnviarUsuario_CheckBox.Checked && Email_EnviarEmpleado_CheckBox.Checked)
                    sendMail.CCAddress = usuarioEmail;

                sendMail.Subject = "Recibo de pago correspondiente a la nómina de pago de fecha " + header.FechaNomina.ToString("dd-MMM-yyyy") + ".";

                StringBuilder mailBody = new StringBuilder();

                mailBody.Append("<b>" + "Estimado " + " " + empleado.Nombre + ",</b><br /><br /><br />");

                mailBody.Append("Por favor reciba, adjunto a este correo, el <b><em>recibo de pago de nómina</b></em> " + "<br />");
                mailBody.Append("que corresponde a la nómina de pago de fecha <b>" + header.FechaNomina.ToString("dd-MMM-yyyy") + "</b>." + "<br />" + "<br />");

                mailBody.Append("Gracias por su atención y saludos ... <br /><br /><br />");
                mailBody.Append(header.tGruposEmpleado.Compania.Nombre);

                sendMail.Body = mailBody.ToString();

                sendMail.AttachmentFileName = filePath;

                string errorMessage = ""; 
                if (!sendMail.Send(out errorMessage))
                {
                    resultMessage = errorMessage;
                    return false; 
                }

                cantidadCorreos++;
            }

            resultMessage = "Ok, los correos han sido enviados de forma satisfactoria:<br /><br />" +
                "En total, se han leído " + cantidadEmpleados.ToString() + " empleados para la nómina seleccionada; <br />" +
                "de éstos, " + cantidadEmpleadosSinCorreoRegistrado.ToString() + " no tienen una dirección de correo registrada.<br />" + 
                "Se han enviado " + cantidadCorreos.ToString() + " correos electrónicos a empledos y/o como copia al usuario de este proceso.";

            return true; 
        }

        protected void EmailFormat_LinkButton_Click(object sender, EventArgs e)
        {
              
        }

        private bool DownLoadFile(string fileName, out string errorMessage)
        {
            errorMessage = "";

            if (!File.Exists(fileName))
            {
                errorMessage = "Error: no hemos podido encontrar el documento requerido ('" + fileName + "') en un directorio en el servidor.<br />" +
                    "Aparentemente, no existe (en el servidor) el documento que se ha requerido .<br />" +
                    "Por favor agregue este documento y luego regrese a ejecutar este proceso.";

                return false;
            }

            FileStream liveStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            byte[] buffer = new byte[(int)liveStream.Length];
            liveStream.Read(buffer, 0, (int)liveStream.Length);
            liveStream.Close();

            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Length", buffer.Length.ToString());
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.BinaryWrite(buffer);
            Response.End();

            return true;
        }

        protected void DownLoadFile_ImageButton_Click(object sender, ImageClickEventArgs e)
        {
            string errorMessage = "";

            if (Session["FileToDownload"] == null)
            {
                errorMessage = "Error: aparentemente, Ud. no ha ejecutado el proceso que construye el documento o archivo.<br />" +
                    "Ud. debe ejecutar un proceso que construya un documento o archivo y luego obtener (download) el mismo usando este mecanismo.";

                this.ModalPopupTitle_span.InnerHtml = "Nómina - Obtención de recibos de pago a empleados";
                this.ModalPopupBody_span.InnerHtml = errorMessage;

                this.btnOk.Visible = false;
                this.btnCancel.Text = "Ok";

                this.ModalPopupExtender1.Show();

                return;
            }

            string file = Session["FileToDownload"].ToString();

            if (!DownLoadFile(file, out errorMessage))
            {
                errorMessage = "Error: hemos obtenido un error al intentar permitir que Ud. haga " +
                    "un 'download' del documento final de este proceso.<br />" +
                    "El mensaje específico del error es: " + errorMessage;

                this.ModalPopupTitle_span.InnerHtml = "Nómina - Obtención de recibos de pago a empleados";
                this.ModalPopupBody_span.InnerHtml = errorMessage;

                this.btnOk.Visible = false;
                this.btnCancel.Text = "Ok";

                this.ModalPopupExtender1.Show();

                return;
            }
        }

        protected void DownLoadFile2_LinkButton_Click1(object sender, EventArgs e)
        {
            string errorMessage = "";

            if (Session["FileToDownload"] == null)
            {
                errorMessage = "Error: aparentemente, Ud. no ha ejecutado el proceso que construye el documento o archivo.<br />" +
                    "Ud. debe ejecutar un proceso que construya un documento o archivo y luego obtener (download) el mismo usando este mecanismo.";

                this.ModalPopupTitle_span.InnerHtml = "Nómina - Obtención de recibos de pago a empleados";
                this.ModalPopupBody_span.InnerHtml = errorMessage;

                this.btnOk.Visible = false;
                this.btnCancel.Text = "Ok";

                this.ModalPopupExtender1.Show();

                return;
            }

            string file = Session["FileToDownload"].ToString();

            if (!DownLoadFile(file, out errorMessage))
            {
                errorMessage = "Error: hemos obtenido un error al intentar permitir que Ud. haga " +
                    "un 'download' del documento final de este proceso.<br />" +
                    "El mensaje específico del error es: " + errorMessage;

                this.ModalPopupTitle_span.InnerHtml = "Nómina - Obtención de recibos de pago a empleados";
                this.ModalPopupBody_span.InnerHtml = errorMessage;

                this.btnOk.Visible = false;
                this.btnCancel.Text = "Ok";

                this.ModalPopupExtender1.Show();

                return;
            }
        }

        protected void ObtenerEmails_ImageButton_Click(object sender, ImageClickEventArgs e)
        {
            EnviarEmailsAEmpleados(); 
        }

        protected void ObtenerEmails_LinkButton_Click(object sender, EventArgs e)
        {
            EnviarEmailsAEmpleados(); 
        }
    }
}