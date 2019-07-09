<%@ Page Language="C#" MasterPageFile="~/Site2.master" CodeBehind="Nomina.aspx.cs" Inherits="NominaASP.Nomina.Consulta.Nomina" %>

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

    <%--<ajaxToolkit:UpdatePanelAnimationExtender ID="UpdatePanelAnimationExtender1" runat="server"
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
    </ajaxToolkit:UpdatePanelAnimationExtender>--%>

   <%-- <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Always">
        <ContentTemplate>--%>

        <%--  --%>
        <%-- div en la izquierda para mostrar funciones de la página --%>
        <%--  --%>
        <div class="notsosmallfont" style="width: 10%; border: 1px solid #C0C0C0; vertical-align: top;
            background-color: #F7F7F7; float: left; text-align: center; margin-top: 0px;">
            <br />
            <br />

            <a runat="server" 
               id="Report_HtmlAnchor" 
               href="javascript:PopupWin('../../ReportViewer.aspx?rpt=consultaNomina&headerID=0', 1000, 680)" >
               <img id="Img3" 
                    border="0" 
                    alt="Click para obtener un reporte que muestre los registros seleccionados" 
                    src="../../Pictures/print_35x35.png" />
            </a>
            <br />
            <a runat="server" 
               href="javascript:PopupWin('../../ReportViewer.aspx?rpt=consultaNomina&headerID=0', 1000, 680)"
               id="Report2_HtmlAnchor">Imprimir</a>

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

                    <ajaxToolkit:tabcontainer id="TabContainer1" runat="server" activetabindex="1" >

                        <ajaxToolkit:TabPanel HeaderText="Aplicar un filtro" runat="server" ID="TabPanel1" >
                            <ContentTemplate>

                                <fieldset>
                                    <legend>Nómina: </legend>

                                    <table class="gridtable">
                                        <tr>
                                            <td>
                                                Fecha: 
                                            </td>
                                            <td>
                                                <asp:TextBox ID="FechaNominaDesde_TextBox" runat="server" style="font-size: smaller; " /> 
                                                <ajaxToolkit:CalendarExtender ID="CalendarExtender1" 
                                                                                  runat="server" 
                                                                                  TargetControlID="FechaNominaDesde_TextBox" 
                                                                                  Format="d-MM-yyyy" />
                                                &nbsp;/&nbsp;
                                                <asp:TextBox ID="FechaNominaHasta_TextBox" runat="server" style="font-size: smaller; " />
                                                <ajaxToolkit:CalendarExtender ID="CalendarExtender2" 
                                                                                  runat="server" 
                                                                                  TargetControlID="FechaNominaHasta_TextBox" 
                                                                                  Format="d-MM-yyyy" />
                                            </td>
                                            <td>
                                                &nbsp;&nbsp;&nbsp;&nbsp;
                                            </td>
                                            <td>
                                                Grupo de nómina: 
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="GruposNomina_DropDownList" 
                                                                  runat="server" 
                                                                  ItemType="NominaASP.Nomina.Consulta.GrupoNomina" 
                                                                  SelectMethod="tGruposEmpleado_DropDownList_SelectMethod"
                                                                  DataTextField="Descripcion" 
                                                                  DataValueField="ID" 
                                                                  AppendDataBoundItems="True" 
                                                                  Font-Size="Smaller">
                                                    <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                                </asp:DropDownList>
                                            </td>
                                            <td />
                                        </tr>

                                        <tr>
                                            <td>
                                                Tipo: 
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="TipoNomina_DropDownList" 
                                                                    runat="server" 
                                                                    Font-Size="Smaller">
                                                    <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                                    <asp:ListItem Value="M">Mensual</asp:ListItem>
                                                    <asp:ListItem Value="Q">Quincenal</asp:ListItem>
                                                    <asp:ListItem Value="1Q">1ra. quincena</asp:ListItem>
                                                    <asp:ListItem Value="2Q">2da. quincena</asp:ListItem>
                                                    <asp:ListItem Value="V">Vacaciones</asp:ListItem>
                                                    <asp:ListItem Value="U">Utilidades</asp:ListItem>
                                                    <asp:ListItem Value="E">Especial</asp:ListItem>
                                                </asp:DropDownList>
                                            </td>
                                            <td>
                                                &nbsp;&nbsp;&nbsp;&nbsp;
                                            </td>
                                            <td>
                                            </td>
                                            <td>
                                                
                                            </td>
                                        </tr>
                                    </table>

                                </fieldset>

                                <br />

                                <fieldset>
                                    <legend>Empleados: </legend>

                                    <table class="gridtable">
                                        <tr>
                                            <td>
                                                Empleados: 
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="Empleados_DropDownList" 
                                                                  runat="server" 
                                                                  ItemType="NominaASP.Models.tEmpleado" 
                                                                  SelectMethod="Empleados_DropDownList_SelectMethod"
                                                                  DataTextField="Nombre" 
                                                                  DataValueField="Empleado" 
                                                                  AppendDataBoundItems="True" 
                                                                  Font-Size="Smaller">
                                                    <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                                </asp:DropDownList>
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
                                            <td>
                                                Cargos: 
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="Cargos_DropDownList" 
                                                                  runat="server" 
                                                                  ItemType="NominaASP.Models.tCargo" 
                                                                  SelectMethod="Cargos_DropDownList_SelectMethod"
                                                                  DataTextField="Descripcion" 
                                                                  DataValueField="Cargo" 
                                                                  AppendDataBoundItems="True" 
                                                                  Font-Size="Smaller">
                                                    <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                                </asp:DropDownList>
                                            </td>
                                            <td>
                                                &nbsp;&nbsp;&nbsp;&nbsp;
                                            </td>
                                            <td>
                                                Departamentos: 
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="Departamentos_DropDownList" 
                                                                  runat="server" 
                                                                  ItemType="NominaASP.Models.tDepartamento" 
                                                                  SelectMethod="Departamentos_DropDownList_SelectMethod"
                                                                  DataTextField="Descripcion" 
                                                                  DataValueField="Departamento" 
                                                                  AppendDataBoundItems="True" 
                                                                  Font-Size="Smaller">
                                                    <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                                </asp:DropDownList>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>
                                                Estado: 
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="Estados_DropDownList" 
                                                                    runat="server" 
                                                                    Font-Size="Smaller">
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
                                                                    Font-Size="Smaller">
                                                    <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                                    <asp:ListItem Value="NO">Normal</asp:ListItem>
                                                    <asp:ListItem Value="VA">Vacaciones</asp:ListItem>
                                                    <asp:ListItem Value="RE">Retirado</asp:ListItem>
                                                    <asp:ListItem Value="LI">Liquidado</asp:ListItem>
                                                </asp:DropDownList>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                            </td>
                                            <td>
                                            </td>
                                            <td>
                                                &nbsp;&nbsp;&nbsp;&nbsp;
                                            </td>
                                            <td>
                                            </td>
                                            <td>
                                            </td>
                                        </tr>
                                    </table>

                                </fieldset>

                                <br />

                                <fieldset>
                                    <legend>Rubros: </legend>

                                    <table class="gridtable">
                                        <tr>
                                            <td>
                                                Rubro: 
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="Rubros_DropDownList" 
                                                                  runat="server" 
                                                                  ItemType="NominaASP.Models.tMaestraRubro" 
                                                                  SelectMethod="Rubros_DropDownList_SelectMethod"
                                                                  DataTextField="NombreCortoRubro" 
                                                                  DataValueField="Rubro" 
                                                                  AppendDataBoundItems="True" 
                                                                  Font-Size="Smaller">
                                                    <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                                </asp:DropDownList>
                                            </td>
                                            <td>
                                                &nbsp;&nbsp;&nbsp;&nbsp;
                                            </td>
                                            <td>
                                                Tipo: 
                                            </td>
                                            <td>
                                                <asp:DropDownList ID="TiposRubro_DropDownList" 
                                                                    runat="server" 
                                                                    Font-Size="Smaller">
                                                    <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                                    <asp:ListItem Value="A">Asignaciones</asp:ListItem>
                                                    <asp:ListItem Value="D">Deducciones</asp:ListItem>
                                                </asp:DropDownList>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>
                                                Descripción: 
                                            </td>
                                            <td>
                                                <asp:TextBox ID="DescripcionRubro_TextBox" runat="server" Font-Size="Smaller"></asp:TextBox>
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
                                            <td>
                                                Sueldo: 
                                            </td>
                                            <td>
                                                <asp:CheckBox ID="Sueldo_CheckBox" runat="server" />
                                            </td>
                                            <td>
                                                &nbsp;&nbsp;&nbsp;&nbsp;
                                            </td>
                                            <td>
                                                Salario: 
                                            </td>
                                            <td>
                                                <asp:CheckBox ID="Salario_CheckBox" runat="server" />
                                            </td>
                                        </tr>
                                    </table>

                                </fieldset>

                                <table width="100%">
                                    <tr>
                                        <td style="text-align: center; ">
                                            <asp:Button ID="Filter_Ok_Button" 
                                                runat="server" 
                                                Text="Ok" 
                                                OnClick="Filter_Ok_Button_Click" 
                                                Width="90px" 
                                                Style="margin-right: 25px; margin-top: 25px; "/>
                                        </td>
                                    </tr>
                                </table>

                            </ContentTemplate>
                        </ajaxToolkit:TabPanel>

                        <ajaxToolkit:TabPanel HeaderText="Nóminas ejecutadas" runat="server" ID="TabPanel2" >
                            <ContentTemplate>

                                <asp:ListView ID="RubrosNomina_ListView" 
                                              runat="server" 
                                              ItemType="NominaASP.Models.tNomina" 
                                              DataKeyNames="NumeroUnico" 
                                              onpagepropertieschanged="RubrosNomina_ListView_PagePropertiesChanged"
                                              onselectedindexchanged="RubrosNomina_ListView_SelectedIndexChanged"
                                              SelectMethod="RubrosNomina_ListView_GetData"
                                              OnLayoutCreated="RubrosNomina_ListView_LayoutCreated"
                                              OnPreRender="RubrosNomina_ListView_PreRender"
                                              OnItemDataBound="RubrosNomina_ListView_ItemDataBound">
                                    <LayoutTemplate>
                                        <table id="Table1" runat="server">
                                            <tr id="Tr1" runat="server">
                                                <td id="Td1" runat="server">
                                                    <table id="itemPlaceholderContainer" 
                                                           class="ListView_DarkBlue"
                                                           runat="server">
                                                        <thead>
                                                            <tr id="Tr4" runat="server" style="font-size:small; " >
                                                                <th />
                                                                <th id="Th13" runat="server" style="text-align: center; " colspan="3" >
                                                                    Nómina
                                                                </th>
                                                                <th id="Th14" runat="server" style="text-align: center; " colspan="2" >
                                                                    Período
                                                                </th>
                                                                <th id="Th15" runat="server" style="text-align: center; " colspan="11" />
                                                            </tr>

                                                            <tr id="Tr2" runat="server" style="">
                                                                <th />
                                                                <th id="Th1" runat="server" style="text-align: center; " >
                                                                    Fecha
                                                                </th>
                                                                <th id="Th2" runat="server" style="text-align: left; " >
                                                                    Grupo<br />nómina
                                                                </th>
                                                                <th id="Th3" runat="server" style="text-align: center; " >
                                                                    Tipo
                                                                </th>
                                                                <th id="Th4" runat="server" style="text-align: center; " >
                                                                    Desde
                                                                </th>
                                                                <th id="Th5" runat="server" style="text-align: center; " >
                                                                    Hasta
                                                                </th>
                                                                <th id="Th6" runat="server" style="text-align: left; " >
                                                                    Empleado
                                                                </th>
                                                                <th id="Th7" runat="server" style="text-align: left; " >
                                                                    Rubro
                                                                </th>
                                                                <th id="Th8" runat="server" style="text-align: left; " >
                                                                    Descripción
                                                                </th>
                                                                <th id="Th9" runat="server" style="text-align: center; " >
                                                                    Tipo
                                                                </th>
                                                                <th id="Th11" runat="server" style="text-align: right; " >
                                                                    Monto
                                                                </th>
                                                                <th id="Th10" runat="server" style="text-align: right; " >
                                                                    Base
                                                                </th>
                                                                <th id="Th19" runat="server" style="text-align: center; " >
                                                                    Cant<br />días
                                                                </th>
                                                                <th id="Th22" runat="server" style="text-align: center; " >
                                                                    (%)
                                                                </th>
                                                                <th id="Th16" runat="server" style="text-align: center; " >
                                                                    Detalles
                                                                </th>
                                                                <th id="Th27" runat="server" style="text-align: center; " >
                                                                    Sueldo
                                                                </th>
                                                                <th id="Th28" runat="server" style="text-align: center; " >
                                                                    Salario
                                                                </th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                            <tr id="itemPlaceholder" runat="server" />
                                                        </tbody>
                                                        <tfoot>
                                                            <tr id="Tr5" runat="server" class="ListView_DarkBlue_Footer">
                                                                <td />
                                                                <td id="Th12" runat="server" style="text-align: left; " >
                                                                    Totales: 
                                                                </td>
                                                                <td id="Th17" runat="server" style="text-align: right; " >
                                                                    <asp:Label ID="CantRecs_Label" runat="server" Text="" />
                                                                    &nbsp; regs
                                                                </td>
                                                                <td />
                                                                <td />
                                                                <td />

                                                                <td id="Th23" runat="server" style="text-align: right; " colspan="6" >
                                                                    (Asignaciones - Deducciones - Saldo)
                                                                    &nbsp;
                                                                    <asp:Label ID="Asignaciones_Label" runat="server" Text="" />
                                                                    &nbsp;&nbsp;&nbsp;
                                                                    <asp:Label ID="Deducciones_Label" runat="server" Text="" />
                                                                    &nbsp;&nbsp;&nbsp;
                                                                    <asp:Label ID="Saldo_Label" runat="server" Text="" />
                                                                </td>
                                                                <td />
                                                                <td />
                                                                <td />
                                                                <td />
                                                                <td />
                                                            </tr>
                                                        </tfoot>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr id="Tr3" runat="server" class="ListView_DarkBlue_Pager">
                                                <td id="Td2" runat="server" >
                                                    <asp:DataPager runat="server" 
                                                                   ID="ListView_DataPager" PageSize="15">
                                                        <Fields>
                                                            <asp:NumericPagerField 
                                                                PreviousPageText="&lt; Prev"
                                                                NextPageText="Next &gt;"
                                                                ButtonCount="8"
                                                                NextPreviousButtonCssClass="PrevNext"
                                                                CurrentPageLabelCssClass="CurrentPage"
                                                                NumericButtonCssClass="PageNumber" />
                                                            </Fields>
                                                    </asp:DataPager>
                                                </td>
                                            </tr>
                                        </table>
                                    </LayoutTemplate>
                                    <ItemTemplate>
                                        <tr class="ListView_DarkBlue_Item">
                                            <td>
                                                <asp:ImageButton ID="SelectItems_Button" runat="server" AlternateText=">" CommandName="Select"
                                                                 ImageUrl="~/Pictures/arrow_right_13x11.png" 
                                                                 ToolTip="Click para seleccionar la linea en la lista" />
                                            </td>
                                            <td style="text-align: center; white-space: nowrap; " >
                                                <asp:Label ID="Label14" runat="server" Text='<%# Item.tNominaHeader.FechaNomina.ToString("dd-MM-yy") %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="Label5" runat="server" Text='<%# Item.tNominaHeader.tGruposEmpleado.NombreGrupo %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="SimboloMonedaOriginalLabel" runat="server" Text='<%# Item.tNominaHeader.Tipo %>' />
                                            </td>
                                            <td style="text-align: center; white-space: nowrap; " >
                                                <asp:Label ID="DescripcionLabel" runat="server" Text='<%# Item.tNominaHeader.Desde != null ? Item.tNominaHeader.Desde.Value.ToString("dd-MM-yy") : "" %>' />
                                            </td>
                                            <td style="text-align: center; white-space: nowrap; " >
                                                <asp:Label ID="ProvieneDeLabel" runat="server" Text='<%# Item.tNominaHeader.Hasta != null ? Item.tNominaHeader.Hasta.Value.ToString("dd-MM-yy") : "" %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="NumPartidasLabel" runat="server" Text='<%# Item.tEmpleado.Alias %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="TotalDebeLabel" runat="server" Text='<%# Item.tMaestraRubro.NombreCortoRubro %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="TotalHaberLabel" runat="server" Text='<%# Item.Descripcion %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label13" runat="server" Text='<%# Item.Tipo %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="MontoNominaLabel" runat="server" Text='<%# Item.Monto.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="Label2" runat="server" Text='<%# Item.MontoBase != null ? Item.MontoBase.Value.ToString("N2") : "" %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label3" runat="server" Text='<%# Item.CantDias != null ? Item.CantDias.Value.ToString() : "" %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label4" runat="server" Text='<%# Item.Fraccion != null ? Item.Fraccion.Value.ToString("N2") : "" %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label1" runat="server" Text='<%# Item.Detalles %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:CheckBox ID="CheckBox1" runat="server" Checked='<%# Item.SueldoFlag %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:CheckBox ID="CheckBox2" runat="server" Checked='<%# Item.SalarioFlag %>' />
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <AlternatingItemTemplate>
                                        <tr class="ListView_DarkBlue_AlternatingItem">
                                            <td>
                                                <asp:ImageButton ID="SelectItems_Button" runat="server" AlternateText=">" CommandName="Select"
                                                                 ImageUrl="~/Pictures/arrow_right_13x11.png" 
                                                                 ToolTip="Click para mostrar los movimientos de la cuenta" />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label14" runat="server" Text='<%# Item.tNominaHeader.FechaNomina.ToString("dd-MM-yy") %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="Label5" runat="server" Text='<%# Item.tNominaHeader.tGruposEmpleado.NombreGrupo %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="SimboloMonedaOriginalLabel" runat="server" Text='<%# Item.tNominaHeader.Tipo %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="DescripcionLabel" runat="server" Text='<%# Item.tNominaHeader.Desde != null ? Item.tNominaHeader.Desde.Value.ToString("dd-MM-yy") : "" %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="ProvieneDeLabel" runat="server" Text='<%# Item.tNominaHeader.Hasta != null ? Item.tNominaHeader.Hasta.Value.ToString("dd-MM-yy") : "" %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="NumPartidasLabel" runat="server" Text='<%# Item.tEmpleado.Alias %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="TotalDebeLabel" runat="server" Text='<%# Item.tMaestraRubro.NombreCortoRubro %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="TotalHaberLabel" runat="server" Text='<%# Item.Descripcion %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label13" runat="server" Text='<%# Item.Tipo %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="MontoNominaLabel" runat="server" Text='<%# Item.Monto.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="Label2" runat="server" Text='<%# Item.MontoBase != null ? Item.MontoBase.Value.ToString("N2") : "" %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label3" runat="server" Text='<%# Item.CantDias != null ? Item.CantDias.Value.ToString() : "" %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label4" runat="server" Text='<%# Item.Fraccion != null ? Item.Fraccion.Value.ToString("N2") : "" %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label1" runat="server" Text='<%# Item.Detalles %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:CheckBox ID="CheckBox1" runat="server" Checked='<%# Item.SueldoFlag %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:CheckBox ID="CheckBox2" runat="server" Checked='<%# Item.SalarioFlag %>' />
                                            </td>
                                        </tr>
                                    </AlternatingItemTemplate>
                                    <SelectedItemTemplate>
                                        <tr class="ListView_DarkBlue_SelectedItem">
                                            <td />
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label14" runat="server" Text='<%# Item.tNominaHeader.FechaNomina.ToString("dd-MM-yy") %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="Label5" runat="server" Text='<%# Item.tNominaHeader.tGruposEmpleado.NombreGrupo %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="SimboloMonedaOriginalLabel" runat="server" Text='<%# Item.tNominaHeader.Tipo %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="DescripcionLabel" runat="server" Text='<%# Item.tNominaHeader.Desde != null ? Item.tNominaHeader.Desde.Value.ToString("dd-MM-yy") : "" %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="ProvieneDeLabel" runat="server" Text='<%# Item.tNominaHeader.Hasta != null ? Item.tNominaHeader.Hasta.Value.ToString("dd-MM-yy") : "" %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="NumPartidasLabel" runat="server" Text='<%# Item.tEmpleado.Alias %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="TotalDebeLabel" runat="server" Text='<%# Item.tMaestraRubro.NombreCortoRubro %>' />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="TotalHaberLabel" runat="server" Text='<%# Item.Descripcion %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label13" runat="server" Text='<%# Item.Tipo %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="MontoNominaLabel" runat="server" Text='<%# Item.Monto.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="Label2" runat="server" Text='<%# Item.MontoBase != null ? Item.MontoBase.Value.ToString("N2") : "" %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label3" runat="server" Text='<%# Item.CantDias != null ? Item.CantDias.Value.ToString() : "" %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label4" runat="server" Text='<%# Item.Fraccion != null ? Item.Fraccion.Value.ToString("N2") : "" %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label1" runat="server" Text='<%# Item.Detalles %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:CheckBox ID="CheckBox1" runat="server" Checked='<%# Item.SueldoFlag %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:CheckBox ID="CheckBox2" runat="server" Checked='<%# Item.SalarioFlag %>' />
                                            </td>
                                        </tr>
                                    </SelectedItemTemplate>
                                    <EmptyDataTemplate>
                                        <table id="Table2" runat="server" class="ListView_DarkBlue_Empty">
                                            <tr>
                                                <td>
                                                    <br />
                                                    No hay registros que mostrar; probablemente no se han establecido criterios de ejecución (filtro) y seleccionado registros ...
                                                </td>
                                            </tr>
                                        </table>
                                    </EmptyDataTemplate>
                                </asp:ListView>
                              

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
            
     <%--   </ContentTemplate>
    </asp:UpdatePanel>--%>

</asp:Content>