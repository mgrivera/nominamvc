<%@ Page Language="C#" MasterPageFile="~/Site2.master" CodeBehind="DiasVacaciones.aspx.cs" Inherits="NominaASP.Empleados.Vacaciones.DiasVacaciones" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server" />

      <script type="text/javascript">
          function PopupWin(url, w, h) {
              ///Parameters url=page to open, w=width, h=height
              window.open(url, "_blank", "width=" + w + ",height=" + h + ",resizable=yes,scrollbars=yes,status=no,location=no,toolbar=no,menubar=no,top=10px,left=8px");
          }
          function RefreshPage() {
              window.document.getElementById("RebindFlagSpan").firstChild.value = "1";
              window.document.forms(0).submit();
          }
          function showAlert() {
              alert('Hello World!');
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

    <%--  --%>
    <%-- div en la izquierda para mostrar funciones de la página --%>
    <%--  --%>
    <div class="notsosmallfont" style="width: 10%; border: 1px solid #C0C0C0; vertical-align: top;
        background-color: #F7F7F7; float: left; text-align: center; margin-top: 0px;">
        <br />
        <br />

        <a runat="server" 
                   id="filterLink"
                   href="javascript:PopupWin('DiasVacaciones_OpcionesReporte.aspx', 1000, 600)">
                        <img id="Img3" 
                             border="0" 
                             runat="server"
                             alt="Para imprimir un reporte con la información seleccionada" 
                             src="~/Pictures/print_25x25.png" />
                </a>
                <br />
                <a href="javascript:PopupWin('DiasVacaciones_OpcionesReporte.aspx', 1000, 600)">Imprimir</a>

        <hr />
        <br />
    </div>

    <div style="text-align: left; float: right; width: 88%;">
            
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

                <asp:ValidationSummary ID="ValidationSummary1" 
                            runat="server" 
                            class="errmessage_background generalfont errmessage"
                            ShowModelStateErrors="true"
                            ForeColor="" />

                <asp:CustomValidator ID="CustomValidator1" runat="server" ErrorMessage="CustomValidator" Display="None" EnableClientScript="False" />

                <ajaxToolkit:tabcontainer id="TabContainer1" runat="server" activetabindex="0" >

                    <ajaxToolkit:TabPanel HeaderText="Empleados" runat="server" ID="TabPanel1" >
                        <ContentTemplate>

                            <asp:ListView ID="DiasVacaciones_ListView" 
                                          ItemType="NominaASP.Empleados.Vacaciones.DiasVacacionesConsulta" 
                                          DataKeyNames="ID" 
                                          SelectMethod="Empleados_GridView_GetData"
                                          OnPagePropertiesChanging="DiasVacaciones_ListView_PagePropertiesChanging"
                                          runat="server">
                                <LayoutTemplate>
                                    <table id="Table1" runat="server">
                                        <tr id="Tr1" runat="server">
                                            <td id="Td1" runat="server">
                                                <table id="itemPlaceholderContainer" runat="server" border="0" style="border: 1px solid #E6E6E6"
                                                    class="smallfont2" cellspacing="0" rules="none">

                                                   <tr id="Tr4" runat="server">
                                                        <th id="Th15" runat="server" colspan="4" />

                                                        <th id="Th16" 
                                                            class="padded, ListViewHeader_Suave" 
                                                            style="border: 1px solid #C0C0C0; text-align: center; padding: 4px;" 
                                                            colspan="8">
                                                                Vacación más reciente
                                                        </th>

                                                        <th id="Th17" 
                                                            runat="server" 
                                                            class="padded, ListViewHeader_Suave" 
                                                            style="border: 1px solid #C0C0C0; text-align: center; padding: 4px;" 
                                                            colspan="2">
                                                                Vacaciones pendientes
                                                        </th>

                                                        <th id="Th18" runat="server" colspan="1" />
                                                    </tr>

                                                    <tr id="Tr2" runat="server" style="" class="ListViewHeader_Suave">
                                                        <th />
                                                        <th id="Th19" runat="server" class="padded" style="text-align: left; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                            Empleado
                                                        </th>
                                                        <th id="Th2" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Fecha<br />Ingreso
                                                        </th>
                                                        <th id="Th3" runat="server" class="padded" style="text-align: left; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Año<br />vacaciones
                                                        </th>


                                                        <th id="Th4" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Desde
                                                        </th>
                                                        <th id="Th5" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Hasta
                                                        </th>
                                                        <th id="Th6" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Año<br />vac
                                                        </th>
                                                        <th id="Th7" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Días pend<br />años ant
                                                        </th>
                                                        <th id="Th8" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Días según<br />tabla
                                                        </th>
                                                        <th id="Th9" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Días disfrutados<br />antes
                                                        </th>
                                                        <th id="Th10" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Días disfrutados<br />ahora
                                                        </th>
                                                        <th id="Th11" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Días<br />pendientes
                                                        </th>

                                                        <th id="Th12" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Cant
                                                        </th>
                                                        <th id="Th13" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Días<br />s/tabla
                                                        </th>

                                                        <th id="Th14" runat="server" class="padded" style="text-align: center; padding-bottom: 8px;
                                                            padding-top: 8px;">
                                                                Total días<br />pendientes
                                                        </th>
                                                    </tr>
                                                    <tr id="itemPlaceholder" runat="server" />
                                                </table>
                                            </td>
                                        </tr>
                                        <tr id="Tr3" runat="server">
                                            <td id="Td2" runat="server" style="">
                                                <asp:DataPager ID="ConsultaFacturas_DataPager" runat="server" PageSize="20">
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
                                                ... no hay información que mostar - aplique un filtro para seleccionar y mostrar información 
                                            </td>
                                        </tr>
                                    </table>
                                </EmptyDataTemplate>
                    
                    
                                <ItemTemplate>
                        
                                    <tr class="ListViewRow" style="">
                                        <td class="padded" style="text-align: left;">
                                            <asp:ImageButton ID="lnkSelect" CommandName="Select" runat="server" ImageUrl="~/Pictures/arrow_right_13x11.png"  />
                                        </td>
                                        <td class="padded" style="text-align: left;">
                                            <asp:Label ID="Label1" runat="server" Text='<%# Item.Alias %>'></asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center; white-space: nowrap; ">
                                            <asp:Label ID="Label2" runat="server" Text='<%# Item.FechaIngreso.ToString("dd-MMM-yy") %>'></asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: left; white-space: nowrap; ">
                                            <asp:Label ID="Label3" runat="server" 
                                                Text='<%# 
                                                        Item.AnoVacaciones_Ano.ToString() + " - " +  
                                                        Item.AnoVacaciones_Desde.ToString("dd-MM-yy") + " a " +  
                                                        Item.AnoVacaciones_Hasta.ToString("dd-MM-yy")
                                                     %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center; white-space: nowrap; ">
                                            <asp:Label ID="Label4" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_Desde != null ? Item.VacacionMasReciente_Desde.Value.ToString("dd-MM-yy") : "" %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center; white-space: nowrap; ">
                                            <asp:Label ID="Label5" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_Hasta != null ? Item.VacacionMasReciente_Hasta.Value.ToString("dd-MM-yy") : "" %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label6" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_AnoVacaciones.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label7" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasPendAnosAnteriores.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label8" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasSegunTabla.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label9" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasDisfrutados_Antes.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label10" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasDisfrutados_Ahora.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label11" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasPendientes.ToString() %>'>
                                            </asp:Label>
                                        </td>

                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label12" runat="server" 
                                                Text='<%# Item.VacacionesPendientes_Cantidad.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label13" runat="server" 
                                                Text='<%# Item.VacacionesPendientes_DiasSegunTabla.ToString() %>'>
                                            </asp:Label>
                                        </td>

                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label14" runat="server" 
                                                Text='<%# Item.TotalDiasPendientes.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                    </tr>
                          
                                </ItemTemplate>

                                <AlternatingItemTemplate>

                                    <tr class="ListViewAlternatingRow" style="">
                                        <td class="padded" style="text-align: left;">
                                            <asp:ImageButton ID="lnkSelect" CommandName="Select" runat="server" ImageUrl="~/Pictures/arrow_right_13x11.png"  />
                                        </td>
                                        <td class="padded" style="text-align: left;">
                                            <asp:Label ID="Label1" runat="server" Text='<%# Item.Alias %>'></asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center; white-space: nowrap; ">
                                            <asp:Label ID="Label2" runat="server" Text='<%# Item.FechaIngreso.ToString("dd-MMM-yy") %>'></asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: left; white-space: nowrap; ">
                                            <asp:Label ID="Label3" runat="server" 
                                                Text='<%# 
                                                        Item.AnoVacaciones_Ano.ToString() + " - " +  
                                                        Item.AnoVacaciones_Desde.ToString("dd-MM-yy") + " a " +  
                                                        Item.AnoVacaciones_Hasta.ToString("dd-MM-yy")
                                                     %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center; white-space: nowrap; ">
                                            <asp:Label ID="Label4" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_Desde != null ? Item.VacacionMasReciente_Desde.Value.ToString("dd-MM-yy") : "" %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center; white-space: nowrap; ">
                                            <asp:Label ID="Label5" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_Hasta != null ? Item.VacacionMasReciente_Hasta.Value.ToString("dd-MM-yy") : "" %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label6" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_AnoVacaciones.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label7" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasPendAnosAnteriores.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label8" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasSegunTabla.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label9" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasDisfrutados_Antes.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label10" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasDisfrutados_Ahora.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label11" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasPendientes.ToString() %>'>
                                            </asp:Label>
                                        </td>

                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label12" runat="server" 
                                                Text='<%# Item.VacacionesPendientes_Cantidad.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label13" runat="server" 
                                                Text='<%# Item.VacacionesPendientes_DiasSegunTabla.ToString() %>'>
                                            </asp:Label>
                                        </td>

                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label14" runat="server" 
                                                Text='<%# Item.TotalDiasPendientes.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                    </tr>

                                </AlternatingItemTemplate>

                                <SelectedItemTemplate>

                                    <tr class="ListViewSelectedRow" style="">
                                        <td />
                                        <td class="padded" style="text-align: left;">
                                            <asp:Label ID="Label1" runat="server" Text='<%# Item.Alias %>'></asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center; white-space: nowrap; ">
                                            <asp:Label ID="Label2" runat="server" Text='<%# Item.FechaIngreso.ToString("dd-MMM-yy") %>'></asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: left; white-space: nowrap; ">
                                            <asp:Label ID="Label3" runat="server" 
                                                Text='<%# 
                                                        Item.AnoVacaciones_Ano.ToString() + " - " +  
                                                        Item.AnoVacaciones_Desde.ToString("dd-MM-yy") + " a " +  
                                                        Item.AnoVacaciones_Hasta.ToString("dd-MM-yy")
                                                     %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center; white-space: nowrap; ">
                                            <asp:Label ID="Label4" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_Desde != null ? Item.VacacionMasReciente_Desde.Value.ToString("dd-MM-yy") : "" %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center; white-space: nowrap; ">
                                            <asp:Label ID="Label5" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_Hasta != null ? Item.VacacionMasReciente_Hasta.Value.ToString("dd-MM-yy") : "" %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label6" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_AnoVacaciones.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label7" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasPendAnosAnteriores.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label8" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasSegunTabla.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label9" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasDisfrutados_Antes.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label10" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasDisfrutados_Ahora.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label11" runat="server" 
                                                Text='<%# Item.VacacionMasReciente_DiasPendientes.ToString() %>'>
                                            </asp:Label>
                                        </td>

                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label12" runat="server" 
                                                Text='<%# Item.VacacionesPendientes_Cantidad.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label13" runat="server" 
                                                Text='<%# Item.VacacionesPendientes_DiasSegunTabla.ToString() %>'>
                                            </asp:Label>
                                        </td>

                                        <td class="padded" style="text-align: center;">
                                            <asp:Label ID="Label14" runat="server" 
                                                Text='<%# Item.TotalDiasPendientes.ToString() %>'>
                                            </asp:Label>
                                        </td>
                                    </tr>

                                </SelectedItemTemplate>

                            </asp:ListView>

                        </ContentTemplate>
                    </ajaxToolkit:TabPanel>

                    <ajaxToolkit:TabPanel HeaderText="Aplicar un filtro" runat="server" ID="TabPanel2" >
                        <ContentTemplate>
                            <div runat="server" id="Filtro_Div">
                                <table class="gridtable">
                                    <tr>
                                        <td>
                                            Mostrar días de vacaciones<br />para el año (calendario): 
                                        </td>
                                        <td>
                                            <asp:TextBox ID="AnoConsulta_TextBox" runat="server" Width="100px"></asp:TextBox>

                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" 
                                                runat="server" 
                                                ErrorMessage="Ud. debe indicar un año para la consulta (ej: 2012, 2013)." ControlToValidate="AnoConsulta_TextBox"
                                                style="color: red; ">*
                                            </asp:RequiredFieldValidator>

                                            <asp:RangeValidator ID="RangeValidator1" 
                                                runat="server" 
                                                ErrorMessage="El año debe ser un entero de cuatro dígitos."
                                                ControlToValidate="AnoConsulta_TextBox" MaximumValue="2025" MinimumValue="1990"
                                                style="color: red; ">*

                                            </asp:RangeValidator>
                                        </td>
                                        <td>
                                            &nbsp;&nbsp;&nbsp;&nbsp;
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                    </tr>





                                    <tr>
                                        <td />
                                        <td />
                                        <td />
                                        <td />
                                        <td />
                                    </tr>

                                    <tr>
                                        <td colspan="5" 
                                            style="text-align: left; background-color: #DFDFDF; border: 1px solid #C0C0C0; font-weight: bold; padding-left: 25px; ">
                                            Empleados: 
                                        </td>
                                    </tr>

                                    <tr>
                                        <td>
                                            Estado: 
                                        </td>
                                        <td>
                                            <asp:DropDownList ID="Estados_DropDownList" 
                                                                runat="server" 
                                                                Font-Size="Small">
                                                <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                                <asp:ListItem Value="A">Activo</asp:ListItem>
                                                <asp:ListItem Value="S">Suspendido</asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                        <td>
                                            &nbsp;&nbsp;&nbsp;&nbsp;
                                        </td>
                                        <td>
                                            Situación actual: 
                                        </td>
                                        <td>
                                            <asp:DropDownList ID="SituacionActual_DropDownList" 
                                                                runat="server" 
                                                                Font-Size="Small">
                                                <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                                <asp:ListItem Value="NO">Normal</asp:ListItem>
                                                <asp:ListItem Value="VA">Vacaciones</asp:ListItem>
                                                <asp:ListItem Value="RE">Retirado</asp:ListItem>
                                                <asp:ListItem Value="LI">Liquidado</asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td colspan="5" />
                                    </tr>
                                    <tr>
                                        <td colspan="5" />
                                    </tr>

                                    <tr>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                            &nbsp;&nbsp;&nbsp;&nbsp;
                                        </td>
                                        <td colspan="2" style="text-align: right; ">
                                            <asp:Button ID="LimpiarFiltro_Button" runat="server" Text="Limpiar filtro" OnClick="LimpiarFiltro_Button_Click" />
                                            &nbsp;&nbsp;
                                            <asp:Button ID="Filter_Ok_Button" runat="server" Text="Ok" OnClick="Filter_Ok_Button_Click" width="80px" />
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </ContentTemplate>
                    </ajaxToolkit:TabPanel>

                </ajaxToolkit:tabcontainer>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

</asp:Content>