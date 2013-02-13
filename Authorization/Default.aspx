<%@ Page Explicit="True" Language="c#" Debug="True" validateRequest="false" %>
<%@ import Namespace="System.Data" %>
<%@ import Namespace="System.Data.OleDb" %>
<script language="C#" runat='server'>
    
    void Page_Load()
    {
        UserAuth.Text = User.Identity.Name;
        if (Session["MySession"] == null)
        {
            Response.Redirect("login.aspx");
            span1.InnerHtml = "NOTHING IN SESSION!! LOGIN FIRST!!";
        }
        else
        {
            span1.InnerHtml = "Session data updated! Your session contains : " + (Session["MySession"].ToString());
        }
    }
    
    void doLogout(object Src, EventArgs E) 
    {
        FormsAuthentication.SignOut();
        Session["MySession"] = null;
        Response.Redirect("login.aspx");
    }
</script>
<html>

<head>
<meta http-equiv="Content-Language" content="en-us" />
<meta http-equiv="Content-Type" content="text/html; charset=windows-1252" />
<link rel="stylesheet" href="incs/styles.css" type="text/css" />
<title>Welcome</title>
</head>

<body>
<form id="Form1" runat="server">
    <table border="0" cellpadding="0" cellspacing="0" width="100%" height="20">
        <tr>
            <td height="20" width="50%"><font face="Tahoma" size="2">User<font color="#0000CC">
            <asp:Label Runat="server" ID="UserAuth"></asp:Label> </font>is 
            now logged in</font></td>
            <td height="20" width="50%" align="right">
            <font face="Verdana" size="2"><a href="cpassword.aspx">
            <font face="Tahoma" size="2">Change Password</font></a></font></td>
            <td height="20" width="50%" align="right">
            <font face="Verdana" size="2"><a href="login.aspx">
            <font face="Tahoma" size="2">
            <asp:LinkButton Runat="server" OnClick="doLogout" ID="linkLogout">Logout</asp:LinkButton>
            </font></a></font></td>
        </tr>
    </table>
</form>
<p><font face="Tahoma" size="2">Here is the protected area of the site where only 
logged in users have access.</font></p>

<hr size=1>
<font size=6><span id=span1 runat=server/></font>


</body>

</html>