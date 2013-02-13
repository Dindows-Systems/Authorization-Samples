using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;

namespace OAuth4Client.OAuth
{    
    public class OAuthContext
    {
        #region " OAuth params "
        public string ConsumerKey;
        public string Realm;
        public string ConsumerSecret;
        public string RequestTokenUrl;
        public string VerifierUrl;
        public string CallbackUrl;
        public string RequestAccessTokenUrl;
        public string RequestProfileUrl;
        public string AccessToken;
        public string AccessTokenSecret;
        public string Token;
        public string TokenSecret;
        public string Verifier;
        public string Scope;
        public string OAuthVersion;
        public string SocialSiteName;

        public string Code; //for version v2 only

        public string ScreenName; //for twitter only
        public string UserId; //for twitter only
        #endregion

        #region " Variables "

        private List<NameValue> _oauthParameters = new List<NameValue>();
        private readonly static string reservedCharacters = "!*'();:@&=+$,/?%#[]";
        private static Random random = new Random();

        #endregion 

        /// <summary>
        /// Constructor.
        /// Initialized callback url with "OAuthVerifier.aspx"
        /// </summary>
        public OAuthContext()
        {
            CallbackUrl = GetCallbackUrl("OAuthVerifier.aspx");//OAuthVerifier.aspx is a return url after login validation
        }

        /// <summary>
        /// To get active context object
        /// </summary>
        public static OAuthContext Current
        {
            get { return (OAuthContext)HttpContext.Current.Session["oAuthContext"]; }
            set { HttpContext.Current.Session["oAuthContext"] = value; }
        }        

        #region " Private methods "

        /// <summary>
        /// generates timestamp
        /// </summary>
        /// <returns></returns>
        private string GenerateTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Math.Truncate(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// generates nonce
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        private string GenerateNonce(string timestamp)
        {
            var buffer = new byte[256];
            random.NextBytes(buffer);
            var hmacsha1 = new HMACSHA1();
            hmacsha1.Key = Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(buffer));
            return ComputeHash(hmacsha1, timestamp);
        }

        /// <summary>
        /// Compte hash. Only for "HMAC-SHA1"
        /// </summary>
        /// <param name="hashAlgorithm"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
                throw new ArgumentNullException("hashAlgorithm");
            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException("data");

            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] bytes = hashAlgorithm.ComputeHash(buffer);

            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Encode OAuth params
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string NormalizeOAuthParameters(IList<NameValue> parameters)
        {
            var sb = new StringBuilder();
            NameValue p = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                p = parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, UrlEncode(p.Value));

                if (i < parameters.Count - 1)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// function to encode url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string NormalizeUrl(string url)
        {
            int questionIndex = url.IndexOf('?');
            if (questionIndex == -1)
                return url;

            var parameters = url.Substring(questionIndex + 1);
            var result = new StringBuilder();
            result.Append(url.Substring(0, questionIndex + 1));

            bool hasQueryParameters = false;
            if (!String.IsNullOrEmpty(parameters))
            {
                string[] parts = parameters.Split('&');
                hasQueryParameters = parts.Length > 0;
                foreach (var part in parts)
                {
                    var nameValue = part.Split('=');
                    result.Append(nameValue[0] + "=");
                    if (nameValue.Length == 2)
                        result.Append(UrlEncode(nameValue[1]));
                    result.Append("&");
                }
                if (hasQueryParameters)
                    result = result.Remove(result.Length - 1, 1);
            }
            return result.ToString();
        }

        /// <summary>
        /// Fetch query string and create name value pairs. Used in adding url querystring to OAuth header. For eg. "scope" in google
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private List<NameValue> ExtractQueryStrings(string url)
        {
            int questionIndex = url.IndexOf('?');
            if (questionIndex == -1)
                return new List<NameValue>();

            var parameters = url.Substring(questionIndex + 1);
            var result = new List<NameValue>();

            if (!String.IsNullOrEmpty(parameters))
            {
                string[] parts = parameters.Split('&');
                foreach (var part in parts)
                {
                    if (!string.IsNullOrEmpty(part) && !part.StartsWith("oauth_"))
                    {
                        if (part.IndexOf('=') != -1)
                        {
                            string[] nameValue = part.Split('=');
                            result.Add(new NameValue(nameValue[0], nameValue[1]));
                        }
                        else
                            result.Add(new NameValue(part, String.Empty));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Generate base string
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <param name="oAuthParameters"></param>
        /// <returns></returns>
        private string GenerateSignatureBaseString(string url, string httpMethod, List<NameValue> oAuthParameters)
        {
            oAuthParameters.Sort(new Comparer());

            var uri = new Uri(url);
            var normalizedUrl = string.Format("{0}://{1}", uri.Scheme, uri.Host);

            if (!((uri.Scheme == "http" && uri.Port == 80) || (uri.Scheme == "https" && uri.Port == 443)))
                normalizedUrl += ":" + uri.Port;
            normalizedUrl += uri.AbsolutePath;

            var normalizedRequestParameters = NormalizeOAuthParameters(oAuthParameters);

            var signatureBaseSb = new StringBuilder();
            signatureBaseSb.AppendFormat("{0}&", httpMethod);
            signatureBaseSb.AppendFormat("{0}&", UrlEncode(normalizedUrl));
            signatureBaseSb.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));
            return signatureBaseSb.ToString();
        }

        /// <summary>
        /// function to generate signature
        /// </summary>
        /// <param name="consumerSecret"></param>
        /// <param name="signatureBaseString"></param>
        /// <param name="tokenSecret"></param>
        /// <returns></returns>
        private string GenerateSignature(string consumerSecret, string signatureBaseString, string tokenSecret = null)
        {           
            var hmacsha1 = new HMACSHA1();
            hmacsha1.Key = Encoding.ASCII.GetBytes(String.Format("{0}&{1}", UrlEncode(consumerSecret), String.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret)));
            return ComputeHash(hmacsha1, signatureBaseString);              
        }

        /// <summary>
        /// Url encoding
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string UrlEncode(string value)
        {
            if (String.IsNullOrEmpty(value))
                return String.Empty;

            var sb = new StringBuilder();

            foreach (char @char in value)
            {
                if (reservedCharacters.IndexOf(@char) == -1)
                    sb.Append(@char);
                else
                    sb.AppendFormat("%{0:X2}", (int)@char);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Create query string for different providers
        /// </summary>
        /// <returns></returns>
        private string CreateQueryString()
        {
            string queryString;
            switch (SocialSiteName.ToLower())
            {
                case "twitter"  : queryString = "?screen_name=" + ScreenName;   break;
                case "facebook" : queryString = "?access_token=" + AccessToken; break;
                case "linkedin" : queryString = "?format=json";                 break;
                case "google"   : queryString = string.Empty;                   break;
                default         : queryString = string.Empty;                   break;
                    
            }
                        
            return (queryString);
        }

        /// <summary>
        /// function to get callback absolute url 
        /// </summary>
        /// <param name="filename">is the file path to return after login verification</param>
        /// <remarks>By default we have taken a file OAuthVerifier.aspx to return to</remarks>
        /// <returns></returns>
        private string GetCallbackUrl(string filename)
        {
            var url = HttpContext.Current.Request.Url;
            return url.Scheme + "://" + url.Host + ":" + url.Port + VirtualPathUtility.ToAbsolute("~/" + filename);
        }

        #endregion Private methods        

        #region " Social site Apis "

        /// <summary>
        /// function to get request token
        /// </summary>
        public void GetRequestToken()
        {            
            HttpWebRequest request = null;

            try
            {
                var requestHeader = GetRequestTokenAuthorizationHeader();                

                /*Make a request and convert data in Byte[]*/               
                var response = MakeHttpRequest(RequestTokenUrl, requestHeader.ToString());
                var data = GetResponseByte(response);
                /*----------------------------------------*/

                //Read byte[] and get Token and TokenSecret
                using (var reader = new StreamReader(new MemoryStream(data)))
                {
                    var resultString = reader.ReadToEnd();
                    var queryStrings = HttpUtility.ParseQueryString(resultString);

                    this.Token = queryStrings["oauth_token"];
                    this.TokenSecret = queryStrings["oauth_token_secret"];                   
                }
            }
            catch (WebException we)
            {
                var errrorResponse = we.Response as HttpWebResponse;

                if(errrorResponse!=null)
                {
                    var sr = new StreamReader(errrorResponse.GetResponseStream());
                    string responseData = sr.ReadToEnd();
                    sr.Close();

                    throw new OAuthNetworkException("ERROR_NETWORK_PROBLEM", request.RequestUri.ToString(), errrorResponse.StatusCode, responseData, request.Headers["Authorization"]);
                }
                else
                {
                    throw new Exception("Initializing error! Please check the Customer key and Customer secret key.");
                }
                
            }
            catch (Exception e)
            {
                throw new OAuthException("ERROR_EXCEPION_OCCURRED", e);
            }           
        }

        /// <summary>
        /// function to get access token
        /// </summary>
        public void GetAccessToken()
        {
            HttpWebRequest request = null;

            try
            {
                /*Make a request and convert data in Byte[]*/
                WebResponse response;
                if(OAuthVersion == OAuth.OAuthVersion.V1)
                {
                    var requestHeader = GetAccessTokenAuthorizationHeader();
                    response = MakeHttpRequest(RequestAccessTokenUrl, requestHeader);
                }                    
                else // for version 2.0
                    response = MakeHttpRequest(string.Format(RequestAccessTokenUrl, this.ConsumerKey, this.CallbackUrl, this.ConsumerSecret, Code), string.Empty, "GET");
                                    
                var data = GetResponseByte(response);
                /*----------------------------------------*/

                //Read byte[] and get Token and TokenSecret
                using (var reader = new StreamReader(new MemoryStream(data)))
                {
                    var resultString = reader.ReadToEnd();
                    var queryStrings = HttpUtility.ParseQueryString(resultString);

                    this.AccessToken = OAuthVersion == OAuth.OAuthVersion.V1 ? queryStrings["oauth_token"] : queryStrings["access_token"];
                    this.AccessTokenSecret = queryStrings["oauth_token_secret"];
                    
                    ScreenName = queryStrings["screen_name"];
                    UserId = queryStrings["user_id"];
                }

            }
            catch (WebException we)
            {
                var errrorResponse = we.Response as HttpWebResponse;

                var sr = new StreamReader(errrorResponse.GetResponseStream());
                string responseData = sr.ReadToEnd();
                sr.Close();

                throw new OAuthNetworkException("ERROR_NETWORK_PROBLEM", request.RequestUri.ToString(), errrorResponse.StatusCode, responseData, request.Headers["Authorization"]);
            }
            catch (Exception e)
            {
                throw new OAuthException("ERROR_EXCEPION_OCCURRED", e);
            }    
        }

        /// <summary>
        /// function to get user profile response
        /// </summary>
        /// <returns></returns>
        public string GetProfileResponse()
        {
                var requestHeader = GetUserProfileAuthorizationHeader();

                var queryString = CreateQueryString();
       
                var request = WebRequest.Create(RequestProfileUrl + queryString);
                request.Headers.Add("Authorization", requestHeader.ToString());
                request.Method = HttpMethod.Get;
                try
                {
                    var response = request.GetResponse();
                    using (var responseStream = response.GetResponseStream())
                    {
                        var reader = new StreamReader(responseStream);
                        var responseText = reader.ReadToEnd();
                        reader.Close();
                        return responseText;
                    }
                }
                catch (WebException we)
                {
                    var errrorResponse = we.Response as HttpWebResponse;

                    var sr = new StreamReader(errrorResponse.GetResponseStream());
                    string responseData = sr.ReadToEnd();
                    sr.Close();

                    throw new OAuthNetworkException("ERROR_NETWORK_PROBLEM", request.RequestUri.ToString(),
                                                    errrorResponse.StatusCode, responseData,
                                                    request.Headers["Authorization"]);
                }
                catch (Exception e)
                {
                    throw new OAuthException("ERROR_EXCEPION_OCCURRED", e);
                }
                
                return string.Empty;            
        }

        /// <summary>
        /// function to redirect ot login site for Verification
        /// </summary>
        public void ObtainVerifier()
        {
            Current = this;

            if (this.OAuthVersion == OAuth.OAuthVersion.V1)
                HttpContext.Current.Response.Redirect(VerifierUrl + "?oauth_token=" + this.Token + "&oauth_callback=" + CallbackUrl);
            else
                HttpContext.Current.Response.Redirect(string.Format(VerifierUrl, this.ConsumerKey, this.CallbackUrl, this.Scope));

        }

        /// <summary>
        /// Last step of request verification. Then we begin with GetAccessToken and then GetProfile
        /// </summary>
        public void EndAuthenticationAndRegisterTokens()
        {
            var request = HttpContext.Current.Request;
            if (this.OAuthVersion == OAuth.OAuthVersion.V1)
            {
                if (!string.IsNullOrEmpty(request.QueryString["oauth_verifier"]))
                    this.Verifier = request.QueryString["oauth_verifier"];
                if (!string.IsNullOrEmpty(request.QueryString["oauth_token"]))
                    this.Token = request.QueryString["oauth_token"];
                if (!string.IsNullOrEmpty(request.QueryString["oauth_token_secret"]))
                    this.TokenSecret = request.QueryString["oauth_token_secret"];
            }
            else
            {
                if (!string.IsNullOrEmpty(request.QueryString["code"]))
                    this.Code = request.QueryString["code"];
            }

        }

        /// <summary>
        /// function to make HttpRequests
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authorizationHeader"></param>
        /// <param name="requestMethod"></param>
        /// <returns></returns>
        public WebResponse MakeHttpRequest(string url, string authorizationHeader, string requestMethod = "POST")
        {
            var normalizedUrl = NormalizeUrl(url);
            var request = WebRequest.Create(normalizedUrl);
            request.Method = requestMethod;
            if (authorizationHeader != string.Empty)
            {
                request.Headers.Add("Authorization", authorizationHeader);
                var requestStream = request.GetRequestStream();
            }


            try
            {
                return (request.GetResponse());
            }
            catch (WebException e)
            {
                using (var resp = e.Response)
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        var errorMessage = sr.ReadToEnd();
                        HttpContext.Current.Response.Write(errorMessage);
                        ((Page)HttpContext.Current.CurrentHandler).ClientScript.RegisterStartupScript(this.GetType(), "Error", "<script>alert('Error');</script> ");
                        throw new WebException(errorMessage, e);
                    }
                }
            }
        }

        /// <summary>
        /// function to get results in byte[]
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public byte[] GetResponseByte(WebResponse response)
        {
            byte[] resultData = null;
            var ms = new MemoryStream();
            var binaryWriter = new BinaryWriter(ms);

            using (var binaryReader = new BinaryReader(response.GetResponseStream()))
            {
                int readedByteCount;
                const int bufferLength = 65536;
                byte[] data;
                do
                {
                    data = new byte[bufferLength];
                    readedByteCount = binaryReader.Read(data, 0, data.Length);

                    if (readedByteCount == 0)
                        break;
                    else
                        binaryWriter.Write(data, 0, readedByteCount);
                }
                while (readedByteCount <= bufferLength);
            }

            ms.Position = 0;
            resultData = ms.ToArray();
            return resultData;
        }

        #endregion       
        
        #region " Authorization Header functions "

        /// <summary>
        /// resets _oauthParameters var
        /// </summary>
        private void ResetOAuthParameters()
        {
            _oauthParameters.Clear();
        }

        /// <summary>
        /// Register basic oauth parameters
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timestamp"></param>
        /// <param name="nounce"></param>
        private void RegisterBasicOAuthParameters(string url, string timestamp, string nounce)
        {
            _oauthParameters = ExtractQueryStrings(url);
            _oauthParameters.Add(new NameValue(OAuthParameters.ConsumerKey, ConsumerKey));
            _oauthParameters.Add(new NameValue(OAuthParameters.SignatureMethod, SignatureMethod.HMACSHA1));
            _oauthParameters.Add(new NameValue(OAuthParameters.Timestamp, timestamp));
            _oauthParameters.Add(new NameValue(OAuthParameters.Nounce, nounce));
            _oauthParameters.Add(new NameValue(OAuthParameters.Version, OAuthVersion));
        }

        /// <summary>
        /// function to return request token authorization header
        /// </summary>
        /// <returns></returns>
        public string GetRequestTokenAuthorizationHeader()
        {            
            var timestamp = GenerateTimeStamp();
            var nounce = GenerateNonce(timestamp);

            ResetOAuthParameters();

            //Register basic oauth parameters
            RegisterBasicOAuthParameters(RequestTokenUrl, timestamp, nounce);

            _oauthParameters.Add(new NameValue(OAuthParameters.Callback, CallbackUrl));

            string signatureBaseString = GenerateSignatureBaseString(RequestTokenUrl, HttpMethod.Post, _oauthParameters);
            var signature = GenerateSignature(ConsumerSecret, signatureBaseString);

            //Create header string
            var header = CreateAuthrorzationHeaderString(signature);
            return header;
        }

        /// <summary>
        /// function to return access token authorization header
        /// </summary>
        /// <returns></returns>
        public string GetAccessTokenAuthorizationHeader()
        {
            var timestamp = GenerateTimeStamp();
            var nounce = GenerateNonce(timestamp);

            ResetOAuthParameters();

            //Register basic oauth parameters
            RegisterBasicOAuthParameters(RequestAccessTokenUrl, timestamp, nounce);

            _oauthParameters.Add(new NameValue(OAuthParameters.Token, Token));
            _oauthParameters.Add(new NameValue(OAuthParameters.Verifier, Verifier));

            string signatureBaseString = GenerateSignatureBaseString(RequestAccessTokenUrl, HttpMethod.Post, _oauthParameters);
            var signature = GenerateSignature(ConsumerSecret, signatureBaseString, TokenSecret);

            //Create header string
            var header = CreateAuthrorzationHeaderString(signature);            
            return header;
        }

        /// <summary>
        /// function to return profile authorization header
        /// </summary>
        /// <returns></returns>
        public string GetUserProfileAuthorizationHeader()
        {            
            var timestamp = GenerateTimeStamp();
            var nounce = GenerateNonce(timestamp);

            ResetOAuthParameters();

            //Register basic oauth parameters
            RegisterBasicOAuthParameters(RequestProfileUrl + CreateQueryString(), timestamp, nounce);

            _oauthParameters.Add(new NameValue(OAuthParameters.Token, AccessToken));

            string signatureBaseString = GenerateSignatureBaseString(RequestProfileUrl, HttpMethod.Get, _oauthParameters);
            var signature = GenerateSignature(ConsumerSecret, signatureBaseString, AccessTokenSecret);

            //Create header string
            var header = CreateAuthrorzationHeaderString(signature);           
            return header;
        }

        /// <summary>
        /// function to create authorization header string
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        public string CreateAuthrorzationHeaderString(string signature)
        {
            var sb = new StringBuilder();
            sb.Append("OAuth ");
            sb.AppendFormat("realm=\"{0}\", ", Realm);
            sb.AppendFormat("{0}=\"{1}\", ", OAuthParameters.Signature, UrlEncode(signature));
            foreach (var oauthParameter in _oauthParameters)
            {
                if (oauthParameter.Name.ToLower() == "scope" || oauthParameter.Name.ToLower() == "format") continue;
                sb.AppendFormat("{0}=\"{1}\", ", oauthParameter.Name, UrlEncode(oauthParameter.Value));
            }

            sb = sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }
        #endregion                      
       
    }
      
    #region " Supporting classes"
    /// <summary>
    /// OAuth parameters constants
    /// </summary>
    public static class OAuthParameters
    {
        public const string ConsumerKey = "oauth_consumer_key";
        public const string SignatureMethod = "oauth_signature_method";
        public const string Signature = "oauth_signature";
        public const string Timestamp = "oauth_timestamp";
        public const string Nounce = "oauth_nonce";
        public const string Version = "oauth_version";
        public const string Callback = "oauth_callback";
        public const string Verifier = "oauth_verifier";
        public const string Token = "oauth_token";
        public const string TokenSecret = "oauth_token_secret";
    }

    /// <summary>
    /// Signature methods constants
    /// </summary>
    public static class SignatureMethod
    {
        public const string HMACSHA1 = "HMAC-SHA1";
        public const string RSASHA1 = "RSA-SHA1";
        public const string PLAINTEXT = "PLAINTEXT";
    }

    /// <summary>
    /// Name value collections
    /// </summary>
    public class NameValue
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public NameValue(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// OAuth versions
    /// </summary>
    public class OAuthVersion
    {
        public static string V1 = "1.0";
        public static string V2 = "2.0";
    }

    /// <summary>
    /// HttpMethods 
    /// </summary>
    public class HttpMethod
    {
        public static string Get = "GET";
        public static string Post = "POST";
    }

    /// <summary>
    /// Object Comparer
    /// </summary>
    public class Comparer : IComparer<NameValue>
    {
        public int Compare(NameValue x, NameValue y)
        {
            if (x.Name == y.Name)
                return string.Compare(x.Value, y.Value);
            else
                return string.Compare(x.Name, y.Name);
        }
    }

    #endregion
}