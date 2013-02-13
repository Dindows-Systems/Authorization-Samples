using System;
using System.Web;
using System.IO;
using ExtremeSwank.Authentication.OpenID;
using PAB.Web.Providers;


/// <summary>
/// This is the default aspx.cs page for the sample Web Auth site.
/// It gets the application ID and user ID for display on the main
/// page.  
/// </summary>
public partial class Default2 : System.Web.UI.Page
{





   protected SessionPersister persister = null;
    
    protected void Page_Load(object sender, EventArgs e)
    {
        persister = new SessionPersister();
        OpenIDUser oiu = (OpenIDUser) persister["UserObject"];
        if(oiu==null)
            Server.Transfer("~/Sample/default.aspx");

        lblMessage.Text = "User " + oiu.Identity + " logged in.";

        
        

         

    }


   
}
