<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage_Simple.Master" AutoEventWireup="true" CodeBehind="OpcionesReporte.aspx.cs" Inherits="NominaASP.Nomina.Nomina.OpcionesReporte" ValidateRequest="false" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <script type="text/javascript">
        function PopupWin(url, w, h) {
            ///Parameters url=page to open, w=width, h=height
            var left = parseInt((screen.availWidth / 2) - (w / 2));
            var top = parseInt((screen.availHeight / 2) - (h / 2));
            window.open(url, "external", "width=" + w + ",height=" + h + ",resizable=yes,scrollbars=yes,status=no,location=no,toolbar=no,menubar=no,left=" + left + ",top=" + top + "screenX=" + left + ",screenY=" + top);
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

            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
   
</asp:Content>
