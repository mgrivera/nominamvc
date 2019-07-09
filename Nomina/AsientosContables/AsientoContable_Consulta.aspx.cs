using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Linq;
using NominaASP.Models.Contab;

namespace NominaASP.Nomina.AsientosContables
{
    public partial class AsientoContable_Consulta : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                HtmlGenericControl MyHtmlH2;

                MyHtmlH2 = (HtmlGenericControl)Master.FindControl("PageTitle_TableCell");
                if (!(MyHtmlH2 == null))
                {
                    MyHtmlH2.InnerHtml = "Consulta de Asientos Contables";
                }

                if (this.Request.QueryString["AsientoContableID"] == null || string.IsNullOrEmpty(this.Request.QueryString["AsientoContableID"].ToString()))
                {
                    string errorMessage = "No hemos podido obtener la clave del asiento contable que Ud. desea consultar en Contab.<br /><br />" + 
                        "Probablemente, Ud. no ha seleccionado una nómina en la lista, o la nómina seleccionada no tiene un asiento contable asociado.";

                    CustomValidator1.IsValid = false;
                    CustomValidator1.ErrorMessage = errorMessage;

                    return; 
                }

                int asientoContableID = Convert.ToInt32(Page.Request.QueryString["AsientoContableID"]); 

                AsientosContables_SqlDataSource.SelectParameters["NumeroAutomatico"].DefaultValue = Page.Request.QueryString["AsientoContableID"];
                Partidas_SqlDataSource.SelectParameters["NumeroAutomatico"].DefaultValue = Page.Request.QueryString["AsientoContableID"];

                this.Partidas_ListView.DataBind(); 

                dbContabEntities dbContext = new dbContabEntities();

                var nTotalDebe = (from sod in dbContext.dAsientos
                                  where sod.NumeroAutomatico == asientoContableID
                                  select (decimal?) sod.Debe).Sum();

                var nTotalHaber = (from soh in dbContext.dAsientos
                                   where soh.NumeroAutomatico == asientoContableID
                                   select (decimal?) soh.Haber).Sum();

                Label MySumOfDebe_Label = (Label)Partidas_ListView.FindControl("SumOfDebe_Label");
                Label MySumOfHaber_Label = (Label)Partidas_ListView.FindControl("SumOfHaber_Label");

                if (MySumOfDebe_Label != null)
                {
                    MySumOfDebe_Label.Text = "0,00";
                    MySumOfDebe_Label.Text = nTotalDebe != null ? nTotalDebe.Value.ToString("#,##0.00") : "0,00";
                }
                
                if (MySumOfHaber_Label != null)
                {
                    MySumOfHaber_Label.Text = "0,00";
                    MySumOfHaber_Label.Text = nTotalHaber != null ? nTotalHaber.Value.ToString("#,##0.00") : "0,00";
                }
                
                dbContext = null;


                // establecemos la propiedad del link que permite obtener el reporte para este asiento contable ... 
                string url = "";
                url = "../../ReportViewer.aspx?rpt=unasientocontable&NumeroAutomatico=" + Page.Request.QueryString["AsientoContableID"].ToString();

                ImprimirAsientoContable_HyperLink.HRef = "javascript:PopupWin('" + url + "', 1200, 680)"; 
            }
        }
    }
}
