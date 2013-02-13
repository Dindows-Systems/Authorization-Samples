using System;
using OAuth4Client.OAuth;

namespace OAuth4Client
{
    public partial class OAuthVerifier : System.Web.UI.Page
    {
        public OAuthContext OContext = null;
        public bool IsVersionV1 { get; set; }        

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                OContext = OAuthContext.Current;

                if(!IsPostBack)
                {
                    //End authentication and register tokens.
                    OContext.EndAuthenticationAndRegisterTokens();

                    txtTokenKey.Text = OContext.Token;
                    txtTokenSecret.Text = OContext.TokenSecret;
                    txtVerifier.Text = OContext.Verifier;    
                }                
            }          
            catch (OAuthException ex)
            {
                Response.Write("<p>OAuth Exception occurred.</p>");
                Response.Write("<p>" + ex.InnerException.Message + "</p>");
                Response.Write("<p>" + ex.InnerException.StackTrace + "</p>");
            }
        }

        /// <summary>
        /// function to get accesstoken
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnGetAccessToken_Click(object sender, EventArgs e)
        {
            try
            {
                OContext.GetAccessToken();

                divAccessToken.Visible = true;
                txtAccessToken.Text = OContext.AccessToken;
                txtAccessTokenSecret.Text = OContext.AccessTokenSecret;
            }
            catch (OAuthException ex)
            {
                Response.Write("<p>OAuth Exception occurred while getting Access Token.</p>");
                Response.Write("<p>" + ex.InnerException.Message + "</p>");
                Response.Write("<p>" + ex.InnerException.StackTrace + "</p>");
            }
        }

        /// <summary>
        /// function get profile response
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnResponse_Click(object sender, EventArgs e)
        {
            try
            {
                var responseText = OContext.GetProfileResponse();

                divResponse.Visible = true;
                txtResponse.Text = responseText;
            }
            catch (OAuthException ex)
            {
                Response.Write("<p>OAuth Exception occurred while getting profile response.</p>");
                Response.Write("<p>" + ex.InnerException.Message + "</p>");
                Response.Write("<p>" + ex.InnerException.StackTrace + "</p>");
            }
        }
    }
}