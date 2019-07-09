using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NominaASP
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SiteMap.SiteMapResolve += new SiteMapResolveEventHandler(this.ExpandForumPaths);
        }

        private SiteMapNode ExpandForumPaths(Object sender, SiteMapResolveEventArgs e)
        {
            // The current node represents a Post page in a bulletin board forum. 
            // Clone the current node and all of its relevant parents. This 
            // returns a site map node that a developer can then 
            // walk, modifying each node.Url property in turn. 
            // Since the cloned nodes are separate from the underlying 
            // site navigation structure, the fixups that are made do not 
            // effect the overall site navigation structure.
            SiteMapNode currentNode = SiteMap.CurrentNode.Clone(true);
            SiteMapNode tempNode = currentNode;

            // cambiamos el Url en el link 'Generales / contab2'; nótese que esta es una forma de cambiar un url que es dinámico ...  
            //tempNode.ChildNodes[2].ChildNodes[1].ReadOnly = false;
            //tempNode.ChildNodes[2].ChildNodes[1].Url = System.Web.Configuration.WebConfigurationManager.AppSettings["contabM_address"].ToString(); 

            return currentNode;
        }

        
    }
}