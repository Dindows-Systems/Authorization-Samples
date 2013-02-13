<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>
    <link rel="Stylesheet" href="style/style.css" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <table align="center" width="300px" style="margin-top: 200px;">
        <tr>
            <td>
                <fieldset style="width: 300px;" align="middle">
                    <legend>Login</legend>
                    <table style="width: 30%;" align="center">
                        <tr>
                            <td>
                                <asp:Label ID="Label1" runat="server" Text="User"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtUser" runat="server"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="Label2" runat="server" Text="Password"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtPassword" TextMode="Password" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                &nbsp;
                            </td>
                            <td>
                                <asp:Button runat="server" Text="Login" ID="btnLogin" OnClick="btnLogin_Click"></asp:Button>
                            </td>
                        </tr>
                    </table>
                </fieldset>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblLoginFailed" CssClass="error" Visible="false" runat="server" Text="Login failed. Use 'Administrator/123' or 'John/123'"></asp:Label>
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
