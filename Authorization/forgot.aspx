<%@ Page Explicit="True" Language="c#" Debug="True" validateRequest="false" %>
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


    
    void reload(object Src, EventArgs E)
    {
        if (Page.IsValid)
        {
            //string MyAddress;
            OleDbConnection MyConn;
            string MySQL;
            string MyRs;
        
            OleDbCommand MyCount;
            OleDbCommand MyAns;
        
            int IntUserCount;
            string strAns;
        
           
            MySQL = "SELECT COUNT(*) FROM [Users] WHERE [Username]='" + Fixquotes(txtUsername.Text) + "';";

            MyRs = ("SELECT Answer FROM [Users] WHERE [Username]=\'"
                        + (txtUsername.Text + "\'"));
            
        
            MyConn = new OleDbConnection(("Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
                            + (Server.MapPath("Data/Userdb.mdb") + ";")));
            MyCount = new OleDbCommand(MySQL, MyConn);
            MyAns = new OleDbCommand(MyRs, MyConn);
        
            MyConn.Open();
            IntUserCount = Convert.ToInt32(MyCount.ExecuteScalar());
            strAns = Convert.ToString(MyAns.ExecuteScalar());
        
            MyConn.Close();
            if (IntUserCount > 0)
            {
                if (strAns == CalculateMD5Hash(txtAns.Text))
                {
                    FormsAuthentication.SetAuthCookie(txtUsername.Text, true);
                    Session["MySession"] = txtUsername.Text;
                    Response.Redirect("password.aspx");
                }

                else
                {
                    lblMsg.Text = "Invalid";
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
    <link rel="stylesheet" href="incs/styles.css" type="text/css" />
    <script language="javascript" type="text/javascript">
    <!--
        function onload()
        {
            document.getElementById('txtUsername').focus();
        }
    //-->
    </script>

    <title>Forgot Password</title>
</head>

<body onload="onload();">
<div align="center">
    <form id="Form1" runat="server">
        <table cellpadding="0" cellspacing="2" width="250" height="115" class="tblMain">
            <tr>
                <td height="28" align="center" width="236" colspan="2">
                <asp:Label CssClass="Treb10Blue" Runat="server" ID="lblMsg"></asp:Label>
                </td>
            </tr>
            <tr>
                <td height="27" width="169"><font face="Tahoma" size="2">
                <label for="txtUsername">Username:</label></font></td>
                <td height="27" width="244">
                <asp:TextBox ID="txtUsername" CssClass="Treb10Blue" Runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" Runat="server" ErrorMessage="*" Display="Dynamic" ControlToValidate="txtUsername"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td height="27" width="169"><font face="Tahoma" size="2">
                <label for="txtEmail">Your Email address:</label></font></td>
                <td height="27" width="244">
                <asp:TextBox ID="txtEmail" CssClass="Treb10Blue" Runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" Runat="server" ErrorMessage="*" Display="Dynamic" ControlToValidate="txtEmail"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="RegularExpressionValidator1" ControlToValidate="txtEmail" ValidationExpression=".*@.*\..*" ErrorMessage="*" Display="Dynamic" Runat="server"></asp:RegularExpressionValidator>
                </td>
            </tr>
            
            <tr>
                <td height="27" width="169"><font face="Tahoma" size="2">
                <label for="txtAns">Secret Code:</label></font></td>
                <td height="27" width="244">
                <asp:TextBox ID="txtAns" CssClass="Treb10Blue" Runat="server" TextMode="Password"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" Runat="server" ErrorMessage="*" Display="Dynamic" ControlToValidate="txtAns"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td height="29" align="right" width="74"></td>
                <td height="29" align="left" width="159">
                <asp:Button ID="forgot1" Runat="server" CssClass="button" Text="Next >>" OnClick="reload"></asp:Button>
                </td>
            </tr>
        </table>
    </form>
    
</div>    
</body>
</html>