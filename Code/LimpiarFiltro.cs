using System.Web.UI; 

namespace NominaASP.Code
{
    public class LimpiarFiltro
    {
        //  para recorrer los controles de una p�gina y limpiar el contenido que puedan tener sus controles
        private ControlCollection _controlCollection; 

        public LimpiarFiltro(ControlCollection MyControlCollection)
        {
            _controlCollection = MyControlCollection;
        }

        public void LimpiarControlesPagina()
        {
            RecorrerYLimpiarControles(_controlCollection);
        }

        private void RecorrerYLimpiarControles(ControlCollection myControlCollection)
        {
            foreach (System.Web.UI.Control MyWebServerControl in myControlCollection)
            {
                if (MyWebServerControl.HasControls() == false)
                {
                    switch (MyWebServerControl.GetType().Name.ToString())
                    {
                        case "TextBox":
                            System.Web.UI.WebControls.TextBox MyWebControlTextBox =
                                (System.Web.UI.WebControls.TextBox)MyWebServerControl;
                            MyWebControlTextBox.Text = "";
                            break;
                        case "CheckBox":
                            System.Web.UI.WebControls.CheckBox MyWebControlCheckbox =
                                (System.Web.UI.WebControls.CheckBox)MyWebServerControl;
                            MyWebControlCheckbox.Checked = false;
                            break;
                        case "ListBox":
                            System.Web.UI.WebControls.ListBox MyWebControlListBox =
                                (System.Web.UI.WebControls.ListBox)MyWebServerControl;
                            MyWebControlListBox.SelectedIndex = -1;
                            break;
                        case "DropDownList":
                            System.Web.UI.WebControls.DropDownList MyWebControlDropDownList =
                                (System.Web.UI.WebControls.DropDownList)MyWebServerControl;
                            MyWebControlDropDownList.SelectedIndex = -1;
                            break;
                    }
                }
                else
                {
                    //  en el control collection vienen Panels y web controls que 
                    //  a su vez, contienen controls. 
                    RecorrerYLimpiarControles(MyWebServerControl.Controls);
                }
            }
        }
    }
}