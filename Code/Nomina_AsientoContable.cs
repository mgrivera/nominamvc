using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Providers.Entities;
using NominaASP.Models.Contab;
//using NominaASP.Models.Nomina;
using NominaASP.Models;

namespace NominaASP.Code
{
    public class Nomina_AsientoContable
    {
        tNominaHeader _nominaHeader; 
        dbNominaEntities _context; 

        public Nomina_AsientoContable(int nominaHeaderID)
        {
            _context = new dbNominaEntities(); 
            _nominaHeader = _context.tNominaHeaders.Where(h => h.ID == nominaHeaderID).FirstOrDefault(); 
        }

        public bool ConstruirAsientoContable(string descripcionAsiento, 
                                             DateTime fechaAsiento, 
                                             decimal factorCambio,
                                             string orderBy, 
                                             string userName, 
                                             out string resultMessage)
        {
            resultMessage = ""; 

            if (_context == null) 
            {
                resultMessage = "Error inesperado: no hemos podido establecer un 'context' para tener acceso a funciones de base de datos (???). "; 
                return false; 
            }

            if (_nominaHeader == null) 
            {
                resultMessage = "Error inesperado: no hemos podido leer los datos genéricos (header) de la nómina que se ha pasado a esta función. "; 
                return false; 
            }


            ParametrosNomina parametros = _context.ParametrosNominas.Where(p => p.Cia == _nominaHeader.tGruposEmpleado.Compania.Numero).FirstOrDefault(); 

            if (parametros == null) 
            {
                resultMessage = "Error: no existe un registro en la tabla Parámetros de nómina para la Cia Contab " + 
                    "que corresponde a la nómina pasada a esta función. "; 
                return false; 
            }

            if (parametros.AgregarAsientosContables == null || !parametros.AgregarAsientosContables.Value) 
            {
                resultMessage = "Error: el item que indica, en la tabla Parámetros de Nómina, que el asiento contable debe ser generado, no está marcado. " + 
                    "Por favor marque este item antes de generar el asiento contable para una nómina. "; 
                return false; 
            }

            if (parametros.MonedaParaElAsiento == null) 
            {
                resultMessage = "Error: no se ha indicado, en la tabla Parámetros de Nómina, la moneda que debe ser usada para la construcción del asiento contable." + 
                    "Por favor registre una moneda en esta tabla. "; 
                return false; 
            }

            if (parametros.CuentaContableNomina == null) 
            {
                resultMessage = "Error: no se ha indicado, en la tabla Parámetros de Nómina, la cuenta contable que debe ser usada para la construcción " + 
                    "del asiento contable. Por favor registre una moneda en esta tabla. "; 
                return false; 
            }

            if (string.IsNullOrEmpty(parametros.TipoAsientoDefault))
            {
                resultMessage = "Error: no se ha indicado, en la tabla Parámetros de Nómina, el tipo de asiento que debe ser usado para la construcción " +
                    "del asiento contable. Por favor registre un tipo de asiento en esta tabla. ";
                return false;
            }

            if (_nominaHeader.tNominas.Count() == 0)
            {
                resultMessage = "Error: aparentemente, la nómina no tiene registros asociados; aunque la definición para esta nómina ha sido creada, " + 
                    "probablemente no ha sido ejecutada todavía. ";
                return false;
            }

            // ------------------------------------------------------------------------------------------
            // validamos que el mes no esté cerrado en Contab; la función regresa, además, el mes y año fiscal ... 

            Contab_Functions contabFunctions = new Contab_Functions();

            short mesFiscal;
            short anoFiscal;

            if (!contabFunctions.ValidarMesCerradoEnContab(fechaAsiento, _nominaHeader.tGruposEmpleado.Cia, out mesFiscal, out anoFiscal, out resultMessage))
                return false;

            // ------------------------------------------------------------------------------------------
            // obtenemos un número negativo para el asiento contable ... 

            short numeroNegativoAsientoContable;

            if (!contabFunctions.DeterminarNumeroNegativoAsiento(fechaAsiento, _nominaHeader.tGruposEmpleado.Cia, out numeroNegativoAsientoContable, out resultMessage))
                return false; 

            // -------------------------------------------------------------------------------------------------------------
            // agregamos el encabezado del asiento contable ... 

            NominaASP.Models.Contab.Asiento asiento = new NominaASP.Models.Contab.Asiento();

            asiento.Numero = numeroNegativoAsientoContable;
            asiento.Fecha = fechaAsiento;
            asiento.Tipo = parametros.TipoAsientoDefault;
            asiento.Descripcion = descripcionAsiento;
            asiento.Mes = Convert.ToByte(fechaAsiento.Month);
            asiento.Ano = Convert.ToInt16(fechaAsiento.Year);
            asiento.MesFiscal = mesFiscal;
            asiento.AnoFiscal = anoFiscal;
            asiento.Moneda = parametros.MonedaParaElAsiento.Value;
            asiento.MonedaOriginal = parametros.MonedaParaElAsiento.Value;
            asiento.FactorDeCambio = factorCambio;
            asiento.ConvertirFlag = true;
            asiento.ProvieneDe = "Nómina";
            asiento.ProvieneDe_ID = _nominaHeader.ID;
            asiento.Cia = _nominaHeader.tGruposEmpleado.Cia;

            asiento.Ingreso = DateTime.Now;
            asiento.UltAct = asiento.Ingreso;
            asiento.Usuario = userName; 


            // -------------------------------------------------------------------------------------------------------------
            // ahora recorremos los items en la nómina seleccionada; para cada item, grabamos un registro a una lista; 

            int cuentaContableID;
            Nomina_AsientoContable_Item itemAsiento;
            List<Nomina_AsientoContable_Item> AsientoContable_List = new List<Nomina_AsientoContable_Item>(); 
            short? sumarizar; 

            foreach (tNomina nomina in _nominaHeader.tNominas)
            {
                if (!DeterminarCuentaContableAsociadaAlRubro(nomina.Rubro,
                                                             nomina.tNominaHeader.tGruposEmpleado.Cia,
                                                             nomina.tEmpleado.Departamento,
                                                             nomina.Empleado,
                                                             out cuentaContableID, 
                                                             out sumarizar))
                {
                        resultMessage = " ... el rubro '" + nomina.tMaestraRubro.Descripcion + " (" + nomina.tMaestraRubro.NombreCortoRubro + ")', " + 
                            "para el empleado '" + nomina.tEmpleado.Nombre +
                            "', no tiene una cuenta contable definida (asociada) en la maestra: 'definición de cuentas contables' " +
                            "(menú Catálogos de nómina).<br />.<br />" +
                            "Ud. debe ir a la tabla indicada y asignar (asociar) una cuenta contable a este rubro y empleado. " +
                            "Recuerde que puede, en la maestra indicada, dejar el item Empleado en blanco. " +
                            "En tal caso, la cuenta contable será definida en forma 'genérica' para ese rubro (y departamento si indica alguno). ";
                        return false;
                }

                itemAsiento = new Nomina_AsientoContable_Item();

                itemAsiento.Rubro = nomina.Rubro;
                itemAsiento.NombreRubro = nomina.tMaestraRubro.Descripcion;
                itemAsiento.AbreviaturaRubro = nomina.tMaestraRubro.NombreCortoRubro; 
                itemAsiento.Sumarizar = sumarizar;          // para sumarizar por: departamento (2); en forma global (1) (una sola partida en el asiento) ... 
                itemAsiento.Descripcion = nomina.tEmpleado.Nombre + " - " + nomina.Descripcion;
                itemAsiento.Departamento = nomina.tEmpleado.Departamento;
                itemAsiento.NombreDepartamento = nomina.tEmpleado.tDepartamento.Descripcion; 
                itemAsiento.Empleado = nomina.Empleado;
                itemAsiento.NombreEmpleado = nomina.tEmpleado.Nombre; 
                itemAsiento.CuentaContable = cuentaContableID;
                itemAsiento.Monto = nomina.Monto;
                itemAsiento.PartidaResumen = 0;                  // para que estos items se muestren primero en el asiento ... 

                AsientoContable_List.Add(itemAsiento); 
            }

            // ahora determinamos los totales a pagar por empleado; básicamente, agrupamos por empleado el resultado anterior y agregamos una 
            // partida para cada uno ... (nótese que en Parámetros se indica si se quiere o no sumarizar) 

            var query = from i in AsientoContable_List
                        group i by i.Empleado into g
                        select new { EmpleadoID = g.Key, ItemsNomina = g.ToList() };

            foreach (var g in query)
            {
                // no tenemos el item tNomina en la lista ... 

                tNomina nomina = _nominaHeader.tNominas.Where(n => n.Empleado == g.EmpleadoID).FirstOrDefault();        // para poder tener el dpto, etc. 

                itemAsiento = new Nomina_AsientoContable_Item();

                itemAsiento.Rubro = -999;                                               // total a cobrar para el empleado; no hay un rubro para esta partida ... 
                itemAsiento.NombreRubro = "Neto a pagar";
                itemAsiento.AbreviaturaRubro = "NetoAPagar"; 
                itemAsiento.Sumarizar = parametros.SumarizarPartidaAsientoContable;     // para sumarizar por: departamento (2); en forma global (1) 
                                                                                        // (una sola partida en el asiento) ... 
                itemAsiento.Descripcion = "Neto a pagar para " + nomina.tEmpleado.Nombre;
                itemAsiento.Departamento = nomina.tEmpleado.Departamento;
                itemAsiento.NombreDepartamento = nomina.tEmpleado.tDepartamento.Descripcion; 
                itemAsiento.Empleado = g.EmpleadoID;
                itemAsiento.NombreEmpleado = nomina.tEmpleado.Nombre; 
                itemAsiento.CuentaContable = parametros.CuentaContableNomina.Value;     // arriba revisamos que haya un valor ... 
                itemAsiento.Monto = g.ItemsNomina.Sum(m => m.Monto) * -1;               // nótese como estas partidas cuadran el asiento a cero ... 
                itemAsiento.PartidaResumen = 1;                                         // para que estos items (Total a pagar ..) se muestren de último en el asiento ... 

                AsientoContable_List.Add(itemAsiento); 
            }



            // -----------------------------------------------------------------------------------------------------------------
            // grabamos todos los items a otra lista, para luego poder ordenar las partidas sumarizadas, de acuerdo a un orden 
            // preestablecido, más un criterio que indica el usuario ... 

            // leemos y sumarizamos los rubros que *no* serán sumarizados ... 

            List<Nomina_PartidaAsiento_Item> partidasAsiento_List = new List<Nomina_PartidaAsiento_Item>(); 
            Nomina_PartidaAsiento_Item partidaAsientoItem; 

            foreach (Nomina_AsientoContable_Item item in AsientoContable_List.Where(x => x.Sumarizar == null).Select(x => x))
            {
                // los rubros que no se sumarizan se convierten, cada uno, en una partida del asiento ... 

                partidaAsientoItem = new Nomina_PartidaAsiento_Item();

                partidaAsientoItem.NombreRubro = item.NombreRubro;
                partidaAsientoItem.AbreviaturaRubro = item.AbreviaturaRubro;
                partidaAsientoItem.NombreDepartamento = item.NombreDepartamento;
                partidaAsientoItem.NombreEmpleado = item.NombreEmpleado;

                partidaAsientoItem.Descripcion = item.Descripcion;

                partidaAsientoItem.CuentaContable = item.CuentaContable; 
                partidaAsientoItem.Monto = item.Monto;
                partidaAsientoItem.PartidaResumen = item.PartidaResumen; 

                partidasAsiento_List.Add(partidaAsientoItem); 
            }


            // ------------------------------------------------------------------------------------------------------
            // ahora leemos las partidas que deben ser sumarizadas por departamento (2) y las agregamos como partidas del asiento 

            var queryDepto = from i in AsientoContable_List
                             where i.Sumarizar != null && i.Sumarizar == 2
                             group i by new { i.Rubro, i.Departamento } into g
                             select new { Rubro = g.Key.Rubro, Departamento = g.Key.Departamento, SumOfMonto = g.Sum(x => x.Monto), itemsNomina = g.ToList() };

            foreach (var itemDepto in queryDepto)
            {
                // cada item representa la sumarización de un rubro para un departamento ... 

                partidaAsientoItem = new Nomina_PartidaAsiento_Item();

                partidaAsientoItem.NombreRubro = itemDepto.itemsNomina.First().NombreRubro;
                partidaAsientoItem.AbreviaturaRubro = itemDepto.itemsNomina.First().AbreviaturaRubro;
                partidaAsientoItem.NombreDepartamento = itemDepto.itemsNomina.First().NombreDepartamento;
                partidaAsientoItem.NombreEmpleado = "ZZZZZZZZZZ";           // no aplica para estas partidas; intentamos que se ordenen de últimas ... 

                partidaAsientoItem.Descripcion = itemDepto.itemsNomina.First().NombreDepartamento +
                                                 " - " + itemDepto.itemsNomina.First().NombreRubro;
                partidaAsientoItem.CuentaContable = itemDepto.itemsNomina.First().CuentaContable; 
                partidaAsientoItem.Monto = itemDepto.SumOfMonto;
                partidaAsientoItem.PartidaResumen = itemDepto.itemsNomina.First().PartidaResumen;

                partidasAsiento_List.Add(partidaAsientoItem); 
            }


            // ------------------------------------------------------------------------------------------------------
            // ahora leemos las partidas que deben ser sumarizadas por rubro (1) y las agregamos como partidas del asiento 

            var queryRubro = from i in AsientoContable_List
                             where i.Sumarizar != null && i.Sumarizar == 1
                             group i by i.Rubro into g
                             select new { Rubro = g.Key, SumOfMonto = g.Sum(x => x.Monto), itemsNomina = g.ToList() };

            foreach (var itemRubro in queryRubro)
            {
                // cada item representa la sumarización de un rubro para un departamento ... 

                // cada item representa la sumarización de un rubro para un departamento ... 

                partidaAsientoItem = new Nomina_PartidaAsiento_Item();

                partidaAsientoItem.NombreRubro = itemRubro.itemsNomina.First().NombreRubro;
                partidaAsientoItem.AbreviaturaRubro = itemRubro.itemsNomina.First().AbreviaturaRubro;
                partidaAsientoItem.NombreDepartamento = "ZZZZZZZZZZ";           // no aplica para estas partidas; intentamos que se ordenen de últimas ... 
                partidaAsientoItem.NombreEmpleado = "ZZZZZZZZZZ";           // no aplica para estas partidas; intentamos que se ordenen de últimas ... 

                partidaAsientoItem.Descripcion = itemRubro.itemsNomina.First().NombreRubro;
                partidaAsientoItem.CuentaContable = itemRubro.itemsNomina.First().CuentaContable; 
                partidaAsientoItem.Monto = itemRubro.SumOfMonto;
                partidaAsientoItem.PartidaResumen = itemRubro.itemsNomina.First().PartidaResumen;

                partidasAsiento_List.Add(partidaAsientoItem); 
            }



            // --------------------------------------------------------------------------------------------------------------
            // finalmente, y dependiendo del ordenamiento que desee el usuario (por empleado o rubro), leemos la lista y 
            // agregamos las partidas al asiento ... 

            dAsiento partidaAsientoContable;
            short numeroPartida = 0;

            var query3 = partidasAsiento_List.OrderBy(p => p.PartidaResumen);

            if (orderBy == "empleado")
                query3 = query3.ThenBy(p => p.NombreEmpleado).ThenBy(p => p.NombreRubro);

            if (orderBy == "rubro")
                query3 = query3.ThenBy(p => p.NombreRubro).ThenBy(p => p.NombreEmpleado);

            foreach (Nomina_PartidaAsiento_Item partida in query3)
            {
                // los rubros que no se sumarizan se convierten, cada uno, en una partida del asiento ... 

                partidaAsientoContable = new dAsiento();

                numeroPartida = Convert.ToInt16(numeroPartida + 10);

                partidaAsientoContable.Partida = numeroPartida;
                partidaAsientoContable.CuentaContableID = partida.CuentaContable;
                partidaAsientoContable.Descripcion = partida.Descripcion;

                partidaAsientoContable.Descripcion = partidaAsientoContable.Descripcion.Length <= 70 ?
                                                     partidaAsientoContable.Descripcion :
                                                     partidaAsientoContable.Descripcion.Substring(0, 70);

                partidaAsientoContable.Referencia = partida.AbreviaturaRubro;
                partidaAsientoContable.Debe = 0;
                partidaAsientoContable.Haber = 0;

                if (partida.Monto >= 0)
                    partidaAsientoContable.Debe = partida.Monto;
                else
                    partidaAsientoContable.Haber = partida.Monto * -1;

                asiento.dAsientos.Add(partidaAsientoContable);
            }

            dbContabEntities contabContext = new dbContabEntities();

            contabContext.Asientos.Add(asiento);

            try
            {
                contabContext.SaveChanges(); 

                // una vez guardados los cambios, podemos tener acceso al ID del asiento contable; lo guardamos en el 
                // header de nómina, para poder relacionar la nómina con el asiento ... 

                _nominaHeader.AsientoContableID = asiento.NumeroAutomatico;
                _context.SaveChanges(); 
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message; 
                if (ex.InnerException != null) 
                    errorMessage += "<br /><br />" + ex.InnerException.Message;

                resultMessage = "Error: hemos obtenido un error al intentar grabar el asiento contable a la base de datos.<br />" +
                    "El mensaje de error es: " + errorMessage;
                return false; 
            }
            finally
            {
                contabContext = null; 
            }



            resultMessage = "Ok, el asiento contable para la nómina seleccionada (<span style='font-style: italic;'>" +  
                _nominaHeader.tGruposEmpleado.Compania.NombreCorto + 
                "</span>) se ha construído en forma exitosa.<br /><br />" +
                "El número asignado al asiento contables es: <span style='font-weight: bold; white-space: nowrap; '>" + numeroNegativoAsientoContable.ToString() +
                "</span> y la fecha: <span style='font-weight: bold; white-space: nowrap; '>" + fechaAsiento.ToString("dd-MMM-yyyy") + "</span>.<br /><br />" + 
                "Recuerde que Ud. debe acceder a este asiento contable desde el programa " + 
                "<span style='font-weight: bold; font-style: italic;'>Contab</span> para asignar un número contable adecuado."; 

            return true; 
        }


        private bool DeterminarCuentaContableAsociadaAlRubro(int rubro, int cia, int departamento, int empleado, out int cuentaContableID, out short? sumarizar)
        {
            cuentaContableID = 0;
            sumarizar = null; 
            tCuentasContablesPorEmpleadoYRubro cuenta; 

            // nota: al principio validamos que cada ocurrencia en la nómina tenga un rubro asociado; sin embargo, no descartamos que 
            // la cuenta contable pueda no existir esta vez ... 

            // la cuenta contable debe existir siempre por cia y rubro ... 
            // sumarizar: si se quiere sumarizar el rubro en una sola partida en el asiento (1), o por departamento (2) ... 


            // primero buscamos en la forma más específica, por empleado ... 
            cuenta = _context.tCuentasContablesPorEmpleadoYRubroes.Where(c => c.Cia == cia && c.Rubro == rubro && c.Empleado == empleado).
                                                                   FirstOrDefault();

            if (cuenta != null)
            {
                cuentaContableID = cuenta.CuentasContable.ID;
                sumarizar = cuenta.SumarizarEnUnaPartidaFlag;           // nota: sumarizar puede ser null si no se quiere sumarizar ... 
                return true;
            }


            // buscamos en forma genérica por departamento ... 
            cuenta = _context.tCuentasContablesPorEmpleadoYRubroes.Where(c => c.Cia == cia && c.Rubro == rubro && c.Departamento == departamento).
                                                                   FirstOrDefault();

            if (cuenta != null)
            {
                cuentaContableID = cuenta.CuentasContable.ID;
                sumarizar = cuenta.SumarizarEnUnaPartidaFlag;           // nota: sumarizar puede ser null si no se quiere sumarizar ... 
                return true;
            }

            // buscamos en forma genérica por rubro 
            cuenta = _context.tCuentasContablesPorEmpleadoYRubroes.Where(c => c.Cia == cia && c.Rubro == rubro && c.Departamento == null && c.Empleado == null).
                                                                   FirstOrDefault();

            if (cuenta != null)
            {
                cuentaContableID = cuenta.CuentasContable.ID;
                sumarizar = cuenta.SumarizarEnUnaPartidaFlag;           // nota: sumarizar puede ser null si no se quiere sumarizar ... 
                return true;
            }

            return false; 
        }

        private class Nomina_AsientoContable_Item
        {
            public int Rubro { get; set; }
            public string NombreRubro { get; set; }
            public string AbreviaturaRubro { get; set; }
            public short? Sumarizar { get; set; }            // 1: en una sola partida (global); 2: por departamento ... 
            public string Descripcion { get; set; }
            public int Departamento { get; set; }
            public string NombreDepartamento { get; set; }
            public int Empleado { get; set; }
            public string NombreEmpleado { get; set; }
            public int CuentaContable { get; set; }
            public decimal Monto { get; set; }
            public byte PartidaResumen { get; set; }            // nos permitirá al final ordenar y poner al final las partidas resumen (Total a pagar para ...) 
        }

        private class Nomina_PartidaAsiento_Item
        {
            public string NombreRubro { get; set; }
            public string AbreviaturaRubro { get; set; }
            public string Descripcion { get; set; }
            public string NombreDepartamento { get; set; }
            public string NombreEmpleado { get; set; }
            public int CuentaContable { get; set; }
            public decimal Monto { get; set; }
            public byte PartidaResumen { get; set; }            // nos permitirá al final ordenar y poner al final las partidas resumen (Total a pagar para ...) 
        }
    }
}