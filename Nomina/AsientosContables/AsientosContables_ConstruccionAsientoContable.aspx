<%@ Page Title="Opciones de reportes" Language="C#" MasterPageFile="~/MasterPage_Simple.master" AutoEventWireup="true" Inherits="NominaASP.Nomina.AsientosContables.AsientosContables_ConstruccionAsientoContable" Codebehind="AsientosContables_ConstruccionAsientoContable.aspx.cs" %>

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

        <fieldset style="margin: 10px; padding: 10px; " 
                    runat="server" 
                    id="DatosTxtFile_Fieldset">
            <legend style="color: Blue; ">Datos para la construcción del asiento contable: </legend>
        
                <table>
                    <tr>
                        <td style="text-align: right; vertical-align: top; ">Descripción: </td>
                        <td>&nbsp;&nbsp;&nbsp;</td>
                        <td style="text-align: left; ">
                            <asp:TextBox ID="Descripcion_TextBox" 
                                         runat="server" 
                                         Rows="3" 
                                         TextMode="MultiLine" 
                                         Width="400"/>
                        </td>
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
                        <td style="text-align: right; ">Factor de cambio: </td>
                        <td>&nbsp;&nbsp;&nbsp;</td>
                        <td style="text-align: left; "><asp:TextBox ID="FactorCambio_TextBox" runat="server"></asp:TextBox></td>
                    </tr>

                    <tr>
                        <td style="text-align: right; " colspan="3">
                            <fieldset style="margin: 10px; padding: 10px; text-align: left; width: 300px; " 
                                        runat="server" 
                                        id="Fieldset1">
                                <legend style="color: Blue; ">Orden de las partidas del asiento: </legend>

                                <asp:RadioButton ID="OrderByEmpleado_RadioButton" runat="server" GroupName="OrdenAsiento_Group" Text="Empleado" />
                                <br />
                                <asp:RadioButton ID="OrderByRubro_RadioButton" runat="server" GroupName="OrdenAsiento_Group" Text="Rubro" />
                                <br />
                                <asp:RadioButton ID="OrderByDefault_RadioButton" runat="server" GroupName="OrdenAsiento_Group" Text="No ordenar" />
                                <br />

                            </fieldset>
                        </td>
                    </tr>
                   
                </table>

        </fieldset>

        <div class="notsosmallfont" style="margin: 10px; width: 400px; padding: 10px; text-align: left; ">
            <b>Nota:</b> independientemente del ordenamiento que Ud. seleccione arriba, las partidas del asiento 
            que corresponden al monto a pagar a cada empleado, serán siempre agregadas al final del asiento. 
        </div>

        <br />

        <asp:Button ID="ConstruirAsiento_Button" 
                    runat="server" 
                    Text="Construir asiento contable" 
                    onclick="Button1_Click" 
                    style="margin: 0px 0px 25px 0px; " />

        

        <%--para mostrar un diálogo que permita al usuario continuar/cancelar--%> 
        <asp:Panel ID="pnlPopup" runat="server" CssClass="modalpopup" style="display:none">
            <div class="popup_container" style="width: 500px; ">
                <div class="popup_form_header" style="overflow: hidden; ">
                    <div id="ModalPopupTitle_div" style="width: 85%; margin-top: 5px;  float: left; ">
                        <span runat="server" id="ModalPopupTitle_span" style="font-weight: bold; display:block; text-align: left;"/> 
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

