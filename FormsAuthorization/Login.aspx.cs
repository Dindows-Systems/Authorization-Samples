using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

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
        string commaSeperatedRoles = string.Empty;

        //Authenticate user against the user database and obtain comma seperated roles
        if (!UserAuthentication.Instance.AuthenticateUser(txtUser.Text, txtPassword.Text, out commaSeperatedRoles))
        {
            lblLoginFailed.Visible = true;
            return;
        }

        //Instead of FormsAuthentication.RedirectFromLoginPage(txtUser.Text, false);
        //Use the following code
        FormsAuthenticationUtil.RedirectFromLoginPage(txtUser.Text, commaSeperatedRoles, true);
  
    }

    
}
