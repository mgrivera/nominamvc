<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="NominaASP.ReportViewer" %>
<%--<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>--%>

<!-- Update version to 15.0.0.0 -->
<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Styles/general.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
    <%-- usamos una tabla (y no un div) pues no se muestra si no tiene contenido   --%>
    <table>
        <tr>
            <td id="ErrMessage_Cell" runat="server" class="generalfont errmessage errmessage_background">
            </td>
        </tr>
    </table>
    
    <div>
        <%--Just for this report to print on google chrome ...--%> 
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" SizeToReportContent="True" Width="100%" Height="100%">
        </rsweb:ReportViewer>
    </div>

    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>

    </form>
</body>
</html>
