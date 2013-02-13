/*
===========================================================================
Copyright (c) 2010 BrickRed Technologies Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sub-license, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
===========================================================================

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Brickred.SocialAuth.NET.Core.BusinessObjects;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System.Web;

namespace Brickred.SocialAuth.NET.Core
{


    public class OAuth2_0server : OAuthStrategyBase, IOAuth2_0
    {
        log4net.ILog logger = log4net.LogManager.GetLogger("OAuth2_0server");

        public OAuth2_0server(IProvider provider)
        {
            this.provider = provider;
        }

        //Called Before Directing User
        public override void Login()
        {
            logger.Info("OAuth2.0 Authorization Flow begins for " + provider.ProviderType.ToString() + "...");
            DirectUserToServiceProvider(); //(A) (B)
        }
        //Called After Directing User
        public override void LoginCallback(QueryParameters responseCollection, Action<bool> AuthenticationCompletionHandler)
        {
            HandleAuthorizationCode(responseCollection); //(C)
            RequestForAccessToken(); // (D)
            //HandleAccessTokenResponse(response); //(E) Handled from above
            logger.Info("OAuth2.0 server side Authorization flow ends ..");
            //Authentication Process is through. Inform Consumer.
            AuthenticationCompletionHandler(isSuccess); // Authentication process complete. Call final method
        }

        #region Oauth2_0Implementation

        public void DirectUserToServiceProvider()
        {
            UriBuilder ub = new UriBuilder(provider.UserLoginEndpoint);
            try
            {
                QueryParameters oauthParams = new QueryParameters();
                oauthParams.Add("client_id", provider.Consumerkey);
                oauthParams.Add("redirect_uri", connectionToken.ProviderCallbackUrl);
                oauthParams.Add("response_type", "code");
                oauthParams.Add("scope", provider.GetScope());
                //ub.SetQueryparameter("client_id", provider.Consumerkey);
                //ub.SetQueryparameter("redirect_uri", connectionToken.ProviderCallbackUrl);
                //ub.SetQueryparameter("response_type", "code");
                //ub.SetQueryparameter("scope", provider.GetScope());

                BeforeDirectingUserToServiceProvider(oauthParams);
                logger.Debug("Redirecting user for login to " + ub.ToString() + "?" + oauthParams.ToEncodedString());
                SocialAuthUser.Redirect(ub.ToString() + "?" + oauthParams.ToEncodedString());
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.UserLoginRedirectionError(ub.ToString()), ex);
                throw new OAuthException(ErrorMessages.UserLoginRedirectionError(ub.ToString()), ex);
            }
        }

        public void HandleAuthorizationCode(QueryParameters responseCollection)
        {
            if (responseCollection.HasName("code"))
            {
                connectionToken.Code = responseCollection["code"];
                logger.Info("User successfully logged in and returned with Authorization code");
            }
            else if (responseCollection.ToList().Exists(x => x.Key.ToLower().Contains("denied") || x.Value.ToLower().Contains("denied")))
            {
                logger.Error(ErrorMessages.UserDeniedAccess(provider.ProviderType, responseCollection));
                throw new OAuthException(ErrorMessages.UserDeniedAccess(provider.ProviderType, responseCollection));
            }
            else
            {
                logger.Error(ErrorMessages.UserLoginResponseError(provider.ProviderType, responseCollection));
                throw new OAuthException(ErrorMessages.UserLoginResponseError(provider.ProviderType, responseCollection));
            }

        }

        public void RequestForAccessToken()
        {
            UriBuilder ub = new UriBuilder(provider.AccessTokenEndpoint);
            ub.SetQueryparameter("client_id", provider.Consumerkey);
            ub.SetQueryparameter("client_secret", provider.Consumersecret);
            ub.SetQueryparameter("code", connectionToken.Code);
            ub.SetQueryparameter("redirect_uri", connectionToken.ProviderCallbackUrl);
            ub.SetQueryparameter("grant_type", "authorization_code");
            //logger.LogAuthorizationRequest(ub.ToString());
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ub.ToString());

            string authToken = "";
            try
            {
                logger.Debug("Requesting Access Token at " + ub.ToString());
                using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                using (Stream responseStream = webResponse.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    authToken = reader.ReadToEnd();
                    HandleAccessTokenResponse(authToken);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.AccessTokenRequestError(request.RequestUri.ToString(), new QueryParameters()), ex);
                throw new OAuthException(ErrorMessages.AccessTokenRequestError(request.RequestUri.ToString(), new QueryParameters()), ex);
            }
        }

        public void HandleAccessTokenResponse(string response)
        {
            QueryParameters responseCollection = new QueryParameters();

            try
            {
                if (response.StartsWith("{")) // access token is returned in JSON format
                {
                    //  {"access_token":"asasdasdAA","expires_in":3600,"scope":"wl.basic","token_type":"bearer"}
                    JObject accessTokenJson = JObject.Parse(response);
                    responseCollection.Add("response", response);
                    connectionToken.AccessToken = accessTokenJson.SelectToken("access_token").ToString().Replace("\"", "");
                    if (accessTokenJson.SelectToken("expires_in") != null)
                        connectionToken.ExpiresOn = DateTime.Now.AddSeconds(int.Parse(accessTokenJson.SelectToken("expires_in").ToString().Replace("\"", "")) - 20);
                    //put in raw list
                    foreach (var t in accessTokenJson.AfterSelf())
                        connectionToken.ResponseCollection.Add(t.Type.ToString(), t.ToString());
                    logger.Info("Access Token successfully received");
                    isSuccess = true;
                }
                else // access token is returned as part of Query String
                {

                    responseCollection = Utility.GetQuerystringParameters(response);
                    string keyForAccessToken = responseCollection.Single(x => x.Key.Contains("token")).Key;

                    connectionToken.AccessToken = responseCollection[keyForAccessToken].Replace("\"", "");
                    if (responseCollection.ToList().Exists(x => x.Key.ToLower().Contains("expir")))
                    {
                        string keyForExpiry = responseCollection.Single(x => x.Key.Contains("expir")).Key;
                        connectionToken.ExpiresOn = connectionToken.ExpiresOn = DateTime.Now.AddSeconds(int.Parse(responseCollection[keyForExpiry].Replace("\"", "")) - 20);
                    }
                    //put in raw list
                    responseCollection.ToList().ForEach(x => connectionToken.ResponseCollection.Add(x.Key, x.Value));
                    logger.Info("Access Token successfully received");
                    isSuccess = true;

                }
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.AccessTokenResponseInvalid(responseCollection), ex);
                throw new OAuthException(ErrorMessages.AccessTokenResponseInvalid(responseCollection), ex);
            }
        }

        #endregion

        public override WebResponse ExecuteFeed(string feedURL, IProvider provider, Token connectionToken, TRANSPORT_METHOD transportMethod)
        {
            UriBuilder ub;

            /******** retrieve standard Fields ************/
            ub = new UriBuilder(feedURL);
   
            ub.SetQueryparameter("access_token", connectionToken.AccessToken);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ub.ToString());
            request.Method = transportMethod.ToString();
            
            //logger.LogContactsRequest(ub.ToString());
            WebResponse wr;
            try
            {
                logger.Debug("Executing " + feedURL + " using " + transportMethod.ToString());
                wr = (WebResponse)request.GetResponse();
                logger.Info("Successfully executed  " + feedURL + " using " + transportMethod.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.CustomFeedExecutionError(feedURL, null), ex);
                throw new OAuthException(ErrorMessages.CustomFeedExecutionError(feedURL, null), ex);
            }

            return wr;
        }

        public override WebResponse ExecuteFeed(string feedURL, IProvider provider, Token connectionToken, TRANSPORT_METHOD transportMethod, byte[] content = null, Dictionary<string, string> headers = null)
        {


            HttpWebRequest request;
            request = (HttpWebRequest)HttpWebRequest.Create(feedURL);
            request.ServicePoint.Expect100Continue = false;
            request.Method = transportMethod.ToString();
           
            //if headers are specified, set/append them
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    switch (header.Key)
                    {
                        case "ContentLength":
                            {
                                request.ContentLength = long.Parse(header.Value);
                                break;
                            }

                        case "ContentType":
                            {
                                request.ContentType = header.Value;
                                break;
                            }
                        default:
                            {
                                request.Headers[header.Key] = header.Value;
                                break;
                            }
                    }

                }

            }

            if (request.ContentLength == 0 & content.Length > 0)
                request.ContentLength = content.Length;
            request.GetRequestStream().Write(content, 0, content.Length);
            WebResponse wr = null;
            try
            {
                logger.Debug("Executing " + feedURL + " using " + transportMethod.ToString());
                wr = (WebResponse)request.GetResponse();
                logger.Info("Successfully executed  " + feedURL + " using " + transportMethod.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.CustomFeedExecutionError(feedURL, null), ex);
                throw new OAuthException(ErrorMessages.CustomFeedExecutionError(feedURL, null), ex);
            }
            return wr;

        }

        //public static WebResponse ExecuteFeed(string feedURL, TRANSPORT_METHOD transportMethod)
        //{
        //    UriBuilder ub;

        //    /******** retrieve standard Fields ************/
        //    ub = new UriBuilder(feedURL);
        //    //if (oauthParameters != null)
        //    //    foreach (var param in oauthParameters)
        //    //        ub.SetQueryparameter(param.Name, param.Value);

        //    HttpWebRequest webRequest = null;

        //    //StreamWriter requestWriter = null;


        //    webRequest = System.Net.WebRequest.Create(feedURL) as HttpWebRequest;
        //    webRequest.Method = transportMethod.ToString();
        //    webRequest.Timeout = 20000;

        //    if (transportMethod == TRANSPORT_METHOD.POST)
        //    {
        //        webRequest.ServicePoint.Expect100Continue = false;
        //        webRequest.ContentType = "application/x-www-form-urlencoded";
        //        //requestWriter = new StreamWriter(webRequest.GetRequestStream());
        //        //try
        //        //{
        //        //    requestWriter.Write(string.Empty);
        //        //}

        //        //catch
        //        //{
        //        //    throw;
        //        //}

        //        //finally
        //        //{
        //        //    requestWriter.Close();
        //        //    requestWriter = null;
        //        //}

        //    }

        //    WebResponse wr;
        //    try
        //    {
        //        wr = (WebResponse)webRequest.GetResponse();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new OAuthException("An Error occurred while executing " + feedURL + "!", ex);
        //    }
        //    return wr;
        //}

        public static WebResponse ExecuteFeed(string feedURL, TRANSPORT_METHOD transportMethod)
        {
            UriBuilder ub;

            /******** retrieve standard Fields ************/
            ub = new UriBuilder(feedURL);
            //if (oauthParameters != null)
            //    foreach (var param in oauthParameters)
            //        ub.SetQueryparameter(param.Name, param.Value);

            HttpWebRequest webRequest = null;

            //StreamWriter requestWriter = null;


            webRequest = System.Net.WebRequest.Create(feedURL) as HttpWebRequest;
            webRequest.Method = transportMethod.ToString();
            webRequest.Timeout = 20000;

            if (transportMethod == TRANSPORT_METHOD.POST)
            {
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                //requestWriter = new StreamWriter(webRequest.GetRequestStream());
                //try
                //{
                //    requestWriter.Write(string.Empty);
                //}

                //catch
                //{
                //    throw;
                //}

                //finally
                //{
                //    requestWriter.Close();
                //    requestWriter = null;
                //}

            }

            WebResponse wr;
            try
            {
                wr = (WebResponse)webRequest.GetResponse();
            }
            catch (Exception ex)
            {
                throw new OAuthException("An Error occurred while executing " + feedURL + "!", ex);
            }
            return wr;
        }


        #region IOAuth2_0 Members

        public event Action<QueryParameters> BeforeDirectingUserToServiceProvider = delegate { };
        public event Action<QueryParameters> BeforeRequestingAccessToken = delegate { };

        #endregion
    }
}



/**************FLOW****************
     +----------+
     | resource |
     |   owner  |
     |          |
     +----------+
          ^
          |
         (B)
     +----|-----+          Client Identifier      +---------------+
     |         -+----(A)--- & Redirect URI ------>|               |
     |  User-   |                                 | Authorization |
     |  Agent  -+----(B)-- User authenticates --->|     Server    |
     |          |                                 |               |
     |         -+----(C)-- Authorization Code ---<|               |
     +-|----|---+                                 +---------------+
       |    |                                         ^      v
      (A)  (C)                                        |      |
       |    |                                         |      |
       ^    v                                         |      |
     +---------+                                      |      |
     |         |>---(D)-- Client Credentials, --------’      |
     |         |          Authorization Code,                |
     | Client  |            & Redirect URI                   |
     |         |                                             |
     |         |<---(E)----- Access Token -------------------’
     +---------+       (w/ Optional Refresh Token)
*********************************/