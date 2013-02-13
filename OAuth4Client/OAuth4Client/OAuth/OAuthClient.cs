using System.Web;


namespace OAuth4Client.OAuth
{
    public class OAuthTwitterClient
    {
        public void BeginAuthentication()
        {
            var oContext = new OAuthContext
            {
                ConsumerKey = Credentials.Twitter.ConsumerKey,
                ConsumerSecret = Credentials.Twitter.ConsumerSecret,
                RequestTokenUrl = Credentials.Twitter.RequestTokenUrl,
                VerifierUrl = Credentials.Twitter.VerifierUrl,
                RequestAccessTokenUrl = Credentials.Twitter.RequestAccessTokenUrl,
                RequestProfileUrl = Credentials.Twitter.RequestProfileUrl,
                Realm = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.DnsSafeHost + HttpContext.Current.Request.ApplicationPath,
                OAuthVersion = OAuthVersion.V1,
                SocialSiteName = "Twitter"
            };

            oContext.GetRequestToken();
            oContext.ObtainVerifier();                       
        }
    }

    public class OAuthFacebookClient
    {
        public void BeginAuthentication()
        {
            var oContext = new OAuthContext
            {
                ConsumerKey = Credentials.Facebook.ConsumerKey,
                ConsumerSecret = Credentials.Facebook.ConsumerSecret,                                   
                VerifierUrl = Credentials.Facebook.VerifierUrl,
                RequestAccessTokenUrl = Credentials.Facebook.RequestAccessTokenUrl,
                RequestProfileUrl = Credentials.Facebook.RequestProfileUrl,
                Scope = Credentials.Facebook.Scope,
                OAuthVersion = OAuthVersion.V2,
                SocialSiteName = "Facebook"
            };

            //In version 2.0 no need to get request token
            oContext.ObtainVerifier();
        }
    }

    public class OAuthLinkedInClient
    {
        public void BeginAuthentication()
        {
            var oContext = new OAuthContext
            {
                ConsumerKey = Credentials.LinkedIn.ConsumerKey,
                ConsumerSecret = Credentials.LinkedIn.ConsumerSecret,
                RequestTokenUrl = Credentials.LinkedIn.RequestTokenUrl,
                VerifierUrl = Credentials.LinkedIn.VerifierUrl,
                RequestAccessTokenUrl = Credentials.LinkedIn.RequestAccessTokenUrl,
                RequestProfileUrl = Credentials.LinkedIn.RequestProfileUrl,
                Realm = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.DnsSafeHost + HttpContext.Current.Request.ApplicationPath,
                OAuthVersion = OAuthVersion.V1,
                SocialSiteName = "LinkedIn"
            };

            oContext.GetRequestToken();
            oContext.ObtainVerifier();
        }
    }

    public class OAuthGoogleClient
    {
        public void BeginAuthentication()
        {
            var oContext = new OAuthContext
            {
                ConsumerKey = Credentials.Google.ConsumerKey,
                ConsumerSecret = Credentials.Google.ConsumerSecret,
                RequestTokenUrl = Credentials.Google.RequestTokenUrl,
                VerifierUrl = Credentials.Google.VerifierUrl,
                RequestAccessTokenUrl = Credentials.Google.RequestAccessTokenUrl,
                RequestProfileUrl = Credentials.Google.RequestProfileUrl,
                Realm = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.DnsSafeHost + HttpContext.Current.Request.ApplicationPath,
                OAuthVersion = OAuthVersion.V1,
                SocialSiteName = "Google"
            };

            oContext.GetRequestToken();
            oContext.ObtainVerifier();
        }
    }

}