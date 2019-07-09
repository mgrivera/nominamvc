<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage_Simple.Master" AutoEventWireup="true" CodeBehind="EmpleadosFaltas_OpcionesReporte.aspx.cs" Inherits="NominaASP.Empleados.Faltas.EmpleadosFaltas_OpcionesReporte" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    
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
            
            <div style="margin: 20px; ">
                <asp:RadioButton ID="NormalFormat_RadioButton" runat="server" GroupName="ReportFormat" Text="Normal" />
                <asp:RadioButton ID="PdfFormat_RadioButton" runat="server" GroupName="ReportFormat" Text="Pdf" />
            </div>

            <asp:Button ID="ObtenerReporte_Button" 
                        runat="server" 
                        Text="ObtenerReporte" 
                        onclick="ObtenerReporte_Button_Click" />

            <br />

        </fieldset>

</asp:Content>
