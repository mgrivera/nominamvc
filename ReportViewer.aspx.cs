using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using NominaASP.Code;
using NominaASP.Empleados.Consulta;
using NominaASP.Empleados.Faltas;
using NominaASP.Empleados.Vacaciones;
//using NominaASP.Models.Nomina;
using NominaASP.Models;
using NominaASP.Nomina.AsientosContables;
using NominaASP.Nomina.Nomina;
using NominaASP.Nomina.PrestacionesSociales;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;

namespace NominaASP
{
    public partial class ReportViewer : System.Web.UI.Page
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

                switch (Request.QueryString["rpt"].ToString())
                {
                    case "empleados":
                        {
                            // los criterios usados para construir un filtro vienen en un objeto en Session ... 

                            if (Session["ConsultaEmpleados_CriteriosFiltro"] == null)
                            {
                                ErrMessage_Cell.InnerHtml = "Esta consulta no ha recibido los parámetros esperados." +
                                    "<br /><br />Ha habido un error al intentar obtener esta consulta. Por favor intente nuevamente.";
                                break;
                            }

                            ConsultaEmpleados_CriteriosFiltro criterioFiltro = Session["ConsultaEmpleados_CriteriosFiltro"] as ConsultaEmpleados_CriteriosFiltro;

                            if (criterioFiltro.CiaContab == null)
                            {
                                ErrMessage_Cell.InnerHtml = "Esta consulta no ha recibido los parámetros esperados." +
                                    "<br /><br />Ha habido un error al intentar obtener esta consulta. Por favor intente nuevamente.";
                                break;
                            }

                            dbNominaEntities context = new dbNominaEntities();

                            //var query = context.tEmpleados.Include("tDepartamento").Include("tCargo").Include("Compania").Where(e => e.Cia == ciaContabSeleccionada);

                            IQueryable <tEmpleado> query = context.tEmpleados;
                            query = query.Where(x => x.Cia == criterioFiltro.CiaContab);

                            if (query.Count() == 0)
                            {
                                ErrMessage_Cell.InnerHtml = "No existe información para mostrar el reporte " +
                                    "que Ud. ha requerido. <br /><br /> Probablemente Ud. no ha aplicado un " +
                                    "filtro y seleccionado información aún.";
                                return;
                            }

                            if (criterioFiltro.Empleado != null)
                                query = query.Where(x => x.Empleado == criterioFiltro.Empleado);

                            if (criterioFiltro.Departamento != null)
                                query = query.Where(x => x.Departamento == criterioFiltro.Departamento);

                            if (criterioFiltro.Cargo != null)
                                query = query.Where(x => x.Cargo == criterioFiltro.Cargo);

                            if (!string.IsNullOrEmpty(criterioFiltro.Status))
                                query = query.Where(x => x.Status == criterioFiltro.Status);

                            if (!string.IsNullOrEmpty(criterioFiltro.SituacionActual))
                                query = query.Where(x => x.SituacionActual == criterioFiltro.SituacionActual);


                            if (query.Count() == 0)
                            {
                                ErrMessage_Cell.InnerHtml = "No existe información para mostrar el reporte " +
                                    "que Ud. ha requerido. <br /><br /> Probablemente Ud. no ha aplicado un " +
                                    "filtro y seleccionado información aún.";
                                return;
                            }

                            // ahora preparamos una lista para usarla como DataSource del report ... 

                            List<Nomina_Report_ConsultaEmpleados> myList = new List<Nomina_Report_ConsultaEmpleados>();
                            Nomina_Report_ConsultaEmpleados infoEmpleado;

                            foreach (tEmpleado empleado in query)
                            {
                                infoEmpleado = new Nomina_Report_ConsultaEmpleados();

                                infoEmpleado.CiaContab = empleado.Compania.Nombre;
                                infoEmpleado.Departamento = empleado.tDepartamento.Descripcion;
                                infoEmpleado.Nombre = empleado.Nombre;
                                infoEmpleado.Cedula = empleado.Cedula;
                                infoEmpleado.Cargo = empleado.tCargo.Descripcion;
                                infoEmpleado.FechaNacimiento = empleado.FechaNacimiento;
                                infoEmpleado.FechaIngreso = empleado.FechaIngreso;
                                infoEmpleado.SituacionActual = GetStatusEmpleado(empleado.Status) + "-" + GetSituacionActualEmpleado(empleado.SituacionActual);
                                infoEmpleado.SueldoBasico = GetSueldoEmpleado(empleado.Empleado);
                                infoEmpleado.MontoCestaTickets = empleado.MontoCestaTickets; 
                                infoEmpleado.FechaRetiro = empleado.FechaRetiro;

                                myList.Add(infoEmpleado);
                            }


                            ReportViewer1.LocalReport.ReportPath = "Empleados/Consulta/ConsultaEmpleados.rdlc";

                            ReportDataSource myReportDataSource = new ReportDataSource();

                            myReportDataSource.Name = "DataSet2";
                            myReportDataSource.Value = myList;

                            ReportViewer1.LocalReport.DataSources.Add(myReportDataSource);

                            ReportViewer1.LocalReport.Refresh();

                            //ReportParameter NumeroReposicion_ReportParameter = new ReportParameter("NumeroReposicion", nReposicion.ToString());
                            //ReportParameter Fecha_ReportParameter = new ReportParameter("Fecha", dFecha.ToString());

                            //ReportParameter[] MyReportParameters = { NumeroReposicion_ReportParameter, Fecha_ReportParameter, };

                            //ReportViewer1.LocalReport.SetParameters(MyReportParameters);

                            break;
                        }

                    case "consultaNomina":
                        {
                            // los criterios usados para construir un filtro vienen en un objeto en Session ... 
                            // TODO: corregir y cambiar por parameter (querystring) cuando tengamos una respuesta a este issue ... 

                            if (Request.QueryString["agrupar"] == null && Request.QueryString["opcion"] == null)
                            {
                                ErrMessage_Cell.InnerHtml = "Esta consulta no ha recibido los parámetros esperados." +
                                    "<br /><br />Ha habido un error al intentar obtener esta consulta. Por favor intente nuevamente.";
                                break;
                            }

                            if (Session["FiltroConsultaNomina"] == null)
                            {
                                ErrMessage_Cell.InnerHtml = "Esta consulta no ha recibido los parámetros esperados." +
                                    "<br /><br />Ha habido un error al intentar obtener esta consulta. Por favor intente nuevamente.";
                                break;
                            }

                            string filter = Session["FiltroConsultaNomina"].ToString();
                            string agruparPor = Request.QueryString["agrupar"].ToString();
                            string opcion = Request.QueryString["opcion"].ToString();       // puede venir de la nómina o de la consulta de nómina 

                            dbNominaEntities context = new dbNominaEntities();

                            // nótese como obtenemos el objectcontext que corresponde al dbcontext ... 
                            var objectContext = (context as IObjectContextAdapter).ObjectContext;

                            ObjectQuery<tNomina> query = new ObjectQuery<tNomina>("Select VALUE nomina From dbNominaEntities.tNominas As nomina", objectContext, MergeOption.NoTracking);

                            //IQueryable<tEmpleado> query = context.tEmpleados;
                            query = query.Include("tNominaHeader").
                                    Include("tNominaHeader.tGruposEmpleado").
                                    Include("tNominaHeader.tGruposEmpleado.Compania").
                                    Include("tEmpleado").
                                    Include("tEmpleado.tDepartamento").
                                    Include("tMaestraRubro").
                                    Where(filter);

                            //var query = context.tNominas.Include("tNominaHeader").
                            //                             Include("tNominaHeader.tGruposEmpleado").
                            //                             Include("tNominaHeader.tGruposEmpleado.Compania").
                            //                             Include("tEmpleado").
                            //                             Include("tEmpleado.tDepartamento").
                            //                             Include("tMaestraRubro").
                            //                             Where(filter);

                            if (query.Count() == 0)
                            {
                                ErrMessage_Cell.InnerHtml = "No existe información para mostrar el reporte " +
                                    "que Ud. ha requerido. <br /><br /> Probablemente Ud. no ha aplicado un " +
                                    "filtro y seleccionado información aún.";
                                return;
                            }

                            // ahora preparamos una lista para usarla como DataSource del report ... 

                            List<Nomina_Report_ConsultaNomina> myList = new List<Nomina_Report_ConsultaNomina>();
                            Nomina_Report_ConsultaNomina infoNomina;

                            decimal? nullDecimal = null;

                            foreach (tNomina item in query)
                            {
                                infoNomina = new Nomina_Report_ConsultaNomina();

                                infoNomina.HeaderID = item.HeaderID;
                                infoNomina.CiaContab = item.tNominaHeader.tGruposEmpleado.Compania.Nombre;
                                infoNomina.GrupoEmpleados = item.tNominaHeader.tGruposEmpleado.Descripcion;
                                infoNomina.FechaNomina = item.tNominaHeader.FechaNomina;
                                infoNomina.Desde = item.tNominaHeader.Desde;
                                infoNomina.Hasta = item.tNominaHeader.Hasta;
                                infoNomina.TipoNomina = GetTipoNomina(item.tNominaHeader.Tipo);
                                infoNomina.Departamento = item.tEmpleado.tDepartamento.Descripcion;
                                infoNomina.Empleado = item.tEmpleado.Nombre;
                                infoNomina.Rubro = item.tMaestraRubro.NombreCortoRubro;
                                infoNomina.Descripcion = item.Descripcion;

                                infoNomina.Asignacion = item.Monto >= 0 ? item.Monto : nullDecimal;
                                infoNomina.Deduccion = item.Monto < 0 ? Math.Abs(item.Monto) : nullDecimal;
                                infoNomina.Monto = item.Monto; 
                                infoNomina.Base = item.MontoBase;
                                infoNomina.CantidadDias = item.CantDias;
                                infoNomina.Fraccion = item.Fraccion;
                                infoNomina.Detalles = item.Detalles; 
                                infoNomina.SueldoFlag = item.SueldoFlag;
                                infoNomina.SalarioFlag = item.SalarioFlag;

                                myList.Add(infoNomina);
                            }

                            if (opcion == "nomina")
                            {
                                if (agruparPor == "empleado")
                                    ReportViewer1.LocalReport.ReportPath = "Nomina/Nomina/Nomina.rdlc";
                                else
                                    ReportViewer1.LocalReport.ReportPath = "Nomina/Nomina/NominaPorRubroDepartamento.rdlc";
                            }
                            else
                            {
                                if (agruparPor == "empleado")
                                    ReportViewer1.LocalReport.ReportPath = "Nomina/Consulta/Nomina.rdlc";
                                else
                                    ReportViewer1.LocalReport.ReportPath = "Nomina/Consulta/NominaPorRubroDepartamento.rdlc";
                            }

                            ReportDataSource myReportDataSource = new ReportDataSource();

                            myReportDataSource.Name = "DataSet1";
                            myReportDataSource.Value = myList;

                            ReportViewer1.LocalReport.DataSources.Add(myReportDataSource);

                            ReportViewer1.LocalReport.Refresh();

                            if (opcion == "nomina")
                            {
                                infoNomina = myList.First();

                                ReportParameter FechaNomina_ReportParameter = new ReportParameter("FechaNomina", infoNomina.FechaNomina.ToString("d-MMM-yyyy"));

                                DateTime fecha = infoNomina.Desde == null ? new DateTime(1970, 1, 1) : infoNomina.Desde.Value;
                                ReportParameter Desde_ReportParameter = new ReportParameter("Desde", fecha.ToString("d-MMM-yyyy"));

                                fecha = infoNomina.Hasta == null ? new DateTime(1970, 1, 1) : infoNomina.Hasta.Value;
                                ReportParameter Hasta_ReportParameter = new ReportParameter("Hasta", fecha.ToString("d-MMM-yyyy"));

                                ReportParameter[] MyReportParameters = { FechaNomina_ReportParameter, Desde_ReportParameter, Hasta_ReportParameter };

                                ReportViewer1.LocalReport.SetParameters(MyReportParameters);
                            }
                            else
                            {
                                string subTituloReporte = Request.QueryString["st"] != null ? Request.QueryString["st"].ToString() : "";

                                ReportParameter SubTitulo_ReportParameter = new ReportParameter("subTitulo", subTituloReporte);
                                ReportParameter[] MyReportParameters = { SubTitulo_ReportParameter };

                                ReportViewer1.LocalReport.SetParameters(MyReportParameters);
                            }
                            
                            break;
                        }

                    case "consultaPrestaciones":
                        {
                            // los criterios usados para construir un filtro vienen en un objeto en Session ... 
                            // TODO: corregir y cambiar por parameter (querystring) cuando tengamos una respuesta a este issue ... 

                            if (Request.QueryString["headerID"] == null)
                            {
                                ErrMessage_Cell.InnerHtml = "Esta consulta no ha recibido los parámetros esperados." +
                                    "<br /><br />Ha habido un error al intentar obtener esta consulta. Por favor intente nuevamente.";
                                break;
                            }

                            int prestacionesHeaderID = 0;

                            if (!int.TryParse(Request.QueryString["headerID"].ToString(), out prestacionesHeaderID))
                            {
                                ErrMessage_Cell.InnerHtml = "Esta consulta no ha recibido los parámetros esperados." +
                                    "<br /><br />Ha habido un error al intentar obtener esta consulta. Por favor intente nuevamente.";
                                break;
                            }

                            dbNominaEntities context = new dbNominaEntities();

                            //IQueryable<tEmpleado> query = context.tEmpleados;
                            var query = context.PrestacionesSociales.Include("PrestacionesSocialesHeader").
                                                         Include("PrestacionesSocialesHeader.Compania").
                                                         Include("tEmpleado").
                                                         Where(n => n.HeaderID == prestacionesHeaderID);

                            if (query.Count() == 0)
                            {
                                ErrMessage_Cell.InnerHtml = "No existe información para mostrar el reporte " +
                                    "que Ud. ha requerido. <br /><br /> Probablemente Ud. no ha aplicado un " +
                                    "filtro y seleccionado información aún.";
                                return;
                            }

                            // ahora preparamos una lista para usarla como DataSource del report ... 

                            List<Nomina_Report_ConsultaPrestacionesSociales> myList = new List<Nomina_Report_ConsultaPrestacionesSociales>();
                            Nomina_Report_ConsultaPrestacionesSociales infoPrestacionesSociales;

                            short? nullShort = null; 

                            foreach (PrestacionesSociale item in query)
                            {
                                infoPrestacionesSociales = new Nomina_Report_ConsultaPrestacionesSociales();

                                infoPrestacionesSociales.HeaderID = item.HeaderID;

                                infoPrestacionesSociales.Desde = item.PrestacionesSocialesHeader.Desde;
                                infoPrestacionesSociales.Hasta = item.PrestacionesSocialesHeader.Hasta;

                                infoPrestacionesSociales.CiaContab = item.PrestacionesSocialesHeader.Compania.Nombre;
                                infoPrestacionesSociales.Empleado = item.tEmpleado.Nombre;

                                infoPrestacionesSociales.FechaIngreso = item.FechaIngreso;
                                infoPrestacionesSociales.AnosServicio = item.AnosServicio;
                                infoPrestacionesSociales.AnosServicioLey = item.AnosServicioPrestaciones;

                                infoPrestacionesSociales.CantDias1erMes = item.CantidadDiasTrabajadosPrimerMes;
                                
                                infoPrestacionesSociales.SalarioPeriodo = 0;
                                infoPrestacionesSociales.SalarioMensual = item.SueldoBasicoPrestaciones.HasValue ? item.SueldoBasicoPrestaciones.Value : 0;
                                infoPrestacionesSociales.SalarioDiario = item.SueldoBasicoDiario; 

                                infoPrestacionesSociales.BonoVacCantDias = 0;
                                infoPrestacionesSociales.BonoVacMonto = item.BonoVacacional;
                                infoPrestacionesSociales.BonoVacDiario = item.BonoVacacionalDiario;

                                infoPrestacionesSociales.UtilidadesCantDias = item.PrestacionesSocialesHeader.CantDiasUtilidades == 0 ? nullShort : item.PrestacionesSocialesHeader.CantDiasUtilidades;
                                infoPrestacionesSociales.UtilidadesMonto = item.Utilidades;
                                infoPrestacionesSociales.UtilidadesDiario = item.UtilidadesDiarias;

                                infoPrestacionesSociales.SalarioTotalDiario = item.SueldoDiarioAumentado;

                                infoPrestacionesSociales.PrestacionesCantDias = item.DiasPrestaciones;
                                infoPrestacionesSociales.PrestacionesMonto = item.MontoPrestaciones;

                                infoPrestacionesSociales.PrestacionesDiasAdicAnoCumplidoFlag = item.AnoCumplidoFlag;
                                infoPrestacionesSociales.PrestacionesDiasAdicAnoCumplidoCantDias = item.CantidadDiasAdicionales;
                                infoPrestacionesSociales.PrestacionesDiasAdicAnoCumplidoMonto = item.MontoPrestacionesDiasAdicionales;

                                infoPrestacionesSociales.PrestacionesTotalMonto = item.MontoPrestaciones;

                                if (item.MontoPrestacionesDiasAdicionales != null)
                                    infoPrestacionesSociales.PrestacionesTotalMonto += item.MontoPrestacionesDiasAdicionales.Value; 

                                myList.Add(infoPrestacionesSociales);
                            }

                            ReportViewer1.LocalReport.ReportPath = "Nomina/PrestacionesSociales/PrestacionesSociales.rdlc";

                            ReportDataSource myReportDataSource = new ReportDataSource();

                            myReportDataSource.Name = "DataSet1";
                            myReportDataSource.Value = myList;

                            ReportViewer1.LocalReport.DataSources.Add(myReportDataSource);

                            ReportViewer1.LocalReport.Refresh();

                            infoPrestacionesSociales = myList.First();

                            DateTime fecha = infoPrestacionesSociales.Desde == null ? new DateTime(1970, 1, 1) : infoPrestacionesSociales.Desde.Value;
                            ReportParameter Desde_ReportParameter = new ReportParameter("Desde", fecha.ToString("d-MMM-yyyy"));

                            fecha = infoPrestacionesSociales.Hasta == null ? new DateTime(1970, 1, 1) : infoPrestacionesSociales.Hasta.Value;
                            ReportParameter Hasta_ReportParameter = new ReportParameter("Hasta", fecha.ToString("d-MMM-yyyy"));

                            ReportParameter[] MyReportParameters = { Desde_ReportParameter, Hasta_ReportParameter };

                            ReportViewer1.LocalReport.SetParameters(MyReportParameters);

                            break;
                        }

                    case "unasientocontable":
                        {
                            int numeroAutomaticoAsientoContable;

                            if (!int.TryParse(Request.QueryString["NumeroAutomatico"], out numeroAutomaticoAsientoContable))
                            {
                                ErrMessage_Cell.InnerHtml = "No se ha pasado un parámetro correcto a esta función. <br /><br />" +
                                    "Esta función debe recibir como parámetro el ID válido de un asiento contable.";
                                return;
                            }

                            List<Asiento2_Report> list = new List<Asiento2_Report>();

                            string sqlSelect =
                                  "SELECT Monedas.Descripcion AS NombreMoneda, Companias.Nombre AS NombreCiaContab, Asientos.Fecha, Asientos.NumeroAutomatico, Asientos.Numero, " +
                                  "TiposDeAsiento.Descripcion AS NombreTipo, Monedas_1.Simbolo AS SimboloMonedaOriginal, dAsientos.Partida, " +
                                  "CuentasContables.CuentaEditada AS CuentaContableEditada, CuentasContables.Descripcion AS NombreCuentaContable, dAsientos.Descripcion, dAsientos.Referencia, " +
                                  "dAsientos.Debe, dAsientos.Haber, Asientos.ProvieneDe, Asientos.Descripcion " +
                                  "FROM Asientos " +
                                  "INNER JOIN Companias ON Asientos.Cia = Companias.Numero " +
                                  "INNER JOIN dAsientos ON Asientos.NumeroAutomatico = dAsientos.NumeroAutomatico " +
                                  "INNER JOIN CuentasContables ON dAsientos.CuentaContableID = CuentasContables.ID " +
                                  "INNER JOIN Monedas ON Asientos.Moneda = Monedas.Moneda " +
                                  "INNER JOIN TiposDeAsiento ON Asientos.Tipo = TiposDeAsiento.Tipo " +
                                  "INNER JOIN Monedas AS Monedas_1 ON Asientos.MonedaOriginal = Monedas_1.Moneda " +
                                  "Where Asientos.NumeroAutomatico = " + numeroAutomaticoAsientoContable.ToString();

                            SqlConnection connection = new SqlConnection();
                            connection.ConnectionString = ConfigurationManager.ConnectionStrings["dbContabConnectionString"].ConnectionString;

                            using (connection)
                            {
                                SqlCommand command = new SqlCommand(sqlSelect, connection);
                                connection.Open();

                                SqlDataReader reader = command.ExecuteReader();

                                Asiento2_Report asiento;

                                while (reader.Read())
                                {
                                    asiento = new Asiento2_Report();

                                    asiento.NombreMoneda = reader.GetString(0);
                                    asiento.NombreCiaContab = reader.GetString(1);
                                    asiento.Fecha = reader.GetDateTime(2);
                                    asiento.NumeroAutomatico = reader.GetInt32(3);
                                    asiento.Numero = reader.GetInt16(4);
                                    asiento.NombreTipo = reader.GetString(5);
                                    asiento.SimboloMonedaOriginal = reader.GetString(6);

                                    asiento.Partida = reader.GetInt16(7);
                                    asiento.CuentaContableEditada = reader.GetString(8);
                                    asiento.NombreCuentaContable = reader.GetString(9);
                                    asiento.Descripcion = reader.GetString(10);
                                    asiento.Referencia = reader.IsDBNull(11) ? string.Empty : reader.GetString(11);
                                    asiento.Debe = reader.GetDecimal(12);
                                    asiento.Haber = reader.GetDecimal(13);

                                    asiento.ProvieneDe = reader.IsDBNull(14) ? string.Empty : reader.GetString(14);
                                    asiento.DescripcionGeneralAsientoContable = reader.IsDBNull(15) ? string.Empty : reader.GetString(15);

                                    list.Add(asiento);
                                }

                                reader.Close();
                            }

                            if (list.Count == 0)
                            {
                                ErrMessage_Cell.InnerHtml = "No existe información para mostrar el reporte que Ud. ha requerido. <br /><br />" +
                                    "Probablemente Ud. no ha aplicado un filtro y seleccionado información aún.";
                                return;
                            }


                            this.ReportViewer1.LocalReport.ReportPath = "Nomina/AsientosContables/ComprobantesContables.rdlc";

                            ReportDataSource myReportDataSource = new ReportDataSource();

                            myReportDataSource.Name = "DataSet1";
                            myReportDataSource.Value = list;

                            this.ReportViewer1.LocalReport.DataSources.Add(myReportDataSource);

                            // pasamos 1-1-1960 para que el reporte no imprima un rango de fechas en el encabezado; cuando se imprimen 
                            // varios comprobantes, si es adecuado hacerlo ... 

                            ReportParameter FechaInicialPeriodo_ReportParameter = new ReportParameter("FechaInicialPeriodo", "1960-1-1");
                            ReportParameter FechaFinalPeriodo_ReportParameter = new ReportParameter("FechaFinalPeriodo", "1960-1-1");

                            ReportParameter[] MyReportParameters = { FechaInicialPeriodo_ReportParameter, FechaFinalPeriodo_ReportParameter };

                            this.ReportViewer1.LocalReport.SetParameters(MyReportParameters);

                            this.ReportViewer1.LocalReport.Refresh();

                            break;
                        }

                    case "faltasEmpleados":
                        {
                            // los criterios usados para construir un filtro vienen en un objeto en Session ... 
                            // TODO: corregir y cambiar por parameter (querystring) cuando tengamos una respuesta a este issue ... 

                            ConsultaEmpleadosFaltas_CriteriosFiltro criterioFiltro;

                            if (Session["ConsultaEmpleadosFaltas_CriteriosFiltro"] != null)
                                criterioFiltro = Session["ConsultaEmpleadosFaltas_CriteriosFiltro"] as ConsultaEmpleadosFaltas_CriteriosFiltro;
                            else
                            {
                                string errorMessage = "Aparentemente, Ud. no ha aplicado un filtro que permita seleccionar registros.<br />" +
                                    "Por favor, cierre esta página y aplique un filtro para seleccionar registros.";

                                ErrMessage_Cell.InnerHtml = errorMessage;
                                break;
                            }

                            string opcionReporte = "normal";

                            if (Request.QueryString["opc"] != null)
                                opcionReporte = Request.QueryString["opc"].ToString();

                            dbNominaEntities context = new dbNominaEntities();

                            var query = context.Empleados_Faltas.Include("tEmpleado").
                                                                 Include("tEmpleado.tDepartamento").
                                                                 Include("tEmpleado.Compania").
                                                                 Where(f => f.tEmpleado.Cia == criterioFiltro.CiaContab.Value);


                            if (criterioFiltro.Empleado != null)
                                query = query.Where(f => f.Empleado == criterioFiltro.Empleado);


                            if (criterioFiltro.Departamento != null)
                                query = query.Where(f => f.tEmpleado.Departamento == criterioFiltro.Departamento);


                            if (criterioFiltro.Desde != null && criterioFiltro.Hasta != null)
                            {
                                query = query.Where(f => (f.Desde >= criterioFiltro.Desde && f.Desde <= criterioFiltro.Hasta) ||    // el período cubre el inicio de la falta 
                                                         (f.Hasta >= criterioFiltro.Desde && f.Hasta <= criterioFiltro.Hasta) ||    // el período cubro el final de la falta 
                                                         (f.Desde < criterioFiltro.Desde && f.Hasta > criterioFiltro.Hasta)         // el periodo esta *dentro* de la falta 

                                    );
                            }

                            if (criterioFiltro.Descontar != null && criterioFiltro.Descontar.Value)
                                query = query.Where(f => f.Descontar);

                            if (!string.IsNullOrEmpty(criterioFiltro.Status))
                                query = query.Where(f => f.tEmpleado.Status == criterioFiltro.Status);

                            if (!string.IsNullOrEmpty(criterioFiltro.SituacionActual))
                                query = query.Where(f => f.tEmpleado.SituacionActual == criterioFiltro.SituacionActual);


                            if (query.Count() == 0)
                            {
                                ErrMessage_Cell.InnerHtml = "No existe información para mostrar el reporte que Ud. ha requerido.<br /><br /> " +
                                    "Probablemente Ud. no ha aplicado un filtro y seleccionado información aún.";
                                return;
                            }

                            List<EmpleadoFaltaConsulta> list = new List<EmpleadoFaltaConsulta>();
                            EmpleadoFaltaConsulta item;

                            foreach (Empleados_Faltas falta in query)
                            {
                                item = new EmpleadoFaltaConsulta();

                                item.CiaContab = falta.tEmpleado.Compania.Nombre;
                                item.Departamento = falta.tEmpleado.tDepartamento.Descripcion;
                                item.Empleado = falta.tEmpleado.Nombre;
                                item.Descontar = falta.Descontar;
                                item.Desde = falta.Desde;
                                item.Hasta = falta.Hasta;
                                item.TotalDias = falta.CantDias;
                                item.SabYDom = falta.CantDiasSabDom;
                                item.Feriados = falta.CantDiasFeriados;
                                item.Faltas = falta.CantDiasHabiles;
                                item.CantHoras = falta.CantHoras;
                                item.FechaNomina = falta.Descontar_FechaNomina;
                                item.Observaciones = falta.Observaciones; 

                                list.Add(item);
                            }

                            ReportViewer1.LocalReport.ReportPath = "Empleados/Faltas/ConsultaEmpleadosFaltas.rdlc";

                            ReportDataSource myReportDataSource = new ReportDataSource();

                            myReportDataSource.Name = "DataSet1";
                            myReportDataSource.Value = list;

                            ReportViewer1.LocalReport.DataSources.Add(myReportDataSource);

                            ReportViewer1.LocalReport.Refresh();

                            //fecha = infoPrestacionesSociales.Hasta == null ? new DateTime(1970, 1, 1) : infoPrestacionesSociales.Hasta.Value;
                            //ReportParameter Hasta_ReportParameter = new ReportParameter("Hasta", fecha.ToString("d-MMM-yyyy"));

                            //ReportParameter[] MyReportParameters = { Desde_ReportParameter, Hasta_ReportParameter };

                            //ReportViewer1.LocalReport.SetParameters(MyReportParameters);

                            if (opcionReporte == "pdf")
                            {
                                Warning[] warnings;
                                string[] streamIds;
                                string mimeType = string.Empty;
                                string encoding = string.Empty;
                                string extension = string.Empty;

                                byte[] bytes = this.ReportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                                // Now that you have all the bytes representing the PDF report, buffer it and send it to the client.
                                Response.Buffer = true;
                                Response.Clear();
                                Response.ContentType = mimeType;
                                Response.AddHeader("content-disposition", "attachment; filename=CompRetIva.pdf");
                                Response.BinaryWrite(bytes); // create the file
                                Response.Flush(); // send it to the client to download
                            }

                            break;
                        }



                    case "ControlDiasVacaciones":
                        {
                            string errorMessage = ""; 

                            // los criterios usados para construir un filtro vienen en un objeto en Session ... 
                            // TODO: corregir y cambiar por parameter (querystring) cuando tengamos una respuesta a este issue ... 

                            ConsultaCriteriosFiltro_ControlDiasVacaciones criterioFiltro;

                            if (Session["ConsultaCriteriosFiltro"] != null)
                                criterioFiltro = Session["ConsultaCriteriosFiltro"] as ConsultaCriteriosFiltro_ControlDiasVacaciones;
                            else
                            {
                                errorMessage = "Aparentemente, Ud. no ha aplicado un filtro que permita seleccionar registros.<br />" +
                                    "Por favor, cierre esta página y aplique un filtro para seleccionar registros.";

                                ErrMessage_Cell.InnerHtml = errorMessage;
                                break;
                            }

                            string opcionReporte = "normal";

                            if (Request.QueryString["opc"] != null)
                                opcionReporte = Request.QueryString["opc"].ToString();



                            dbNominaEntities context = new dbNominaEntities();

                            var query = context.tEmpleados.Include("Compania").Include("tDepartamento").Where(f => f.Cia == criterioFiltro.CiaContab.Value);

                            if (!string.IsNullOrEmpty(criterioFiltro.Status))
                                query = query.Where(f => f.Status == criterioFiltro.Status);

                            if (!string.IsNullOrEmpty(criterioFiltro.SituacionActual))
                                query = query.Where(f => f.SituacionActual == criterioFiltro.SituacionActual);

                            int totalRowCount = query.Count();

                            if (totalRowCount == 0)
                            {
                                errorMessage = "No se han seleccionado registros que mostrar; probablemente no se ha indicado un filtro correcto a esta página.";
                                ErrMessage_Cell.InnerHtml = errorMessage;

                                break; 
                            }

                            query = query.OrderBy(f => f.Alias);

                            List<DiasVacacionesConsulta> list = new List<DiasVacacionesConsulta>();
                            DiasVacacionesConsulta item;

                            Nomina_FuncionesGenericas funcionesNomina = new Nomina_FuncionesGenericas();

                            DateTime primerDiaAnoConsulta = new DateTime(criterioFiltro.AnoConsulta.Value, 1, 1);
                            DateTime ultimoDiaAnoConsulta = new DateTime(criterioFiltro.AnoConsulta.Value, 12, 31);

                            //foreach (tEmpleado f in query)
                            //{
                            //    item = new DiasVacacionesConsulta();

                            //    item.CiaContab = f.Compania.Nombre;
                            //    item.Departamento = f.tDepartamento.Descripcion;

                            //    item.ID = f.Empleado;
                            //    item.Nombre = f.Nombre;
                            //    item.Alias = f.Alias;
                            //    item.FechaIngreso = f.FechaIngreso;

                            //    // -----------------------------------------------------------------------------------------------------------
                            //    // 1) determinamos, según el año de la consulta (ej: 2013), el año de vacaciones del empleado; por ejemplo: un empleado 
                            //    //    puede tener 15 años en la empresa y su año de vacaciones (con respecto al 2.013) podría ser: 1/7/2012 al 30/6/2013. 

                            //    DateTime anoEmpleadoDesde = f.FechaIngreso;
                            //    DateTime anoEmpleadoHasta = f.FechaIngreso.AddYears(1).AddDays(-1);
                            //    int anoEmpleadoEnEmpresa = 0;

                            //    while (anoEmpleadoHasta < ultimoDiaAnoConsulta.AddDays(-1))
                            //    {
                            //        item.AnoVacaciones_Ano = anoEmpleadoEnEmpresa;
                            //        item.AnoVacaciones_Desde = anoEmpleadoDesde;
                            //        item.AnoVacaciones_Hasta = anoEmpleadoHasta;

                            //        anoEmpleadoDesde = anoEmpleadoDesde.AddYears(1);
                            //        anoEmpleadoHasta = anoEmpleadoDesde.AddYears(1).AddDays(-1);

                            //        anoEmpleadoEnEmpresa++;
                            //    }

                            //    // -----------------------------------------------------------------------------------------------------------
                            //    // 2) ahora leemos la vacación más reciente, siempre con respecto al año de la consulta ... 

                            //    Vacacione vacacion = context.Vacaciones.Where(v => v.AnoVacaciones <= anoEmpleadoEnEmpresa).OrderByDescending(v => v.Salida).FirstOrDefault();

                            //    if (vacacion != null)
                            //    {
                            //        item.VacacionMasReciente_Desde = vacacion.Salida;
                            //        item.VacacionMasReciente_Hasta = vacacion.Regreso;
                            //        item.VacacionMasReciente_AnoVacaciones = vacacion.AnoVacaciones;
                            //        item.VacacionMasReciente_DiasPendAnosAnteriores = vacacion.CantDiasVacPendAnosAnteriores;
                            //        item.VacacionMasReciente_DiasSegunTabla = vacacion.CantDiasVacSegunTabla;

                            //        item.VacacionMasReciente_DiasDisfrutados_Antes = vacacion.CantDiasVacDisfrutadosAntes;
                            //        item.VacacionMasReciente_DiasDisfrutados_Ahora = vacacion.CantDiasVacDisfrutadosAhora;

                            //        item.VacacionMasReciente_DiasPendientes = vacacion.CantDiasVacPendientes;
                            //    }

                            //    // -----------------------------------------------------------------------------------------------------------
                            //    // 3) intentamos determinar si existen vacaciones pendientes (entre la última tomada y el año actual) 

                            //    item.VacacionesPendientes_Cantidad = 0;
                            //    item.VacacionesPendientes_DiasSegunTabla = 0;

                            //    for (int ano = item.VacacionMasReciente_AnoVacaciones; ano <= item.AnoVacaciones_Ano; ano++)
                            //    {
                            //        // buscamos la cantidad de días según tabla, para el año pendiente específico ... 

                            //        int cantidadDiasVacaciones = 0;
                            //        int cantidadDiasBono = 0;

                            //        if (!funcionesNomina.DeterminarDiasVacacionesParaEmpleado(context,
                            //                                                                  f.Empleado,
                            //                                                                  ano,
                            //                                                                  out cantidadDiasVacaciones,
                            //                                                                  out cantidadDiasBono,
                            //                                                                  out errorMessage))
                            //        {
                            //            ErrMessage_Cell.InnerHtml = errorMessage;
                            //            break;
                            //        }


                            //        item.VacacionesPendientes_DiasSegunTabla += (short)cantidadDiasVacaciones;
                            //        item.VacacionesPendientes_Cantidad++;
                            //    }


                            //    // -----------------------------------------------------------------------------------------------------------
                            //    // 4) finalmente, determinamos la cantidad de días de vacaciones pendientes ... 

                            //    item.TotalDiasPendientes = 0;

                            //    if (item.VacacionMasReciente_DiasPendientes != null)
                            //        item.TotalDiasPendientes = item.VacacionMasReciente_DiasPendientes.Value;

                            //    item.TotalDiasPendientes += item.VacacionesPendientes_DiasSegunTabla;

                            //    list.Add(item);
                            //}


                            foreach (tEmpleado emp in query)
                            {
                                item = new DiasVacacionesConsulta();

                                item.CiaContab = emp.Compania.Nombre;
                                item.Departamento = emp.tDepartamento.Descripcion;

                                item.ID = emp.Empleado;
                                item.Nombre = emp.Nombre;
                                item.Alias = emp.Alias;
                                item.FechaIngreso = emp.FechaIngreso;

                                // -----------------------------------------------------------------------------------------------------------
                                // 1) determinamos, según el año de la consulta (ej: 2013), el año de vacaciones del empleado; por ejemplo: un empleado 
                                //    puede tener 15 años en la empresa y su año de vacaciones (con respecto al 2.013) podría ser: 1/7/2012 al 30/6/2013. 

                                DateTime anoEmpleadoDesde = emp.FechaIngreso;
                                DateTime anoEmpleadoHasta = emp.FechaIngreso.AddYears(1).AddDays(-1);
                                int anoEmpleadoEnEmpresa = 0;

                                while (anoEmpleadoHasta < ultimoDiaAnoConsulta.AddDays(-1))
                                {
                                    item.AnoVacaciones_Ano = anoEmpleadoEnEmpresa + 1;
                                    item.AnoVacaciones_Desde = anoEmpleadoDesde;
                                    item.AnoVacaciones_Hasta = anoEmpleadoHasta;

                                    anoEmpleadoDesde = anoEmpleadoDesde.AddYears(1);
                                    anoEmpleadoHasta = anoEmpleadoDesde.AddYears(1).AddDays(-1);

                                    anoEmpleadoEnEmpresa++;
                                }

                                // -----------------------------------------------------------------------------------------------------------
                                // 2) ahora leemos la vacación más reciente, siempre con respecto al año de la consulta ... 

                                Vacacione vacacion = context.Vacaciones.Where(v => v.AnoVacaciones <= anoEmpleadoEnEmpresa && v.Empleado == emp.Empleado).
                                                                        OrderByDescending(v => v.Salida).
                                                                        FirstOrDefault();

                                if (vacacion != null)
                                {
                                    item.VacacionMasReciente_Desde = vacacion.Salida;
                                    item.VacacionMasReciente_Hasta = vacacion.Regreso;
                                    item.VacacionMasReciente_AnoVacaciones = vacacion.AnoVacaciones;
                                    item.VacacionMasReciente_DiasPendAnosAnteriores = vacacion.CantDiasVacPendAnosAnteriores;
                                    item.VacacionMasReciente_DiasSegunTabla = vacacion.CantDiasVacSegunTabla;

                                    item.VacacionMasReciente_DiasDisfrutados_Antes = vacacion.CantDiasVacDisfrutadosAntes;
                                    item.VacacionMasReciente_DiasDisfrutados_Ahora = vacacion.CantDiasVacDisfrutadosAhora;

                                    item.VacacionMasReciente_DiasPendientes = vacacion.CantDiasVacPendientes;
                                }

                                // -----------------------------------------------------------------------------------------------------------
                                // 3) intentamos determinar si existen vacaciones pendientes (entre la última tomada y el año actual) 

                                item.VacacionesPendientes_Cantidad = 0;
                                item.VacacionesPendientes_DiasSegunTabla = 0;

                                for (int ano = item.VacacionMasReciente_AnoVacaciones; ano < item.AnoVacaciones_Ano; ano++)
                                {
                                    // buscamos la cantidad de días según tabla, para el año pendiente específico ... 

                                    int cantidadDiasVacaciones = 0;
                                    int cantidadDiasBono = 0;

                                    if (!funcionesNomina.DeterminarDiasVacacionesParaEmpleado(context,
                                                                                              emp.Empleado,
                                                                                              ano,
                                                                                              out cantidadDiasVacaciones,
                                                                                              out cantidadDiasBono,
                                                                                              out errorMessage))
                                    {
                                        ErrMessage_Cell.InnerHtml = errorMessage;
                                        break;
                                    }


                                    item.VacacionesPendientes_DiasSegunTabla += (short)cantidadDiasVacaciones;
                                    item.VacacionesPendientes_Cantidad++;
                                }


                                // -----------------------------------------------------------------------------------------------------------
                                // 4) finalmente, determinamos la cantidad de días de vacaciones pendientes ... 

                                item.TotalDiasPendientes = 0;

                                if (item.VacacionMasReciente_DiasPendientes != null)
                                    item.TotalDiasPendientes = item.VacacionMasReciente_DiasPendientes.Value;

                                item.TotalDiasPendientes += item.VacacionesPendientes_DiasSegunTabla;

                                list.Add(item);
                            }

                            ReportViewer1.LocalReport.ReportPath = "Empleados/Vacaciones/DiasVacaciones.rdlc";

                            ReportDataSource myReportDataSource = new ReportDataSource();

                            myReportDataSource.Name = "DataSet1";
                            myReportDataSource.Value = list;

                            ReportViewer1.LocalReport.DataSources.Add(myReportDataSource);

                            ReportViewer1.LocalReport.Refresh();

                            ReportParameter AnoConsulta_ReportParameter = new ReportParameter("AnoConsulta", criterioFiltro.AnoConsulta.Value.ToString());

                            ReportParameter[] MyReportParameters = { AnoConsulta_ReportParameter };

                            ReportViewer1.LocalReport.SetParameters(MyReportParameters);

                            if (opcionReporte == "pdf")
                            {
                                Warning[] warnings;
                                string[] streamIds;
                                string mimeType = string.Empty;
                                string encoding = string.Empty;
                                string extension = string.Empty;

                                byte[] bytes = this.ReportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                                // Now that you have all the bytes representing the PDF report, buffer it and send it to the client.
                                Response.Buffer = true;
                                Response.Clear();
                                Response.ContentType = mimeType;
                                Response.AddHeader("content-disposition", "attachment; filename=CompRetIva.pdf");
                                Response.BinaryWrite(bytes); // create the file
                                Response.Flush(); // send it to the client to download
                            }

                            break;
                        }
                }
            }
        }


        public string GetStatusEmpleado(string status)
        {
            switch (status)
            {
                case "A":
                    return "Activo";
                case "S":
                    return "Suspendido";
            }
            return "Indefinido";
        }

        public string GetSituacionActualEmpleado(string situacionActual)
        {
            switch (situacionActual)
            {
                case "NO":
                    return "Normal";
                case "VA":
                    return "Vacaciones";
                case "RE":
                    return "Reposo";
                case "LI":
                    return "Liquidado";
            }
            return "Indefinido";
        }

        public string GetTipoNomina(string tipoNomina)
        {
            switch (tipoNomina)
            {
                case "N":
                    return "Normal";
                case "Q":
                    return "Quincenal";
                case "M":
                    return "Mensual";
                case "V":
                    return "Vacaciones";
                case "E":
                    return "Especial";
                case "U":
                    return "Utilidades";
            }
            return "Indefinido";
        }

        public decimal? GetSueldoEmpleado(int empleadoID)
        {
            decimal? sueldoEmpleado = null;

            using (dbNominaEntities context = new dbNominaEntities())
            {
                sueldoEmpleado = context.Empleados_Sueldo.Where(s => s.Empleado == empleadoID).
                                                          OrderByDescending(s => s.Desde).
                                                          Select(s => s.Sueldo).
                                                          FirstOrDefault();
            }

            return sueldoEmpleado;
        }
    }
}