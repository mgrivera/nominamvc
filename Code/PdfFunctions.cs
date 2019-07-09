using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Bullzip.PdfWriter;

namespace NominaASP.Code
{
    public class PdfFunctions
    {
        string _fileName;  
        string _outputFileName; 

        public string ErrorMessage { get; set; }

        public PdfFunctions(string fileName, string outputFileName)
        {
            _fileName = fileName;
            _outputFileName = outputFileName; 
        }

        public bool ConvertToPdf()
        {
            try
            {
                if (File.Exists(_outputFileName))
                    File.Delete(_outputFileName);

                //
                // Set the PDF settings
                //

                PdfSettings pdfSettings = new PdfSettings();
                string printerName = "Bullzip PDF Printer";
                pdfSettings.PrinterName = printerName;
                pdfSettings.SetValue("Output", _outputFileName);
                pdfSettings.SetValue("ShowPDF", "no");
                pdfSettings.SetValue("ShowSettings", "never");
                pdfSettings.SetValue("ShowSaveAS", "never");
                pdfSettings.SetValue("ShowProgress", "no");
                pdfSettings.SetValue("ShowProgressFinished", "no");
                pdfSettings.SetValue("ConfirmOverwrite", "no");
                pdfSettings.WriteSettings(PdfSettingsFileType.RunOnce);

                PdfUtil.PrintFile(_fileName, printerName);
            }
            catch (Exception ex)
            {
                string errorMessage = "Error inesperado: hemos encontrado un error al intentar convertir el documento Word (docx) a formato pdf.<br /> " +
                        "El mensaje específico de error es: " + ex.Message;

                if (ex.InnerException != null) 
                    errorMessage += "<br /><br />" + ex.InnerException.Message;

                ErrorMessage = errorMessage; 
                return false; 
            }

            return true;
        }
    }
}