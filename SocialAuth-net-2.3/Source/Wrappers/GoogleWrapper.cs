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
using System.Web;
using System.Net;
using System.IO;
using Brickred.SocialAuth.NET.Core.BusinessObjects;
using System.Collections.Specialized;
using System.Xml.Linq;
using log4net;
using Newtonsoft.Json.Linq;

namespace Brickred.SocialAuth.NET.Core.Wrappers
{
    internal class GoogleWrapper : Provider, IProvider
    {
        #region IProvider Members

        //****** PROPERTIES
        private static readonly ILog logger = log4net.LogManager.GetLogger("GoogleWrapper");
        public override PROVIDER_TYPE ProviderType { get { return PROVIDER_TYPE.GOOGLE; } }
        public override string OpenIdDiscoveryEndpoint { get { return "https://www.google.com/accounts/o8/id"; } }
        public override string RequestTokenEndpoint { get { return "https://www.google.com/accounts/OAuthGetRequestToken"; } }
        string userloginendpoint = "https://www.google.com/accounts/OAuthAuthorizeToken";
        public override string UserLoginEndpoint { get { return userloginendpoint; } set { userloginendpoint = value; } }
        public override string AccessTokenEndpoint { get { return "https://www.google.com/accounts/OAuthGetAccessToken"; } }
        public override OAuthStrategyBase AuthenticationStrategy
        {
            get
            {
                OAuth1_0Hybrid oauth1_0HybridStrategy = new OAuth1_0Hybrid(this);
                oauth1_0HybridStrategy.BeforeDirectingUserToServiceProvider += (x) => { x.Add(new QueryParameter("openid.oauth.scope", GetScope())); };
                return oauth1_0HybridStrategy;
            }
        }
        public override string ProfileEndpoint { get { return "https://www.googleapis.com/oauth2/v1/userinfo"; } }//https://www-opensocial.googleusercontent.com/api/people/@me/@self
        public override string ContactsEndpoint { get { return "http://www.google.com/m8/feeds/contacts/default/full/?max-results=1000"; } }
        public override SIGNATURE_TYPE SignatureMethod { get { return SIGNATURE_TYPE.HMACSHA1; } }
        public override TRANSPORT_METHOD TransportName { get { return TRANSPORT_METHOD.GET; } }
        public override string ScopeDelimeter
        {
            get
            {
                return " ";
            }
        }
        public override bool IsProfileSupported
        {
            get
            {
                return true;
            }
        }

        public override string DefaultScope { get { return "http://www.google.com/m8/feeds/ https://www.googleapis.com/auth/userinfo.profile"; } }



        //****** OPERATIONS
        public override UserProfile GetProfile()
        {

            Token token = SocialAuthUser.GetCurrentUser().GetConnection(this.ProviderType).GetConnectionToken();
            UserProfile profile = new UserProfile(ProviderType);
            string response = "";
            //If token already has profile for this provider, we can return it to avoid a call
            if (token.Profile.IsSet || !IsProfileSupported)
                return token.Profile;

            var provider = ProviderFactory.GetProvider(token.Provider);

            if (GetScope().ToLower().Contains("https://www.googleapis.com/auth/userinfo.profile"))
            {
                try
                {
                    logger.Debug("Executing profile feed");
                    Stream responseStream = AuthenticationStrategy.ExecuteFeed(ProfileEndpoint, this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                    response = new StreamReader(responseStream).ReadToEnd();
                }
                catch
                { throw; }

                try
                {
                    JObject profileJson = JObject.Parse(response);
                    //{"entry":{"profileUrl":"https://plus.google.com/103908432244378021535","isViewer":true,"id":"103908432244378021535",
                    //    "name":{"formatted":"deepak Aggarwal","familyName":"Aggarwal","givenName":"deepak"},
                    //    "thumbnailUrl":"http://www.,"urls":[{"value":"https://plus.google.com/103908432244378021535","type":"profile"}],
                    //    "photos":[{"value":"http://www.google.com/ig/c/photos/public/AIbEiAIAAABDCJ_d1payzeKeNiILdmNhcmRfcGhvdG8qKGFjM2RmMzQ1ZDc4Nzg5NmI5NmFjYTc1NDNjOTA3MmQ5MmNmOTYzZWIwAe0HZMa7crOI_laYBG7LxYvlAvqe","type":"thumbnail"}],"displayName":"deepak Aggarwal"}}
                    profile.Provider = ProviderType;
                    profile.ID = profileJson.Get("id");
                    profile.ProfileURL = profileJson.Get("link");
                    profile.FirstName = profileJson.Get("given_name");
                    profile.LastName = profileJson.Get("family_name");
                    profile.ProfilePictureURL = profileJson.Get("picture");
                    profile.GenderType = Utility.ParseGender(profileJson.Get("gender"));
                }
                catch (Exception ex)
                {
                    logger.Error(ErrorMessages.ProfileParsingError(response), ex);
                    throw new DataParsingException(response, ex);
                }
            }
            else
            {
                profile.FirstName = token.ResponseCollection["openid.ext1.value.firstname"];
                profile.LastName = token.ResponseCollection["openid.ext1.value.lastname"];
            }
            profile.Email = token.ResponseCollection.Get("openid.ext1.value.email");
            profile.Country = token.ResponseCollection.Get("openid.ext1.value.country");
            profile.Language = token.ResponseCollection.Get("openid.ext1.value.language");
            profile.IsSet = true;
            token.Profile = profile;
            logger.Info("Profile successfully received");

            return profile;

        }
        public override List<Contact> GetContacts()
        {
            Token token = SocialAuthUser.GetCurrentUser().GetConnection(this.ProviderType).GetConnectionToken();

            //If only OpenID is used and also there is no scope for contacts, return blank list straight away
            if (string.IsNullOrEmpty(token.AccessToken) || !(GetScope().ToLower().Contains("/m8/feeds")))
                return new List<Contact>();

            IEnumerable<Contact> contacts;
            string response = "";
            try
            {
                logger.Debug("Executing contacts feed");
                Stream responseStream = AuthenticationStrategy.ExecuteFeed(ContactsEndpoint, this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                response = new StreamReader(responseStream).ReadToEnd();
            }
            catch { throw; }
            try
            {
                XDocument contactsXML = XDocument.Parse(response);
                XNamespace xn = "http://schemas.google.com/g/2005";
                contacts = from c in contactsXML.Descendants(contactsXML.Root.GetDefaultNamespace() + "entry")
                           select new Contact()
                           {
                               ID = c.Element(contactsXML.Root.GetDefaultNamespace() + "id").Value,
                               Name = c.Element(contactsXML.Root.GetDefaultNamespace() + "title").Value,
                               Email = (c.Element(xn + "email") == null) ? "" : c.Element(xn + "email").Attribute("address").Value,
                               ProfilePictureURL = ""
                           };
                logger.Info("Contacts successfully received");
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.ContactsParsingError(response), ex);
                throw new DataParsingException(ErrorMessages.ContactsParsingError(response), ex);
            }
            return contacts.ToList();


        }
        public override WebResponse ExecuteFeed(string feedUrl, TRANSPORT_METHOD transportMethod)
        {
            return AuthenticationStrategy.ExecuteFeed(feedUrl, this, SocialAuthUser.GetCurrentUser().GetConnection(ProviderType).GetConnectionToken(), transportMethod);
        }
        public static WebResponse ExecuteFeed(string feedUrl, string accessToken, string tokenSecret, TRANSPORT_METHOD transportMethod)
        {
            GoogleWrapper wrapper = new GoogleWrapper();
            return wrapper.AuthenticationStrategy.ExecuteFeed(feedUrl, wrapper, new Token() { AccessToken = accessToken, TokenSecret = tokenSecret }, transportMethod);
        }

        #endregion
    }
}
