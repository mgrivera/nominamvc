<%@ Page Language="C#" MasterPageFile="~/MasterPage_Simple.master" AutoEventWireup="true" CodeBehind="AsientoContable_Consulta.aspx.cs" Inherits="NominaASP.Nomina.AsientosContables.AsientoContable_Consulta" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

<script type="text/javascript">
    function PopupWin(url, w, h) {
        ///Parameters url=page to open, w=width, h=height
        /// nótese el nombre del window (external2) para que, como se abre ya desde un subwindow, no lo haga
        /// sobre el mismo sino uno nuevo

        window.open(url, "_blank", "width=" + w + ",height=" + h + ",resizable=yes,scrollbars=yes,status=no,location=no,toolbar=no,menubar=no,top=10px,left=8px");
    }
    function RefreshPage() {
        window.document.getElementById("RebindFlagSpan").firstChild.value = "1";
        window.document.forms(0).submit();
    }
</script>

    <div style="text-align: left; padding: 0px 16px 10px 10px;">

        <asp:ValidationSummary ID="ValidationSummary1" 
                                runat="server" 
                                class="errmessage_background generalfont errmessage"
                                ShowModelStateErrors="true"
                                ForeColor="" />

        <asp:CustomValidator ID="CustomValidator1" runat="server" ErrorMessage="CustomValidator" Display="None" EnableClientScript="False" />

        <table style="width: 100%; ">
            <tr>
                <td style="width: 75%; ">
                </td>
                <td style="text-align: center; width: 25%; ">
                    <a runat="server" id="ImprimirAsientoContable_HyperLink" 
                        href="javascript:PopupWin('../../../ReportViewer.aspx?rpt=unasientocontable', 1000, 680)">
                        <img id="Img1" 
                             border="0" 
                             runat="server" 
                             alt="Click para mostrar el asiento contable asociado" 
                             src="~/Pictures/print_25x25.png" />
                    </a>
                </td>
            </tr>
            <tr>
                <td style="width: 75%; ">
                </td>
                <td style="color: #4F4F4F; text-align: center; font-size: small; width: 25%; ">
                    Reporte
                </td>
            </tr>
        </table>
    </div>

    <asp:ListView ID="AsientosContables_ListView" 
                  runat="server" 
                  DataSourceID="AsientosContables_SqlDataSource">
        
        <LayoutTemplate>
            <div style="border: 1px solid #D3D3D3; padding: 10px 10px 20px 10px; background-color: #FDFDFD; margin-right: 10px; margin-left: 10px; text-align: center; ">
                <table id="Table1" runat="server" align="center" style="width: 70%; border: 0px; ">
                    <tr id="itemPlaceholderContainer" runat="server" class="smallfont">
                        <td id="itemPlaceholder" runat="server" class="smallfont">
                        </td>
                    </tr>
                </table>
            </div>
            <div style="">
            </div>
        </LayoutTemplate>
       <ItemTemplate>

           <tr style="text-align: left; " class="smallfont">
               <td style="font-weight: bold; color: #000000; text-align: right; padding: 12px 0 0 0; ">
                   Número:
               </td>
               <td style="font-weight: bold; color: #000000; text-align: left; padding: 12px 0 0 5px; ">
                   <%# Eval("Numero") %>
               </td>
               <td colspan="4"></td>
           </tr>

           <tr  style="text-align: left; " class="smallfont">
              
               <td style="font-weight: bold; color: #000000; text-align: right; padding: 12px 0 0 0; ">
                   Fecha:
               </td>
               <td style="text-align: left; white-space: nowrap; padding: 12px 0 0 5px; ">
                   <%#Eval("Fecha", "{0:dd-MMM-yy}")%>
               </td>
               <td style="font-weight: bold; color: #000000; text-align: right; padding: 12px 0 0 0; ">
                   Moneda:
               </td>
               <td style="text-align: left; padding: 12px 0 0 5px; ">
                   <%#Eval("SimboloMoneda")%>
               </td>
               <td style="font-weight: bold; color: #000000; text-align: right; white-space: nowrap; padding: 12px 0 0 0; ">
                   Moneda original:
               </td>
               <td style="text-align: left; padding: 12px 0 0 5px; ">
                   <%#Eval("SimboloMonedaOriginal")%>
               </td>
           </tr>

           <tr style="text-align: left; " class="smallfont">
               <td style="font-weight: bold; color: #000000; text-align: right; padding: 5px 0 0 0; padding: 12px 0 0 0; ">
                   Descripción:
               </td>
               <td colspan="3" style="white-space: normal; text-align: left; padding: 12px 0 0 5px; ">
                   <%#Eval("Descripcion")%>
               </td>
               <td style="font-weight: bold; color: #000000; text-align: right; white-space: nowrap; padding: 12px 0 0 0; ">
                   Cierre anual:
               </td>
               <td style="text-align: left; padding: 12px 0 0 5px; ">
                   <%#Eval("AsientoTipoCierreAnualFlag")%>
               </td>
           </tr>

           <tr style="text-align: left; " class="smallfont">
               <td style="font-weight: bold; color: #000000; text-align: right; padding: 5px 0 0 0; padding: 12px 0 0 0; ">
                   Tipo:
               </td>
               <td style="text-align: left; padding: 12px 0 0 5px; ">
                   <%#Eval("Tipo")%>
               </td>
               <td style="font-weight: bold; color: #000000; text-align: right; white-space: nowrap; padding: 12px 0 0 0; ">
                   Proviene de:
               </td>
               <td style="text-align: left; padding: 12px 0 0 5px; ">
                   <%#Eval("ProvieneDe")%>
               </td>
               <td style="font-weight: bold; color: #000000; text-align: right; white-space: nowrap; padding: 12px 0 0 0; ">
                   Factor de cambio:
               </td>
               <td style="text-align: left; padding: 12px 0 0 5px; ">
                   <%#Eval("FactorDeCambio", "{0:N2}")%>
               </td>
           </tr>

           <tr style="text-align: center; " class="smallfont">
               <td colspan="6" style="text-align: center; width: 100%; ">
                    <table id="Table2" runat="server" align="center" style="min-width: 70%; border: 1px solid lightgray; border-collapse: collapse; margin-top: 15px; ">
                        <tr>
                            <td>
                                Ingreso
                            </td>
                            <td>
                                Ult Act
                            </td>
                            <td>
                                Usuario
                            </td>
                        </tr>

                        <tr>
                            <td>
                                <%#Eval("Ingreso", "{0:dd-MMM-yyyy}")%>
                            </td>
                            <td>
                                <%#Eval("UltAct", "{0:dd-MMM-yyyy}")%>
                            </td>
                            <td>
                                <%#Eval("Usuario", "{0:dd-MMM-yyyy}")%>
                            </td>
                        </tr>
                    </table>
               </td>
           </tr>

        </ItemTemplate>

        <EmptyDataTemplate>
            <table style="">
                <tr>
                    <td>No hay datos que mostrar en esta sección</td>
                </tr>
            </table>
        </EmptyDataTemplate>

    </asp:ListView>
    <br />
    <asp:ListView ID="Partidas_ListView" runat="server" DataSourceID="Partidas_SqlDataSource">
        <LayoutTemplate>
            <table id="Table2" runat="server">
                <tr id="Tr1" runat="server">
                    <td id="Td1" runat="server">
                        <table ID="itemPlaceholderContainer" runat="server" border="0" style="" class="smallfont" cellspacing="0">
                            <tr id="Tr2" runat="server" class="ListViewHeader_Suave smallfont" style="font-size: 9px; ">
                                <th id="Th1" runat="server" class="padded" style="text-align: center; padding-bottom:5px; padding-top:5px; ">
                                    Partida</th>
                                <th id="Th8" runat="server" class="padded" style="text-align: left; padding-bottom:5px; padding-top:5px; ">
                                    Cuenta</th>
                                <th id="Th2" runat="server" class="padded" style="text-align: left; padding-bottom:5px; padding-top:5px; ">
                                    Nombre de la cuenta</th>
                                <th id="Th3" runat="server" class="padded" style="text-align: left; padding-bottom:5px; padding-top:5px; ">
                                    Descripción</th>
                                <th  runat="server" class="padded" style="text-align: left; padding-bottom:5px; padding-top:5px; ">
                                    Referencia</th>
                                <th  runat="server" class="padded" style="text-align: left; padding-bottom:5px; padding-top:5px; ">
                                    Centro costo</th>
                                <th id="Th4" runat="server" class="padded" style="text-align: right; padding-bottom:5px; padding-top:5px; ">
                                    Debe</th>
                                <th id="Th5" runat="server" class="padded" style="text-align: right; padding-bottom:5px; padding-top:5px; ">
                                    Haber</th>
                            </tr>
                            <tr ID="itemPlaceholder" runat="server" class="smallfont">
                            </tr>
                            <tr id="footerRow" runat="server" class="ListViewFooter smallfont" style="font-size: 9px; ">
                                <th id="Th6" runat="server" class="padded" style="text-align: center; padding-bottom:5px; padding-top:5px; ">
                                    </th>
                                <th id="Th9" runat="server" class="padded" style="text-align: center; padding-bottom:5px; padding-top:5px; ">
                                    </th>
                                <th id="Th7" runat="server" class="padded" style="text-align: left; padding-bottom:5px; padding-top:5px; ">
                                    </th>
                                <th  runat="server" class="padded" style="text-align: left; padding-bottom:5px; padding-top:5px; ">
                                    </th>
                                <th  runat="server" class="padded" style="text-align: left; padding-bottom:5px; padding-top:5px; ">
                                    </th>
                                <th id="Th10" runat="server" class="padded" style="text-align: left; padding-bottom:5px; padding-top:5px; ">
                                    </th>
                                <th id="Th11" runat="server" class="padded" style="text-align: right; padding-bottom:5px; padding-top:5px; ">
                                    <asp:Label ID="SumOfDebe_Label" runat="server" Text="Label"></asp:Label></th>
                                <th id="Th12" runat="server" class="padded" style="text-align: right; padding-bottom:5px; padding-top:5px; ">
                                    <asp:Label ID="SumOfHaber_Label" runat="server" Text="Label"></asp:Label></th>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr id="Tr4" runat="server">
                    <td id="Td2" runat="server" style="text-align: left; " class="smallfont">
                        <hr />
                        <asp:DataPager ID="DataPager1" runat="server" PageSize="16">
                            <Fields>
                                                <asp:NextPreviousPagerField ButtonType="Link" FirstPageImageUrl="../../../Pictures/first_16x16.gif"
                                                    FirstPageText="&lt;&lt;" NextPageText="&gt;" PreviousPageImageUrl="../../../Pictures/left_16x16.gif"
                                                    PreviousPageText="&lt;" ShowFirstPageButton="True" ShowNextPageButton="False"
                                                    ShowPreviousPageButton="True" />
                                                <asp:NumericPagerField />
                                                <asp:NextPreviousPagerField ButtonType="Link" LastPageImageUrl="../../../Pictures/last_16x16.gif"
                                                    LastPageText="&gt;&gt;" NextPageImageUrl="../../../Pictures/right_16x16.gif"
                                                    NextPageText="&gt;" PreviousPageText="&lt;" ShowLastPageButton="True" ShowNextPageButton="True"
                                                    ShowPreviousPageButton="False" />
                                            </Fields>
                        </asp:DataPager>
                    </td>
                </tr>
            </table>
        </LayoutTemplate>
        <ItemTemplate>
            <tr style="font-size: 9px; " class="smallfont">
                <td class="padded" style="text-align: center; white-space: nowrap; ">
                    <asp:Label ID="Label2" runat="server" 
                        Text='<%# Eval("Partida") %>' />
                </td>
                <td class="padded" style="text-align: left; white-space: nowrap; ">
                    <asp:Label ID="CuentaEditadaLabel" runat="server" 
                        Text='<%# Eval("CuentaEditada") %>' />
                </td>
                <td class="padded" style="text-align: left;">
                    <asp:Label ID="NombreCuentaLabel" runat="server" 
                        Text='<%# Eval("NombreCuenta") %>' />
                </td>
                <td class="padded" style="text-align: left;">
                    <asp:Label ID="DescripcionPartidaLabel" runat="server" 
                        Text='<%# Eval("DescripcionPartida") %>' />
                </td>
                <td class="padded" style="text-align: left; white-space: nowrap; ">
                    <asp:Label ID="ReferenciaLabel" runat="server" 
                        Text='<%# Eval("Referencia") %>' />
                </td>
                <td class="padded" style="text-align: center ;">
                    <asp:Label ID="Label1" runat="server" 
                        Text='<%# Eval("NombreCentroCosto") %>' />
                </td>
                <td class="padded" style="text-align: right; white-space: nowrap; ">
                    <asp:Label ID="DebeLabel" runat="server" 
                    Text='<%#(decimal)Eval("Debe")==0 ? "" : Eval("Debe", "{0:N2}")%>' />
                </td>
                <td class="padded" style="text-align: right; white-space: nowrap; ">
                    <asp:Label ID="HaberLabel" runat="server" 
                    Text='<%#(decimal)Eval("Haber")==0 ? "" : Eval("Haber", "{0:N2}")%>' />
                </td>
            </tr>
        </ItemTemplate>
        <AlternatingItemTemplate>
            <tr style="font-size: 9px; " class="ListViewAlternatingRow smallfont">
                <td class="padded" style="text-align: center; white-space: nowrap; ">
                    <asp:Label ID="Label2" runat="server" 
                        Text='<%# Eval("Partida") %>' />
                </td>
                <td class="padded" style="text-align: left; white-space: nowrap; ">
                    <asp:Label ID="CuentaEditadaLabel" runat="server" 
                        Text='<%# Eval("CuentaEditada") %>' />
                </td>
                <td class="padded" style="text-align: left;">
                    <asp:Label ID="NombreCuentaLabel" runat="server" 
                        Text='<%# Eval("NombreCuenta") %>' />
                </td>
                <td class="padded" style="text-align: left;">
                    <asp:Label ID="DescripcionPartidaLabel" runat="server" 
                        Text='<%# Eval("DescripcionPartida") %>' />
                </td>
                <td class="padded" style="text-align: left; white-space: nowrap; ">
                    <asp:Label ID="ReferenciaLabel" runat="server" 
                        Text='<%# Eval("Referencia") %>' />
                </td>
                <td class="padded" style="text-align: center; ">
                    <asp:Label ID="Label1" runat="server" 
                        Text='<%# Eval("NombreCentroCosto") %>' />
                </td>
                <td class="padded" style="text-align: right; white-space: nowrap; ">
                    <asp:Label ID="DebeLabel" runat="server" 
                    Text='<%#(decimal)Eval("Debe")==0 ? "" : Eval("Debe", "{0:N2}")%>' />
                </td>
                <td class="padded" style="text-align: right; white-space: nowrap; ">
                    <asp:Label ID="HaberLabel" runat="server" 
                    Text='<%#(decimal)Eval("Haber")==0 ? "" : Eval("Haber", "{0:N2}")%>' />
                </td>
            </tr>
        </AlternatingItemTemplate>
        <EmptyDataTemplate>
            <table id="Table3" runat="server" style="">
                <tr>
                    <td>El asiento no existe o no tiene partidas.</td>
                </tr>
            </table>
        </EmptyDataTemplate>
      
       
    </asp:ListView>
   
    <asp:SqlDataSource ID="AsientosContables_SqlDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:dbContabConnectionString %>" SelectCommand="
                SELECT Asientos.Numero, Asientos.Fecha, Asientos.Tipo, Asientos.Descripcion, 
                Monedas.Simbolo AS SimboloMoneda, Monedas_1.Simbolo AS SimboloMonedaOriginal, Asientos.FactorDeCambio, Asientos.ProvieneDe, 
                Companias.NombreCorto AS NombreCiaContab, Case MesFiscal When 13 Then 'Si' Else 
                Case IsNull(AsientoTipoCierreAnualFlag, 0) When 1 Then 'Si' Else 'No' End End As AsientoTipoCierreAnualFlag, 
                Asientos.Ingreso, Asientos.UltAct, Asientos.Usuario
                FROM Asientos INNER JOIN Companias ON Asientos.Cia = Companias.Numero 
                INNER JOIN Monedas ON Asientos.Moneda = Monedas.Moneda 
                INNER JOIN Monedas AS Monedas_1 ON Asientos.MonedaOriginal = Monedas_1.Moneda
                Where NumeroAutomatico = @NumeroAutomatico">
        <SelectParameters>
            <asp:Parameter Name="NumeroAutomatico" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="Partidas_SqlDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:dbContabConnectionString %>" 
        SelectCommand="
            SELECT dAsientos.Partida, CuentasContables.CuentaEditada, CuentasContables.Descripcion AS NombreCuenta, 
            dAsientos.Descripcion AS DescripcionPartida, dAsientos.Referencia, dAsientos.Debe, dAsientos.Haber, 
            CentrosCosto.DescripcionCorta AS NombreCentroCosto 
            FROM dAsientos INNER JOIN CuentasContables ON dAsientos.CuentaContableID = CuentasContables.ID 
            LEFT OUTER JOIN CentrosCosto ON dAsientos.CentroCosto = CentrosCosto.CentroCosto 
            WHERE (dAsientos.NumeroAutomatico = @NumeroAutomatico)
            Order By dAsientos.Partida
            ">
        <SelectParameters>
            <asp:Parameter Name="NumeroAutomatico" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

</asp:Content>