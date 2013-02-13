<%@ Page Explicit="True" Language="c#" Debug="True" validateRequest="false" %>
<%@ import Namespace="System.Data" %>
<%@ import Namespace="System.Data.OleDb" %>
<%@ Import Namespace="System.Security.Cryptography" %>
<%@ Import Namespace="System.Text" %>
<script language="C#" runat='server'>
    
    void Page_Load()
    {
        //UserAuth.Text = User.Identity.Name;
        if ((Session["MySession"] == null))
        {
            span1.InnerHtml = "NOTHING, SESSION DATA LOST!";
        }
    }
    
    String Fixquotes(string thesqlenemy)
    {
        return thesqlenemy.Replace("\'", "\'\'");
    }

   /* void Session_Add(object sender, EventArgs e)
    {
        Session["MySession"] = txtUsername.Text;
      //  span1.InnerHtml = "Session data updated! Your session contains : " + (Session["MySession"].ToString() );
    }

    void Session_Remove(object sender, EventArgs e)
    {
        Session["MySession"] = null;
        //  span1.InnerHtml = "Session data updated! Your session contains : " + (Session["MySession"].ToString() );
    }

    void CheckSession(object sender, EventArgs e)
    {
        if ((Session["MySession"] == null))
        {
            span1.InnerHtml = "NOTHING, SESSION DATA LOST!";
        }
        else
        {
          //  span1.InnerHtml = "Your session contains:" + (Session["MySession"].ToString());
        }
    }*/

    public string CalculateMD5Hash(string input)
    {
        // step 1, calculate MD5 hash from input
        MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);

        // step 2, convert byte array to hex string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }
        return sb.ToString();
    }
    
    void btnLogin_OnClick(object Src, EventArgs E) 
    {
        if (Page.IsValid) 
        {
            //string MyAddress;
            OleDbConnection MyConn;
            string MySQL;
            string MyRs;
            OleDbCommand MyCount;
            OleDbCommand MyPassword;
            int IntUserCount;
            string strPassword;
            
            /*if (Request.QueryString["ReturnUrl"] == "")
            {
                MyAddress = "defaultc.aspx";
            }
            else 
            {
                MyAddress = Request.QueryString["ReturnUrl"];
            }
            */
            
            MySQL = ("SELECT COUNT(*) FROM [Users] WHERE [Username]=\'"
                        + (Fixquotes(txtUsername.Text) + "\'"));

            MyRs = ("SELECT Password FROM [Users] WHERE [Username]=\'"
                        + (Fixquotes(txtUsername.Text) + "\'"));
            
            MyConn = new OleDbConnection(("Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
                            + (Server.MapPath("Data/Userdb.mdb") + ";")));
            MyCount = new OleDbCommand(MySQL, MyConn);
            MyPassword = new OleDbCommand(MyRs, MyConn);
            
            MyConn.Open();
            IntUserCount = Convert.ToInt32(MyCount.ExecuteScalar());
            strPassword = Convert.ToString(MyPassword.ExecuteScalar());
            MyConn.Close();
            if (IntUserCount > 0) 
            {
                if (strPassword == CalculateMD5Hash(txtPassword.Text))
                {
                    FormsAuthentication.SetAuthCookie(txtUsername.Text, true);
                    Session["MySession"] = txtUsername.Text;
                    Response.Redirect("default.aspx");
                    
                }
                else 
                {
                    lblMsg.Text = "Invalid Password...";
                }
            }
            else 
            {
                lblMsg.Text = "Invalid Username...";
            }
        }
        else 
        {
            return;
        }
    }
</script>
<html xmlns="http://www.w3.org/1999/xhtml">

<head>
<meta http-equiv="Content-Language" content="en-us" />
<meta http-equiv="Content-Type" content="text/html; charset=windows-1252" />
<link rel="stylesheet" href="incs/styles.css" type="text/css" />
<script language="javascript" type="text/javascript">
<!--
    function onload()
    {
        document.getElementById('txtUsername').focus();
    }
//-->
</script>
<title>Login</title>
</head>

<body onload="onload();">

<div align="center">
    <form runat="server">
        <table cellpadding="0" cellspacing="2" width="250" height="115" class="tblMain">
            <tr>
                <td height="28" align="center" width="236" colspan="2">
                <asp:Label CssClass="Treb10Blue" Runat="server" ID="lblMsg"></asp:Label>
                </td>
            </tr>
            <tr>
                <td height="24" width="68"><font face="Tahoma" size="2">
                <label for="txtUsername">Username:</label></font></td>
                <td height="24" width="159">
                <asp:TextBox ID="txtUsername" CssClass="Treb10Blue" Runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator Runat="server" ErrorMessage="*" Display="Dynamic" ControlToValidate="txtUsername"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td height="24" width="68"><font face="Tahoma" size="2">
                <label for="txtPassword">Password:</label></font></td>
                <td height="24" width="159">
                <asp:TextBox ID="txtPassword" CssClass="Treb10Blue" Runat="server" TextMode="Password"></asp:TextBox>
                <asp:RequiredFieldValidator Runat="server" ErrorMessage="*" Display="Dynamic" ControlToValidate="txtPassword"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td height="29" align="right" width="74"></td>
                <td height="29" align="left" width="159">
                <asp:Button ID="btnLogin" Runat="server" CssClass="button" Text="Login" OnClick="btnLogin_OnClick"></asp:Button>
                </td>
            </tr>
        </table>
        
      </form>
      <hr size=1>
<font size=6><span id=span1 runat=server/></font>

    
</div>
<div align="center">
<a href="forgot.aspx">Forgot Password??</a>
<a href="register.aspx">Register now!</a>

<br>
<hr noshade size="1px" color="#b0bec7" width="80%">
    <font face="Tahoma" size="2"><a href="default.aspx">Here</a> is the page we 
    are actually protecting. If you are not logged in, you will be redirected to 
    this page again!</font></div>

</body>

</html>