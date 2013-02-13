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
using Newtonsoft.Json.Linq;
using log4net;

namespace Brickred.SocialAuth.NET.Core.Wrappers
{
    internal class LinkedInWrapper : Provider, IProvider
    {
        #region IProvider Members

        //****** PROPERTIES
        private static readonly ILog logger = log4net.LogManager.GetLogger("LinkedInWrapper");
        public override PROVIDER_TYPE ProviderType { get { return PROVIDER_TYPE.LINKEDIN; } }
        public override string RequestTokenEndpoint { get { return "https://api.linkedin.com/uas/oauth/requestToken"; } }
        public override string UserLoginEndpoint { get { return "https://www.linkedin.com/uas/oauth/authenticate"; } set { } }
        public override string AccessTokenEndpoint { get { return "https://api.linkedin.com/uas/oauth/accessToken"; } }
        public override OAuthStrategyBase AuthenticationStrategy { get { return new OAuth1_0a(this); } }
        public override string ProfileEndpoint { get { return "http://api.linkedin.com/v1/people/~:(id,first-name,last-name,languages,date-of-birth,picture-url,location:(name))"; } }
        public override string ContactsEndpoint { get { return "http://api.linkedin.com/v1/people/~/connections:(id,first-name,last-name,public-profile-url)"; } }
        public override SIGNATURE_TYPE SignatureMethod { get { return SIGNATURE_TYPE.HMACSHA1; } }
        public override TRANSPORT_METHOD TransportName { get { return TRANSPORT_METHOD.POST; } }

        public override string DefaultScope { get { return ""; } }



        //****** OPERATIONS
        public override UserProfile GetProfile()
        {

            Token token = SocialAuthUser.GetCurrentUser().GetConnection(this.ProviderType).GetConnectionToken();
            string response = "";

            //If token already has profile for this provider, we can return it to avoid a call
            if (token.Profile.IsSet)
            {
                logger.Debug("Profile successfully returned from session");
                return token.Profile;
            }

            try
            {
                logger.Debug("Executing Profile feed");
                Stream responseStream = AuthenticationStrategy.ExecuteFeed(ProfileEndpoint, this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                response = new StreamReader(responseStream).ReadToEnd();
            }
            catch
            {
                throw;
            }

            try
            {

                XDocument profileXml = XDocument.Parse(response);
                XElement person = profileXml.Element("person");
                token.Profile.ID = person.Element("id") != null ? person.Element("id").Value : "";
                token.Profile.ProfileURL = "http://www.linkedin.com/profile/view?id=" + person.Element("id").Value;
                token.Profile.FirstName = person.Element("first-name") != null ? person.Element("first-name").Value : "";
                token.Profile.LastName = person.Element("first-name") != null ? person.Element("last-name").Value : "";
                token.Profile.ProfilePictureURL = person.Element("picture-url") != null ? person.Element("picture-url").Value : "";
                if (person.Element("date-of-birth") != null)
                {
                    string d = person.Element("date-of-birth").Element("day") == null ? "" : person.Element("date-of-birth").Element("day").Value;
                    string m = person.Element("date-of-birth").Element("month") == null ? "" : person.Element("date-of-birth").Element("month").Value;
                    string y = person.Element("date-of-birth").Element("year") == null ? "" : person.Element("date-of-birth").Element("year").Value;
                    token.Profile.DateOfBirth = string.Join("/", d, m, y);
                }

                if (person.Element("location") != null)
                    person.Element("location").Elements().ToList().ForEach(
                        x => token.Profile.Country += x.Value);

                token.Profile.IsSet = true;
                logger.Info("Profile successfully received");
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.ProfileParsingError(response), ex);
                throw new DataParsingException(ErrorMessages.ProfileParsingError(response), ex);
            }
            return token.Profile;
        }
        public override List<Contact> GetContacts()
        {
            Token token = SocialAuthUser.GetCurrentUser().GetConnection(this.ProviderType).GetConnectionToken();
            List<Contact> contacts = new List<Contact>();
            string response = "";
            try
            {

                Stream responseStream = AuthenticationStrategy.ExecuteFeed(ContactsEndpoint, this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                response = new StreamReader(responseStream).ReadToEnd();
            }
            catch
            {
                throw;
            }
            try
            {
                XDocument contactsXml = XDocument.Parse(response);
                IEnumerable<XElement> persons = contactsXml.Root.Elements("person");
                foreach (var person in persons)
                {
                    contacts.Add(new Contact()
                    {
                        ID = person.Element("id") != null ? person.Element("id").Value : "",
                        ProfileURL = person.Element("public-profile-url") != null ? person.Element("public-profile-url").Value : "",
                        Name = person.Element("first-name") != null ? person.Element("first-name").Value : "" + " " + person.Element("first-name") != null ? person.Element("last-name").Value : ""

                    });
                }
                logger.Info("Contacts successfully received");
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.ContactsParsingError(response), ex);
                throw new DataParsingException(ErrorMessages.ContactsParsingError(response), ex);
            }
            return contacts;
        }
        public override WebResponse ExecuteFeed(string feedUrl, TRANSPORT_METHOD transportMethod)
        {
            return AuthenticationStrategy.ExecuteFeed(feedUrl, this, SocialAuthUser.GetCurrentUser().GetConnection(ProviderType).GetConnectionToken(), transportMethod);
        }
        public static WebResponse ExecuteFeed(string feedUrl, string accessToken, string tokenSecret, TRANSPORT_METHOD transportMethod)
        {
            LinkedInWrapper wrapper = new LinkedInWrapper();
            return wrapper.AuthenticationStrategy.ExecuteFeed(feedUrl, wrapper, new Token() { AccessToken = accessToken, TokenSecret = tokenSecret }, transportMethod);
        }

        #endregion
    }
}
