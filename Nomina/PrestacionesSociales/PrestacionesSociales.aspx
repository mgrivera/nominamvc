<%@ Page Language="C#" MasterPageFile="~/Site2.master" CodeBehind="PrestacionesSociales.aspx.cs" Inherits="NominaASP.Nomina.PrestacionesSociales.PrestacionesSociales" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server" />

       <script type="text/javascript">
           function PopupWin(url, w, h) {
               ///Parameters url=page to open, w=width, h=height
               myWindow = window.open(url, "external2", "width=" + w + ",height=" + h + ",resizable=yes,scrollbars=yes,status=no,location=no,toolbar=no,menubar=no,top=10px,left=8px");
               myWindow.focus();
           }
           function RefreshPage() {
               window.document.getElementById("RebindFlagSpan").firstChild.value = "1";
               window.document.forms(0).submit();
           }
    </script>

    <%--  para refrescar la página cuando el popup se cierra (y saber que el refresh es por eso) --%>
    <span id="RebindFlagSpan">
        <asp:HiddenField ID="RebindFlagHiddenField" runat="server" Value="0" />
    </span>


    <%-- para mostrar la cia seleccionada en la parte superior derecha  --%>
    <div style="text-align: right; margin-right: 10px; font-size: small; color: #004080; font-style: italic; margin-top: 5px;">
        <span id="CiaContabSeleccionada_span" runat="server" />
    </div>

    <ajaxToolkit:UpdatePanelAnimationExtender ID="UpdatePanelAnimationExtender1" runat="server"
        TargetControlID="UpdatePanel1" Enabled="True">
        <Animations>
            <OnUpdating>
                <Parallel duration=".5">
                    <FadeOut minimumOpacity=".5" />
                </Parallel>
            </OnUpdating>
            <OnUpdated>
                <Parallel duration=".5">
                    <FadeIn minimumOpacity=".5" />
                </Parallel>
            </OnUpdated>
        </Animations>
    </ajaxToolkit:UpdatePanelAnimationExtender>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Always">
        <ContentTemplate>

        <%--  --%>
        <%-- div en la izquierda para mostrar funciones de la página --%>
        <%--  --%>
        <div class="notsosmallfont" style="width: 10%; border: 1px solid #C0C0C0; vertical-align: top;
            background-color: #F7F7F7; float: left; text-align: center; margin-top: 0px;">
            <br />
            <br />

            <asp:ImageButton ID="DeterminarPrestacionesSociales_ImageButton" 
                runat="server" 
                ImageUrl="~/Pictures/Gears_25x25.png" 
                OnClick="DeterminarPrestacionesSociales_ImageButton_Click"/>
            <br />
            <asp:LinkButton ID="DeterminarPrestacionesSociales_LinkButton" 
                runat="server" 
                OnClick="DeterminarPrestacionesSociales_LinkButton_Click">Calcular<br />prestaciones</asp:LinkButton>
        
            <hr />

            <%--<asp:ImageButton ID="ObtenerReporte_ImageButton" runat="server" ImageUrl="~/Pictures/print_35x35.png" OnPreRender="ObtenerReporte_ImageButton_PreRender"/>
            <br />
            <asp:LinkButton ID="ObtenerReporte_LinkButton" runat="server" OnPreRender="ObtenerReporte_LinkButton_PreRender">Reporte</asp:LinkButton>--%>

            <a runat="server" 
               id="Report_HtmlAnchor" 
               href="javascript:PopupWin('../../ReportViewer.aspx?rpt=consultaPrestaciones&headerID=0', 1000, 680)" >
               <img id="Img3" 
                    border="0" 
                    alt="Click para obtener un reporte que muestre los registros seleccionados" 
                    src="../../Pictures/print_35x35.png" />
            </a>
            <br />
            <a runat="server" 
               href="javascript:PopupWin('../../ReportViewer.aspx?rpt=consultaPrestaciones&headerID=0', 1000, 680)"
               id="Report2_HtmlAnchor">Imprimir</a>

            <hr />

            <a runat="server" 
               id="ObtenerArchivoTxt1_HtmlAnchor"
               href="javascript:PopupWin('PrestacionesSociales_ObtencionTxtFile.aspx', 1000, 600)">
                    <img id="Img2" 
                         runat="server"
                         border="0" 
                         alt="Para exportar los datos seleccionados a un archivo de texto" 
                         src="~/Pictures/Disk.png" />
            </a>
            <br />
            <a runat="server" 
               id="ObtenerArchivoTxt2_HtmlAnchor"
               href="javascript:PopupWin('PrestacionesSociales_ObtencionTxtFile.aspx', 1000, 600)">Exportar a<br />archivos de texto</a>

            <hr />

            <br />
        </div>

        <div style="text-align: left; float: right; width: 88%;">
           
                    <asp:ValidationSummary ID="ValidationSummary1" 
                                runat="server" 
                                class="errmessage_background generalfont errmessage"
                                ShowModelStateErrors="true"
                                ForeColor="" />

                    <asp:CustomValidator ID="CustomValidator1" runat="server" ErrorMessage="CustomValidator" Display="None" EnableClientScript="False" />

                    <ajaxToolkit:tabcontainer id="TabContainer1" runat="server" activetabindex="0" >

                        <ajaxToolkit:TabPanel HeaderText="Prestaciones sociales" runat="server" ID="TabPanel1" >
                            <ContentTemplate>

                                <asp:GridView runat="server" 
                                              ID="PrestacionesSocialesHeaders_GridView"
                                              ItemType="NominaASP.Models.PrestacionesSocialesHeader" 
                                              DataKeyNames="ID" 
                                              SelectMethod="PrestacionesSocialesHeaders_GridView_GetData"
                                              OnSelectedIndexChanged="PrestacionesSocialesHeaders_GridView_SelectedIndexChanged"
                                              OnPageIndexChanging="PrestacionesSocialesHeaders_GridView_PageIndexChanging"
                                              AutoGenerateColumns="False" 
                                              AllowPaging="True" 
                                              PageSize="15"  
                                              CssClass="Grid">
                                    <Columns>

                                        <asp:buttonfield CommandName="Select" ImageUrl="~/Pictures/arrow_right_13x11.png" ButtonType="Image" Text="Select" />

                                        <asp:TemplateField HeaderText="Número">
                                            <ItemTemplate>
                                                <asp:Label ID="Label1" runat="server" Text='<%# Item.ID %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Desde">
                                            <ItemTemplate>
                                                <asp:Label ID="Label2" runat="server" Text='<%# Item.Desde != null ? Item.Desde.Value.ToString("d-MMM-yyyy") : "" %>' />
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Hasta">
                                            <ItemTemplate>
                                                <asp:Label ID="Label3" runat="server" Text='<%# Item.Hasta != null ? Item.Hasta.Value.ToString("d-MMM-yyyy") : "" %>' />
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Mes">
                                            <ItemTemplate>
                                                <asp:Label ID="Label4" runat="server" Text='<%# NombreMes(Item.Mes) %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Left" />
                                            <ItemStyle HorizontalAlign="Left" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Año">
                                            <ItemTemplate>
                                                <asp:Label ID="Label5" runat="server" Text='<%# Item.Ano %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Cant días<br />utilidades">
                                            <ItemTemplate>
                                                <asp:Label ID="Label6" runat="server" Text='<%# Item.CantDiasUtilidades %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Agregar monto<br />cesta tickets">
                                            <ItemTemplate>
                                                <asp:Label ID="Label8" runat="server" Text='<%# Item.AgregarMontoCestaTickets ? "Si" : "No" %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Cia contab">
                                            <ItemTemplate>
                                                <asp:Label ID="Label7" runat="server" Text='<%# Item.Compania.Nombre %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Left" />
                                            <ItemStyle HorizontalAlign="Left" />
                                        </asp:TemplateField>

                                    </Columns>

                                    <EmptyDataTemplate>
                                        <br />
                                        No hay registros que mostrar; probablemente no se han establecido criterios de ejecución y seleccionado registros (movimientos) ...
                                    </EmptyDataTemplate> 

                                    <SelectedRowStyle backcolor="LightCyan" forecolor="DarkBlue" font-bold="True" />  
                                    <AlternatingRowStyle CssClass="GridAltItem" />
				                    <RowStyle CssClass="GridItem" />
				                    <HeaderStyle CssClass="GridHeader" />
				                    <PagerStyle CssClass="GridPager" />
                                    <EmptyDataRowStyle CssClass="GridEmptyData" />

                                </asp:GridView>

                            </ContentTemplate>
                        </ajaxToolkit:TabPanel>

                        <ajaxToolkit:TabPanel HeaderText="Detalles" runat="server" ID="TabPanel2" >
                            <ContentTemplate>

                                <div style="overflow:scroll; ">

                                    <asp:ListView ID="Detalles_ListView" 
                                                  runat="server" 
                                                  DataKeyNames="ID" 
                                                  ItemType="NominaASP.Models.PrestacionesSociale" 
                                                  SelectMethod="Detalles_ListView_GetData" 
                                                  onpagepropertieschanged="Detalles_ListView_PagePropertiesChanged"
                                                  onselectedindexchanged="Detalles_ListView_SelectedIndexChanged">

                                        <LayoutTemplate>
                                            <table id="Table1" runat="server">
                                                <tr id="Tr1" runat="server">
                                                    <td id="Td1" runat="server">
                                                        <table id="itemPlaceholderContainer" runat="server" border="0" 
                                                               style="border: 1px solid #E6E6E6; font-size: xx-small; "
                                                               cellspacing="0" rules="none">

                                                            <tr id="Tr4" runat="server" style="" class="ListViewHeader_DarkBlue" >
                                                                <th colspan="7" />
                                                                <th id="Th23" runat="server" colspan="2" class="padded" 
                                                                    style="text-align: center; border-left-style: solid; border-left-width: 1px; border-left-color: #808080; 
                                                                           border-bottom-style: solid; border-bottom-width: 1px; border-bottom-color: #808080;">
                                                                    1er. Mes
                                                                </th>
                                                                 <th id="Th24" runat="server" colspan="2" class="padded" 
                                                                     style="text-align: center; border-left-style: solid; border-left-width: 1px; border-left-color: #808080; 
                                                                           border-bottom-style: solid; border-bottom-width: 1px; border-bottom-color: #808080;">
                                                                     Salario prestaciones
                                                                </th>
                                                                <th id="Th25" runat="server" colspan="3" class="padded" 
                                                                    style="text-align: center; border-left-style: solid; border-left-width: 1px; border-left-color: #808080; 
                                                                           border-bottom-style: solid; border-bottom-width: 1px; border-bottom-color: #808080;">
                                                                    Vacaciones
                                                                </th>
                                                                <th id="Th26" runat="server" colspan="3" class="padded" 
                                                                    style="text-align: center; border-left-style: solid; border-left-width: 1px; border-left-color: #808080; 
                                                                           border-bottom-style: solid; border-bottom-width: 1px; border-bottom-color: #808080;">
                                                                    Utilidades
                                                                </th>
                                                                <th style="border-left-style: solid; border-left-width: 1px; border-left-color: #808080;"/>
                                                                <th id="Th28" runat="server" colspan="2" class="padded" 
                                                                    style="text-align: center; border-left-style: solid; border-left-width: 1px; border-left-color: #808080; 
                                                                           border-bottom-style: solid; border-bottom-width: 1px; border-bottom-color: #808080;">
                                                                    Prestaciones
                                                                </th>
                                                                <th id="Th29" runat="server" colspan="3" class="padded" 
                                                                    style="text-align: center; border-left-style: solid; border-left-width: 1px; border-left-color: #808080; 
                                                                           border-bottom-style: solid; border-bottom-width: 1px; border-bottom-color: #808080;">
                                                                    Días adicionales (año cumplido)
                                                                </th>
                                                                <th style="border-left-style: solid; border-left-width: 1px; border-left-color: #808080;"/>
                                                            </tr>


                                                            <tr id="Tr2" runat="server" style="" class="ListViewHeader_DarkBlue">
                                                                <th />
                                                                <th id="Th1" runat="server" class="padded" style="text-align: left; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Empleado
                                                                </th>
                                                                <th id="Th12" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Fecha<br />ingreso
                                                                </th>
                                                                 <th id="Th2" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                     Años<br />servicio
                                                                </th>
                                                                <th id="Th3" runat="server" class="padded" style="text-align: center; padding-bottom: 8px; padding-top: 0;">
                                                                    Años<br />ley
                                                                </th>



                                                                <th id="Th22" runat="server" class="padded" style="text-align: right; padding-bottom: 8px; padding-top: 0;">
                                                                    Salario<br />(últ mes período)
                                                                </th>
                                                                <th id="Th27" runat="server" class="padded" style="text-align: right; padding-bottom: 8px; padding-top: 0;">
                                                                    Monto<br />cesta tickets
                                                                </th>





                                                                <th id="Th4" runat="server" class="padded" style="text-align: center; white-space: nowrap;
                                                                    padding-bottom: 8px; padding-top: 0px;">
                                                                    1er.<br />mes
                                                                </th>
                                                                <th id="Th5" runat="server" class="padded" style="text-align: center; white-space: nowrap;
                                                                    padding-bottom: 8px; padding-top: 0px;">
                                                                    Cant<br />días
                                                                </th>
                                                                <th id="Th6" runat="server" class="padded" style="text-align: right; white-space: nowrap;
                                                                    padding-bottom: 8px; padding-top: 0px;">
                                                                    Mes
                                                                </th>
                                                                <th id="Th7" runat="server" class="padded" style="text-align: right; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Diario
                                                                </th>
                                                                <th id="Th8" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Días
                                                                </th>
                                                                <th id="Th9" runat="server" class="padded" style="text-align: right; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Bono
                                                                </th>
                                                                 <th id="Th10" runat="server" class="padded" style="text-align: right; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Diario
                                                                </th>
                                                                 <th id="Th11" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Días
                                                                </th>
                                                                <th id="Th13" runat="server" class="padded" style="text-align: right; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Monto
                                                                </th>
                                                                <th id="Th14" runat="server" class="padded" style="text-align: right; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Diario
                                                                </th>
                                                                <th id="Th15" runat="server" class="padded" style="text-align: right; white-space: nowrap;
                                                                    padding-bottom: 8px; padding-top: 0px;">
                                                                    Salario<br />diario<br />(integral)
                                                                </th>
                                                                <th id="Th16" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Días
                                                                </th>
                                                                <th id="Th17" runat="server" class="padded" style="text-align: right; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Monto
                                                                </th>
                                                                <th id="Th21" runat="server" class="padded" style="text-align: left; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                </th>
                                                                <th id="Th18" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Días
                                                                </th>
                                                                <th id="Th19" runat="server" class="padded" style="text-align: right; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Monto
                                                                </th>
                                                                <th id="Th20" runat="server" class="padded" style="text-align: right; padding-bottom: 8px;
                                                                    padding-top: 0px;">
                                                                    Total
                                                                </th>

                                                            </tr>
                                                            <tr id="itemPlaceholder" runat="server" />
                                                        </table>
                                                    </td>
                                                </tr>

                                                <tr id="Tr3" runat="server">
                                                    <td id="Td2" runat="server" style="">
                                                        <asp:DataPager ID="Detalles_DataPager" runat="server">
                                                            <Fields>
                                                                <asp:NextPreviousPagerField ButtonType="Image" FirstPageText="&lt;&lt;" NextPageText="&gt;"
                                                                    PreviousPageImageUrl="~/Pictures/ListView_Buttons/PgPrev.gif" PreviousPageText="&lt;"
                                                                    ShowFirstPageButton="True" ShowNextPageButton="False" ShowPreviousPageButton="True"
                                                                    FirstPageImageUrl="~/Pictures/ListView_Buttons/PgFirst.gif" />
                                                                <asp:NumericPagerField />
                                                                <asp:NextPreviousPagerField ButtonType="Image" LastPageImageUrl="~/Pictures/ListView_Buttons/PgLast.gif"
                                                                    LastPageText="&gt;&gt;" NextPageImageUrl="~/Pictures/ListView_Buttons/PgNext.gif"
                                                                    NextPageText="&gt;" PreviousPageText="&lt;" ShowLastPageButton="True" ShowNextPageButton="True"
                                                                    ShowPreviousPageButton="False" />
                                                            </Fields>
                                                        </asp:DataPager>
                                                    </td>
                                                </tr>

                                            </table>
                                        </LayoutTemplate>

                                        <EmptyDataTemplate>
                                            <table id="Table2" runat="server" style="">
                                                <tr>
                                                    <td>
                                                        No existen registros que mostrar ... 
                                                    </td>
                                                </tr>
                                            </table>
                                        </EmptyDataTemplate>

                                        <ItemTemplate>
                                            <tr class="ListViewItem_DarkBlue">
                                                <td>
                                                    <asp:ImageButton ID="SelectItems_Button" runat="server" AlternateText=">" CommandName="Select"
                                                                     ImageUrl="~/Pictures/arrow_right_13x11.png" 
                                                                     ToolTip="Click para seleccionar la linea en la lista" />
                                                </td>
                                                <td class="padded" style="text-align: Left; white-space: nowrap; ">
                                                    <asp:Label ID="FechaRecepcionLabel" runat="server" Text='<%# Item.tEmpleado.Nombre %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space: nowrap; ">
                                                    <asp:Label ID="Label2" runat="server" Text='<%# Item.FechaIngreso.ToString("d-MMM-yyyy") %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="TotalFacturaLabel" runat="server" Text='<%# Item.AnosServicio %>' />
                                                </td>
                                                <td class="padded" style="text-align: center;">
                                                    <asp:Label ID="IvaLabel" runat="server" Text='<%# Item.AnosServicioPrestaciones %>' />
                                                </td>

                                                <td class="padded" style="text-align: right;">
                                                    <asp:Label ID="Label21" runat="server" Text='<%# Item.SueldoBasico.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right;">
                                                    <asp:Label ID="Label24" runat="server" Text='<%# Item.MontoCestaTickets.HasValue ? Item.MontoCestaTickets.Value.ToString("N2") : "" %>' />
                                                </td>

                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:CheckBox ID="PrimerMesPrestacionesFlag_CheckBox" runat="server" Checked='<%# Item.PrimerMesPrestacionesFlag %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="AnticipoLabel" runat="server" Text='<%# Item.CantidadDiasTrabajadosPrimerMes %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="SaldoLabel" runat="server" Text='<%# Item.SueldoBasicoPrestaciones.HasValue ? Item.SueldoBasicoPrestaciones.Value.ToString("N2") : "" %>' />
                                                </td>
                                                <td class="padded" style="text-align: right;">
                                                    <asp:Label ID="NombreEstadoLabel" runat="server" Text='<%# Item.SueldoBasicoDiario.ToString("N2") %>' />
                                                </td>
                                                    <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label1" runat="server" Text='<%# Item.DiasVacaciones %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label3" runat="server" Text='<%# Item.BonoVacacional.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label6" runat="server" Text='<%# Item.BonoVacacionalDiario.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label13" runat="server" Text='<%# Item.PrestacionesSocialesHeader.CantDiasUtilidades %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label7" runat="server" Text='<%# Item.Utilidades.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label8" runat="server" Text='<%# Item.UtilidadesDiarias.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label14" runat="server" Text='<%# Item.SueldoDiarioAumentado.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label15" runat="server" Text='<%# Item.DiasPrestaciones %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label16" runat="server" Text='<%# Item.MontoPrestaciones.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:CheckBox ID="AnoCumplidoFlag_CheckBox" runat="server" Checked='<%# Item.AnoCumplidoFlag %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label18" runat="server" Text='<%# Item.CantidadDiasAdicionales %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label19" runat="server" Text='<%# Item.MontoPrestacionesDiasAdicionales != null ? Item.MontoPrestacionesDiasAdicionales.Value.ToString("N2") : "" %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label17" runat="server" Text='<%# Item.MontoPrestacionesDiasAdicionales == null ? Item.MontoPrestaciones.ToString("N2") : (Item.MontoPrestacionesDiasAdicionales.Value + Item.MontoPrestaciones).ToString("N2") %>' />
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                    
                                        <AlternatingItemTemplate>
                                            <tr class="ListViewAltItem_DarkBlue">
                                                <td>
                                                    <asp:ImageButton ID="SelectItems_Button" runat="server" AlternateText=">" CommandName="Select"
                                                                     ImageUrl="~/Pictures/arrow_right_13x11.png" 
                                                                     ToolTip="Click para seleccionar la linea en la lista" />
                                                </td>
                                                <td class="padded" style="text-align: Left; white-space: nowrap; ">
                                                    <asp:Label ID="FechaRecepcionLabel" runat="server" Text='<%# Item.tEmpleado.Nombre %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space: nowrap; ">
                                                    <asp:Label ID="Label2" runat="server" Text='<%# Item.FechaIngreso.ToString("d-MMM-yyyy") %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="TotalFacturaLabel" runat="server" Text='<%# Item.AnosServicio %>' />
                                                </td>
                                                <td class="padded" style="text-align: center;">
                                                    <asp:Label ID="IvaLabel" runat="server" Text='<%# Item.AnosServicioPrestaciones %>' />
                                                </td>
                                                <td class="padded" style="text-align: right;">
                                                    <asp:Label ID="Label21" runat="server" Text='<%# Item.SueldoBasico.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right;">
                                                    <asp:Label ID="Label24" runat="server" Text='<%# Item.MontoCestaTickets.HasValue ? Item.MontoCestaTickets.Value.ToString("N2") : "" %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:CheckBox ID="PrimerMesPrestacionesFlag_CheckBox" runat="server" Checked='<%# Item.PrimerMesPrestacionesFlag %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="AnticipoLabel" runat="server" Text='<%# Item.CantidadDiasTrabajadosPrimerMes %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="SaldoLabel" runat="server" Text='<%# Item.SueldoBasicoPrestaciones.HasValue ? Item.SueldoBasicoPrestaciones.Value.ToString("N2") : "" %>' />
                                                </td>
                                                <td class="padded" style="text-align: right;">
                                                    <asp:Label ID="NombreEstadoLabel" runat="server" Text='<%# Item.SueldoBasicoDiario.ToString("N2") %>' />
                                                </td>
                                                    <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label1" runat="server" Text='<%# Item.DiasVacaciones %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label3" runat="server" Text='<%# Item.BonoVacacional.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label6" runat="server" Text='<%# Item.BonoVacacionalDiario.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label13" runat="server" Text='<%# Item.PrestacionesSocialesHeader.CantDiasUtilidades %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label7" runat="server" Text='<%# Item.Utilidades.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label8" runat="server" Text='<%# Item.UtilidadesDiarias.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label14" runat="server" Text='<%# Item.SueldoDiarioAumentado.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label15" runat="server" Text='<%# Item.DiasPrestaciones %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label16" runat="server" Text='<%# Item.MontoPrestaciones.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:CheckBox ID="AnoCumplidoFlag_CheckBox" runat="server" Checked='<%# Item.AnoCumplidoFlag %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label18" runat="server" Text='<%# Item.CantidadDiasAdicionales %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label19" runat="server" Text='<%# Item.MontoPrestacionesDiasAdicionales != null ? Item.MontoPrestacionesDiasAdicionales.Value.ToString("N2") : "" %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label17" runat="server" Text='<%# Item.MontoPrestacionesDiasAdicionales == null ? Item.MontoPrestaciones.ToString("N2") : (Item.MontoPrestacionesDiasAdicionales.Value + Item.MontoPrestaciones).ToString("N2") %>' />
                                                </td>
                                            </tr>
                                        </AlternatingItemTemplate>

                                        <SelectedItemTemplate>
                                            <tr class="ListView_DarkBlue_SelectedItem">
                                                <td />
                                                <td class="padded" style="text-align: Left; white-space: nowrap; ">
                                                    <asp:Label ID="FechaRecepcionLabel" runat="server" Text='<%# Item.tEmpleado.Nombre %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space: nowrap; ">
                                                    <asp:Label ID="Label2" runat="server" Text='<%# Item.FechaIngreso.ToString("d-MMM-yyyy") %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="TotalFacturaLabel" runat="server" Text='<%# Item.AnosServicio %>' />
                                                </td>
                                                <td class="padded" style="text-align: center;">
                                                    <asp:Label ID="IvaLabel" runat="server" Text='<%# Item.AnosServicioPrestaciones %>' />
                                                </td>
                                                <td class="padded" style="text-align: right;">
                                                    <asp:Label ID="Label21" runat="server" Text='<%# Item.SueldoBasico.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right;">
                                                    <asp:Label ID="Label24" runat="server" Text='<%# Item.MontoCestaTickets.HasValue ? Item.MontoCestaTickets.Value.ToString("N2") : "" %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:CheckBox ID="PrimerMesPrestacionesFlag_CheckBox" runat="server" Checked='<%# Item.PrimerMesPrestacionesFlag %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="AnticipoLabel" runat="server" Text='<%# Item.CantidadDiasTrabajadosPrimerMes %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="SaldoLabel" runat="server" Text='<%# Item.SueldoBasicoPrestaciones.HasValue ? Item.SueldoBasicoPrestaciones.Value.ToString("N2") : "" %>' />
                                                </td>
                                                <td class="padded" style="text-align: right;">
                                                    <asp:Label ID="NombreEstadoLabel" runat="server" Text='<%# Item.SueldoBasicoDiario.ToString("N2") %>' />
                                                </td>
                                                    <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label1" runat="server" Text='<%# Item.DiasVacaciones %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label3" runat="server" Text='<%# Item.BonoVacacional.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label6" runat="server" Text='<%# Item.BonoVacacionalDiario.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label13" runat="server" Text='<%# Item.PrestacionesSocialesHeader.CantDiasUtilidades %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label7" runat="server" Text='<%# Item.Utilidades.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label8" runat="server" Text='<%# Item.UtilidadesDiarias.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label14" runat="server" Text='<%# Item.SueldoDiarioAumentado.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label15" runat="server" Text='<%# Item.DiasPrestaciones %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label16" runat="server" Text='<%# Item.MontoPrestaciones.ToString("N2") %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:CheckBox ID="AnoCumplidoFlag_CheckBox" runat="server" Checked='<%# Item.AnoCumplidoFlag %>' />
                                                </td>
                                                <td class="padded" style="text-align: center; white-space:nowrap; ">
                                                    <asp:Label ID="Label18" runat="server" Text='<%# Item.CantidadDiasAdicionales %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label19" runat="server" Text='<%# Item.MontoPrestacionesDiasAdicionales != null ? Item.MontoPrestacionesDiasAdicionales.Value.ToString("N2") : "" %>' />
                                                </td>
                                                <td class="padded" style="text-align: right; white-space:nowrap; ">
                                                    <asp:Label ID="Label17" runat="server" Text='<%# Item.MontoPrestacionesDiasAdicionales == null ? Item.MontoPrestaciones.ToString("N2") : (Item.MontoPrestacionesDiasAdicionales.Value + Item.MontoPrestaciones).ToString("N2") %>' />
                                                </td>
                                            </tr>
                                        </SelectedItemTemplate>
                                    
                                    </asp:ListView>

                                </div>

                            </ContentTemplate>
                        </ajaxToolkit:TabPanel>

                        <ajaxToolkit:TabPanel HeaderText="Agregar/Editar un registro" runat="server" ID="TabPanel3" >
                            <ContentTemplate>

                                <asp:FormView runat="server" 
                                              ID="PrestacionesSocialesHeaders_FormView"
                                              ItemType="NominaASP.Models.PrestacionesSocialesHeader" 
                                              SelectMethod="PrestacionesSocialesHeaders_FormView_GetItem"
                                              UpdateMethod="PrestacionesSocialesHeaders_FormView_UpdateItem"
                                              InsertMethod="PrestacionesSocialesHeaders_FormView_InsertItem"
                                              DeleteMethod="PrestacionesSocialesHeaders_FormView_DeleteItem"
                                              OnItemCreated="PrestacionesSocialesHeaders_FormView_ItemCreated"

                                              DataKeyNames="ID"

                                              OnItemInserted="PrestacionesSocialesHeaders_FormView_ItemInserted"
                                              OnItemDeleted="PrestacionesSocialesHeaders_FormView_ItemDeleted"
                                              OnItemUpdated="PrestacionesSocialesHeaders_FormView_ItemUpdated"

                                              BackColor="White"  
                                              CellPadding="3" >

                                    <ItemTemplate>
                                        <table cellspacing="10px" style="border: 1px solid #808080">

                                            <tr>
                                                <td style="font-weight: bold">Número: </td>
                                                <td>
                                                    <asp:Label ID="Label9" runat="server" Text='<%# Item.ID.ToString() %>'></asp:Label>
                                                </td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                            </tr>

                                            <tr>
                                                <td style="font-weight: bold">Desde: </td>
                                                <td>
                                                    <asp:Label ID="Label1" runat="server" Text='<%# Item.Desde == null ? "" : Item.Desde.Value.ToString("d-MMM-yyyy") %>'></asp:Label>
                                                </td>
                                                <td>&nbsp;</td>
                                                <td style="font-weight: bold">Hasta: </td>
                                                <td>
                                                    <asp:Label ID="Label10" runat="server" Text='<%# Item.Hasta == null ? "" : Item.Hasta.Value.ToString("d-MMM-yyyy") %>'></asp:Label>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td style="font-weight: bold">Mes: </td>
                                                <td>
                                                    <asp:Label ID="Label11" runat="server" Text='<%# NombreMes(Item.Mes) %>'></asp:Label>
                                                </td>
                                                <td>&nbsp;</td>
                                                <td style="font-weight: bold">Año: </td>
                                                <td>
                                                    <asp:Label ID="Label12" runat="server" Text='<%# Item.Ano.ToString() %>'></asp:Label>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td style="font-weight: bold">Cant días<br />utilidades: </td>
                                                <td>
                                                    <asp:Label ID="Label20" runat="server" Text='<%# Item.CantDiasUtilidades %>'></asp:Label>
                                                </td>
                                                <td>&nbsp;</td>
                                                <td style="font-weight: bold">Agregar monto<br />cesta tickets: </td>
                                                <td>
                                                    <asp:CheckBox ID="agregarMontoCT_checkBox" 
                                                                  runat="server" 
                                                                  Checked="<%# Item.AgregarMontoCestaTickets %>" />
                                                </td>
                                            </tr>

                                            <tr>
                                                <td></td>
                                                <td></td>
                                                <td>&nbsp;</td>
                                                <td style="font-weight: bold">Cia Contab: </td>
                                                <td>
                                                    <asp:Label ID="Label23" runat="server" Text='<%# Item.Compania.NombreCorto %>'></asp:Label>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                            </tr>

                                            <tr>
                                                <td colspan="5" style="text-align: right; ">
                                                    <asp:LinkButton id="Edit_LinkButton" Text="Editar" CommandName="Edit" Runat="server"  />
                                                    &nbsp;
                                                    <asp:LinkButton id="Delete_LinkButton" Text="Eliminar" CommandName="Delete" 
                                                                    OnClientClick="return confirm('Desea eliminar este registro?');" 
                                                                    runat="server" />
                                                    &nbsp;
                                                    <asp:LinkButton id="New_LinkButton" Text="Nuevo" CommandName="New" Runat="server" />
                                                    &nbsp;
                                                </td>
                                            </tr>
                                        </table>
                                    </ItemTemplate>

                                    <EditItemTemplate>
                                        <table cellspacing="10px">

                                            <tr>
                                                <td style="font-weight: bold">Número: </td>
                                                <td>
                                                    <asp:Label ID="Label9" runat="server" Text='<%# Item.ID.ToString() %>'></asp:Label>
                                                </td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                            </tr>

                                            <tr>
                                                <td>Desde: </td>
                                                <td>
                                                    <asp:TextBox ID="Desde_TextBox" runat="server" Text='<%# Bind("Desde", "{0:dd/MM/yyyy}") %>' />
                                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender2" runat="server" TargetControlID="Desde_TextBox" Format="d-MM-yyyy" />
                                                </td>
                                                <td>&nbsp;</td>
                                                <td>Hasta: </td>
                                                <td>
                                                    <asp:TextBox ID="Hasta_TextBox" runat="server" Text='<%# Bind("Hasta", "{0:dd/MM/yyyy}") %>' />
                                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender3" runat="server" TargetControlID="Hasta_TextBox" Format="d-MM-yyyy" />
                                                </td>
                                            </tr>

                                           <tr>
                                                <td style="font-weight: bold">Mes: </td>
                                                <td>
                                                    <asp:Label ID="Label11" runat="server" Text='<%# NombreMes(Item.Mes) %>'></asp:Label>
                                                </td>
                                                <td>&nbsp;</td>
                                                <td style="font-weight: bold">Año: </td>
                                                <td>
                                                    <asp:Label ID="Label12" runat="server" Text='<%# Item.Ano.ToString() %>'></asp:Label>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td style="font-weight: bold">Cant días<br />utilidades: </td>
                                                <td>
                                                    <asp:TextBox ID="CantDiasUtilidades_TextBox" runat="server" Width="50%" Text='<%# BindItem.CantDiasUtilidades %>' />
                                                </td>
                                                <td>&nbsp;</td>
                                                <td style="font-weight: bold">Agregar monto<br />cesta tickets: </td>
                                                <td>
                                                    <asp:CheckBox ID="agregarMontoCT_checkBox" 
                                                                  runat="server" 
                                                                  Checked="<%# BindItem.AgregarMontoCestaTickets %>" />
                                                </td>
                                            </tr>

                                            <tr>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td style="font-weight: bold">Cia Contab: </td>
                                                <td>
                                                    <asp:Label ID="Label22" runat="server" Text='<%# Item.Compania.NombreCorto %>'></asp:Label>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                            </tr>

                                            <tr>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td style="text-align: right; margin-right: 15px; " >
                                                    <asp:LinkButton ID="UpdateButton" runat="server" Text="Modificar" CommandName="Update" />
                                                    &nbsp;&nbsp;
                                                    <asp:LinkButton ID="CancelButton" runat="server" Text="Cancelar" CausesValidation="False" OnClick="cancelButton_Click" />
                                                    &nbsp;&nbsp;
                                                </td>
                                            </tr>
                                        </table>

                                    </EditItemTemplate>

                                    <InsertItemTemplate>

                                        <table cellspacing="10px">

                                            <tr>
                                                <td>Desde: </td>
                                                <td>
                                                    <asp:TextBox ID="Desde_TextBox" runat="server" Text='<%# BindItem.Desde %>' />
                                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender2" runat="server" TargetControlID="Desde_TextBox" Format="d-MM-yyyy" />
                                                </td>
                                                <td>&nbsp;</td>
                                                <td>Hasta: </td>
                                                <td>
                                                    <asp:TextBox ID="Hasta_TextBox" runat="server" Text='<%# BindItem.Hasta %>' />
                                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender3" runat="server" TargetControlID="Hasta_TextBox" Format="d-MM-yyyy" />
                                                </td>
                                            </tr>

                                            <tr>
                                                <td style="font-weight: bold">Cant días<br />utilidades: </td>
                                                <td>
                                                    <asp:TextBox ID="CantDiasUtilidades_TextBox" runat="server" Width="50%" Text='<%# BindItem.CantDiasUtilidades %>' />
                                                </td>
                                                <td>&nbsp;</td>
                                                <td style="font-weight: bold">Agregar monto<br />cesta tickets: </td>
                                                <td>
                                                    <asp:CheckBox ID="agregarMontoCT_checkBox" 
                                                                  runat="server" 
                                                                  Checked="<%# BindItem.AgregarMontoCestaTickets %>" />
                                                </td>
                                            </tr>

                                            <tr>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                            </tr>

                                            <tr>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td>&nbsp;</td>
                                                <td style="text-align: right; margin-right: 15px; " >
                                                    <asp:LinkButton ID="InsertButton" runat="server" Text="Grabar" CommandName="Insert" />
                                                    &nbsp;&nbsp;
                                                    <asp:LinkButton ID="CancelButton" runat="server" Text="Cancelar" CausesValidation="False" OnClick="cancelButton_Click" />
                                                    &nbsp;&nbsp;
                                                </td>
                                            </tr>
                                        </table>
                                    </InsertItemTemplate>

                                    <FooterStyle BackColor="#B5C7DE" ForeColor="#4A3C8C" />
                                    <HeaderStyle BackColor="#4A3C8C" Font-Bold="True" ForeColor="#F7F7F7" />
                                    <EditRowStyle BackColor="#738A9C" Font-Bold="True" ForeColor="#F7F7F7" />
                                    <PagerStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" HorizontalAlign="Right" />
                                    <RowStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" />

                                    <EmptyDataTemplate>
                                        <h5>Haga un click en Nuevo para agregar un registro ...</h5>
                                        <asp:LinkButton id="New_LinkButton" Text="Nuevo" CommandName="New" Runat="server" />
                                    </EmptyDataTemplate>

                                </asp:FormView>

                            </ContentTemplate>
                        </ajaxToolkit:TabPanel>

                    </ajaxToolkit:tabcontainer>

                    <%--para mostrar un diálogo que permita al usuario continuar/cancelar--%> 
                    <asp:Panel ID="pnlPopup" runat="server" CssClass="modalpopup" style="display:none">
                        <div class="popup_container" style="max-width: 500px; ">
                            <div class="popup_form_header" style="overflow: hidden; ">
                                <div id="ModalPopupTitle_div" style="width: 85%; margin-top: 5px;  float: left;  ">
                                    <span runat="server" id="ModalPopupTitle_span" style="font-weight: bold; "/> 
                                </div>
                                <div style="width: 15%; float: right; ">
                                    <asp:ImageButton ID="ImageButton1" runat="server" OnClientClick="$find('popup').hide(); return false;" ImageUrl="~/Pictures/PopupCloseButton.png" />
                                </div>
                            </div>
                            <div class="inner_container">
                                <div class="popup_form_content">
                                    <span runat="server" id="ModalPopupBody_span" /> 
                                </div>
                                <div class="popup_form_footer">
                                    <asp:Button ID="btnOk" runat="server" Text="Continuar" OnClick="btnOk_Click" />
                                    <asp:Button ID="btnCancel" runat="server" Text="Cancelar" OnClientClick="$find('popup').hide(); return false;" Width="80px"/>
                                </div>   
                            </div>                                          
                        </div>
                    </asp:Panel>

                    <asp:HiddenField ID="HiddenField1" runat="server" />
                    <ajaxToolkit:ModalPopupExtender ID="ModalPopupExtender1" 
                                                    runat="server" 
                                                    BehaviorID="popup" 
                                                    TargetControlID="HiddenField1" 
                                                    PopupControlID="pnlPopup" 
                                                    BackgroundCssClass="modalBackground" 
                                                    PopupDragHandleControlID="ModalPopupTitle_div" 
                                                    Drag="True" />

            </div>
            
        </ContentTemplate>
    </asp:UpdatePanel>
    

    <asp:SqlDataSource ID="Empleados_SqlDataSource" 
                       runat="server" 
                       ConnectionString="<%$ ConnectionStrings:dbContabConnectionString %>" 
                       SelectCommand="SELECT Empleado, Nombre FROM tEmpleados Where Empleado In (Select Empleado From tNomina Where HeaderID = @HeaderID) ORDER BY Nombre">
        <SelectParameters>
            <asp:Parameter Name="HeaderID" Type="Int32" DefaultValue="-999" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="Rubros_SqlDataSource" 
                       runat="server" 
                       ConnectionString="<%$ ConnectionStrings:dbContabConnectionString %>" 
                       SelectCommand="SELECT Rubro, Descripcion + ' (' + NombreCortoRubro + ')' AS Descripcion FROM tMaestraRubros Where Rubro In (Select Rubro From tNomina Where HeaderID = @HeaderID) ORDER BY Descripcion">
        <SelectParameters>
            <asp:Parameter Name="HeaderID" Type="Int32" DefaultValue="-999" />
        </SelectParameters>
    </asp:SqlDataSource>

</asp:Content>