using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NominaASP.Code;

namespace NominaASP
{
    public partial class MostrarPdfWordFile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                // esta página recibe un tipo de formato y un nombre de archivo; la página intenta regresar al usuario 
                // con el archivo mostrado en su formato (pdf / word) ... 

                string format = "";
                string fileName = "";

                if (this.Request.QueryString["format"] == null || this.Request.QueryString["fileName"] == null)
                {
                    string errorMessage = "Error: esta página no ha recibido los parámetros adecuados (format y fileName).";

                    CustomValidator1.IsValid = false;
                    CustomValidator1.ErrorMessage = errorMessage;

                    return;
                }

                format = this.Request.QueryString["format"].ToString();
                fileName = this.Request.QueryString["fileName"].ToString(); 

                switch (format)
                {
                    case "pdf":
                        {
                            // siempre recibimos un documento word (2007 o más); cuando el usuario quiere ver pdf, intentamos convertirlo aquí ... 
                            string errorMessage = ""; 
                            if (!ConvertirDocxAPdf(fileName, out errorMessage))
                            {
                                CustomValidator1.IsValid = false;
                                CustomValidator1.ErrorMessage = errorMessage;
                            }

                            Response.ContentType = "application/pdf";
                            break;
                        }
                    case "word":
                        {
                            Response.ContentType = "Application/msword";
                            break;
                        }
                }

                
                Response.WriteFile(fileName);
                Response.End();
            }
        }

        private bool ConvertirDocxAPdf(string file, out string errorMessage)
        {
            errorMessage = "";

            PdfFunctions pdfFunctions = new PdfFunctions(file, file.Replace("docx", "pdf"));

            if (!pdfFunctions.ConvertToPdf())
            {
                errorMessage = pdfFunctions.ErrorMessage;
                pdfFunctions = null; 
                return false;
            }

            pdfFunctions = null; 

            return true; 
        }
    }
}