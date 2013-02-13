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

    public class FacebookWrapper : Provider, IProvider
    {
        #region IProvider Members

        //****** PROPERTIES
        private static readonly ILog logger = log4net.LogManager.GetLogger("FacebookWrapper");
        public override PROVIDER_TYPE ProviderType { get { return PROVIDER_TYPE.FACEBOOK; } }
        public override string UserLoginEndpoint { get { return "https://www.facebook.com/dialog/oauth"; } set { } }
        public override string AccessTokenEndpoint { get { return "https://graph.facebook.com/oauth/access_token"; } }
        public override OAuthStrategyBase AuthenticationStrategy { get { return new OAuth2_0server(this); } }
        public override string ProfileEndpoint { get { return "https://graph.facebook.com/me"; } }

        public override string ContactsEndpoint { get { return "https://graph.facebook.com/me/friends"; } }
        public override string ProfilePictureEndpoint { get { return "https://graph.facebook.com/me/picture?type=large"; } }
        public override SIGNATURE_TYPE SignatureMethod { get { throw new NotImplementedException(); } }
        public override TRANSPORT_METHOD TransportName { get { return TRANSPORT_METHOD.POST; } }

        public override string DefaultScope { get { return "user_birthday,user_location"; } }


        //****** OPERATIONS
        public override UserProfile GetProfile()
        {
            Token token = SocialAuthUser.GetConnection(ProviderType).GetConnectionToken();
            OAuthStrategyBase strategy = AuthenticationStrategy;
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
                Stream responseStream = strategy.ExecuteFeed(ProfileEndpoint, this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                response = new StreamReader(responseStream).ReadToEnd();
            }
            catch
            {
                throw;
            }

            try
            {

                JObject jsonObject = JObject.Parse(response);
                token.Profile.ID = jsonObject.Get("id");
                token.Profile.FirstName = jsonObject.Get("first_name");
                token.Profile.LastName = jsonObject.Get("last_name");
                token.Profile.Username = jsonObject.Get("username");
                token.Profile.DisplayName = token.Profile.FullName;
                string[] locale = jsonObject.Get("locale").Split(new char[] { '_' });
                if (locale.Length > 0)
                {
                    token.Profile.Language = locale[0];
                    token.Profile.Country = locale[1];
                }
                token.Profile.ProfileURL = jsonObject.Get("link");
                token.Profile.Email = HttpUtility.UrlDecode(jsonObject.Get("email"));
                if (!string.IsNullOrEmpty(jsonObject.Get("birthday")))
                {
                    string[] dt = jsonObject.Get("birthday").Split(new char[] { '/' });
                    token.Profile.DateOfBirth = dt[1] + "/" + dt[0] + "/" + dt[2];
                }
                token.Profile.GenderType = Utility.ParseGender(jsonObject.Get("gender"));
                //get profile picture
                if (!string.IsNullOrEmpty(ProfileEndpoint))
                {
                    token.Profile.ProfilePictureURL = strategy.ExecuteFeed(ProfilePictureEndpoint, this, token, TRANSPORT_METHOD.GET).ResponseUri.AbsoluteUri.Replace("\"", "");
                }
                token.Profile.IsSet = true;
                logger.Info("Profile successfully received");
                //Session token updated with profile
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
            Token token = SocialAuthUser.GetConnection(ProviderType).GetConnectionToken();
            OAuthStrategyBase strategy = this.AuthenticationStrategy;
            Stream responseStream = null;
            string response = "";
            try
            {
                logger.Debug("Executing contacts feed");
                responseStream = strategy.ExecuteFeed(ContactsEndpoint, this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                response = new StreamReader(responseStream).ReadToEnd();
            }
            catch
            {
                throw;
            }

            try
            {
                JObject jsonObject = JObject.Parse(response);
                var friends = from f in jsonObject["data"].Children()
                              select new Contact
                              {
                                  Name = (string)f["name"],
                                  ID = (string)f["id"],
                                  ProfileURL = "http://www.facebook.com/profile.php?id=" + (string)f["id"]
                              };
                logger.Info("Contacts successfully received");
                return friends.ToList<Contact>();
            }

            catch (Exception ex)
            {
                logger.Error(ErrorMessages.ContactsParsingError(response), ex);
                throw new DataParsingException(ErrorMessages.ContactsParsingError(response), ex);
            }

        }
        public override WebResponse ExecuteFeed(string feedUrl, TRANSPORT_METHOD transportMethod)
        {
            logger.Debug("Calling execution of " + feedUrl);
            return AuthenticationStrategy.ExecuteFeed(feedUrl, this, SocialAuthUser.GetConnection(ProviderType).GetConnectionToken(), transportMethod);
        }

        #endregion
    }


    internal class MSNWrapper : Provider, IProvider
    {
        #region IProvider Members

        //****** PROPERTIES
        private static readonly ILog logger = log4net.LogManager.GetLogger("MSNWrapper");
        public override PROVIDER_TYPE ProviderType { get { return PROVIDER_TYPE.MSN; } }
        public override string UserLoginEndpoint { get { return "https://oauth.live.com/authorize"; } set { } }
        public override string AccessTokenEndpoint { get { return "https://oauth.live.com/token"; } }
        public override OAuthStrategyBase AuthenticationStrategy { get { return new OAuth2_0server(this); } }
        public override string ProfileEndpoint { get { return "https://apis.live.net/v5.0/me"; } }
        public override string ContactsEndpoint { get { return "https://apis.live.net/v5.0/me/contacts"; } }
        public override SIGNATURE_TYPE SignatureMethod { get { throw new NotImplementedException(); } }
        public override TRANSPORT_METHOD TransportName { get { return TRANSPORT_METHOD.POST; } }


        public override string DefaultScope { get { return "wl.emails,wl.birthday"; } }



        //****** OPERATIONS
        public override UserProfile GetProfile()
        {

            //If token already has profile for this provider, we can return it to avoid a call
            Token token = SocialAuthUser.GetConnection(ProviderType).GetConnectionToken();
            if (token.Profile.IsSet)
            {
                logger.Debug("Profile successfully returned from session");
                return token.Profile;
            }

            //Fetch Profile
            OAuthStrategyBase strategy = AuthenticationStrategy;
            string response = "";

            try
            {
                logger.Debug("Executing profile feed");
                Stream responseStream = strategy.ExecuteFeed(ProfileEndpoint, this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                response = new StreamReader(responseStream).ReadToEnd();
            }
            catch
            {
                throw;
            }

            try
            {
                JObject profileJson = JObject.Parse(response);
                token.Profile.ID = profileJson.Get("id");
                token.Profile.FirstName = profileJson.Get("first_name");
                token.Profile.LastName = profileJson.Get("last_name");
                token.Profile.Country = profileJson.Get("Location");
                token.Profile.ProfilePictureURL = profileJson.Get("ThumbnailImageLink");
                token.Profile.Email = profileJson.Get("emails.account");
                token.Profile.GenderType = Utility.ParseGender(profileJson.Get("gender"));
                token.Profile.IsSet = true;
                logger.Info("Profile successfully received");
                return token.Profile;
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.ProfileParsingError(response), ex);
                throw new DataParsingException(ErrorMessages.ProfileParsingError(response), ex);
            }

        }
        public override List<Contact> GetContacts()
        {
            Token token = SocialAuthUser.GetConnection(ProviderType).GetConnectionToken();
            List<Contact> contacts = new List<Contact>();
            string response = "";
            try
            {
                logger.Debug("Executing contacts feed");
                Stream responseStream = AuthenticationStrategy.ExecuteFeed(ContactsEndpoint + "?access_token=" + token.AccessToken, null, token, TRANSPORT_METHOD.GET).GetResponseStream();
                response = new StreamReader(responseStream).ReadToEnd();
            }
            catch { throw; }

            try
            {
                JObject contactsJson = JObject.Parse(response);
                contactsJson.SelectToken("data").ToList().ForEach(x =>
                    {
                        contacts.Add(new Contact()
                        {
                            ID = x.SelectToken("id").ToString(),
                            Name = x.SelectToken("first_name").ToString() + " " + x.SelectToken("last_name").ToString()
                        });
                    }
                    );
                logger.Info("Contacts successfully received");
                return contacts;
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.ContactsParsingError(response), ex);
                throw new DataParsingException(ErrorMessages.ContactsParsingError(response), ex);
            }
        }
        public override WebResponse ExecuteFeed(string feedUrl, TRANSPORT_METHOD transportMethod)
        {
            logger.Debug("Calling execution of " + feedUrl);
            return AuthenticationStrategy.ExecuteFeed(feedUrl, this, SocialAuthUser.GetConnection(ProviderType).GetConnectionToken(), transportMethod);
        }
        public static WebResponse ExecuteFeed(string feedUrl, string accessToken, TRANSPORT_METHOD transportMethod)
        {
            MSNWrapper msn = new MSNWrapper();
            return msn.AuthenticationStrategy.ExecuteFeed(feedUrl, msn, new Token() { AccessToken = accessToken }, transportMethod);
        }

        #endregion
    }


    internal class TwitterWrapper : Provider, IProvider
    {
        #region IProvider Members

        //****** PROPERTIES
        private static readonly ILog logger = log4net.LogManager.GetLogger("TwitterWrapper");
        public override PROVIDER_TYPE ProviderType { get { return PROVIDER_TYPE.TWITTER; } }
        public override string RequestTokenEndpoint { get { return "https://api.twitter.com/oauth/request_token"; } }
        public override string UserLoginEndpoint { get { return "https://api.twitter.com/oauth/authorize"; } set { } }
        public override string AccessTokenEndpoint { get { return "https://api.twitter.com/oauth/access_token"; } }
        public override OAuthStrategyBase AuthenticationStrategy { get { return new OAuth1_0a(this); } }
        public override string ProfileEndpoint { get { return "http://api.twitter.com/1/users/show.json"; } }
        public override string ContactsEndpoint { get { return "http://api.twitter.com/1/friends/ids.json?screen_name={0}&cursor=-1"; } }
        public override SIGNATURE_TYPE SignatureMethod { get { return SIGNATURE_TYPE.HMACSHA1; } }
        public override TRANSPORT_METHOD TransportName { get { return TRANSPORT_METHOD.POST; } }

        public override string DefaultScope { get { return ""; } }



        public override void AuthenticationCompleting(bool isSuccess)
        {
            Token token = SocialAuthUser.InProgressToken();
            token.Profile.DisplayName = token.ResponseCollection["screen_name"];
            token.Profile.ID = token.ResponseCollection["user_id"];
        }


        //****** OPERATIONS
        public override UserProfile GetProfile()
        {
            Token token = SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken();
            UserProfile profile = new UserProfile(ProviderType);
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
                string profileUrl = ProfileEndpoint + "?user_id=" + token.Profile.ID;
                Stream responseStream = AuthenticationStrategy.ExecuteFeed(profileUrl, this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                response = new StreamReader(responseStream).ReadToEnd();

            }
            catch
            {
                throw;
            }

            try
            {



                JObject profileJson = JObject.Parse(response);
                profile.ID = profileJson.Get("id_str");
                profile.FirstName = profileJson.Get("name");
                profile.Country = profileJson.Get("location");
                profile.DisplayName = profileJson.Get("screen_name");
                //profile.Email =  not provided
                profile.Language = profileJson.Get("lang");
                profile.ProfilePictureURL = profileJson.Get("profile_image_url");
                profile.IsSet = true;
                token.Profile = profile;
                logger.Info("Profile successfully received");
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.ProfileParsingError(response), ex);
                throw new DataParsingException(ErrorMessages.ProfileParsingError(response), ex);
            }
            return profile;


        }
        public override List<BusinessObjects.Contact> GetContacts()
        {
            List<Contact> contacts = new List<Contact>();
            string response = "";
            List<string> sets = new List<string>();
               
            Token token = SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken();
            string friendsUrl = string.Format(ContactsEndpoint, token.Profile.Email);
            try
            {
                logger.Debug("Executing contacts feed");
                Stream responseStream = AuthenticationStrategy.ExecuteFeed(friendsUrl, this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                response = new StreamReader(responseStream).ReadToEnd();
            }
            catch { throw; }
            try
            {
                string friendIDs = "";
                var friends = JObject.Parse(response).SelectToken("ids").Children().ToList();
                friendIDs = "";
                foreach (var s in friends)
                    friendIDs += (s.ToString() + ",");

                char[] arr = friendIDs.ToArray<char>();
                var iEnumerator = arr.GetEnumerator();
                int counter = 0;
                string temp = "";
                while (iEnumerator.MoveNext())
                {
                    if (iEnumerator.Current.ToString() == ",")
                        counter += 1;
                    if (counter == 100)
                    {
                        sets.Add(temp);
                        temp = "";
                        counter = 0;
                        continue;
                    }
                    temp += iEnumerator.Current;
                }
                if (temp != "")
                    sets.Add(temp);
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.ContactsParsingError(response), ex);
                throw new DataParsingException(ErrorMessages.ContactsParsingError(response), ex);
            }
            foreach (string set in sets)
            {

                contacts.AddRange(Friends(set, token));
            }
            logger.Info("Contacts successfully received");
            return contacts;
        }
        public override WebResponse ExecuteFeed(string feedUrl, TRANSPORT_METHOD transportMethod)
        {
            logger.Debug("Calling execution of " + feedUrl);
            return AuthenticationStrategy.ExecuteFeed(feedUrl, this, SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken(), transportMethod);
        }
        public static WebResponse ExecuteFeed(string feedUrl, string accessToken, string tokenSecret, TRANSPORT_METHOD transportMethod)
        {
            TwitterWrapper wrapper = new TwitterWrapper();
            return wrapper.AuthenticationStrategy.ExecuteFeed(feedUrl, wrapper, new Token() { AccessToken = accessToken, TokenSecret = tokenSecret }, transportMethod);
        }
        private List<Contact> Friends(string friendUserIDs, Token token)
        {
            string lookupUrl = "http://api.twitter.com/1/users/lookup.json?user_id=" + friendUserIDs;
            OAuthHelper helper = new OAuthHelper();
            string friendsData = "";
            try
            {
                Stream responseStream = AuthenticationStrategy.ExecuteFeed(lookupUrl, this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                friendsData = new StreamReader(responseStream).ReadToEnd();
            }
            catch { throw; }

            List<Contact> friends = new List<Contact>();

            try
            {
                JArray j = JArray.Parse(friendsData);
                j.ToList().ForEach(f =>
                {
                    friends.Add(
                      new Contact()
                      {
                          Name = (string)f["name"],
                          ID = (string)f["id_str"],
                          ProfileURL = "http://twitter.com/#!/" + (string)f["screen_name"]
                      });

                });
            }
            catch 
            {
                throw;
            }
            return friends.ToList<Contact>();

        }

        #endregion
    }


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

            Token token = SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken();
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
            Token token = SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken();
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
                        ProfileURL = person.Element("public-profile-url").Value,
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
            return AuthenticationStrategy.ExecuteFeed(feedUrl, this, SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken(), transportMethod);
        }
        public static WebResponse ExecuteFeed(string feedUrl, string accessToken, string tokenSecret, TRANSPORT_METHOD transportMethod)
        {
            LinkedInWrapper wrapper = new LinkedInWrapper();
            return wrapper.AuthenticationStrategy.ExecuteFeed(feedUrl, wrapper, new Token() { AccessToken = accessToken, TokenSecret = tokenSecret }, transportMethod);
        }

        #endregion
    }


    internal class YahooWrapper : Provider, IProvider
    {
        #region IProvider Members

        //****** PROPERTIES
        private static readonly ILog logger = log4net.LogManager.GetLogger("YahooWrapper");
        public override PROVIDER_TYPE ProviderType { get { return PROVIDER_TYPE.YAHOO; } }
        public override string RequestTokenEndpoint { get { return "https://api.login.yahoo.com/oauth/v2/get_request_token"; } }
        string userloginendpoint = "https://api.login.yahoo.com/oauth/v2/request_auth";
        public override string UserLoginEndpoint { get { return userloginendpoint; } set { userloginendpoint = value; } }
        public override string AccessTokenEndpoint { get { return "https://api.login.yahoo.com/oauth/v2/get_token"; } }
        public override OAuthStrategyBase AuthenticationStrategy { get { return new OAuth1_0Hybrid(this); } }
        public override string ProfileEndpoint { get { return "http://social.yahooapis.com/v1/user/{0}/profile"; } }
        public override string ContactsEndpoint { get { return "http://social.yahooapis.com/v1/user/{0}/contacts"; } }
        public override SIGNATURE_TYPE SignatureMethod { get { return SIGNATURE_TYPE.HMACSHA1; } }
        public override TRANSPORT_METHOD TransportName { get { return TRANSPORT_METHOD.GET; } }
        public override string OpenIdDiscoveryEndpoint { get { return "http://open.login.yahooapis.com/openid20/www.yahoo.com/xrds"; } }
        public override string DefaultScope { get { return ""; } }



        //****** OPERATIONS
        public override UserProfile GetProfile()
        {
            Token token = SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken();
            UserProfile profile = new UserProfile(ProviderType);
            string response = "";

            //If token already has profile for this provider, we can return it to avoid a call
            if (token.Profile.IsSet)
                return token.Profile;

            try
            {
                logger.Debug("Executing Profile feed");
                Stream responseStream = AuthenticationStrategy.ExecuteFeed(
                    string.Format(ProfileEndpoint, token.ResponseCollection["xoauth_yahoo_guid"]), this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                response = new StreamReader(responseStream).ReadToEnd();
            }
            catch
            {
                throw;
            }

            try
            {

                XDocument xDoc = XDocument.Parse(response);
                XNamespace xn = xDoc.Root.GetDefaultNamespace();
                profile.ID = xDoc.Root.Element(xn + "guid") != null ? xDoc.Root.Element(xn + "guid").Value : string.Empty;
                profile.FirstName = xDoc.Root.Element(xn + "givenName") != null ? xDoc.Root.Element(xn + "givenName").Value : string.Empty;
                profile.LastName = xDoc.Root.Element(xn + "familyName") != null ? xDoc.Root.Element(xn + "familyName").Value : string.Empty;
                profile.DateOfBirth = xDoc.Root.Element(xn + "birthdate") != null ? xDoc.Root.Element(xn + "birthdate").Value : "/";
                profile.DateOfBirth = xDoc.Root.Element(xn + "birthyear") != null ? "/" + xDoc.Root.Element(xn + "birthyear").Value : "/";
                profile.Country = xDoc.Root.Element(xn + "location") != null ? xDoc.Root.Element(xn + "location").Value : string.Empty;
                profile.ProfileURL = xDoc.Root.Element(xn + "profileUrl") != null ? xDoc.Root.Element(xn + "profileUrl").Value : string.Empty;
                profile.ProfilePictureURL = xDoc.Root.Element(xn + "image") != null ? xDoc.Root.Element(xn + "image").Element(xn + "imageUrl").Value : string.Empty;
                profile.Language = xDoc.Root.Element(xn + "lang") != null ? xDoc.Root.Element(xn + "lang").Value : string.Empty;
                if (xDoc.Root.Element(xn + "gender") != null)
                    profile.GenderType = Utility.ParseGender(xDoc.Root.Element(xn + "gender").Value);

                if (string.IsNullOrEmpty(profile.FirstName))
                    profile.FirstName = token.ResponseCollection["openid.ax.value.firstname"];
                if (string.IsNullOrEmpty(profile.FirstName))
                    profile.LastName = token.ResponseCollection["openid.ax.value.lastname"];
                profile.Email = token.ResponseCollection.Get("openid.ax.value.email");
                profile.Country = token.ResponseCollection.Get("openid.ax.value.country");
                profile.Language = token.ResponseCollection.Get("openid.ax.value.language");
                profile.IsSet = true;

                profile.IsSet = true;
                token.Profile = profile;
                logger.Info("Profile successfully received");
            }
            catch (Exception ex)
            {
                logger.Error(ErrorMessages.ProfileParsingError(response), ex);
                throw new DataParsingException(ErrorMessages.ProfileParsingError(response), ex);
            }
            return profile;
        }
        public override List<Contact> GetContacts()
        {
            Token token = SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken();
            List<Contact> contacts = new List<Contact>();
            string response = "";
            try
            {
                logger.Debug("Executing contacts feed");
                Stream responseStream = AuthenticationStrategy.ExecuteFeed(
                    string.Format(ContactsEndpoint, token.ResponseCollection["xoauth_yahoo_guid"]), this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                response = new StreamReader(responseStream).ReadToEnd();
            }
            catch { throw; }

            try
            {

                //Extract information from XML

                XDocument xdoc = XDocument.Parse(response);
                XNamespace xn = xdoc.Root.GetDefaultNamespace();
                XNamespace attxn = "http://www.yahooapis.com/v1/base.rng";

                xdoc.Root.Descendants(xdoc.Root.GetDefaultNamespace() + "contact").ToList().ForEach(x =>
                 {
                     IEnumerable<XElement> contactFields = x.Elements(xn + "fields").ToList();
                     foreach (var field in contactFields)
                     {
                         Contact contact = new Contact();

                         if (field.Attribute(attxn + "uri").Value.Contains("/yahooid/"))
                         {
                             //contact.Name = field.Element(xn + "value").Value;
                             //contact.Email = field.Element(xn + "value").Value + "@yahoo.com";
                         }
                         else if (field.Attribute(attxn + "uri").Value.Contains("/name/"))
                         {
                             //Contact c = contacts.Last<Contact>();
                             //c.Name = field.Element(xn + "value").Element(xn + "givenName").Value + " " + field.Element(xn + "value").Element(xn + "familyName").Value;
                             //contacts[contacts.Count - 1] = c;
                             //continue;
                         }
                         else if (field.Attribute(attxn + "uri").Value.Contains("/email/"))
                         {
                             contact.Name = field.Element(xn + "value").Value.Replace("@yahoo.com", "");
                             contact.Email = field.Element(xn + "value").Value;
                         }
                         if (!string.IsNullOrEmpty(contact.Name) && !contacts.Exists(y => y.Email == contact.Email))
                             contacts.Add(contact);
                     }
                 });
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
            return AuthenticationStrategy.ExecuteFeed(feedUrl, this, SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken(), transportMethod);
        }
        public static WebResponse ExecuteFeed(string feedUrl, string accessToken, string tokenSecret, TRANSPORT_METHOD transportMethod)
        {
            YahooWrapper wrapper = new YahooWrapper();
            return wrapper.AuthenticationStrategy.ExecuteFeed(feedUrl, wrapper, new Token() { AccessToken = accessToken, TokenSecret = tokenSecret }, transportMethod);
        }

        #endregion
    }


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
                //[OAUTH 1.0a] oauth1_0astrategy.BeforeRequestingRequestToken += (x) => { x.Add(new QueryParameter("scope", GetScope())); };
                oauth1_0HybridStrategy.BeforeDirectingUserToServiceProvider += (x) => { x.Add(new QueryParameter("openid.ext2.scope", GetScope())); };
                return oauth1_0HybridStrategy;
            }
        }
        public override string ProfileEndpoint { get { return "http://www-opensocial.googleusercontent.com/api/people/@me"; } }
        public override string ContactsEndpoint { get { return "http://www.google.com/m8/feeds/contacts/default/full/?max-results=1000&"; } }
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

        public override string DefaultScope { get { return "https://www-opensocial.googleusercontent.com/api/people/ http://www.google.com/m8/feeds/"; } }



        //****** OPERATIONS
        public override UserProfile GetProfile()
        {

            Token token = SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken();
            UserProfile profile = new UserProfile(ProviderType);
            string response = "";
            //If token already has profile for this provider, we can return it to avoid a call
            if (token.Profile.IsSet || !IsProfileSupported)
                return token.Profile;

            var provider = ProviderFactory.GetProvider(token.Provider);

            if (GetScope().Contains("https://www-opensocial.googleusercontent.com/api/people/"))
            {
                try
                {
                    logger.Debug("Executing profile feed");
                    Stream responseStream = AuthenticationStrategy.ExecuteFeed(ProfileEndpoint, this, token, TRANSPORT_METHOD.GET).GetResponseStream();
                    response = new StreamReader(responseStream).ReadToEnd();
                }
                catch (Exception ex)
                { throw; }

                try
                {
                    JObject profileJson = JObject.Parse(response);
                    //{"entry":{"profileUrl":"https://plus.google.com/103908432244378021535","isViewer":true,"id":"103908432244378021535",
                    //    "name":{"formatted":"deepak Aggarwal","familyName":"Aggarwal","givenName":"deepak"},
                    //    "thumbnailUrl":"http://www.,"urls":[{"value":"https://plus.google.com/103908432244378021535","type":"profile"}],
                    //    "photos":[{"value":"http://www.google.com/ig/c/photos/public/AIbEiAIAAABDCJ_d1payzeKeNiILdmNhcmRfcGhvdG8qKGFjM2RmMzQ1ZDc4Nzg5NmI5NmFjYTc1NDNjOTA3MmQ5MmNmOTYzZWIwAe0HZMa7crOI_laYBG7LxYvlAvqe","type":"thumbnail"}],"displayName":"deepak Aggarwal"}}
                    profile.Provider = ProviderType;
                    profile.ID = profileJson.Get("entry.id");
                    profile.ProfileURL = profileJson.Get("entry.profileUrl");
                    profile.FirstName = profileJson.Get("entry.name.givenName");
                    profile.LastName = profileJson.Get("entry.name.familyName");
                    profile.ProfilePictureURL = profileJson.Get("entry.thumbnailUrl");
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
            Token token = SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken();
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
                               Email = (c.Element(xn + "email") == null) ? "" : c.Element(xn + "email").Attribute("address").Value
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
            return AuthenticationStrategy.ExecuteFeed(feedUrl, this, SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken(), transportMethod);
        }
        public static WebResponse ExecuteFeed(string feedUrl, string accessToken, string tokenSecret, TRANSPORT_METHOD transportMethod)
        {
            GoogleWrapper wrapper = new GoogleWrapper();
            return wrapper.AuthenticationStrategy.ExecuteFeed(feedUrl, wrapper, new Token() { AccessToken = accessToken, TokenSecret = tokenSecret }, transportMethod);
        }

        #endregion
    }


    internal class MySpaceWrapper : Provider, IProvider
    {
        #region IProvider Members

        //****** PROPERTIES
        private static readonly ILog logger = log4net.LogManager.GetLogger("MySpaceWrapper");
        public override PROVIDER_TYPE ProviderType { get { return PROVIDER_TYPE.MYSPACE; } }
        public override string RequestTokenEndpoint { get { return "http://api.myspace.com/request_token"; } }
        public override string UserLoginEndpoint { get { return "http://api.myspace.com/authorize"; } set { } }
        public override string AccessTokenEndpoint { get { return "http://api.myspace.com/access_token"; } }
        public override OAuthStrategyBase AuthenticationStrategy
        {
            get
            {
                OAuth1_0a oauth1_0astrategy = new OAuth1_0a(this);
                oauth1_0astrategy.BeforeDirectingUserToServiceProvider += (x) => { x.Add(new QueryParameter("oauth_callback", Utility.UrlEncode(SocialAuthUser.InProgressToken().ProviderCallbackUrl))); };
                return oauth1_0astrategy;
            }
        }
        public override string ProfileEndpoint { get { return "http://api.myspace.com/1.0/people/@me/@self"; } }
        public override string ContactsEndpoint { get { return "http://api.myspace.com/1.0/people/@me/@all"; } }
        public override SIGNATURE_TYPE SignatureMethod { get { return SIGNATURE_TYPE.HMACSHA1; } }
        public override TRANSPORT_METHOD TransportName { get { return TRANSPORT_METHOD.GET; } }
        public override string DefaultScope { get { return ""; } }



        //****** OPERATIONS
        public override UserProfile GetProfile()
        {
            Token token = SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken();
            string response = "";
            //If token already has profile for this provider, we can return it to avoid a call
            if (token.Profile.IsSet)
                return token.Profile;
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
                JObject profileJson = JObject.Parse(response);
                token.Profile.ID = profileJson.Get("person.id");
                token.Profile.FirstName = profileJson.Get("person.name.givenName");
                token.Profile.LastName = profileJson.Get("person.name.familyName");
                token.Profile.Country = profileJson.Get("person.location");
                token.Profile.Language = profileJson.Get("person.lang");
                token.Profile.ProfilePictureURL = profileJson.Get("person.thumbnailUrl");
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
            Token token = SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken();
            List<Contact> contacts = new List<Contact>();
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
                JArray contactsJson = JArray.Parse(JObject.Parse(response).SelectToken("entry").ToString());
                contactsJson.ToList().ForEach(person =>
                               contacts.Add(new Contact()
                               {
                                   ID = person.SelectToken("person.id") != null ? person.SelectToken("person.id").ToString().Replace("\"", "") : "",
                                   ProfileURL = person.SelectToken("person.profileUrl").ToString().Replace("\"", ""),
                                   Name = person.SelectToken("person.name.givenName") != null ? person.SelectToken("person.name.givenName").ToString() : "" + " " + person.SelectToken("person.name.familyName") != null ? person.SelectToken("person.name.familyName").ToString().Replace("\"", "") : ""

                               }));
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
            return AuthenticationStrategy.ExecuteFeed(feedUrl, this, SocialAuthUser.GetConnection(this.ProviderType).GetConnectionToken(), transportMethod);
        }
        public static WebResponse ExecuteFeed(string feedUrl, string accessToken, string tokenSecret, TRANSPORT_METHOD transportMethod)
        {
            MySpaceWrapper wrapper = new MySpaceWrapper();
            return wrapper.AuthenticationStrategy.ExecuteFeed(feedUrl, wrapper, new Token() { AccessToken = accessToken, TokenSecret = tokenSecret }, transportMethod);
        }

        #endregion
    }

}



