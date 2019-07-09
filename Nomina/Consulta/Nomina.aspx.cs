using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
//using NominaASP.Models.Nomina;
using NominaASP.Models;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;

namespace NominaASP.Nomina.Consulta
{
    public partial class Nomina : System.Web.UI.Page
    {
        int _ciaSeleccionada; 

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                return;
            }

            if (!Page.IsPostBack)
            {
                dbNominaEntities context = new dbNominaEntities();
                string usuario = User.Identity.Name;

                Compania companiaSeleccionada = context.Companias.Where(c => c.tCiaSeleccionadas.Any(t => t.UsuarioLS == usuario)).FirstOrDefault(); 

                if (companiaSeleccionada == null)
                {
                    string errorMessage = "No hay una Cia Contab seleccionada; debe seleccionar una Cia Contab ...";

                    //CustomValidator1.IsValid = false;
                    //CustomValidator1.ErrorMessage = errorMessage;

                    this.CiaContabSeleccionada_span.InnerHtml = errorMessage; 

                    return;
                }

                this.CiaContabSeleccionada_span.InnerHtml = companiaSeleccionada.Nombre; 

                // para guardar la cia contab seleccionada 

                Session["CiaContabSeleccionada"] = companiaSeleccionada.Numero;
            }

            if (Session["CiaContabSeleccionada"] == null)
            {
                string errorMessage = "No hay una Cia Contab seleccionada; debe seleccionar una Cia Contab ...";
                this.CiaContabSeleccionada_span.InnerHtml = errorMessage;

                return;
            }

            _ciaSeleccionada = (int)Session["CiaContabSeleccionada"]; 
        }

        protected void btnOk_Click(object sender, EventArgs e)
        {
        }

        public string DescripcionTipoNomina(string tipo)
        {
            switch (tipo)
            {
                case "M":
                    return "Mensual";
                case "N":
                    return "Normal";
                case "Q":
                    return "Quincenal";
                case "V":
                    return "Vacaciones";
                case "E":
                    return "Especial";
                case "U":
                    return "Utilidades";
            }
            return "Indefinida (" + tipo + ")";
        }

        protected void RubrosNomina_ListView_PagePropertiesChanged(object sender, EventArgs e)
        {
            // cuando el usuario cambia de página, intentamos quitar la selección a algún item que lo tenga 
            ListView lv = sender as ListView;
            lv.SelectedIndex = -1;
        }

        protected void RubrosNomina_ListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // The return type can be changed to IEnumerable, however to support
        // paging and sorting, the following parameters must be added:
        //     int maximumRows
        //     int startRowIndex
        //     out int totalRowCount
        //     string sortByExpression
        public IQueryable<NominaASP.Models.tNomina> RubrosNomina_ListView_GetData()
        {
            // regresamos null cuando se abre la página, para esperar que el usuario agregue un filtro ... 

            if (!this.Page.IsPostBack)
                return null; 

            dbNominaEntities context = new dbNominaEntities();

            StringBuilder sqlFilter;
            string filter = ConstruirFiltro(out sqlFilter);


            // nótese como obtenemos el objectcontext que corresponde al dbcontext ... 
            var objectContext = (context as IObjectContextAdapter).ObjectContext;


            ObjectQuery<tNomina> query = new ObjectQuery<tNomina>("Select value nomina From dbNominaEntities.tNominas as nomina", objectContext, MergeOption.NoTracking);
            query = query.Include("tEmpleado");
            query = query.Include("tNominaHeader.tGruposEmpleado");
            query = query.Include("tMaestraRubro");
            query = query.Where(filter); 
            query = query.OrderBy("it.tNominaHeader.FechaNomina desc");
            query = query.OrderBy("it.tEmpleado.Alias asc");
            query = query.OrderBy("it.Monto desc");                     // TODO: debemos ordenar por el valor ABSOLUTO; cómo??? 


            // TODO: este es el query que debemos duplicar con ObjectQuery ... 
            //var query = context.tNominas.Include("tEmpleado").
            //                             Include("tNominaHeader").
            //                             Include("tNominaHeader.tGruposEmpleado").
            //                             Include("tMaestraRubro").
            //                             Where(filter).
            //                             Select(n => n);

            //query = query.OrderByDescending(n => n.tNominaHeader.FechaNomina).
            //              ThenBy(n => n.tEmpleado.Alias).
            //              ThenByDescending(n => Math.Abs(n.Monto)); 

            return query;
        }

        protected void RubrosNomina_ListView_LayoutCreated(object sender, EventArgs e)
        {
            
        }

        protected void RubrosNomina_ListView_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                Label montoNominaLabel = (Label)e.Item.FindControl("MontoNominaLabel");
                if (montoNominaLabel != null)
                {
                    decimal montoNomina = 0;

                    if (!decimal.TryParse(montoNominaLabel.Text, out montoNomina))
                        return;

                    if (montoNomina >= 0)
                        montoNominaLabel.ForeColor = System.Drawing.Color.Blue;
                    else
                    {
                        montoNominaLabel.ForeColor = System.Drawing.Color.Red;
                        montoNominaLabel.Text = Math.Abs(montoNomina).ToString("N2");
                    }
                }
            }
        }

        protected void Filter_Ok_Button_Click(object sender, EventArgs e)
        {
            // para mostrar siempre la 1ra. página, cada vez que el usuario aplica un filtro 
            DataPager pgr = RubrosNomina_ListView.FindControl("ListView_DataPager") as DataPager;
            if (pgr != null && RubrosNomina_ListView.Items.Count != pgr.TotalRowCount)
            {
                pgr.SetPageProperties(0, pgr.MaximumRows, false);
            }

            this.RubrosNomina_ListView.DataBind();

            this.RubrosNomina_ListView.SelectedIndex = -1;

            this.TabContainer1.ActiveTabIndex = 1;

            // ------------------------------------------------------------------------------------------------------------------------------------
            // aprovechamos para establecer los links que permiten obtener el reporte; nótese que lo hacemos cada vez que cambia el filtro ... 

            StringBuilder sqlFilter;
            string filter = ConstruirFiltro(out sqlFilter);
            Session["FiltroConsultaNomina"] = filter; 

            HtmlAnchor link = this.Report_HtmlAnchor as HtmlAnchor;
            link.HRef = @"javascript:PopupWin('" + "OpcionesReporte.aspx', 1000, 680)";

            link = this.Report2_HtmlAnchor as HtmlAnchor;
            link.HRef = @"javascript:PopupWin('" + "OpcionesReporte.aspx', 1000, 680)";
            // ------------------------------------------------------------------------------------------------------------------------------------
        }

        public IQueryable<NominaASP.Models.tEmpleado> Empleados_DropDownList_SelectMethod()
        {
            int ciaContabSeleccionada = -999;

            if (Session["CiaContabSeleccionada"] != null)
                ciaContabSeleccionada = Convert.ToInt32(Session["CiaContabSeleccionada"]);

            dbNominaEntities context = new dbNominaEntities();

            var query = context.tEmpleados.Where(e => e.Cia == ciaContabSeleccionada);

            query = query.OrderBy(e => e.Nombre);

            return query;
        }

        public IQueryable<NominaASP.Models.tCargo> Cargos_DropDownList_SelectMethod()
        {
            dbNominaEntities context = new dbNominaEntities();

            IOrderedQueryable<tCargo> query = context.tCargos;
            query = query.OrderBy(e => e.Descripcion);

            return query;
        }

        public IQueryable<NominaASP.Models.tDepartamento> Departamentos_DropDownList_SelectMethod()
        {
            dbNominaEntities context = new dbNominaEntities();

            IOrderedQueryable<tDepartamento> query = context.tDepartamentos;
            query = query.OrderBy(e => e.Descripcion);

            return query;
        }

        public IQueryable<GrupoNomina> tGruposEmpleado_DropDownList_SelectMethod()
        {
            int ciaContabSeleccionada = -999;

            if (Session["CiaContabSeleccionada"] != null)
                ciaContabSeleccionada = Convert.ToInt32(Session["CiaContabSeleccionada"]);

            dbNominaEntities context = new dbNominaEntities();

            var query = context.tGruposEmpleados.Where(e => e.Cia == ciaContabSeleccionada).
                                                 Select(g => new GrupoNomina { ID = g.Grupo, Descripcion = g.Descripcion + " (" + g.NombreGrupo + ")" } );

            query = query.OrderBy(e => e.Descripcion);

            return query;
        }

        public IQueryable<NominaASP.Models.tMaestraRubro> Rubros_DropDownList_SelectMethod()
        {
            dbNominaEntities context = new dbNominaEntities();

            IOrderedQueryable<tMaestraRubro> query = context.tMaestraRubros;
            query = query.OrderBy(e => e.NombreCortoRubro);

            return query;
        }

        protected void RubrosNomina_ListView_PreRender(object sender, EventArgs e)
        {
            if (!this.Page.IsPostBack)
                return;

            ListView lv = sender as ListView;

            if (Session["CiaContabSeleccionada"] == null)
                return;

            int ciaSeleccionada = (int)Session["CiaContabSeleccionada"];

            dbNominaEntities context = new dbNominaEntities();

            // obtenemos el ObjectContext, en base al context ... 
            var objectContext = (context as IObjectContextAdapter).ObjectContext;

            StringBuilder sqlFilter;
            string filter = ConstruirFiltro(out sqlFilter);

            string sqlCommand = "Select Count(*) As CantidadRubros, " +
                                 "SUM(Case When Monto >= 0 Then Monto Else 0 End) As TotalAsignaciones, " +
                                 "SUM(Case When Monto < 0 Then Monto Else 0 End) As TotalDeducciones, " +
                                 "Sum(Monto) As Saldo " +
                                 "From tNomina Inner Join tEmpleados On tNomina.Empleado = tEmpleados.Empleado " +
                                 "Inner Join tNominaHeaders On tNomina.HeaderID = tNominaHeaders.ID " +
                                 "Inner Join tGruposEmpleados On tNominaHeaders.GrupoNomina = tGruposEmpleados.Grupo " +
                                 "Where " + sqlFilter;

            
            TotalesConsultaNomina totalesNomina = objectContext.ExecuteStoreQuery<TotalesConsultaNomina>(sqlCommand).FirstOrDefault(); 


            //var query = context.tNominas.Include("tEmpleado").
            //                             Include("tNominaHeader").
            //                             Include("tNominaHeader.tGruposEmpleado").
            //                             Include("tMaestraRubro").
            //                             Where(filter).
            //                             Select(n => n);

            //var query2 = query.GroupBy(g => g.tNominaHeader.tGruposEmpleado.Cia).
            //             Select(g => new
            //             {
            //                 Asignaciones = g.Sum(x => x.Monto >= 0 ? x.Monto : 0),
            //                 Deducciones = g.Sum(x => x.Monto < 0 ? x.Monto : 0),
            //                 Saldo = g.Sum(x => x.Monto),
            //                 CantRecs = g.Count()
            //             });

            

            decimal asignaciones = 0;
            decimal deducciones = 0;
            decimal saldo = 0;

            if (totalesNomina.CantidadRubros != null && totalesNomina.CantidadRubros > 0)
            {
                asignaciones = totalesNomina.TotalAsignaciones != null ? totalesNomina.TotalAsignaciones.Value : 0;
                deducciones = totalesNomina.TotalDeducciones != null ? totalesNomina.TotalDeducciones.Value : 0;
                saldo = totalesNomina.Saldo != null ? totalesNomina.Saldo.Value : 0; 
            }

            Label lblSum = (Label)lv.FindControl("Asignaciones_Label");

            if (lblSum != null)
            {
                lblSum.Text = asignaciones.ToString("N2");

                if (asignaciones >= 0)
                    lblSum.ForeColor = System.Drawing.Color.Blue;
                else
                    lblSum.ForeColor = System.Drawing.Color.Red;
            }

            lblSum = (Label)lv.FindControl("Deducciones_Label");
            if (lblSum != null)
            {
                lblSum.Text = Math.Abs(deducciones).ToString("N2");

                if (deducciones >= 0)
                    lblSum.ForeColor = System.Drawing.Color.Blue;
                else
                    lblSum.ForeColor = System.Drawing.Color.Red;
            }

            lblSum = (Label)lv.FindControl("Saldo_Label");
            if (lblSum != null)
            {
                lblSum.Text = Math.Abs(saldo).ToString("N2");

                if (saldo >= 0)
                    lblSum.ForeColor = System.Drawing.Color.Blue;
                else
                    lblSum.ForeColor = System.Drawing.Color.Red;
            }

            lblSum = (Label)lv.FindControl("CantRecs_Label");
            if (lblSum != null)
            {
                lblSum.Text = totalesNomina.CantidadRubros != null ? totalesNomina.CantidadRubros.Value.ToString() : "0,00";
            }
        }

        private string ConstruirFiltro(out StringBuilder filtroSql)
        {
            // construimos, además, un filtro adecuado para un Select del tipo sql, pues usamos un ExecuteStoreQuery y debemos usar 
            // transact-sql allí ... 

            StringBuilder sb = new StringBuilder();
            filtroSql = new StringBuilder(); 

            sb.Append("(it.tNominaHeader.tGruposEmpleado.Cia == " + _ciaSeleccionada.ToString() + ")");
            filtroSql.Append("(tGruposEmpleados.Cia = " + _ciaSeleccionada.ToString() + ")"); 


            if (!string.IsNullOrEmpty(this.FechaNominaDesde_TextBox.Text))
            {
                if (!string.IsNullOrEmpty(this.FechaNominaHasta_TextBox.Text))
                {
                    // si vienen las 2 fechas, intentamos buscar por rango 
                    DateTime desde;
                    DateTime hasta;
                    if (DateTime.TryParse(this.FechaNominaDesde_TextBox.Text, out desde))
                        if (DateTime.TryParse(this.FechaNominaHasta_TextBox.Text, out hasta))
                        {
                            sb.Append(" And (it.tNominaHeader.FechaNomina >=  DateTime'" + desde.ToString("yyyy-MM-dd H:m:s") + "' And it.tNominaHeader.FechaNomina <=  DateTime'" + hasta.ToString("yyyy-MM-dd 23:59:59") + "')");
                            filtroSql.Append(" And (tNominaHeaders.FechaNomina >= '" + desde.ToString("yyyy-MM-dd H:m:s") + "' And tNominaHeaders.FechaNomina <= '" + hasta.ToString("yyyy-MM-dd 23:59:59") + "')");
                        }
                }
                else
                {
                    // si viene solo desde, buscamos justo para esa fecha 
                    DateTime desde;
                    if (DateTime.TryParse(this.FechaNominaDesde_TextBox.Text, out desde))
                    {
                        sb.Append(" And (it.tNominaHeader.FechaNomina ==  DateTime'" + desde.ToString("yyyy-MM-dd H:m:s") + "')");
                        filtroSql.Append(" And (tNominaHeaders.FechaNomina = '" + desde.ToString("yyyy-MM-dd H:m:s") + "')");
                    }
                        
                }
            }

            if (this.GruposNomina_DropDownList.SelectedValue != "-999")
            {
                int selectedValue = Convert.ToInt32(GruposNomina_DropDownList.SelectedValue);
                sb.Append(" And (it.tNominaHeader.GrupoNomina == " + selectedValue.ToString() + ")");
                filtroSql.Append(" And (tNominaHeaders.GrupoNomina = " + selectedValue.ToString() + ")");
            }


            if (this.TipoNomina_DropDownList.SelectedValue != "-999")
            {
                sb.Append(" And (it.tNominaHeader.Tipo == '" + TipoNomina_DropDownList.SelectedValue + "')");
                filtroSql.Append(" And (tNominaHeaders.Tipo = '" + TipoNomina_DropDownList.SelectedValue + "')");
            }


            if (this.Rubros_DropDownList.SelectedValue != "-999")
            {
                int selectedValue = Convert.ToInt32(Rubros_DropDownList.SelectedValue);
                sb.Append(" And (it.Rubro == " + selectedValue.ToString() + ")");
                filtroSql.Append(" And (tNomina.Rubro = " + selectedValue.ToString() + ")");
            }


            if (this.TiposRubro_DropDownList.SelectedValue != "-999")
            {
                sb.Append(" And (it.Tipo == '" + this.TiposRubro_DropDownList.SelectedValue + "')");
                filtroSql.Append(" And (tNomina.Tipo = '" + this.TiposRubro_DropDownList.SelectedValue + "')");
            }
                

            if (this.Empleados_DropDownList.SelectedValue != "-999")
            {
                int selectedValue = Convert.ToInt32(Empleados_DropDownList.SelectedValue);
                sb.Append(" And (it.Empleado == " + selectedValue.ToString() + ")");
                filtroSql.Append(" And (tNomina.Empleado = " + selectedValue.ToString() + ")");
            }

            if (!string.IsNullOrEmpty(this.DescripcionRubro_TextBox.Text))
            {
                sb.Append(" And (it.Descripcion Like '%" + this.DescripcionRubro_TextBox.Text + "%')");
                filtroSql.Append(" And (tNomina.Descripcion Like '%" + this.DescripcionRubro_TextBox.Text + "%')");
            }
                

            if (this.Sueldo_CheckBox.Checked)
            {
                sb.Append(" And (it.SueldoFlag)");
                filtroSql.Append(" And (tNomina.SueldoFlag = 1)");
            }


            if (this.Salario_CheckBox.Checked)
            {
                sb.Append(" And (it.SalarioFlag)");
                filtroSql.Append(" And (tNomina.SalarioFlag = 1)");
            }
                

            if (this.Departamentos_DropDownList.SelectedValue != "-999")
            {
                int selectedValue = Convert.ToInt32(Departamentos_DropDownList.SelectedValue);
                sb.Append(" And (it.tEmpleado.Departamento == " + selectedValue.ToString() + ")");
                filtroSql.Append(" And (tEmpleados.Departamento = " + selectedValue.ToString() + ")");
            }

            if (this.Cargos_DropDownList.SelectedValue != "-999")
            {
                sb.Append(" And (it.tEmpleado.Cargo == " + Cargos_DropDownList.SelectedValue + ")");
                filtroSql.Append(" And (tEmpleados.Cargo = " + Cargos_DropDownList.SelectedValue + ")");
            }

            if (this.SituacionActual_DropDownList.SelectedValue != "-999")
            {
                sb.Append(" And (it.tEmpleado.SituacionActual == '" + SituacionActual_DropDownList.SelectedValue + "')");
                filtroSql.Append(" And (tEmpleados.SituacionActual = '" + SituacionActual_DropDownList.SelectedValue + "')");
            }
                

            if (this.Estados_DropDownList.SelectedValue != "-999")
            {
                sb.Append(" And (it.tEmpleado.Status == '" + Estados_DropDownList.SelectedValue + "')");
                filtroSql.Append(" And (tEmpleados.Status = '" + Estados_DropDownList.SelectedValue + "')");
            }
                

            return sb.ToString(); 
        }
    }

    public class GrupoNomina
    {
        public int ID { get; set; }
        public string Descripcion { get; set; }
    }

    public class TotalesConsultaNomina
    {
        public int? CantidadRubros { get; set; }
        public decimal? TotalAsignaciones { get; set; }
        public decimal? TotalDeducciones { get; set; }
        public decimal? Saldo { get; set; }
    }
}