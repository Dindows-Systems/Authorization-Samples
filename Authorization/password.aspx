<%@ Page Explicit="True" Language="c#" Debug="True" validateRequest="false" %>
<%@ import Namespace="System.Data" %>
<%@ import Namespace="System.Data.OleDb" %>
<%@ Import Namespace="System.Security.Cryptography" %>
<%@ Import Namespace="System.Text" %>
<script language="C#" runat='server'>
    
    void Page_Load()
    {
        UserAuth.Text = User.Identity.Name;
        if (Session["MySession"] == null)
        {
            Response.Redirect("login.aspx");
        }
    }
    
    String Fixquotes(string thesqlenemy)
    {
        return thesqlenemy.Replace("\'", "\'\'");
    }

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
    
    void genpass(object Src, EventArgs E)
    {
        if (Page.IsValid)
        {
            //string MyAddress;
            OleDbConnection MyConn;
            OleDbCommand MyCount;
            OleDbCommand addUser;


            string strSQL = "UPDATE [Users] SET [Password]='" + CalculateMD5Hash(txtPassword.Text) + "' WHERE [Username]='" + Fixquotes(UserAuth.Text) + "';";
           
 
            MyConn = new OleDbConnection(("Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
                            + (Server.MapPath("Data/Userdb.mdb") + ";")));
            MyCount = new OleDbCommand(strSQL, MyConn);


            MyConn.Open();
            
            addUser = new OleDbCommand(strSQL, MyConn);
            addUser.ExecuteNonQuery();
            MyConn.Close();
            Session["MySession"] = null;
            Response.Redirect("login.aspx");
            
        }
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <link rel="stylesheet" href="incs/styles.css" type="text/css" />
    <script language="javascript" type="text/javascript">
    <!--
        function onload()
        {
            document.getElementById('txtUsername').focus();
        }
    //-->
    </script>

    <title>New Password</title>
</head>

<body onload="onload();">
<div align="center">
    <form id="Form1" runat="server">
        <table cellpadding="0" cellspacing="2" width="250" height="115" class="tblMain">
            <tr>
                <td height="28" align="center" width="236" colspan="2">
                <asp:Label CssClass="Treb10Blue" Runat="server" ID="UserAuth"></asp:Label>
                </td>
            </tr>
            <tr>
                <td height="27" width="129"><font face="Tahoma" size="2">
                <label for="txtPassword">Choose a new password:</label></font></td>
                <td height="27" width="244">
                <asp:TextBox ID="txtPassword" CssClass="Treb10Blue" Runat="server" TextMode="Password"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" Runat="server" ErrorMessage="*" Display="Dynamic" ControlToValidate="txtPassword"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td height="29" align="right" width="74"></td>
                <td height="29" align="left" width="159">
                <asp:Button ID="forgot1" Runat="server" CssClass="button" Text="Change" OnClick="genpass"></asp:Button>
                </td>
            </tr>
        </table>
    </form>
    
</div>    
</body>
</html>