using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Linq;
using System.Web.DynamicData;
using System.Web.UI.WebControls;

namespace ControlVentas
{
    public partial class Site2 : System.Web.UI.MasterPage
    {
        protected void MainMenu_MenuItemDataBound(object sender, MenuEventArgs e)
        {
            // para agregar 'target="_blank"' a alguno de los links en el menú ... 
            SiteMapNode node = (SiteMapNode)e.Item.DataItem;
            if (node["target"] != null)
                e.Item.Target = node["target"];

            if (!Page.IsPostBack)
            {
                // obteemos el nombre de la página, para asignar una dirección de la documentación al botón Help ... 
                string s = this.Page.GetType().FullName;
                string[] array = s.Split('_');
                int count = array.Count();
                string currentPage = array[count - 2];

                switch (currentPage)
                {
                    case "prestacionessociales": 
                    case "PrestacionesSociales":
                        this.Help_HyperLink.NavigateUrl = "https://nominadoc.wordpress.com/2014/04/22/prestaciones-socialesclculo";
                        break;
                }
            }
        }
    }
}
