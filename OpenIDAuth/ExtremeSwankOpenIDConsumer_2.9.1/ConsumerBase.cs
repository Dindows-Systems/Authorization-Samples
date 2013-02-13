using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// A common base class for both Full Trust and Partial Trust versions
    /// of the OpenID Consumer
    /// </summary>
    [Serializable]
    public abstract class ConsumerBase
    {
        /// <summary>
        /// The OpenID, in URL form
        /// </summary>
        protected string openid_url_identity;
        /// <summary>
        /// The OpenID, in normalized form
        /// </summary>
        protected string openid_identity;
        /// <summary>
        /// A dictionary containing all accessible URLs
        /// </summary>
        protected Dictionary<string, string> URLs;
        /// <summary>
        /// The session persistence object
        /// </summary>
        protected SessionPersister SessionPersister;
        /// <summary>
        /// The global persistence object
        /// </summary>
        protected GlobalPersister GlobalPersister;
        /// <summary>
        /// Gets or sets the URL of Identity Provider
        /// </summary>
        public string OpenIDServer
        {
            get { return URLs["openid_server"]; }
            set { URLs["openid_server"] = value; }
        }
        /// <summary>
        /// Gets or sets the URL that will serve as the base root of trust - defaults to current domain
        /// </summary>
        public string TrustRoot
        {
            get
            {
                if (URLs["trust_root"] == null) { URLs["trust_root"] = WebRoot + "/"; }
                return URLs["trust_root"];
            }
            set { URLs["trust_root"] = value; }
        }
        /// <summary>
        /// Gets or sets a URL to transfer user upon approval - defaults to current page
        /// </summary>
        public string ReturnURL
        {
            get { return URLs["approved"]; }
            set { URLs["approved"] = value; }
        }

        /// <summary>
        /// Gets or sets the OpenID idenitifer and normalizes the value
        /// </summary>
        public string Identity
        {
            get { return openid_identity; }
            set
            {
                if (value == null) { openid_identity = null; return; }
                if (value.ToString() == "") { openid_identity = null; return; }
                string[] norm = Normalize(value);
                openid_identity = norm[0];
                openid_url_identity = norm[1];
            }
        }
        /// <summary>
        /// List containing IExtension objects to use when performing authentication requests.
        /// </summary>
        public List<IExtension> PluginsExtension
        {
            get
            {
                if (SessionPersister["Extensions"] == null) { SessionPersister["Extensions"] = new List<IExtension>(); }
                return (List<IExtension>)SessionPersister["Extensions"];
            }
            set
            {
                SessionPersister["Extensions"] = value;
            }
        }
        /// <summary>
        /// List containing IDiscovery objects to use when performing name discovery.
        /// </summary>
        public List<IDiscovery> PluginsDiscovery
        {
            get
            {
                if (SessionPersister["DiscoveryPlugins"] == null) { SessionPersister["DiscoveryPlugins"] = new List<IDiscovery>(); }
                return (List<IDiscovery>)SessionPersister["DiscoveryPlugins"];
            }
            set
            {
                SessionPersister["DiscoveryPlugins"] = value;
            }
        }
        /// <summary>
        /// Converts a supplied OpenID into two distinct entities - a normalized name and a URI
        /// </summary>
        /// <param name="openid">OpenID to normalize</param>
        /// <returns>A string array with two fields - the normalized name (extremeswank.com), 
        /// and the URI (http://extremeswank.com).</returns>
        public string[] Normalize(string openid)
        {
            if (openid == null) { return null; }
            string tid = openid;
            string url_id = "";

            string[] result = null;

            // Loop through the registered Discovery Plugins
            // and attempt to get a processed ID.
            foreach (IDiscovery disc in PluginsDiscovery)
            {
                result = disc.ProcessID(openid);
                if (result != null) { break; }
            }

            // Ensure the plugins returned an ID
            if (result != null)
            {
                tid = result[0];
                url_id = result[1];
            }

            // If no URL was returned, ensure it
            // gets set to a default value.
            if (url_id == "")
            {
                url_id = "http://" + tid;
            }

            string[] ret = { tid, url_id };
            return ret;
        }
        /// <summary>
        /// Returns a string describing the current error state
        /// </summary>
        /// <returns></returns>
        public string GetError()
        {
            return ErrorStore.Error;
        }
        /// <summary>
        /// Gets a boolean value stating whether or not an error condition exists
        /// </summary>
        /// <returns></returns>
        public bool IsError()
        {
            if (ErrorStore.Error != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Persists the cnonce value in the current session so it can
        /// be verified when the authentication response is received.
        /// </summary>
        /// <remarks>
        /// This is extremely important to ensure that simply replaying
        /// the authentication response does not result in successful
        /// authentication.  AllowLogin is not populated until an authentication
        /// request has been triggered, and it is cleared as soon as the 
        /// matching request is received and verified.
        /// </remarks>
        protected int AllowLogin
        {
            get
            {
                if (SessionPersister["AllowLogin"] == null)
                {
                    return -1;
                }
                return (int)SessionPersister["AllowLogin"];
            }
            set { SessionPersister["AllowLogin"] = value; }
        }
        /// <summary>
        /// Return a URL representing the current host
        /// </summary>
        protected string WebRoot
        {
            get
            {
                HttpRequest Request = HttpContext.Current.Request;
                string root = "";
                if (Request.ServerVariables["SERVER_PORT_SECURE"] == "0")
                {
                    if (Request.ServerVariables["SERVER_PORT"] != "80")
                    {
                        root = "http://" + Request.ServerVariables["SERVER_NAME"] + ":" + Request.ServerVariables["SERVER_PORT"];
                    }
                    else
                    {
                        root = "http://" + Request.ServerVariables["SERVER_NAME"];
                    }
                }
                else
                {
                    if (Request.ServerVariables["SERVER_PORT"] != "443")
                    {
                        root = "https://" + Request.ServerVariables["SERVER_NAME"] + ":" + Request.ServerVariables["SERVER_PORT"];
                    }
                    else
                    {
                        root = "https://" + Request.ServerVariables["SERVER_NAME"];
                    }
                }
                return root;
            }
        }
        /// <summary>
        /// Remove spaces between words and comma delimiters
        /// </summary>
        /// <param name="s">String to process</param>
        /// <returns>The corrected string</returns>
        protected string RemoveSpaces(string s)
        {
            string[] t = s.Split(',');
            for (int i = 0; i < t.Length; i++)
            {
                t[i] = t[i].Trim();
            }
            string a = String.Join(",", t);
            return a;
        }
        /// <summary>
        /// Version of protocol currently being used.
        /// </summary>
        public ProtocolVersion AuthVersion
        {
            get
            {
                if (SessionPersister["AuthVersion"] == null)
                {
                    SessionPersister["AuthVersion"] = ProtocolVersion.V2_0;
                }
                return (ProtocolVersion)SessionPersister["AuthVersion"];
            }
            set
            {
                SessionPersister["AuthVersion"] = value;
            }
        }
        /// <summary>
        /// After successful validation, provides an object to hold the user information
        /// </summary>
        /// <returns>OpenIDUser object containing identifier and Extension data</returns>
        public OpenIDUser RetrieveUser()
        {
            OpenIDUser ret = new OpenIDUser(Identity);
            ret.Retrieve(this);
            return ret;
        }

        /// <summary>
        /// Checks the current page request and returns the requested
        /// mode.
        /// </summary>
        /// <returns>RequestedMode representing the current mode.</returns>
        public RequestedMode RequestedMode
        {
            get
            {
                switch (HttpContext.Current.Request.QueryString["openid.mode"])
                {
                    case "id_res":
                        return RequestedMode.IdResolution;
                    case "cancel":
                        return RequestedMode.CancelledByUser;
                }
                return RequestedMode.None;
            }
        }
        /// <summary>
        /// Retrieve the URL of the OpenID Server using configured discovery plugins
        /// </summary>
        /// <returns>The URL of the OpenID Server</returns>
        protected string GetOpenIDServer()
        {
            if (Identity == null) { return null; }
            TraceContext tc = System.Web.HttpContext.Current.Trace;

            tc.Warn("Creating HTTP Request.");

            string[][] systems = null;
            string response = Utility.MakeRequest(openid_url_identity);

            if (response == null) { return null; }

            tc.Write("HTTP Request successful.  Passing to discovery plugins.");

            for (int i = 0; i < PluginsDiscovery.Count; i++)
            {
                IDiscovery disc = (IDiscovery)PluginsDiscovery[i];
                tc.Write("Trying plugin " + disc.Name);
                systems = disc.Discover(response);
                if (systems != null)
                {
                    tc.Write("Plugin discovered endpoint.");
                    AuthVersion = disc.Version;
                    break;
                }
            }

            if (systems == null)
            {
                
                Trace.Write("Plugins did not discover endpoint.");
                return null;
            }
            string[] servers = systems[0];
            string[] delegates = systems[1];

            if (servers.Length == 0)
            {
                tc.Write("No servers found.");
                ErrorStore.Store(Errors.NoServersFound);
                return null;
            }
            if (delegates.Length > 0)
            {
                if (delegates[0].ToString() != "")
                {
                    tc.Write("Delegated OpenID.");
                    openid_url_identity = delegates[0].ToString();
                }
            }
            OpenIDServer = servers[0].ToString();
            tc.Write("Discovery successful - " + servers[0].ToString());
            return servers[0].ToString();
        }

        /// <summary>
        /// Independently performs discovery on the supplied OpenID and determines whether
        /// or not it is valid.
        /// </summary>
        /// <returns>True if discovery was successful, false if not.</returns>
        public bool IsValidID()
        {
            string id = GetOpenIDServer();
            if (String.IsNullOrEmpty(id)) { return false; }
            return true;
        }

        /// <summary>
        /// Get the redirect URL needed for Stateless authentication
        /// </summary>
        protected string RedirectURLStateless
        {
            get
            {
                Dictionary<string, string> pms = new Dictionary<string, string>();
                switch (AuthVersion)
                {
                    case ProtocolVersion.V1_1:
                        pms["openid.return_to"] = HttpUtility.UrlEncode(URLs["approved"]);
                        pms["openid.mode"] = "checkid_setup";
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
                        pms["openid.identity"] = HttpUtility.UrlEncode(openid_url_identity);
                        pms["openid.claimed_id"] = HttpUtility.UrlEncode(openid_identity);
                        pms["openid.realm"] = HttpUtility.UrlEncode(URLs["trust_root"]);
                        pms["openid.return_to"] = HttpUtility.UrlEncode(URLs["approved"]);

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
        /// <summary>
        /// Redirects the user to a URL.
        /// </summary>
        /// <param name="url">URL to redirect</param>
        protected void Redirect(string url)
        {
            HttpContext.Current.Response.Redirect(url, false);
        }
        /// <summary>
        /// Validate the assertion by contacting the OpenID Server
        /// </summary>
        /// <param name="fallback">Set to true if a previous stateful request has failed</param>
        /// <returns>True if validation is successful, false if not</returns>
        protected bool ValidateWithServer(bool fallback)
        {
            HttpContext.Current.Trace.Write("Beginning stateless validation");
            if (Identity == null) { return false; };
            Dictionary<string, string> pms = new Dictionary<string, string>();
            NameValueCollection Request = HttpContext.Current.Request.QueryString;
            pms["openid.assoc_handle"] = HttpUtility.UrlEncode(Request["openid.assoc_handle"]);
            pms["openid.signed"] = HttpUtility.UrlEncode(Request["openid.signed"]);
            pms["openid.sig"] = HttpUtility.UrlEncode(Request["openid.sig"]);
            if (fallback)
            {
                pms["openid.invalidate_handle"] = HttpUtility.UrlEncode(Request["openid.invalidate_handle"]);
            }

            // Send only required parameters to confirm validity
            string[] arr_signed = Request["openid.signed"].Split(',');
            for (int i = 0; i < arr_signed.Length; i++)
            {
                string s = arr_signed[i];
                string c = Request["openid." + arr_signed[i]];
                pms["openid." + s] = HttpUtility.UrlEncode(c);
            }
            pms["openid.mode"] = "check_authentication";
            string openid_server = GetOpenIDServer();
            if (openid_server == "")
            {
                HttpContext.Current.Trace.Write(Errors.NoServersFound);
                ErrorStore.Store(Errors.NoServersFound);
                return false;
            }

            // Connect to IdP
            string response = Utility.MakeRequest(openid_server, "GET", pms);

            if (response == null) 
            {
                HttpContext.Current.Trace.Write("No response from Identity Provider.");
                return false; 
            }

            // Parse reponse
            Dictionary<string, string> data = Utility.SplitResponse(response);

            // Check for validity of authentication request
            if (data.ContainsKey("is_valid") && data["is_valid"] == "true")
            {
                HttpContext.Current.Trace.Write("Server has validated authentication response.");
                return true;
            }
            else
            {
                HttpContext.Current.Trace.Write("Server has not validated authentication response.");
                ErrorStore.Store(Errors.RequestRefused);
                return false;
            }
        }

        /// <summary>
        /// Begin authentication request
        /// </summary>
        /// <remarks>
        /// Needs to be implemented by the class that inherits
        /// this class.  Should first check to ensure all required
        /// arguments are set, perform discovery, then redirect the
        /// User Agent as required.
        /// </remarks>
        public abstract void BeginAuth();
        /// <summary>
        /// Validate authentication response
        /// </summary>
        /// <returns>True if validation was successful, false if not.</returns>
        public abstract bool Validate();

        /// <summary>
        /// Shared initialization method - should be used by constructor.
        /// </summary>
        protected void Init() {
            HttpContext.Current.Trace.Write("Initializing OpenID Consumer");
            SessionPersister = new SessionPersister();
            GlobalPersister = new GlobalPersister();
            URLs = new Dictionary<string, string>();
            this.TrustRoot = WebRoot + "/";

            string queryString = HttpContext.Current.Request.ServerVariables["QUERY_STRING"];
            if (!string.IsNullOrEmpty(queryString))
                queryString = "?" + queryString;
            this.ReturnURL = WebRoot
                + HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"]
                + queryString;

            new Plugins.Discovery.XRDS(this);
            new Plugins.Discovery.Yadis(this);
            new Plugins.Discovery.HTML(this);
            HttpContext.Current.Trace.Write("Finished initialization");
        }
    }
}
