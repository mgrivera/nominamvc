using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
//using System.Data.Objects;
using System.Data.Entity.Core.Objects; 
using NominaASP.Models.Nomina;
//using NominaASP.Models;
using NominaASP.Models.Users;

namespace NominaASP.Generales.SeleccionarCiaContab
{
    public partial class SeleccionarCiaContab : System.Web.UI.Page
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
                var currentUser = Membership.GetUser();
                // el usuario en la tabla de compañías y usuarios es de tipo uniqueIdentifier (Guid) 
                Guid userID = new Guid(currentUser.ProviderUserKey.ToString()); 

                dbContabUserEntities userContext = new dbContabUserEntities(); 

                // nótese como determinamos las compañías que se han asignado al usuario. De no haber ninguna, asumimos todas 
                List<CompaniasYUsuario> companiasAsignadas = userContext.CompaniasYUsuarios.Where(c => c.Usuario == userID).
                                                                                            ToList<CompaniasYUsuario>();

                if (companiasAsignadas.Count() == 0)
                {
                    // si el usuario no tiene compañías asignadas, asumimos todas 
                    List<Compania> companias = userContext.Companias.OrderBy(c => c.Nombre).ToList();
                    this.GridView1.DataSource = companias;
                    this.GridView1.DataBind(); 
                }
                else
                {
                    List<Compania> companias = userContext.Companias.OrderBy(c => c.Nombre).ToList();
                    // seleccionamos solo las compañías que existen en la lista anterior (companiasAsignadas) 
                    var companias2 = companias.Where(c => companiasAsignadas.Any(x => x.Compania == c.Numero)).ToList<Compania>();
                    this.GridView1.DataSource = companias2; 

                    this.GridView1.DataBind(); 
                }
            }
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // intentamos seleccionar la compañia que el usuario selecciona en la lista ... 

            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }

            string usuario = User.Identity.Name;
            int? pk = Convert.ToInt32(GridView1.SelectedDataKey.Value.ToString());

            if (pk != null)
            {
                dbContabUserEntities userContext = new dbContabUserEntities();

                var query = userContext.tCiaSeleccionada2.Where(s => s.UsuarioLS == usuario);

                foreach (tCiaSeleccionada2 s in query)
                    userContext.tCiaSeleccionada2.Remove(s);

                Compania compania = userContext.Companias.Where(c => c.Numero == pk.Value).FirstOrDefault(); 

                if (compania == null) 
                {
                    string errorMessage = "Error inesperado: no hemos podido encontrar la compañía en la tabla Compañías.";

                    CustomValidator1.IsValid = false;
                    CustomValidator1.ErrorMessage = errorMessage;

                    return;
                }

                tCiaSeleccionada2 ciaSeleccionada = new tCiaSeleccionada2()
                {
                    CiaSeleccionada = compania.Numero,
                    Nombre = compania.Nombre,
                    NombreCorto = compania.NombreCorto,
                    UsuarioLS = usuario,
                    Usuario = 0
                };

                userContext.tCiaSeleccionada2.Add(ciaSeleccionada);

                try
                {
                    userContext.SaveChanges();

                    //string errorMessage = "Ok, la compañía (Contab) " + compania.Nombre + " ha sido seleccionada.";

                    //CustomValidator1.IsValid = false;
                    //CustomValidator1.ErrorMessage = errorMessage;

                    this.ModalPopupTitle_span.InnerHtml = "Nómina";
                    this.ModalPopupBody_span.InnerHtml = "Ok, la compañía <em><b>(Contab) " + compania.Nombre + "</b></em> ha sido seleccionada.";

                    // usamos btnOk solo si queremos un postback (a btnOk.Click); por ejemplo, si mostramos un mensaje al usuario pero él puede 
                    // decidir continuar (o cancelar); si continúa, hacemos el postback y continuamos en el servidor. 
                    // En este caso, solo queremos que el usuario lea el mensaje y cierre el diálogo. Por eso, no mostramos btnOk y usamos Cancel como Ok ... 

                    this.btnOk.Visible = false;
                    this.btnCancel.Text = "Ok";

                    this.ModalPopupExtender1.Show();
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;

                    if (ex.InnerException != null)
                        errorMessage += "<br /><br />" + ex.InnerException.Message;

                    CustomValidator1.IsValid = false;
                    CustomValidator1.ErrorMessage = errorMessage;

                    return;
                }
                finally
                {
                    userContext = null; 
                }
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView gv = (GridView)sender;
            gv.SelectedIndex = -1;
        }

        protected void btnOk_Click(object sender, EventArgs e)
        {
        }
    }
}