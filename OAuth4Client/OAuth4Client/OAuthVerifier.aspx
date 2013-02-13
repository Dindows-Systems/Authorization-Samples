<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OAuthVerifier.aspx.cs"
    Inherits="OAuth4Client.OAuthVerifier" %>
<%@ Import Namespace="OAuth4Client.OAuth" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        input[type=text]
        {
            width:500px;   
        }
        span
        {
            display:inline-block;
            width:120px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <h1>OAuth(<%=OContext.OAuthVersion%>) <%=OContext.SocialSiteName%></h1>        
        <div style="display: <%=OContext.OAuthVersion == OAuthVersion.V1 ? "block":"none" %>" >
            <span>Token:</span><asp:TextBox ID="txtTokenKey" runat="server"></asp:TextBox><br />
            <span>Token Secret:</span><asp:TextBox ID="txtTokenSecret" runat="server"></asp:TextBox><br />
            <span>Verifier:</span><asp:TextBox ID="txtVerifier" runat="server"></asp:TextBox>
            <br /><br />
        </div>      
        
        <asp:Button ID="btnGetAccessToken" OnClick="btnGetAccessToken_Click" runat="server"
            Text="Get Access Token" />
        <br />
        <br />
        <div runat="server" visible="false" id="divAccessToken">
            <span>Access Token:</span><asp:TextBox ID="txtAccessToken" runat="server"></asp:TextBox><br />            
            <span>Access Secret:</span><asp:TextBox ID="txtAccessTokenSecret" runat="server"></asp:TextBox>                        
            <br /><br/>
            <asp:Button ID="btnResponse" runat="server" Text="GetResponse" OnClick="btnResponse_Click" />
        </div>
        <div id="divResponse" visible="false" runat="server">
            <br />
            <span>Response text:</span><br />
            <asp:TextBox ID="txtResponse" Width="100%" TextMode="MultiLine" Rows="10" runat="server"></asp:TextBox>
        </div>   <br/>       
        <asp:HyperLink runat="server" ID="hHome" NavigateUrl="/Default.aspx" Text="Go Home"></asp:HyperLink></>
    </form>
</body>
</html>
