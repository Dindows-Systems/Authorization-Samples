
namespace OAuth4Client.OAuth
{
    /// <summary>
    /// class holdes credentials and endpoints of different social sites
    /// </summary>
    public class Credentials
    {
        public static class Facebook
        {
            public static string ConsumerKey = "";
            public static string ConsumerSecret = "";           
            public static string VerifierUrl = "https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}";
            public static string RequestAccessTokenUrl = "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}";
            public static string RequestProfileUrl = "https://graph.facebook.com/me";
            public static string Scope = "publish_stream,read_stream,email,offline_access"; 
        }

        public static class Twitter
        {
            public static string ConsumerKey = "";
            public static string ConsumerSecret = "";            
            public static string RequestTokenUrl = "http://twitter.com/oauth/request_token";
            public static string VerifierUrl = "http://api.twitter.com/oauth/authorize";            
            public static string RequestAccessTokenUrl = "https://api.twitter.com/oauth/access_token";
            public static string RequestProfileUrl = "http://api.twitter.com/1/users/show.json";
            public static string Scope = "";            
        }

        public static class LinkedIn
        {
            public static string ConsumerKey = "";
            public static string ConsumerSecret = "";
            public static string RequestTokenUrl = "https://api.linkedin.com/uas/oauth/requestToken";
            public static string VerifierUrl = "https://www.linkedin.com/uas/oauth/authenticate";
            public static string RequestAccessTokenUrl = "https://api.linkedin.com/uas/oauth/accessToken";
            public static string RequestProfileUrl = "http://api.linkedin.com/v1/people/~";
            public static string Scope = ""; 
        }

        public static class Google
        {
            public static string ConsumerKey = "";
            public static string ConsumerSecret = "";
            public static string RequestTokenUrl = "https://www.google.com/accounts/OAuthGetRequestToken?scope=http://www-opensocial.googleusercontent.com/api/people/";
            public static string VerifierUrl = "https://www.google.com/accounts/OAuthAuthorizeToken";
            public static string RequestAccessTokenUrl = "https://www.google.com/accounts/OAuthGetAccessToken";
            public static string RequestProfileUrl = "http://www-opensocial.googleusercontent.com/api/people/@me/@self";
            public static string Scope = "http://www-opensocial.googleusercontent.com/api/people/ https://www.googleapis.com/auth/userinfo#email";
        }
    }
}