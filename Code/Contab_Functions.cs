using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NominaASP.Models.Contab;

namespace NominaASP.Code
{
    public class Contab_Functions
    {
        // nota: estas funciones están originalmente escritas en una clase en ContabSysNetLS (Contab_Functions). Fueron tomadas y 
        // copiadas aquí; los cambios para adaptarlas fueron mínimos, por ejemplo: usar un context de Entity Framework en vez del 
        // Workspace de LightSwitch ... 

        dbContabEntities _context; 

        public Contab_Functions()
        {
            _context = new dbContabEntities(); 
        }

        public bool ValidarMesCerradoEnContab(DateTime fechaAsiento,
                                              int ciaAsiento,
                                              out short mesFiscalAsientoContable, 
                                              out short anoFiscalAsientoContable, 
                                              out string errMessage)
        {
            errMessage = "";

            mesFiscalAsientoContable = 0;
            anoFiscalAsientoContable = 0;

            // ----------------------------------------------------------------------------------------------
            // determinamos el mes y año fiscal en base al mes y año calendario en la fecha del asiento

            if (!DeterminarMesFiscalAsientoContable(fechaAsiento, ciaAsiento, out mesFiscalAsientoContable, out anoFiscalAsientoContable, out errMessage))
                return false;

            // ------------------------------------------------------------------------------------
            // leemos el mes cerrado para la cia del asiento (nota importante: el último mes cerrado corresponde a
            // mes y año *fiscal* y no calendario) 

            UltimoMesCerradoContab ultimoMesCerrado = _context.UltimoMesCerradoContabs.Where(u => u.Cia == ciaAsiento).FirstOrDefault();


            if (ultimoMesCerrado == null)
            {
                errMessage = "Error: no hemos encontrado un registro en la tabla 'Ultimo Mes Cerrado' en Contab " +
                    "que corresponda a la cia Contab indicada." +
                    "Por favor revise y corrija esta situación.";

                return false;
            }

            byte mesCerradoContab_Fiscal = ultimoMesCerrado.Mes;
            short anoCerradoContab_Fiscal = ultimoMesCerrado.Ano;

            // nótese que la validación es compleja, pues debemos tomar en cuenta la situación que crea el 
            // proceso de cierre anual ... 

            if (mesCerradoContab_Fiscal < 12)
            {
                // cuando el mes (fiscal!) cerrado en anterior a 12, la validación es muy simple 

                if ((anoFiscalAsientoContable < anoCerradoContab_Fiscal) ||
                    (anoFiscalAsientoContable == anoCerradoContab_Fiscal &&
                     mesFiscalAsientoContable <= mesCerradoContab_Fiscal))
                {
                    errMessage = "Error: la fecha del asiento contable que se desea editar o registrar corresponde " +
                        "a un mes ya cerrado en Contab. " +
                        "Ud. no puede editar o registrar un asiento cuya fecha corresponda a un mes cerrado en Contab.";

                    return false;
                }
                return true;
            }


            // en adelante en este código, el mes cerrado (fiscal) es 12 o 13 ... 

            if (mesCerradoContab_Fiscal == 13)
            {
                // en la contabilidad, para la cia del asiento, se hizo el cierre anual *más no* el traspaso de saldos 

                if (anoFiscalAsientoContable <= anoCerradoContab_Fiscal)
                {
                    errMessage = "Error: la fecha del asiento contable que se desea editar o registrar corresponde " +
                        "a un mes ya cerrado en Contab. " +
                        "Ud. no puede editar o registrar un asiento cuya fecha corresponda a un mes cerrado en Contab.";

                    return false;
                }
                return true;
            }



            if (mesCerradoContab_Fiscal == 12)
            {
                // en la contabilidad, para la cia del asiento, se hizo el cierre anual *más no* el traspaso de saldos 

                if (anoFiscalAsientoContable > anoCerradoContab_Fiscal)
                {
                    // el asiento es de un año posterior, simplemente regresamos ... 
                    return true;
                }
                else if ((anoFiscalAsientoContable < anoCerradoContab_Fiscal) ||
                        (anoFiscalAsientoContable == anoCerradoContab_Fiscal &&
                         mesFiscalAsientoContable < mesCerradoContab_Fiscal))
                {
                    // el asiento es de un mes anterior; regresamos con error 
                    errMessage = "Error: la fecha del asiento contable que se desea editar o registrar corresponde " +
                        "a un mes ya cerrado en Contab. " +
                        "Ud. no puede editar o registrar un asiento cuya fecha corresponda a un mes cerrado en Contab.";

                    return false;
                }


                // los asientos que llegan aquí son del mes fiscal 12 (y el mes cerrado es 12) 
                // impedimos continuar ... 

                errMessage = "Error: el mes fiscal cerrado ahora en Contab es 12; " +
                "bajo tales circunstancias, Ud. solo puede agregar, mediante Contab, asientos de tipo Cierre Anual.";

                return false;
            }

            return true;
        }

        public bool DeterminarMesFiscalAsientoContable(DateTime fechaAsiento,
                                                       int cia,
                                                       out short mesFiscal,
                                                       out short anoFiscal,
                                                       out string errorMessage)
        {
            // ----------------------------------------------------------------------------------------------
            // determinamos el mes y año fiscal en base al mes y año calendario del asiento 

            mesFiscal = 0;
            anoFiscal = 0;
            errorMessage = "";

            byte mesCalendario = Convert.ToByte(fechaAsiento.Month);
            short anoCalendario = Convert.ToInt16(fechaAsiento.Year);

            MesesDelAnoFiscal mesAnoFiscal = (from m in _context.MesesDelAnoFiscals
                                              where m.Mes == mesCalendario &&
                                                    m.Compania.Numero == cia
                                              select m).FirstOrDefault();

            if (mesAnoFiscal == null)
            {
                errorMessage = "No hemos encontrado un registro en la tabla de meses fiscales en Contab para " +
                    "el mes que corresponde a la fecha del asiento (" +
                    fechaAsiento.ToString("dd-MMM-yyyy") + ".\n\n" +
                    "Por favor revise y corrija esta situación.";

                return false;
            }


            mesFiscal = mesAnoFiscal.MesFiscal;
            anoFiscal = anoCalendario;

            if (mesAnoFiscal.Ano == 1)
                anoFiscal--;

            return true;
        }

        public bool DeterminarNumeroNegativoAsiento(DateTime fechaAsiento, 
                                                    int cia,
                                                    out short numeroNegativoAsiento,
                                                    out string errorMessage)
        {
            errorMessage = "";
            numeroNegativoAsiento = 0;

            int mesCalendario = fechaAsiento.Month;
            int anoCalendario = fechaAsiento.Year; 

            // nótese que creamos un nuevo context (en vez de usar la variable para esta clase 
            // que creamos para ello), pues debemos grabrar de inmediato en esta 
            // función; si lo hacemos con el dataworkspace que usamos para registrar el asiento, 
            // el asiento sería grabado también, lo cual no sería correcto 

            dbContabEntities contabContext = new dbContabEntities();

            AsientosNegativosId numeroNegativo = (from n in contabContext.AsientosNegativosIds
                                                  where n.Mes == mesCalendario &&
                                                        n.Ano == anoCalendario &&
                                                        n.Cia == cia
                                                  select n).FirstOrDefault();

            if (numeroNegativo == null)
            {
                // no existe un registro para el mes y ano en la tabla; agregamos uno 

                //numeroNegativo = context2.AsientosNegativosIds.AddNew();

                AsientosNegativosId asientosNegativosId = new AsientosNegativosId(); 

                asientosNegativosId.Mes = Convert.ToInt16(mesCalendario);
                asientosNegativosId.Ano = Convert.ToInt16(anoCalendario);
                asientosNegativosId.Cia = cia;
                asientosNegativosId.Numero = 2;

                contabContext.AsientosNegativosIds.Add(asientosNegativosId); 

                numeroNegativoAsiento = -1;
            }
            else
            {
                numeroNegativoAsiento = Convert.ToInt16(numeroNegativo.Numero * -1);
                numeroNegativo.Numero++;
            }


            // intentamos guardar los cambios 

            try
            {
                contabContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // por algún motivo, falló el acceso al db 

                errorMessage = ex.Message;

                if (ex.InnerException != null)
                    errorMessage += ex.InnerException.Message;

                return false;
            }

            return true;
        }
    }
}