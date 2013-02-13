using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void btnLogin_Click(object sender, EventArgs e)
    {
        Authenticate(txtUser.Text, txtPassword.Text);
    }

    /// <summary>
    /// Authenticates user and redirects to the originally request page is authentication
    /// is successful
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    private void Authenticate(string userName, string password)
    {
        FormsAuthenticationUtil.RedirectFromLoginPage("Lewis", "Administrators", true);
    }

    

}
