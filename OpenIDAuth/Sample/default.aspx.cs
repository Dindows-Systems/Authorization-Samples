using System;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;
using System.IO;
using System.Web.Security;
using ExtremeSwank.Authentication.OpenID;
using PAB.Web.Providers;



/// <summary>
/// This is the default aspx.cs page for the sample Web Auth site.
/// It gets the application ID and user ID for display on the main
/// page.  
/// </summary>
public partial class DefaultPage : System.Web.UI.Page
{
 
    
    protected void Page_Load(object sender, EventArgs e)
    {
         
    }


    protected void OpenID_ValidateSuccess(object sender, EventArgs e)
    {
        
    }

    public OpenIDUser OpenIDUser
    {
        get
        {
            return OpenIDControl1.UserObject;
        }
    }

    protected void Page_PreRender(object sender, EventArgs e)
    {
        OpenIDUser oiu = this.OpenIDUser;
        string prefix = "openid.sreg.";
        if (oiu != null)
        {
            string email =  oiu.GetValue(prefix + "email");
            if (!IsUserRegistered(oiu.Identity))
                this.pnlRegister.Visible = true;
                this.txtUserName.Text = oiu.Identity;
                this.txtEmail.Text = email;
        }
    }


    protected bool IsUserRegistered(string userId)
    {
        bool registered = false;
       MembershipUser usr= Membership.GetUser(userId);
       if (usr!=null && usr.Email != null)
           registered = true;
        return registered;
    }

    protected void btnRegister_Click(object sender, EventArgs e)
    {
        string UserId = this.txtUserName.Text;
        MembershipUser user = Membership.CreateUser(UserId, UserId, txtEmail.Text);
        if (user != null)
        {
            FormsAuthentication.Authenticate(UserId, UserId);
            WebProfile Profile = new WebProfile();
            Profile.Initialize(UserId, true);
            Profile.FirstName = this.txtFirstName.Text;
            Profile.LastName = this.txtLastName.Text;
            Profile.Newsletter = this.chkNewsLetter.Checked;
            Profile.Email = this.txtEmail.Text;
            Profile.Save();

            GenericIdentity userIdentity = new GenericIdentity(UserId);
            GenericPrincipal userPrincipal =
              new GenericPrincipal(userIdentity, new string[] { "User" });
            Context.User = userPrincipal;

            if (!Roles.IsUserInRole(User.Identity.Name, "User"))
            {
                PAB.Web.Providers.SimpleSqlRoleProvider prov = new SimpleSqlRoleProvider();
                NameValueCollection config = new NameValueCollection();
                config["connectionStringName"] = "OpenId";
                System.Configuration.ConnectionStringSettings ConnectionStringSettings =
                    System.Configuration.ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
                prov.Initialize("", config);
                prov.AddUsersToRoles(new string[] { User.Identity.Name }, new string[] { "User" });
            }
            // go to a page for users who are authenticated
            Response.Redirect("Default2.aspx");
        }
        else
        {
            //uh-oh! you handle it appropriately.
        }

    }

}
