<%@ Page Language="C#" MasterPageFile="~/Site2.master" CodeBehind="EmpleadosFaltasConsulta.aspx.cs" Inherits="NominaASP.Empleados.Faltas.EmpleadosFaltasConsulta" %>

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
                   href="javascript:PopupWin('EmpleadosFaltas_OpcionesReporte.aspx', 1000, 600)">
                        <img id="Img3" 
                             border="0" 
                             runat="server"
                             alt="Para imprimir un reporte con la información seleccionada" 
                             src="~/Pictures/print_25x25.png" />
                </a>
                <br />
                <a href="javascript:PopupWin('EmpleadosFaltas_OpcionesReporte.aspx', 1000, 600)">Imprimir</a>

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

                            <asp:GridView runat="server" 
                                          ID="Empleados_GridView"
                                          ItemType="NominaASP.Empleados.Faltas.EmpleadoFaltaConsulta" 
                                          DataKeyNames="Empleado" 
                                          SelectMethod="Empleados_GridView_GetData"
                                          OnSelectedIndexChanged="Empleados_GridView_SelectedIndexChanged"
                                          OnPageIndexChanging="Empleados_GridView_PageIndexChanging"
                                          AutoGenerateColumns="False" 
                                          AllowPaging="True" 
                                          PageSize="15"  
                                          CssClass="Grid">
                                <Columns>
                                    <%--<asp:buttonfield CommandName="Select" Text="Select"/>--%>
                                    <asp:buttonfield CommandName="Select" ImageUrl="../../Pictures/SelectRow.png" ButtonType="Image" Text="Select" />

                                    <asp:TemplateField HeaderText="Nombre">
                                        <ItemTemplate>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Item.Empleado %>'></asp:Label>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Left" />
                                        <ItemStyle HorizontalAlign="Left" />
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Departamento">
                                        <ItemTemplate>
                                            <asp:Label ID="Label4" runat="server" Text='<%# Item.Departamento %>'></asp:Label>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Left" />
                                        <ItemStyle HorizontalAlign="Left" />
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Descontar">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="Label5" runat="server" Checked='<%# Item.Descontar %>'></asp:CheckBox>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Desde">
                                        <ItemTemplate>
                                            <asp:Label ID="Label6" runat="server" Text='<%# Item.Desde.ToString("d-MMM-yy") %>'></asp:Label>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Hasta">
                                        <ItemTemplate>
                                            <asp:Label ID="Label7" runat="server" Text='<%# Item.Hasta.ToString("d-MMM-yy") %>'></asp:Label>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Días<br />(total)">
                                        <ItemTemplate>
                                            <asp:Label ID="Label8" runat="server" Text='<%# Item.TotalDias.ToString() %>'></asp:Label>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Días<br />sab dom">
                                        <ItemTemplate>
                                            <asp:Label ID="Label9" runat="server" Text='<%# Item.SabYDom.ToString() %>'></asp:Label>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Días<br />feriados">
                                        <ItemTemplate>
                                            <asp:Label ID="Label10" runat="server" Text='<%# Item.Feriados.ToString() %>'></asp:Label>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Días<br />hábiles">
                                        <ItemTemplate>
                                            <asp:Label ID="Label11" runat="server" Text='<%# Item.Faltas.ToString() %>'></asp:Label>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Cant<br />horas">
                                        <ItemTemplate>
                                            <asp:Label ID="Label12" runat="server" Text='<%# Item.CantHoras %>'></asp:Label>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Fecha<br />nómina">
                                        <ItemTemplate>
                                            <asp:Label ID="Label13" runat="server" Text='<%# Item.FechaNomina != null ? Item.FechaNomina.Value.ToString("dd-MMM-yy") : "" %>'></asp:Label>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Cia<br/>contab">
                                        <ItemTemplate>
                                            <asp:Label ID="Label14" runat="server" Text='<%# Item.CiaContab %>'></asp:Label>
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

                    <ajaxToolkit:TabPanel HeaderText="Aplicar un filtro" runat="server" ID="TabPanel2" >
                        <ContentTemplate>
                            <div runat="server" id="Filtro_Div">
                                <table class="gridtable">
                                    <tr>
                                        <td>
                                            Período: 
                                        </td>
                                        <td>
                                            <asp:TextBox ID="Desde_TextBox" runat="server" Width="100px"></asp:TextBox>

                                            <ajaxToolkit:CalendarExtender ID="CalendarExtender1" 
                                                                          runat="server" 
                                                                          CssClass="custom-calendar"
                                                                          TargetControlID="Desde_TextBox" 
                                                                          Format="d-MM-yyyy" />
                                            &nbsp;/&nbsp;
                                            <asp:TextBox ID="Hasta_TextBox2" runat="server" Width="100px"></asp:TextBox>

                                            <ajaxToolkit:CalendarExtender ID="CalendarExtender2" 
                                                                          runat="server" 
                                                                          TargetControlID="Hasta_TextBox2" 
                                                                          CssClass="custom-calendar"
                                                                          Format="d-MM-yyyy" />
                                        </td>
                                        <td>
                                            &nbsp;&nbsp;&nbsp;&nbsp;
                                        </td>
                                        <td>
                                            Descontar: 
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="Descontar_CheckBox" runat="server" />
                                        </td>
                                    </tr>

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
                                                              Font-Size="Small">
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
                                                              Font-Size="Small">
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
                                            <asp:Button ID="LimpiarFiltro_Button" runat="server" Text="Limpiar filtro" OnClick="LimpiarFiltro_Button_Click" Width="80px" />
                                            &nbsp;&nbsp;
                                            <asp:Button ID="Filter_Ok_Button" runat="server" Text="Ok" OnClick="Filter_Ok_Button_Click" Width="80px" />
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