<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MostrarPdfWordFile.aspx.cs" Inherits="NominaASP.MostrarPdfWordFile" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ValidationSummary ID="ValidationSummary1" 
                               runat="server" 
                               class="errmessage_background generalfont errmessage"
                               ShowModelStateErrors="true"
                               ForeColor="" />

        <asp:CustomValidator ID="CustomValidator1" runat="server" ErrorMessage="CustomValidator" Display="None" EnableClientScript="False" />
    </div>
    </form>
</body>
</html>
