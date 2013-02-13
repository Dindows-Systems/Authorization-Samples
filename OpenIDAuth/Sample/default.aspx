<%@ Page Language="C#" AutoEventWireup="true" Inherits="DefaultPage" Codebehind="default.aspx.cs" %>

<%@ Register src="../OpenIDControl.ascx" tagname="OpenIDControl" tagprefix="uc1" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">
<html>
<head>
   <meta http-equiv="Pragma" content="no-cache" />
   <meta http-equiv="Expires" content="-1" />
   <title>OpenId Web Authentication Sample</title>
       <style>.openid_url { background: url(http://www.openid.net/login-bg.gif) no-repeat; background-color: #fff; background-position: 0 50%; color: #000; padding-left: 18px; border-style: solid; border-color: black; border-width : 1px; }</style>

 
</head>

<body>
    <form id="form1" runat="server">
    <table width="320"><tr><td>
    <h1>Welcome to the C# Sample for the OpenId Membership Provider Integration App</h1>
        <p>&nbsp;</p>

        <uc1:OpenIDControl ID="OpenIDControl1" runat="server" RequiredFields="email" OptionalFields="gender,fullname" />

 
  <asp:Label ID="lblWelcome" runat="server"></asp:Label>
 
  

                  

</td></tr></table>

<asp:Panel ID="pnlRegister" runat="server" Visible ="false">
        <asp:Label ID="Label1" runat="server"></asp:Label>&nbsp;<br />
        <br />
        <div style="text-align: left">
            <table border="0" cellpadding="2" cellspacing="2" style="width: 316px">
                <caption>
                    Registration</caption>
                <tr>
                    <td style="width: 100px">
                        UserName</td>
                    <td style="width: 100px">
                        <asp:TextBox ID="txtUserName" runat="server"></asp:TextBox></td>
                </tr>
                <tr>
                    <td style="width: 100px">
                        First Name</td>
                    <td style="width: 100px">
                        <asp:TextBox ID="txtFirstName" runat="server"></asp:TextBox></td>
                </tr>
                 <tr>
                    <td style="width: 100px">
                        Last Name</td>
                    <td style="width: 100px">
                        <asp:TextBox ID="txtLastName" runat="server"></asp:TextBox></td>
                </tr>
                <tr>
                    <td style="width: 100px">
                        Email</td>
                    <td style="width: 100px">
                        <asp:TextBox ID="txtEmail" runat="server"></asp:TextBox></td>
                </tr>
                <tr>
                    <td style="width: 100px">
                        Receive Newsletter?</td>
                    <td style="width: 100px">
                        <asp:CheckBox ID="chkNewsLetter" runat="server" Checked="True" Text="Yes" /></td>
                </tr>
                <tr>
                    <td style="width: 100px">
                    </td>
                    <td style="width: 100px">
                        <asp:Button ID="btnRegister" runat="server" OnClick="btnRegister_Click" Text="Register" /></td>
                </tr>
                <tr>
                    <td colspan=2>
                    <asp:Label ID=lblResult runat=server></asp:Label>
                    </td>
                  
                </tr>
                
            </table>
            </div>
        </asp:Panel>
                    
    
    
    </form>
   </body>
</html>
