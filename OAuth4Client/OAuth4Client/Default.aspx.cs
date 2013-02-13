using System;
using OAuth4Client.OAuth;

namespace OAuth4Client
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void twt_OnClick(object sender, EventArgs e)
        {
            var tClient = new OAuthTwitterClient();
            tClient.BeginAuthentication();
        }

        protected void google_OnClick(object sender, EventArgs e)
        {
            var tClient = new OAuthGoogleClient();
            tClient.BeginAuthentication();
        }

        protected void li_OnClick(object sender, EventArgs e)
        {
            var tClient = new OAuthLinkedInClient();
            tClient.BeginAuthentication();
        }

        protected void fb_OnClick(object sender, EventArgs e)
        {
            var tClient = new OAuthFacebookClient();
            tClient.BeginAuthentication();
        }
    }
}