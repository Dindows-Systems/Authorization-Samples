<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>User Page</title>
    <link rel="Stylesheet" href="../style/style.css"type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    
    <div style="text-align:center;margin-top:200px;">
        <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
        <asp:LinkButton ID="lnkLogout" CssClass="anchor" runat="server" onclick="lnkLogout_Click">Log out</asp:LinkButton>
    </div>
    </form>
</body>
</html>
