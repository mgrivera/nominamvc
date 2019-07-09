<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage_Simple.Master" AutoEventWireup="true" CodeBehind="OpcionesReporte.aspx.cs" Inherits="NominaASP.Nomina.Nomina.OpcionesReporte" ValidateRequest="false" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <script type="text/javascript">
        function PopupWin(url, w, h) {
            ///Parameters url=page to open, w=width, h=height
            myWindow = window.open(url, "external2", "width=" + w + ",height=" + h + ",resizable=yes,scrollbars=yes,status=no,location=no,toolbar=no,menubar=no,top=10px,left=8px");
            myWindow.focus();
        }
    </script>

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
            <div>

                <asp:HiddenField ID="FileName_HiddenField" 
                                 runat="server" />
    
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

        
                <fieldset style="margin: 0px 25px 25px 25px; padding: 15px; text-align: center; " class="generalfont">
                    <legend>Opciones para la obtención del reporte: </legend>

                    <div style="margin: 10px; ">
                        <asp:RadioButton ID="AgruparPorEmpleado_RadioButton" 
                            runat="server" 
                            Text="Agrupar por empleado" 
                            GroupName="AgruparConsultaPor" AutoPostBack="True" OnCheckedChanged="SetLinkAddress" />

                        <asp:RadioButton ID="AgruparPorRubro_RadioButton" 
                            runat="server" 
                            Text="Agrupar por rubro" 
                            GroupName="AgruparConsultaPor" AutoPostBack="True" OnCheckedChanged="SetLinkAddress"/>
                    </div>
                    
                    <div  style="margin: 20px 0px 0px; text-align: right; ">
                        <a runat="server" 
                           href="javascript:PopupWin('../../ReportViewer.aspx?rpt=consultaNomina&headerID=0', 1000, 680)"
                           id="ObtenerReporte_HtmlAnchor">Imprimir</a>
                    <div>
                </fieldset>

                <br />

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

            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
   
</asp:Content>
