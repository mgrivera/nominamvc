<%@ Page Language="C#" MasterPageFile="~/Site2.master" CodeBehind="AsientosContables.aspx.cs" Inherits="NominaASP.Nomina.AsientosContables.AsientosContables_page" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server" />

       <script type="text/javascript">
           function PopupWin(url, w, h) {
               ///Parameters url=page to open, w=width, h=height
               var left = parseInt((screen.availWidth / 2) - (w / 2));
               var top = parseInt((screen.availHeight / 2) - (h / 2));
               window.open(url, "external", "width=" + w + ",height=" + h + ",resizable=yes,scrollbars=yes,status=no,location=no,toolbar=no,menubar=no,left=" + left + ",top=" + top + "screenX=" + left + ",screenY=" + top);
           }

           function RefreshPage() {
               window.document.getElementById("RebindFlagSpan").firstChild.value = "1";
               window.document.forms(0).submit();
           }
    </script>

    <script type="text/javascript">
        function showPopup()
        {
            $find('ConstruirAsientosContables_ModalPopupExtender').show();
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

            <a runat="server" 
               id="ConstruirAsientoContable_HtmlAnchor"  
               href="javascript:PopupWin('AsientosContables_ConstruccionAsientoContable.aspx', 1000, 680)" >
               <img id="Img3" 
                    border="0" 
                    alt="Click para iniciar el proceso que construye el asiento contable para la nómina seleccionada" 
                    src="../../Pictures/Gears_25x25.png" />
            </a>
            <br />
            <a runat="server" 
               href="javascript:PopupWin('AsientosContables_ConstruccionAsientoContable.aspx', 1000, 680)"
               id="ConstruirAsientoContable2_HtmlAnchor">Construir<br />asiento</a>
        
            <hr />

            <a runat="server" 
               id="ConsultarAsientoContable_HtmlAnchor"  
               href="javascript:PopupWin('AsientoContable_Consulta.aspx?AsientoContableID=0', 1000, 680)" >
               <img id="Img1" 
                    border="0" 
                    alt="Click para consultar el asiento contable para la nómina seleccionada" 
                    src="../../Pictures/NewWindow_25x25.png?AsientoContableID=0" />
            </a>
            <br />
            <a runat="server" 
               href="javascript:PopupWin('AsientoContable_Consulta.aspx', 1000, 680)"
               id="ConsultarAsientoContable2_HtmlAnchor">Consultar<br />asiento</a>
        
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

                        <ajaxToolkit:TabPanel HeaderText="Nóminas ejecutadas" runat="server" ID="TabPanel1" >
                            <ContentTemplate>

                                <asp:GridView runat="server" 
                                              ID="Nomina_GridView"
                                              DataKeyNames="ID" 
                                              OnSelectedIndexChanged="Nomina_GridView_SelectedIndexChanged"
                                              OnPageIndexChanging="Nomina_GridView_PageIndexChanging"
                                              AutoGenerateColumns="False" 
                                              AllowPaging="True" 
                                              PageSize="15"  
                                              CssClass="Grid" DataSourceID="SqlDataSource1">
                                    <Columns>
                                        <asp:buttonfield CommandName="Select" ImageUrl="../../Pictures/SelectRow.png" ButtonType="Image" Text="Select" />

                                        <asp:TemplateField HeaderText="F nómina">
                                            <ItemTemplate>
                                                <asp:Label ID="Label1" runat="server" Text='<%# Bind("FechaNomina", "{0:d-MMM-yyyy}") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Desde">
                                            <ItemTemplate>
                                                <asp:Label ID="Label3" runat="server" Text='<%# Bind("Desde", "{0:d-MMM-yy}") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Hasta">
                                            <ItemTemplate>
                                                <asp:Label ID="Label4" runat="server" Text='<%# Bind("Hasta", "{0:d-MMM-yy}") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Cant<br/>días">
                                            <ItemTemplate>
                                                <asp:Label ID="Label5" runat="server" Text='<%# Bind("CantidadDias") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Tipo">
                                            <ItemTemplate>
                                                <asp:Label ID="Label6" runat="server" Text='<%# Bind("Tipo") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Agregar<br/>sueldo">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="AgregarSueldo_CheckBox" runat="server" Checked='<%# Bind("AgregarSueldo") %>' Enabled="false" />
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Agregar<br/>deducciones">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="AgregarDeducciones_CheckBox" runat="server" Checked='<%# Bind("AgregarDeduccionesObligatorias")  %>' Enabled="false" />
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Proviene de">
                                            <ItemTemplate>
                                                <asp:Label ID="Label7" runat="server" Text='<%# Bind("ProvieneDe")  %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Left" />
                                            <ItemStyle HorizontalAlign="Left" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="ID asiento">
                                            <ItemTemplate>
                                                <asp:Label ID="AsientoID_Label" runat="server" Text='<%# Bind("AsientoID")  %>'></asp:Label> 
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Número asiento">
                                            <ItemTemplate>
                                                <asp:Label ID="NumeroAsiento_Label" runat="server" Text='<%# Bind("NumeroAsiento")  %>'></asp:Label> 
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Fecha<br/>asiento">
                                            <ItemTemplate>
                                                <asp:Label ID="FechaAsiento_Label" runat="server"  Text='<%# Bind("FechaAsiento", "{0:d-MMM-yy}") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Fecha<br/>ejecución">
                                            <ItemTemplate>
                                                <asp:Label ID="FechaEjecucion_Label" runat="server" Text='<%# Bind("FechaEjecucion", "{0:d-MMM-yy h:m tt}")  %>'></asp:Label> 
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Cia">
                                            <ItemTemplate>
                                                <asp:Label ID="AbreviaturaCiaContab_Label" runat="server" Text='<%# Bind("AbreviaturaCiaContab")  %>'></asp:Label> 
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
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

                    </ajaxToolkit:tabcontainer>
            </div>
            
        </ContentTemplate>
    </asp:UpdatePanel>
    
    <asp:SqlDataSource ID="SqlDataSource1" 
                       runat="server" 
                       ConnectionString="<%$ ConnectionStrings:dbContabConnectionString %>" 
                       SelectCommand="SELECT t.ID, t.FechaNomina, g.Descripcion, t.Desde, t.Hasta, t.CantidadDias, t.Tipo, t.AgregarSueldo,
	                    t.AgregarDeduccionesObligatorias, t.ProvieneDe,  t.FechaEjecucion, t.AsientoContableID, 
                        a.NumeroAutomatico as AsientoID, a.Numero as NumeroAsiento, a.Fecha as FechaAsiento, 
                        c.Abreviatura as AbreviaturaCiaContab 
                        FROM tNominaHeaders t Inner Join tGruposEmpleados g On t.GrupoNomina = g.Grupo
                        Inner Join Companias c On g.Cia = c.Numero
                        Left Outer Join Asientos a On t.AsientoContableID = a.NumeroAutomatico 
                        WHERE (g.Cia = @numeroCiaContab)
                        Order By t.FechaNomina Desc">
        <SelectParameters>
            <asp:Parameter Name="numeroCiaContab" Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>

</asp:Content>