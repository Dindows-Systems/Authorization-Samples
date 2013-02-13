<%@ Page Explicit="True" Language="C#" Debug="True" validateRequest="false" %>
<%@ import Namespace="System.Data" %>
<%@ import Namespace="System.Data.OleDb" %>
<%@ Import Namespace="System.Security.Cryptography" %>
<%@ Import Namespace="System.Text" %>
<script language="C#" runat='server'>
    
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

    void Createacc(object sender, EventArgs e)
    {
        if (Page.IsValid) 
        {
            OleDbConnection objConn = new OleDbConnection(("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" 
                            + (Server.MapPath("Data/Userdb.mdb") + ";")));
            IDbCommand chkUsername;
            IDbCommand addUser;
            string strSQL1;
            int strUserCount;
            
            strSQL1 = ("SELECT COUNT(*) FROM [Users] WHERE [Username]=\'" 
                        + (Fixquotes(txtUsername.Text) + "\'"));
            string strSQL2 = ("INSERT INTO [Users] ([Fullname], [Email], [Username], [Answer], [Password]) " + "VALUES ");
            strSQL2 = (strSQL2 + ("(\'" 
                        + (Fixquotes(txtFullname.Text) + ("\', \'" 
                        + (Fixquotes(txtEmail.Text) + ("\', \'" 
                        + (Fixquotes(txtUsername.Text) + ("\', \'" 
                        + (CalculateMD5Hash(txtAns.Text) + ("\', \'"
                        + (CalculateMD5Hash(txtPassword.Text) + "\');")))))))))));
            
            OleDbCommand objCmd = new OleDbCommand(strSQL1, objConn);
            
            objConn.Open();
            chkUsername = new OleDbCommand(strSQL1, objConn);
            strUserCount = Convert.ToInt32(chkUsername.ExecuteScalar());
            if ((strUserCount == 0)) 
            {
                addUser = new OleDbCommand(strSQL2, objConn);
                addUser.ExecuteNonQuery();
                objConn.Close();
                Response.Redirect("login.aspx");
            }
            else 
            {
                lblMsg.Text = "Username already exists. Please choose another...";
            }
            objConn.Close();
        }
    }
</script>
<html>

<head>
<meta http-equiv="Content-Language" content="en-us" />
<meta http-equiv="Content-Type" content="text/html; charset=windows-1252" />
<link rel="stylesheet" href="incs/styles.css" type="text/css" />
<title>Register</title>
</head>

<body>

<form runat="server" language="c#">
    <div align="center">
        <table cellpadding="0" cellspacing="2" width="397" height="169" class="tblMain">
            <tr>
                <td height="32" align="center" width="383" colspan="2">
                <asp:Label CssClass="Treb10Blue" ID="lblMsg" Runat="Server"></asp:Label>
                </td>
            </tr>
            <tr>
                <td height="27" width="129"><font face="Tahoma" size="2">
                <label for="txtUsername">Choose a Username:</label></font></td>
                <td height="27" width="244">
                <asp:TextBox ID="txtUsername" CssClass="Treb10Blue" Runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator Runat="server" ErrorMessage="*" Display="Dynamic" ControlToValidate="txtUsername"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td height="27" width="129"><font face="Tahoma" size="2">
                <label for="txtPassword">Choose a password:</label></font></td>
                <td height="27" width="244">
                <asp:TextBox ID="txtPassword" CssClass="Treb10Blue" Runat="server" TextMode="Password"></asp:TextBox>
                <asp:RequiredFieldValidator Runat="server" ErrorMessage="*" Display="Dynamic" ControlToValidate="txtPassword"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td height="27" width="129"><font face="Tahoma" size="2">
                <label for="txtFullname">Your Fullname:</label></font></td>
                <td height="27" width="244">
                <asp:TextBox ID="txtFullname" CssClass="Treb10Blue" Runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator Runat="server" ErrorMessage="*" Display="Dynamic" ControlToValidate="txtFullname"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td height="27" width="129"><font face="Tahoma" size="2">
                <label for="txtEmail">Your Email address:</label></font></td>
                <td height="27" width="244">
                <asp:TextBox ID="txtEmail" CssClass="Treb10Blue" Runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator Runat="server" ErrorMessage="*" Display="Dynamic" ControlToValidate="txtEmail"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ControlToValidate="txtEmail" ValidationExpression=".*@.*\..*" ErrorMessage="*" Display="Dynamic" Runat="server"></asp:RegularExpressionValidator>
                </td>
            </tr>
            <tr>
                <td height="27" width="129"><font face="Tahoma" size="2">
                <label for="txtAns">Secret Code:</label></font></td>
                <td height="27" width="244">
                <asp:TextBox ID="txtAns" CssClass="Treb10Blue" Runat="server" TextMode="Password"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" Runat="server" ErrorMessage="*" Display="Dynamic" ControlToValidate="txtAns"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td height="29" width="129"> </td>
                <td height="29" width="244">
                <asp:Button ID="btnRegister" Runat="server" CssClass="button" Text="Register" OnClick="Createacc"></asp:Button>
                </td>
            </tr>
        </table>
    </div>
    <div align="center">
 <hr noshade size="1px" color="#b0bec7" width="80%">
        <font face="Tahoma" size="2">Just remembered you already have an account? 
        Login <a href="login.aspx">Here!</a></font></div>
</form>

</body>

</html>