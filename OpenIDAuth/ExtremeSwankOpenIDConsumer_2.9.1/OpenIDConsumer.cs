using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Web;
using System.Security.Cryptography;
using Mono.Security.Cryptography;
using System.Diagnostics;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// Provides an OpenID Relying Party (Consumer) compatible with OpenID 1.1 and 2.0 specifications.
    /// </summary>
    [Serializable]
    public class OpenIDConsumer : ConsumerBase
    {
        #region Fields and Properties

        AuthorizationMode authmode;
        KeyEncryption keyenc;
        DateTime NextCleanup;

        /// <summary>
        /// Authorization mode - either Stateful or Stateless
        /// </summary>
        public AuthorizationMode AuthMode
        {
            get
            {
                return authmode;
            }
            set { authmode = value; }
        }
        /// <summary>
        /// For Stateful mode, choose whether or not the shared secret will be encrypted
        /// </summary>
        public KeyEncryption Encryption
        {
            get
            {
                return keyenc;
            }
            set { keyenc = value; }
        }
        private bool DumbMode
        {
            get { return (bool)SessionPersister["ForceDumb"]; }
            set { SessionPersister["ForceDumb"] = value; }
        }
        private string RedirectURLStateful
        {
            get
            {
                DataRow assoc = FindAssocByServer(OpenIDServer);

                Random r = new Random();
                AllowLogin = r.Next();

                Dictionary<string, string> pms = new Dictionary<string, string>();
                if (URLs["approved"].Contains("?"))
                {
                    pms["openid.return_to"] = HttpUtility.UrlEncode(URLs["approved"] + "&cnonce=" + AllowLogin);
                }
                else
                {
                    pms["openid.return_to"] = HttpUtility.UrlEncode(URLs["approved"] + "?cnonce=" + AllowLogin);
                }

                switch (AuthVersion)
                {
                    case ProtocolVersion.V1_1:
                        pms["openid.mode"] = "checkid_setup";
                        pms["openid.assoc_handle"] = HttpUtility.UrlEncode((string)assoc["handle"]);
                        pms["openid.identity"] = HttpUtility.UrlEncode(openid_url_identity);
                        pms["openid.trust_root"] = HttpUtility.UrlEncode(URLs["trust_root"]);

                        foreach (IExtension e in PluginsExtension)
                        {
                            foreach (string key in e.AuthorizationData.Keys)
                            {
                                pms[key] = e.AuthorizationData[key];
                            }
                        }
                        break;
                    case ProtocolVersion.V2_0:
                        pms["openid.ns"] = "http://specs.openid.net/auth/2.0";
                        pms["openid.mode"] = "checkid_setup";
                        pms["openid.assoc_handle"] = HttpUtility.UrlEncode((string)assoc["handle"]);
                        pms["openid.identity"] = HttpUtility.UrlEncode(openid_url_identity);
                        pms["openid.claimed_id"] = HttpUtility.UrlEncode(openid_identity);
                        pms["openid.realm"] = HttpUtility.UrlEncode(URLs["trust_root"]);

                        foreach (IExtension e in PluginsExtension)
                        {
                            foreach (string key in e.AuthorizationData.Keys)
                            {
                                pms[key] = e.AuthorizationData[key];
                            }
                        }
                        break;
                }
                if (URLs["openid_server"].Contains("?"))
                {
                    return URLs["openid_server"] + "&" + Utility.Keyval2URL(pms);
                }
                return URLs["openid_server"] + "?" + Utility.Keyval2URL(pms);
            }
        }
        private string RedirectURLImmediate
        {
            get
            {
                DataRow assoc = FindAssocByServer(OpenIDServer);

                Random r = new Random();
                AllowLogin = r.Next();

                Dictionary<string, string> pms = new Dictionary<string, string>();
                if (URLs["approved"].Contains("?"))
                {
                    pms["openid.return_to"] = HttpUtility.UrlEncode(URLs["approved"] + "&cnonce=" + AllowLogin);
                }
                else
                {
                    pms["openid.return_to"] = HttpUtility.UrlEncode(URLs["approved"] + "?cnonce=" + AllowLogin);
                }

                switch (AuthVersion)
                {
                    case ProtocolVersion.V1_1:
                        pms["openid.mode"] = "checkid_immediate";
                        pms["openid.identity"] = HttpUtility.UrlEncode(openid_url_identity);
                        pms["openid.assoc_handle"] = HttpUtility.UrlEncode((string)assoc["handle"]);
                        pms["openid.trust_root"] = HttpUtility.UrlEncode(URLs["trust_root"]);

                        foreach (IExtension e in PluginsExtension)
                        {
                            foreach (string key in e.AuthorizationData.Keys)
                            {
                                pms[key] = e.AuthorizationData[key];
                            }
                        }
                        break;
                    case ProtocolVersion.V2_0:
                        pms["openid.mode"] = "checkid_immediate";
                        pms["openid.identity"] = HttpUtility.UrlEncode(openid_url_identity);
                        pms["openid.claimed_id"] = HttpUtility.UrlEncode(openid_identity);
                        pms["openid.assoc_handle"] = HttpUtility.UrlEncode((string)assoc["handle"]);
                        pms["openid.realm"] = HttpUtility.UrlEncode(URLs["trust_root"]);

                        foreach (IExtension e in PluginsExtension)
                        {
                            foreach (string key in e.AuthorizationData.Keys)
                            {
                                pms[key] = e.AuthorizationData[key];
                            }
                        }
                        break;
                }
                if (URLs["openid_server"].Contains("?"))
                {
                    return URLs["openid_server"] + "&" + Utility.Keyval2URL(pms);
                }
                return URLs["openid_server"] + "?" + Utility.Keyval2URL(pms);
            }
        }
        private DataTable Associations
        {
            get
            {
                if (GlobalPersister["Associations"] == null)
                {
                    InitAssocs();
                }
                return (DataTable)GlobalPersister["Associations"];
            }
            set
            {
                if (GlobalPersister["Associations"] == null)
                {
                    InitAssocs();
                }
                GlobalPersister["Associations"] = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Provides a new OpenIDConsumer object with default settings.
        /// </summary>
        public OpenIDConsumer()
        {
            // Set Defaults
            Init();
            this.DumbMode = false;
            this.authmode = AuthorizationMode.Stateful;
            this.keyenc = KeyEncryption.DHSHA256;
            CleanupAssociations();
        }

        #endregion

        #region Private Methods

        private bool ShareKey()
        {
            // Look for pre-existing valid association
            HttpContext.Current.Trace.Write("Looking up OpenID Server in Associations table");
            DataRow assoc = FindAssocByServer(OpenIDServer);
            if (assoc != null)
            {
                if ((DateTime)assoc["expiration"] > DateTime.Now)
                {
                    HttpContext.Current.Trace.Write("Valid association found.");
                    return true;
                }
            }

            // No valid pre-existing association. Create a new association.
            HttpContext.Current.Trace.Write("No valid association found.");
            Dictionary<string, string> sd = new Dictionary<string, string>();
            DiffieHellmanManaged dhm = new DiffieHellmanManaged();
            sd["openid.mode"] = "associate";

            switch (AuthVersion)
            {
                case ProtocolVersion.V2_0:
                    sd["openid.ns"] = "http://specs.openid.net/auth/2.0";
                    break;
                case ProtocolVersion.V1_1:
                    if (Encryption == KeyEncryption.DHSHA256)
                    {
                        Encryption = KeyEncryption.DHSHA1;
                    }
                    break;
            }

            byte[] pubkey = null;
            DHParameters dp;

            switch (Encryption)
            {
                case KeyEncryption.None:
                    sd["openid.assoc_type"] = "HMAC-SHA1";
                    switch (AuthVersion)
                    {
                        case ProtocolVersion.V2_0:
                            sd["openid.session_type"] = "no-encryption";
                            break;
                        case ProtocolVersion.V1_1:
                            sd["openid.session_type"] = "";
                            break;
                    }
                    break;
                case KeyEncryption.DHSHA1:
                    pubkey = dhm.CreateKeyExchange();
                    dp = dhm.ExportParameters(true);

                    sd["openid.assoc_type"] = "HMAC-SHA1";
                    sd["openid.session_type"] = "DH-SHA1";
                    sd["openid.dh_modulus"] = HttpUtility.UrlEncode(Utility.UnsignedToBase64(dp.P));
                    sd["openid.dh_gen"] = HttpUtility.UrlEncode(Utility.UnsignedToBase64(dp.G));
                    sd["openid.dh_consumer_public"] = HttpUtility.UrlEncode(Utility.UnsignedToBase64(pubkey));
                    break;
                case KeyEncryption.DHSHA256:
                    pubkey = dhm.CreateKeyExchange();
                    dp = dhm.ExportParameters(true);

                    sd["openid.assoc_type"] = "HMAC-SHA256";
                    sd["openid.session_type"] = "DH-SHA256";
                    sd["openid.dh_modulus"] = HttpUtility.UrlEncode(Utility.UnsignedToBase64(dp.P));
                    sd["openid.dh_gen"] = HttpUtility.UrlEncode(Utility.UnsignedToBase64(dp.G));
                    sd["openid.dh_consumer_public"] = HttpUtility.UrlEncode(Utility.UnsignedToBase64(pubkey));
                    break;
            }

            HttpContext.Current.Trace.Write("Opening connection to OpenID Server");
            string response = "";
            response = Utility.MakeRequest(OpenIDServer, "GET", sd);

            Dictionary<string, string> association = new Dictionary<string, string>();
            if (response != null)
            {
                HttpContext.Current.Trace.Write("Association response received.");
                association = Utility.SplitResponse(response);
            }
            else
            {
                HttpContext.Current.Trace.Write("No association response received.");
                return false;
            }

            if (association.ContainsKey("error"))
            {
                // FIXME - Add error handling code
                HttpContext.Current.Trace.Write("Association response contains error.");
                return false;
            }

            if (Encryption == KeyEncryption.DHSHA1 || Encryption == KeyEncryption.DHSHA256)
            {
                HttpContext.Current.Trace.Write("Expecting DHSHA1 or DHSHA256.");
                if (association["enc_mac_key"] != null)
                {
                    HttpContext.Current.Trace.Write("Encrypted association key is present.");
                    byte[] serverpublickey = Convert.FromBase64String(association["dh_server_public"]);
                    byte[] mackey = Convert.FromBase64String(association["enc_mac_key"]);

                    byte[] dhShared = dhm.DecryptKeyExchange(serverpublickey);
                    byte[] shaShared = new byte[0];

                    if (Encryption == KeyEncryption.DHSHA1)
                    {
                        HttpContext.Current.Trace.Write("Decoding DHSHA1 Association.");
                        SHA1 sha1 = new SHA1CryptoServiceProvider();
                        shaShared = sha1.ComputeHash(Utility.EnsurePositive(dhShared));
                    }
                    else if (Encryption == KeyEncryption.DHSHA256)
                    {
                        HttpContext.Current.Trace.Write("Decoding DHSHA256 Association.");
                        SHA256 sha256 = new SHA256Managed();
                        shaShared = sha256.ComputeHash(Utility.EnsurePositive(dhShared));
                    }

                    byte[] secret = new byte[mackey.Length];
                    for (int i = 0; i < mackey.Length; i++)
                    {
                        secret[i] = (byte)(mackey[i] ^ shaShared[i]);
                    }
                    association["mac_key"] = Utility.ToBase64String(secret);
                }
                else
                {
                    ErrorStore.Store(Errors.ReceivedPlaintext);
                    HttpContext.Current.Trace.Write("Error: Received plaintext association when expecting encrypted.");
                    return false;
                }
            }
            AddAssoc(association);
            HttpContext.Current.Trace.Write("Successfully added association to table.");
            return true;
        }

        private bool ValidateWithSharedKey()
        {
            NameValueCollection Request = HttpContext.Current.Request.QueryString;
            if (AllowLogin == -1)
            {
                HttpContext.Current.Trace.Write("Error: The cnonce is not valid.");
                ErrorStore.Store(Errors.SessionTimeout);
                return false;
            }
            else
            {
                if (AllowLogin != Convert.ToInt32(Request["cnonce"]))
                {
                    HttpContext.Current.Trace.Write("Error: The cnonce has expired.");
                    ErrorStore.Store(Errors.SessionTimeout);
                    return false;
                }
                // Reset lock
                AllowLogin = -1;
            }
            HttpContext.Current.Trace.Write("Looking up association in association table.");
            DataRow assoc = FindAssocByHandle(Request["openid.assoc_handle"]);

            if (assoc == null)
            {
                // Check to see if the handle has been invalidated
                if (Request["openid.invalidate_handle"] != null)
                {
                    HttpContext.Current.Trace.Write("Association handle has been invalidated.");
                    ErrorStore.Store(Errors.HandleInvalidated);
                    return false;
                }
                else
                {
                    HttpContext.Current.Trace.Write("Association handle was not found in the table.");
                    ErrorStore.Store(Errors.AssociationNotFound);
                    return false;
                }
            }

            // Ensure association key has not expired
            if ((DateTime)assoc["expiration"] < DateTime.Now)
            {
                HttpContext.Current.Trace.Write("Association has expired, removing from table.");
                ErrorStore.Store(Errors.AssociationExpired);
                Associations.Rows.Remove(assoc);
                Associations.AcceptChanges();
                return false;
            }

            // Compare data from browser to association handle from server
            if ((string)assoc["handle"] == Request["openid.assoc_handle"])
            {
                string[] tokens = Request["openid.signed"].ToString().Split(',');
                string token_contents = "";
                foreach (string token in tokens)
                {
                    token_contents += token + ":" + Request["openid." + token] + "\n";
                }
                HttpContext.Current.Trace.Write("Generating signature for tokens: " + token_contents);
                string sig = Request["openid.sig"].ToString();
                byte[] sigbarr = Convert.FromBase64String(sig);

                byte[] secretkey = (byte[])assoc["secret"];
                byte[] tokenbyte = System.Text.Encoding.UTF8.GetBytes(token_contents);
                HashAlgorithm hmac = null;

                if ((string)assoc["assoc_type"] == "HMAC-SHA1")
                {
                    hmac = new HMACSHA1(secretkey);
                }
                else if ((string)assoc["assoc_type"] == "HMAC-SHA256")
                {
                    hmac = new HMACSHA256(secretkey);
                }
                byte[] realHash = hmac.ComputeHash(tokenbyte);

                HttpContext.Current.Trace.Write("Expected signature: " + sig);
                HttpContext.Current.Trace.Write("Generated signature: " + Convert.ToBase64String(realHash));

                for (int i = 0; i < realHash.Length; i++)
                {
                    if (realHash[i] != sigbarr[i])
                    {
                        HttpContext.Current.Trace.Write(Errors.BadSignature);
                        ErrorStore.Store(Errors.BadSignature);
                        return false;
                    }
                }
                return true;
            }
            HttpContext.Current.Trace.Write(Errors.HandlesDoNotMatch);
            ErrorStore.Store(Errors.HandlesDoNotMatch);
            return false;
        }

        private void InitAssocs()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("protocol", typeof(ProtocolVersion));
            dt.Columns.Add("server", typeof(string));
            dt.Columns.Add("handle", typeof(string));
            dt.Columns.Add("assoc_type", typeof(string));
            dt.Columns.Add("session_type", typeof(string));
            dt.Columns.Add("secret", typeof(byte[]));
            dt.Columns.Add("expiration", typeof(DateTime));
            dt.AcceptChanges();

            GlobalPersister["Associations"] = dt;
        }
        private void AddAssoc(Dictionary<string, string> association)
        {
            // Check for existing association
            DataRow[] result = Associations.Select("server = '" + OpenIDServer + "'");
            if (result.Length > 0)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    Associations.Rows.Remove(result[i]);
                }
            }

            // Add new row
            DataRow dr = Associations.NewRow();
            dr["protocol"] = AuthVersion;
            dr["server"] = OpenIDServer;
            dr["handle"] = association["assoc_handle"];
            dr["assoc_type"] = association["assoc_type"];
            dr["session_type"] = association["session_type"];
            dr["secret"] = Convert.FromBase64String(association["mac_key"]);

            DateTime expiration = DateTime.Now.AddSeconds(Convert.ToDouble(association["expires_in"]));
            dr["expiration"] = expiration;
            Associations.Rows.Add(dr);
            Associations.AcceptChanges();
        }
        private DataRow FindAssocByHandle(string handle)
        {
            DataRow[] result = Associations.Select("handle = '" + handle + "'");
            if (result.Length > 0)
            {
                return result[0];
            }
            return null;
        }
        private DataRow FindAssocByServer(string server)
        {
            DataRow[] result = Associations.Select("server = '" + server + "'");
            if (result.Length > 0)
            {
                return result[0];
            }
            return null;
        }
        private void CleanupAssociations()
        {
            if (NextCleanup == null || NextCleanup > DateTime.Now)
            {
                foreach (DataRow dr in Associations.Rows)
                {
                    if ((DateTime)dr["expiration"] > DateTime.Now)
                    {
                        dr.Delete();
                    }
                }
                Associations.AcceptChanges();
                NextCleanup = DateTime.Now.AddMinutes(10);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Begins performing a standard OpenID authentiction request.
        /// </summary>
        public override void BeginAuth()
        {
            HttpContext.Current.Trace.Write("Beginning standard authentication check");
            // Check to ensure Identity is set, and the server name
            // can be retrieved.
            if (Identity == null)
            {
                ErrorStore.Store(Errors.NoIdSpecified);
                HttpContext.Current.Trace.Write("No OpenID specified, ending check");
                return;
            }

            OpenIDServer = GetOpenIDServer();

            if (OpenIDServer == null)
            {
                if (!IsError())
                {
                    ErrorStore.Store(Errors.NoServersFound);
                    HttpContext.Current.Trace.Write("No OpenID Server found.");
                }
                return;
            }

            HttpContext.Current.Trace.Write("OpenID Version Discovered: " + AuthVersion.ToString());

            // If a association handle request has turned out to be unsatisfactory, force dumb mode.
            if (DumbMode == true)
            {
                authmode = AuthorizationMode.Stateless;
                HttpContext.Current.Trace.Write("Dumb mode forced, switching to Stateless mode");
            }

            // Perform stateless (dumb) authentication
            if (authmode == AuthorizationMode.Stateless)
            {
                if (OpenIDServer != "")
                {
                    HttpContext.Current.Trace.Write("Redirecting using Stateless URL");
                    Redirect(RedirectURLStateless);
                }
            }

            // Perform stateful (smart) authentication
            else if (authmode == AuthorizationMode.Stateful)
            {
                if (OpenIDServer != "")
                {
                    if (ShareKey() == true)
                    {
                        HttpContext.Current.Trace.Write("Redirecting using Stateful URL");
                        Redirect(RedirectURLStateful);
                    }
                    else
                    {
                        DumbMode = true;
                        HttpContext.Current.Trace.Write("Stateful key exchange failed, forcing Dumb mode and re-running authentication");
                        BeginAuth();
                    }
                }
            }
        }
        /// <summary>
        /// Begins authentication process, but returns an Immediate-mode authentication URL for external use.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Immediate mode allows the Consumer to verify that the User Agent is already
        /// logged in the OpenID Provider.  By using a hidden iframe and some AJAX or Javascript,
        /// the data can be passed behind the scenes without the appearance of ever leaving the
        /// Consumer's website.
        /// </para>
        /// <para>
        /// First, call AuthImmediate(), which will perform discovery and return a fully
        /// populated URL that should be opened by the User Agent.  For web browsers, set
        /// the target of the iframe to the supplied URL.
        /// </para>
        /// <para>
        /// The iframe will be redirected to the OpenID Provider.  The OpenID Provider will
        /// immediately redirect the iframe back to the Consumer.  Once that has occurred, be sure to
        /// use the ValidateImmediate() method to verify whether or not the user is currently
        /// logged in at the provider.
        /// </para>
        /// </remarks>
        /// <returns>String containing the immediate-mode URL. Null if method cannot be used with current settings.</returns>
        public string AuthImmediate()
        {
            HttpContext.Current.Trace.Write("Beginning immediate authentication check");

            if (Identity == null)
            {
                HttpContext.Current.Trace.Write("No OpenID specified, ending check");
                ErrorStore.Store(Errors.NoIdSpecified);
                return null;
            }
            OpenIDServer = GetOpenIDServer();
            if (OpenIDServer == null)
            {
                ErrorStore.Store(Errors.NoServersFound);
                return null;
            }

            if (authmode == AuthorizationMode.Stateless)
            {
                ErrorStore.Store(Errors.NoStatelessImmediate);
                HttpContext.Current.Trace.Write(Errors.NoStatelessImmediate);
                return null;
            }
            else if (authmode == AuthorizationMode.Stateful)
            {
                if (ShareKey())
                {
                    HttpContext.Current.Trace.Write("Redirecting using immediate URL");
                    return RedirectURLImmediate;
                }
                else
                {
                    HttpContext.Current.Trace.Write("Stateful key exchange failed, aborting");
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Validates regular (non-immediate) OpenID authentication responses.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To determine if this method should be used, look at the value
        /// of the RequestedMode property, which detects the operational mode
        /// requested by the current HTTP request.  
        /// </para>
        /// <para>
        /// If RequestedMode is set to RequestedMode.IdResolution, the request
        /// is an authentication response from an OpenID Provider.
        /// </para>
        /// <para>
        /// Therefore, either Validate() or ValidateImmediate() should be used to verify
        /// the validity of the response.
        /// </para>
        /// </remarks>
        /// <returns>True if successfully authenticated, false if not.</returns>
        public override bool Validate()
        {
            if (DumbMode == true)
            {
                HttpContext.Current.Trace.Write("Dumb mode has been forced, switching to stateless authentication");
                authmode = AuthorizationMode.Stateless;
            }

            if (authmode == AuthorizationMode.Stateless)
            {
                HttpContext.Current.Trace.Write("Stateless mode enabled, beginning validation request with server");
                return ValidateWithServer(false);
            }
            if (authmode == AuthorizationMode.Stateful)
            {
                HttpContext.Current.Trace.Write("Stateful mode enabled, beginning validation check using shared key");
                bool success = ValidateWithSharedKey();
                if (success)
                {
                    return true;
                }
                else
                {
                    HttpContext.Current.Trace.Write("Validation failed, performing stateless validation check");
                    authmode = AuthorizationMode.Stateless;
                    return ValidateWithServer(true);
                }
            }
            HttpContext.Current.Trace.Write("Request refused, authentication failed");
            ErrorStore.Store(Errors.RequestRefused);
            return false;
        }
        /// <summary>
        /// Validate an immediate-mode response.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To determine if this method should be used, look at the value
        /// of the RequestedMode property, which detects the operational mode
        /// requested by the current HTTP request.  
        /// </para>
        /// <para>
        /// If RequestedMode is set to RequestedMode.IdResolution, the request
        /// is an authentication response from an OpenID Provider.
        /// </para>
        /// <para>
        /// Therefore, either Validate() or ValidateImmediate() should be used to verify
        /// the validity of the response.
        /// </para>
        /// </remarks>
        /// <returns>Returns either "true", "false", or a URL.  If "true", validation was successful.
        /// If "false", this method cannot be used with current settings.  If a URL, then the 
        /// validation has failed, and the User Agent should be redirected to the returned URL.</returns>
        public string ValidateImmediate()
        {
            HttpContext.Current.Trace.Write("Validating immediate mode response");
            if (authmode == AuthorizationMode.Stateless)
            {
                HttpContext.Current.Trace.Write(Errors.NoStatelessImmediate);
                ErrorStore.Store(Errors.NoStatelessImmediate);
                return "false";
            }
            if (HttpContext.Current.Request["openid.user_setup_url"] != null)
            {
                HttpContext.Current.Trace.Write("User action required, returning user_setup_url = " + HttpContext.Current.Request["openid.user_setup_url"]);
                return HttpContext.Current.Request["openid.user_setup_url"];
            }

            HttpContext.Current.Trace.Write("No action required, validating with shared key");
            return ValidateWithSharedKey().ToString();
        }

        #endregion

    }
}
