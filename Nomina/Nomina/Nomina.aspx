<%@ Page Language="C#" MasterPageFile="~/Site2.master" CodeBehind="Nomina.aspx.cs" Inherits="NominaASP.Nomina.Nomina.Nomina" %>

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

            <asp:ImageButton ID="EjecutarNomina_ImageButton" runat="server" ImageUrl="~/Pictures/Gears_25x25.png" OnClick="EjecutarNomina_ImageButton_Click"/>
            <br />
            <asp:LinkButton ID="EjecutarNomina_LinkButton" runat="server" OnClick="EjecutarNomina_LinkButton_Click">Ejecutar<br />nómina</asp:LinkButton>
        
            <hr />

            <%--<asp:ImageButton ID="ObtenerReporte_ImageButton" runat="server" ImageUrl="~/Pictures/print_35x35.png" OnPreRender="ObtenerReporte_ImageButton_PreRender"/>
            <br />
            <asp:LinkButton ID="ObtenerReporte_LinkButton" runat="server" OnPreRender="ObtenerReporte_LinkButton_PreRender">Reporte</asp:LinkButton>--%>

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

             <a runat="server" 
               id="ContruirArchivosMailMerge1_HtmlAnchor" 
               href="javascript:PopupWin('Nomina_ConstruccionTextFiles.aspx', 1000, 680)" >
               <img id="Img2" 
                    border="0" 
                    alt="Para exportar los datos seleccionados a un archivo (txt) que permita efectuar una combinación (mail-merge) con Microsoft Word" 
                    src="../../Pictures/MailMerge_25x25.png" />
            </a>
            <br />
            <a runat="server" 
               href="javascript:PopupWin('Nomina_ConstruccionTextFiles.aspx', 1000, 680)"
               id="ContruirArchivosMailMerge2_HtmlAnchor">Microsoft Word</a>

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
                                              ItemType="NominaASP.Models.tNominaHeader" 
                                              DataKeyNames="ID" 
                                              SelectMethod="Nomina_GridView_GetData"
                                              DeleteMethod="Nomina_GridView_DeleteItem"
                                              OnSelectedIndexChanged="Nomina_GridView_SelectedIndexChanged"
                                              OnPageIndexChanging="Nomina_GridView_PageIndexChanging"
                                              AutoGenerateColumns="False" 
                                              AllowPaging="True" 
                                              PageSize="15"  
                                              CssClass="Grid">
                                    <Columns>
                                        <asp:buttonfield CommandName="Select" ImageUrl="~/Pictures/arrow_right_13x11.png" ButtonType="Image" Text="Select" />

                                        <asp:TemplateField HeaderText="F nómina">
                                            <ItemTemplate>
                                                <asp:Label ID="Label1" runat="server" Text='<%# Item.FechaNomina.ToString("d-MMM-yy") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Grupo<br/>nómina">
                                            <ItemTemplate>
                                                <asp:Label ID="Label2" runat="server" Text='<%# Item.tGruposEmpleado.Descripcion %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Left" />
                                            <ItemStyle HorizontalAlign="Left" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Desde">
                                            <ItemTemplate>
                                                <asp:Label ID="Label3" runat="server" Text='<%# Item.Desde == null ? "" : Item.Desde.Value.ToString("d-MMM-yy") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Hasta">
                                            <ItemTemplate>
                                                <asp:Label ID="Label4" runat="server" Text='<%# Item.Hasta == null ? "" : Item.Hasta.Value.ToString("d-MMM-yy") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Cant<br/>días">
                                            <ItemTemplate>
                                                <asp:Label ID="Label5" runat="server" Text='<%# Item.CantidadDias %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Tipo">
                                            <ItemTemplate>
                                                <asp:Label ID="Label6" runat="server" Text='<%# Item.Tipo %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Agregar<br/>sueldo">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="AgregarSueldo_CheckBox" runat="server" Checked='<%# Item.AgregarSueldo %>' Enabled="false" />
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Agregar<br/>deducciones">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="AgregarDeducciones_CheckBox" runat="server" Checked='<%# Item.AgregarDeduccionesObligatorias %>' Enabled="false" />
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Proviene de">
                                            <ItemTemplate>
                                                <asp:Label ID="Label7" runat="server" Text='<%# Item.ProvieneDe %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Left" />
                                            <ItemStyle HorizontalAlign="Left" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Fecha<br/>ejecución">
                                            <ItemTemplate>
                                                <asp:Label ID="Label8" runat="server" Text='<%# Item.FechaEjecucion.ToString("d-MMM-yy") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                   <%--     <asp:buttonfield CommandName="Delete" ImageUrl="../../Pictures/SelectRow.png" Text="Eliminar" />--%>

                                        <asp:TemplateField HeaderText="">
	                                        <ItemTemplate>
		                                        <asp:ImageButton ID="deleteButton" runat="server" 
                                                            CommandName="Delete" Text="Eliminar"
                                                            OnClientClick="return confirm('Seguro desea eliminar esta nómina?');" 
                                                            ImageUrl="~/Pictures/Delete_Gray.png" />
	                                        </ItemTemplate>
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

                        <ajaxToolkit:TabPanel HeaderText="Rubros para la nómina seleccionada" runat="server" ID="TabPanel2" >
                            <ContentTemplate>

                                <div style="font-size: smaller; margin-top: 10px; margin-bottom: 10px; ">
                                    Empleados: 
                                    <asp:DropDownList ID="Empleados_DropDownList" 
                                                        runat="server" 
                                                        DataSourceID="Empleados_SqlDataSource" 
                                                        DataTextField="Nombre" 
                                                        DataValueField="Empleado" 
                                                        AutoPostBack="True" 
                                                        AppendDataBoundItems="True" 
                                                        OnSelectedIndexChanged="Empleados_DropDownList_SelectedIndexChanged" 
                                                        Font-Size="XX-Small">
                                        <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                    </asp:DropDownList>

                                    Rubros: 
                                    <asp:DropDownList ID="Rubros_DropDownList" 
                                                        runat="server" 
                                                        DataSourceID="Rubros_SqlDataSource" 
                                                        DataTextField="Descripcion" 
                                                        DataValueField="Rubro" 
                                                        AppendDataBoundItems="True" 
                                                        AutoPostBack="True" 
                                                        OnSelectedIndexChanged="Rubros_DropDownList_SelectedIndexChanged" 
                                                        Font-Size="XX-Small">
                                        <asp:ListItem Value="-999" Text=" "></asp:ListItem>
                                    </asp:DropDownList>

                                    Tipos: <asp:DropDownList ID="Tipos_DropDownList" 
                                                                runat="server" 
                                                                AutoPostBack="True" 
                                                                OnSelectedIndexChanged="Tipos_DropDownList_SelectedIndexChanged"
                                                                Font-Size="XX-Small">
                                        <asp:ListItem></asp:ListItem>
                                        <asp:ListItem Value="D">Deducciones</asp:ListItem>
                                        <asp:ListItem Value="A">Asignaciones</asp:ListItem>
                                    </asp:DropDownList>
                                </div>

                                 <asp:GridView runat="server" 
                                              ID="NominaDetalles_GridView"
                                              ItemType="NominaASP.Models.tNomina" 
                                              DataKeyNames="NumeroUnico" 
                                              SelectMethod="NominaDetalles_GridView_GetData"
                                              OnSelectedIndexChanged="NominaDetalles_GridView_SelectedIndexChanged"
                                              OnPageIndexChanging="NominaDetalles_GridView_PageIndexChanging"
                                              AutoGenerateColumns="False"
                                              OnRowDataBound="NominaDetalles_GridView_RowDataBound" 
                                              AllowPaging="True" 
                                              PageSize="15"  
                                              CssClass="Grid" ShowFooter="True">
                                    <Columns>

                                        <asp:TemplateField HeaderText="Empleado">
                                            <ItemTemplate>
                                                <asp:Label ID="Label1" runat="server" Text='<%# Item.tEmpleado.Nombre %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Left" />
                                            <ItemStyle HorizontalAlign="Left" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Rubro">
                                            <ItemTemplate>
                                                <asp:Label ID="Label2" runat="server" Text='<%# Item.tMaestraRubro.NombreCortoRubro %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Left" />
                                            <ItemStyle HorizontalAlign="Left" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Descripción">
                                            <ItemTemplate>
                                                <asp:Label ID="Label3" runat="server" Text='<%# Item.Descripcion %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Left" />
                                            <ItemStyle HorizontalAlign="Left" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Tipo">
                                            <ItemTemplate>
                                                <asp:Label ID="Label4" runat="server" Text='<%# Item.Tipo %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Monto">
                                            <ItemTemplate>
                                                <asp:Label ID="monto_Label" runat="server" Text='<%# Item.Monto %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Right" />
                                            <ItemStyle HorizontalAlign="Right" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Base">
                                            <ItemTemplate>
                                                <asp:Label ID="montoBase_Label" runat="server" Text='<%# Item.MontoBase == null ? "" : Item.MontoBase.Value.ToString("N2") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Right" />
                                            <ItemStyle HorizontalAlign="Right" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Cant<br />días">
                                            <ItemTemplate>
                                                <asp:Label ID="cantDias_Label" runat="server" Text='<%# Item.CantDias %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="%">
                                            <ItemTemplate>
                                                <asp:Label ID="Fraccion_Label" runat="server" Text='<%# Item.Fraccion == null ? "" : Item.Fraccion.Value.ToString("N3") %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Detalles">
                                            <ItemTemplate>
                                                <asp:Label ID="Detalles_Label" runat="server" Text='<%# Item.Detalles %>'></asp:Label>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Sueldo">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="sueldo_CheckBox" runat="server" Checked='<%# Item.SueldoFlag %>'></asp:CheckBox>
                                            </ItemTemplate>
                                            <HeaderStyle HorizontalAlign="Center" />
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Salario">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="salario_CheckBox" runat="server" Checked='<%# Item.SalarioFlag %>'></asp:CheckBox>
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
                                    <FooterStyle CssClass="GridFooter" />
                                    <EmptyDataRowStyle CssClass="GridEmptyData" />

                                </asp:GridView>

                            </ContentTemplate>
                        </ajaxToolkit:TabPanel>

                        <ajaxToolkit:TabPanel HeaderText="Salario integral" runat="server" ID="TabPanel4" >
                            <ContentTemplate>

                                <asp:ListView ID="SalarioIntegral_ListView" 
                                              runat="server" 
                                              ItemType="NominaASP.Models.tNomina_SalarioIntegral" 
                                              DataKeyNames="ID" 
                                              onpagepropertieschanged="SalarioIntegral_ListView_PagePropertiesChanged"
                                              onselectedindexchanged="SalarioIntegral_ListView_SelectedIndexChanged"
                                              SelectMethod="SalarioIntegral_ListView_GetData" 
                                              OnLayoutCreated="SalarioIntegral_ListView_LayoutCreated"
                                              OnItemDataBound="SalarioIntegral_ListView_ItemDataBound">
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
                                                                <th />
                                                                <th />
                                                                <th id="Th13" runat="server" style="text-align: center; " colspan="2" >
                                                                    Sueldo básico
                                                                </th>
                                                                <th id="Th14" runat="server" style="text-align: center; " colspan="3" >
                                                                    Bono vacacional
                                                                </th>
                                                                <th id="Th15" runat="server" style="text-align: center; " colspan="3" >
                                                                    Utilidades
                                                                </th>
                                                                <th id="Th16" runat="server" style="text-align: center; " colspan="2" >
                                                                    Salario integral
                                                                </th>
                                                            </tr>

                                                            <tr id="Tr2" runat="server" style="">
                                                                <th />
                                                                <th id="Th1" runat="server" style="text-align: left; " >
                                                                    Empleado
                                                                </th>
                                                                <th id="Th19" runat="server" style="text-align: center; " >
                                                                    F ingreso
                                                                </th>
                                                                <th id="Th2" runat="server" style="text-align: right; " >
                                                                    Mensual
                                                                </th>
                                                                <th id="Th3" runat="server" style="text-align: right; " >
                                                                    Diario
                                                                </th>
                                                                <th id="Th4" runat="server" style="text-align: center; " >
                                                                    Días
                                                                </th>
                                                                <th id="Th5" runat="server" style="text-align: right; " >
                                                                    Monto
                                                                </th>
                                                                <th id="Th6" runat="server" style="text-align: left; " >
                                                                    Diario
                                                                </th>
                                                                <th id="Th7" runat="server" style="text-align: center; " >
                                                                    Días
                                                                </th>
                                                                <th id="Th8" runat="server" style="text-align: right; " >
                                                                    Monto
                                                                </th>
                                                                <th id="Th9" runat="server" style="text-align: right; " >
                                                                    Diario
                                                                </th>
                                                                <th id="Th11" runat="server" style="text-align: right; " >
                                                                    Diario
                                                                </th>
                                                                <th id="Th10" runat="server" style="text-align: right; " >
                                                                    Mensual
                                                                </th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                            <tr id="itemPlaceholder" runat="server" />
                                                        </tbody>
                                                        <tfoot>
                                                            <tr id="Tr5" runat="server" class="ListView_DarkBlue_Footer">
                                                                <td />
                                                                <td />
                                                                <td id="Th12" runat="server" style="text-align: left; " >
                                                                    Totales: 
                                                                </td>
                                                                <td id="Th17" runat="server" style="text-align: right; " >
                                                                    <asp:Label ID="SueldoMensual_Label" runat="server" Text="" />
                                                                </td>
                                                                <td id="Th18" runat="server" style="text-align: right; " >
                                                                    <asp:Label ID="SueldoDiario_Label" runat="server" Text="" />
                                                                </td>
                                                                <td />
                                                                <td id="Th20" runat="server" style="text-align: right; " >
                                                                    <asp:Label ID="BonoVac_Label" runat="server" Text="" />
                                                                </td>
                                                                <td id="Th21" runat="server" style="text-align: left; " >
                                                                    <asp:Label ID="BonoVacDiario_Label" runat="server" Text="" />
                                                                </td>
                                                                <td />
                                                                <td id="Th23" runat="server" style="text-align: right; " >
                                                                    <asp:Label ID="Utilidades_Label" runat="server" Text="" />
                                                                </td>
                                                                <td id="Th24" runat="server" style="text-align: right; " >
                                                                    <asp:Label ID="UtilidadesDiarias_Label" runat="server" Text="" />
                                                                </td>
                                                                <td id="Th25" runat="server" style="text-align: right; " >
                                                                    <asp:Label ID="SalarioIntegralDiario_Label" runat="server" Text="" />
                                                                </td>
                                                                <td id="Th26" runat="server" style="text-align: right; " >
                                                                    <asp:Label ID="SalarioIntegralMensual_Label" runat="server" Text="" />
                                                                </td>
                                                            </tr>
                                                        </tfoot>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr id="Tr3" runat="server" class="ListView_DarkBlue_Pager">
                                                <td id="Td2" runat="server" >
                                                    <%--<asp:DataPager ID="ComprobantesContables_DataPager" runat="server" PageSize="20">
                                                        <Fields>
                                                            <asp:NextPreviousPagerField ButtonType="Link" FirstPageImageUrl="~/Pictures/first_16x16.gif"
                                                                FirstPageText="&lt;&lt;" NextPageText="&gt;" PreviousPageImageUrl="~/Pictures/left_16x16.gif"
                                                                PreviousPageText="&lt;" ShowFirstPageButton="True" ShowNextPageButton="False"
                                                                ShowPreviousPageButton="True" />
                                                            <asp:NumericPagerField />
                                                            <asp:NextPreviousPagerField ButtonType="Link" LastPageImageUrl="~/Pictures/last_16x16.gif"
                                                                LastPageText="&gt;&gt;" NextPageImageUrl="~/Pictures/right_16x16.gif"
                                                                NextPageText="&gt;" PreviousPageText="&lt;" ShowLastPageButton="True" ShowNextPageButton="True"
                                                                ShowPreviousPageButton="False" />
                                                        </Fields>
                                                    </asp:DataPager>--%>

                                                    <asp:DataPager runat="server" 
                                                                   ID="ContactsDataPager" PageSize="15">
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
                                                                 ToolTip="Click para mostrar los movimientos de la cuenta" />
                                            </td>
                                            <td style="text-align: left;" >
                                                <asp:Label ID="Label14" runat="server" Text='<%# Item.tEmpleado.Nombre %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label15" runat="server" Text='<%# Item.tEmpleado.FechaIngreso.ToString("dd-MM-yy") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="NombreTipoLabel" runat="server" Text='<%# Item.SueldoBasico_Mensual.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right; white-space: nowrap;" >
                                                <asp:Label ID="FechaLabel" runat="server" Text='<%# Item.SueldoBasico_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="SimboloMonedaOriginalLabel" runat="server" Text='<%# Item.BonoVacacional_Dias %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="DescripcionLabel" runat="server" Text='<%# Item.BonoVacacional_Monto.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="ProvieneDeLabel" runat="server" Text='<%# Item.BonoVacacional_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="NumPartidasLabel" runat="server" Text='<%# Item.Utilidades_Dias %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="TotalDebeLabel" runat="server" Text='<%# Item.Utilidades_Monto.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="TotalHaberLabel" runat="server" Text='<%# Item.Utilidades_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="Label13" runat="server" Text='<%# Item.SalarioIntegral_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="Label1" runat="server" Text='<%# Item.SalarioIntegral_Monto.ToString("N2") %>' />
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
                                            <td style="text-align: left;" >
                                                <asp:Label ID="Label14" runat="server" Text='<%# Item.tEmpleado.Nombre %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label15" runat="server" Text='<%# Item.tEmpleado.FechaIngreso.ToString("dd-MM-yy") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="NombreTipoLabel" runat="server" Text='<%# Item.SueldoBasico_Mensual.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right; white-space: nowrap;" >
                                                <asp:Label ID="FechaLabel" runat="server" Text='<%# Item.SueldoBasico_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="SimboloMonedaOriginalLabel" runat="server" Text='<%# Item.BonoVacacional_Dias %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="DescripcionLabel" runat="server" Text='<%# Item.BonoVacacional_Monto.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="ProvieneDeLabel" runat="server" Text='<%# Item.BonoVacacional_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="NumPartidasLabel" runat="server" Text='<%# Item.Utilidades_Dias %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="TotalDebeLabel" runat="server" Text='<%# Item.Utilidades_Monto.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="TotalHaberLabel" runat="server" Text='<%# Item.Utilidades_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="Label13" runat="server" Text='<%# Item.SalarioIntegral_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="Label1" runat="server" Text='<%# Item.SalarioIntegral_Monto.ToString("N2") %>' />
                                            </td>
                                        </tr>
                                    </AlternatingItemTemplate>
                                    <SelectedItemTemplate>
                                        <tr class="ListView_DarkBlue_SelectedItem">
                                            <td />
                                             <td style="text-align: left;" >
                                                <asp:Label ID="Label14" runat="server" Text='<%# Item.tEmpleado.Nombre %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="Label15" runat="server" Text='<%# Item.tEmpleado.FechaIngreso.ToString("dd-MM-yy") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="NombreTipoLabel" runat="server" Text='<%# Item.SueldoBasico_Mensual.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right; white-space: nowrap;" >
                                                <asp:Label ID="FechaLabel" runat="server" Text='<%# Item.SueldoBasico_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="SimboloMonedaOriginalLabel" runat="server" Text='<%# Item.BonoVacacional_Dias %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="DescripcionLabel" runat="server" Text='<%# Item.BonoVacacional_Monto.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="ProvieneDeLabel" runat="server" Text='<%# Item.BonoVacacional_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: center;" >
                                                <asp:Label ID="NumPartidasLabel" runat="server" Text='<%# Item.Utilidades_Dias %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="TotalDebeLabel" runat="server" Text='<%# Item.Utilidades_Monto.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="TotalHaberLabel" runat="server" Text='<%# Item.Utilidades_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="Label13" runat="server" Text='<%# Item.SalarioIntegral_Diario.ToString("N2") %>' />
                                            </td>
                                            <td style="text-align: right;" >
                                                <asp:Label ID="Label1" runat="server" Text='<%# Item.SalarioIntegral_Monto.ToString("N2") %>' />
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


                        <ajaxToolkit:TabPanel HeaderText="Agregar/Editar un registro" runat="server" ID="TabPanel3" >
                            <ContentTemplate>

                                <asp:FormView runat="server" 
                                              ID="NominaHeaders_FormView"
                                              ItemType="NominaASP.Models.tNominaHeader" 
                                              SelectMethod="Nomina_FormView_GetData"
                                              UpdateMethod="Nomina_FormView_UpdatItem"
                                              InsertMethod="Nomina_FormView_InsertItem"
                                              DeleteMethod="Nomina_FormView_DeleteItem"

                                              DataKeyNames="ID"

                                              OnItemInserted="NominaHeaders_FormView_ItemInserted"
                                              OnItemDeleted="NominaHeaders_FormView_ItemDeleted"
                                              OnItemUpdated="NominaHeaders_FormView_ItemUpdated" 

                                              BackColor="White"  
                                              CellPadding="3" >

                                    <ItemTemplate>
                                        <table cellspacing="10px" style="border: 1px solid #808080">
                                            <tr>
                                                <td style="font-weight: bold">Fecha de nómina: </td>
                                                <td>
                                                    <asp:Label ID="Label9" runat="server" Text='<%# Item.FechaNomina.ToString("d-MMM-yyyy") %>'></asp:Label>
                                                </td>
                                                <td>&nbsp;</td>
                                                <td style="font-weight: bold">Grupo de empleados: </td>
                                                <td>
                                                    <asp:Label ID="lblName" runat="server" Text='<%# Item.tGruposEmpleado.Descripcion %>'></asp:Label>
                                                </td>
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
                                                <td style="font-weight: bold">Cantidad de días: </td>
                                                <td>
                                                    <asp:Label ID="Label11" runat="server" Text='<%# Item.CantidadDias %>'></asp:Label>
                                                </td>
                                                <td>&nbsp;</td>
                                                <td style="font-weight: bold">Tipo de nómina: </td>
                                                <td>
                                                    <asp:Label ID="Label12" runat="server" Text='<%# DescripcionTipoNomina(Item.Tipo) %>'></asp:Label>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td style="font-weight: bold">Fecha de pago: </td>
                                                <td>
                                                    <asp:Label ID="FechaPago_Label" runat="server" Text='<%# Item.FechaPago == null ? "" : 
                                                                                                                 Item.FechaPago.Value.ToString("d-MMM-yyyy") %>' />
                                                </td>
                                                <td>&nbsp;</td>
                                                <td style="font-weight: bold">Descripción rubro sueldo: </td>
                                                <td>
                                                    <asp:Label ID="DescripcionRubroSueldo_Label" runat="server" Text='<%# Item.DescripcionRubroSueldo %>'></asp:Label>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td colspan="5">
                                                    <fieldset>
                                                        <legend style="color: #4A3C8C; font-weight: bold;">Determinar y agregar a la nómina: </legend>
                                                        <div style="margin: 10px; ">
                                                    
                                                            <asp:CheckBox ID="AgregarSueldoFlag_CheckBox" 
                                                                          runat="server"  
                                                                          Enabled="false"
                                                                          Checked='<%# Item.AgregarSueldo %>'
                                                                          Text="Sueldo básico (desde la maestra de empleados)" />

                                                            <br />

                                                            <asp:CheckBox ID="AgregarDedeccionesObligatorias_CheckBox" 
                                                                          runat="server" 
                                                                          Enabled="false"
                                                                          Checked='<%# Item.AgregarDeduccionesObligatorias %>'
                                                                          Text="Deducciones obligatorias (desde la tabla de deducciones obligatorias)" />
                                                         </div>   
                                                    </fieldset>
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
                                                <td>Fecha de nómina: </td>
                                                <td>
                                                    <asp:TextBox ID="FechaNomina_TextBox" runat="server" Text='<%# Bind("FechaNomina", "{0:dd/MM/yyyy}") %>' />
                                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender1" 
                                                                                  runat="server" 
                                                                                  TargetControlID="FechaNomina_TextBox" 
                                                                                  Format="d-MM-yyyy" />
                                                </td>
                                                <td>&nbsp;</td>
                                                <td>Grupo de empleados: </td>
                                                <td>
                                                    <asp:DropDownList ID="GrupoEmpleados_DropDownList" 
                                                                      runat="server"
                                                                      SelectMethod="Get_GrupoEmpleadosDropDownList_Items"
                                                                      ItemType="NominaASP.Models.tGruposEmpleado" 
                                                                      DataTextField="Descripcion"
                                                                      DataValueField="Grupo"
                                                                      SelectedValue="<%# BindItem.GrupoNomina %>" />
                                                </td>
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
                                                <td>Cantidad de días: </td>
                                                <td>
                                                    <asp:TextBox ID="CantidadDias_TextBox" runat="server" Text='<%# BindItem.CantidadDias %>' BackColor="#E9E9E9" />
                                                </td>
                                                <td>&nbsp;</td>
                                                <td>Tipo de nómina: </td>
                                                <td>
                                                    <asp:DropDownList ID="DropDownList2" 
                                                                      runat="server" 
                                                                      SelectedValue="<%# BindItem.Tipo %>" >
                                                        <asp:ListItem Text="1ra. quinc" Value="1Q" />
                                                        <asp:ListItem Text="2da. quinc" Value="2Q" />
                                                        <asp:ListItem Text="Quincenal" Value="Q" />
                                                        <asp:ListItem Text="Mensual" Value="M" />
                                                        <asp:ListItem Text="Vacaciones" Value="V" />
                                                        <asp:ListItem Text="Utilidades" Value="U" />
                                                        <asp:ListItem Text="Especial" Value="E" />
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td>Fecha de pago: </td>
                                                <td>
                                                    <asp:TextBox ID="FechaPago_TextBox" runat="server" Text='<%# Bind("FechaPago", "{0:dd/MM/yyyy}") %>' />
                                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender4" 
                                                                                  runat="server" 
                                                                                  TargetControlID="FechaPago_TextBox" 
                                                                                  Format="d-MM-yyyy" />
                                                </td>
                                                <td>&nbsp;</td>
                                                <td>Descripción rubro sueldo: </td>
                                                <td>
                                                    <asp:TextBox ID="DescripcionRubroSueldo_TextBox" runat="server" Text='<%# BindItem.DescripcionRubroSueldo %>' />
                                                </td>
                                            </tr>

                                            <tr>
                                                <td colspan="5">
                                                    <fieldset>
                                                        <legend style="color: #F7F7F7; font-weight: bold;">Determinar y agregar a la nómina: </legend>
                                                        <div style="margin: 10px; ">
                                                    
                                                            <asp:CheckBox ID="AgregarSueldoFlag_CheckBox" 
                                                                          runat="server"  
                                                                          Checked='<%# BindItem.AgregarSueldo %>'
                                                                          Text="Sueldo básico (desde la maestra de empleados)" />

                                                            <br />

                                                            <asp:CheckBox ID="AgregarDedeccionesObligatorias_CheckBox" 
                                                                          runat="server" 
                                                                          Checked='<%# BindItem.AgregarDeduccionesObligatorias %>'
                                                                          Text="Deducciones obligatorias (desde la tabla de deducciones obligatorias)" />
                                                         </div>   
                                                    </fieldset>
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
                                        <div>
                                            <span style="font-size: x-small; color: #0033CC">
                                                (Deje la cantidad de días en blanco para que el programa la calcule en forma automática, <br />
                                                en base a los valores indicados para <em>Desde</em> y <em>Hasta</em>; 
                                                Ud. siempre podrá indicar un valor luego<br />
                                                y el programa lo respetará) 
                                            </span>
                                        </div>
                                    </EditItemTemplate>

                                    <InsertItemTemplate>
                                        <table cellspacing="10px">
                                            <tr>
                                                <td>Fecha de nómina: </td>
                                                <td>
                                                    <asp:TextBox ID="FechaNomina_TextBox" runat="server" Text='<%# Bind("FechaNomina", "{0:dd/MM/yyyy}") %>' />
                                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender1" runat="server" TargetControlID="FechaNomina_TextBox" Format="d-MM-yyyy" />
                                                </td>
                                                <td>&nbsp;</td>
                                                <td>Grupo de empleados: </td>
                                                <td>
                                                    <asp:DropDownList ID="GrupoEmpleados_DropDownList" 
                                                                      runat="server"
                                                                      SelectMethod="Get_GrupoEmpleadosDropDownList_Items"
                                                                      ItemType="NominaASP.Models.tGruposEmpleado" 
                                                                      DataTextField="Descripcion"
                                                                      DataValueField="Grupo"
                                                                      SelectedValue="<%# BindItem.GrupoNomina %>" />
                                                </td>
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
                                                <td>Cantidad de días: </td>
                                                <td>
                                                    <asp:TextBox ID="CantidadDias_TextBox" runat="server" Text='<%# BindItem.CantidadDias %>' BackColor="#E9E9E9" />
                                                </td>
                                                <td>&nbsp;</td>
                                                <td>Tipo de nómina: </td>
                                                <td>
                                                    <asp:DropDownList ID="DropDownList2" 
                                                                      runat="server" 
                                                                      SelectedValue="<%# BindItem.Tipo %>" >
                                                        <asp:ListItem Text="1ra. quinc" Value="1Q" />
                                                        <asp:ListItem Text="2da. quinc" Value="2Q" />
                                                        <asp:ListItem Text="Quincenal" Value="Q" />
                                                        <asp:ListItem Text="Mensual" Value="M" />
                                                        <asp:ListItem Text="Vacaciones" Value="V" />
                                                        <asp:ListItem Text="Utilidades" Value="U" />
                                                        <asp:ListItem Text="Especial" Value="E" />
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td>Fecha de pago: </td>
                                                <td>
                                                    <asp:TextBox ID="FechaPago_TextBox" runat="server" Text='<%# Bind("FechaPago", "{0:dd/MM/yyyy}") %>' />
                                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender4" 
                                                                                  runat="server" 
                                                                                  TargetControlID="FechaPago_TextBox" 
                                                                                  Format="d-MM-yyyy" />
                                                </td>
                                                <td>&nbsp;</td>
                                                <td>Descripción rubro sueldo: </td>
                                                <td>
                                                    <asp:TextBox ID="DescripcionRubroSueldo_TextBox" runat="server" Text='<%# BindItem.DescripcionRubroSueldo %>' />
                                                </td>
                                            </tr>

                                            <tr>
                                                <td colspan="5">
                                                    <fieldset>
                                                        <legend style="color: #F7F7F7; font-weight: bold;">Determinar y agregar a la nómina: </legend>
                                                        <div style="margin: 10px; ">
                                                    
                                                            <asp:CheckBox ID="AgregarSueldoFlag_CheckBox" 
                                                                          runat="server"  
                                                                          Checked='<%# BindItem.AgregarSueldo %>'
                                                                          Text="Sueldo básico (desde la maestra de empleados)" />

                                                            <br />

                                                            <asp:CheckBox ID="AgregarDedeccionesObligatorias_CheckBox" 
                                                                          runat="server" 
                                                                          Checked='<%# BindItem.AgregarDeduccionesObligatorias %>'
                                                                          Text="Deducciones obligatorias (desde la tabla de deducciones obligatorias)" />
                                                         </div>   
                                                    </fieldset>
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
                                        <div>
                                            <span style="font-size: x-small; color: #0033CC">
                                                (Deje la cantidad de días en blanco para que el programa la calcule en forma automática, <br />
                                                en base a los valores indicados para <em>Desde</em> y <em>Hasta</em>; 
                                                Ud. siempre podrá indicar un valor luego<br />
                                                y el programa lo respetará) 
                                            </span>
                                        </div>
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
    

    <asp:SqlDataSource ID="Empleados_SqlDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:dbContabConnectionString %>" SelectCommand="SELECT Empleado, Nombre FROM tEmpleados Where Empleado In (Select Empleado From tNomina Where HeaderID = @HeaderID) ORDER BY Nombre">
        <SelectParameters>
            <asp:Parameter Name="HeaderID" Type="Int32" DefaultValue="-999" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="Rubros_SqlDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:dbContabConnectionString %>" SelectCommand="SELECT Rubro, Descripcion + ' (' + NombreCortoRubro + ')' AS Descripcion FROM tMaestraRubros Where Rubro In (Select Rubro From tNomina Where HeaderID = @HeaderID) ORDER BY Descripcion">
        <SelectParameters>
            <asp:Parameter Name="HeaderID" Type="Int32" DefaultValue="-999" />
        </SelectParameters>
    </asp:SqlDataSource>

</asp:Content>