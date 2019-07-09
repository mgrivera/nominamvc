<%@ Page Title="Opciones de reportes" Language="C#" MasterPageFile="~/MasterPage_Simple.master" AutoEventWireup="true" Inherits="NominaASP.Nomina.PrestacionesSociales.PrestacionesSociales_ObtencionTxtFile" Codebehind="PrestacionesSociales_ObtencionTxtFile.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

        <asp:HiddenField ID="FileName_HiddenField" runat="server" />

        <asp:ValidationSummary ID="ValidationSummary1" 
                                runat="server" 
                                class="errmessage_background generalfont errmessage"
                                ShowModelStateErrors="true"
                                ForeColor="" />

        <asp:CustomValidator ID="CustomValidator1" 
                                runat="server" 
                                ErrorMessage="CustomValidator" 
                                Display="None" 
                                EnableClientScript="False" />

        <fieldset style="margin: 10px; width: 400px; padding: 10px; " 
                    runat="server" 
                    id="DatosTxtFile_Fieldset">
            <legend style="color: Blue; ">Datos para la obtención del archivo (txt): </legend>
        
                <table>
                    <tr>
                        <td style="text-align: right; ">Código de contrato: </td>
                        <td>&nbsp;&nbsp;&nbsp;</td>
                        <td style="text-align: left; "><asp:TextBox ID="CodigoContrato_TextBox" runat="server"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td style="text-align: right; ">Fecha: </td>
                        <td>&nbsp;&nbsp;&nbsp;</td>
                        <td style="text-align: left; ">
                            <asp:TextBox ID="Fecha_TextBox" runat="server"></asp:TextBox>
                            <ajaxToolkit:CalendarExtender ID="CalendarExtender1" 
                                                            runat="server" 
                                                            TargetControlID="Fecha_TextBox" 
                                                            Format="d-MM-yyyy" />

                        </td>


                    </tr>
                    <tr>
                        <td style="text-align: right; ">Oficina: </td>
                        <td>&nbsp;&nbsp;&nbsp;</td>
                        <td style="text-align: left; "><asp:TextBox ID="Oficina_TextBox" runat="server"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td style="text-align: right; ">Código area: </td>
                        <td>&nbsp;&nbsp;&nbsp;</td>
                        <td style="text-align: left; "><asp:TextBox ID="CodigoArea_TextBox" runat="server"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td style="text-align: right; ">Centro costo: </td>
                        <td>&nbsp;&nbsp;&nbsp;</td>
                        <td style="text-align: left; "><asp:TextBox ID="CentroCosto_TextBox" runat="server"></asp:TextBox></td>
                    </tr>
                </table>

        </fieldset>

        <br />

        <fieldset style="margin: 10px; width: 400px; padding: 10px; text-align: left; " 
                    runat="server" 
                    id="Fieldset1">
            <legend style="color: Blue; ">Opciones de construcción del archivo: </legend>

            <asp:CheckBox ID="AgregarEncabezados_CheckBox" 
                            runat="server" 
                            Text="Agregar encabezados para las secciones (2) del archivo" />

            <br />

            <asp:CheckBox ID="AgregarDelimitadores_CheckBox" 
                            runat="server" 
                            Text="Agregar delimitadores para separar los datos del archivo" />

        </fieldset>

        <br />

        <div class="notsosmallfont" style="margin: 10px; width: 400px; padding: 10px; text-align: left; ">
            <b>Nota:</b> Agregar encabezados y delimitadores facilita la revisión del archivo al abrirlo usando algún programa adecuado; 
            sin embargo, cuando el archivo deba ser enviado a la institución específica, debe ser obtenido sin encabezados ni delimitadores. 
        </div>

        <br />

        <asp:Button ID="Button1" 
                    runat="server" 
                    Text="Obtener archivo" 
                    onclick="Button1_Click" 
                    style="margin: 0px 0px 25px 0px; " />

        <div style="text-align: right; ">
            <asp:LinkButton ID="ObtencionArchivoRetencionesIva_DownloadFile_LinkButton" 
                            runat="server" 
                            onclick="ObtencionArchivoRetencionesIva_DownloadFile_LinkButton_Click"
                            Visible="False">
                Copiar el archivo al disco duro local
            </asp:LinkButton>
        </div>




        <%--para mostrar un diálogo que permita al usuario continuar/cancelar--%> 
        <asp:Panel ID="pnlPopup" runat="server" CssClass="modalpopup" style="display:none">
            <div class="popup_container" style="width: 500px; ">
                <div class="popup_form_header" style="overflow: hidden; ">
                    <div id="ModalPopupTitle_div" style="width: 85%; margin-top: 5px;  float: left; ">
                        <span runat="server" id="ModalPopupTitle_span" style="font-weight: bold; display:block; text-align: left; "/> 
                    </div>
                    <div style="width: 15%; float: right; ">
                        <asp:ImageButton ID="ImageButton1" runat="server" OnClientClick="$find('popup').hide(); return false;" ImageUrl="~/Pictures/PopupCloseButton.png" />
                    </div>
                </div>
                <div class="inner_container">
                    <div class="popup_form_content">
                        <span runat="server" id="ModalPopupBody_span" style="display:block; text-align: left; "/> 
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

</asp:Content>

