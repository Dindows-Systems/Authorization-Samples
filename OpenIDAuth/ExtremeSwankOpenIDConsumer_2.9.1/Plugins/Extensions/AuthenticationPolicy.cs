using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace ExtremeSwank.Authentication.OpenID.Plugins.Extensions
{
    /// <summary>
    /// Provides support for the OpenID Authentication Policy Extension.
    /// </summary>
    public class AuthenticationPolicy : IExtension
    {
        ConsumerBase _Parent;

        /// <summary>
        /// Creates an instance of AuthenticationPolicy extension.
        /// </summary>
        /// <param name="openid">The parent OpenIDConsumer object.</param>
        public AuthenticationPolicy(ConsumerBase openid)
        {
            _Parent = openid;
            _Parent.PluginsExtension.Add(this);
            PreferredPolicies = new List<string>();
        }

        private long _MaxAge;

        /// <summary>
        /// The longest period of time that can pass since the user was
        /// last authenticated by the Identity Provider.
        /// </summary>
        public long MaxAge
        {
            get { return _MaxAge; }
            set { _MaxAge = value; }
        }

        private List<string> _PreferredPolicies;

        /// <summary>
        /// A List of preferred policy URIs that are requested for this authentication
        /// request.  Use the AuthenticationURI static methods.
        /// </summary>
        public List<string> PreferredPolicies
        {
            get { return _PreferredPolicies; }
            set { _PreferredPolicies = value; }
        }

        #region IExtension Members

        /// <summary>
        /// The human-readable name of this extension.
        /// </summary>
        public string Name
        {
            get { return "Authentication Policy Extension"; }
        }

        /// <summary>
        /// The OpenIDConsumer object that is parent to this extension.
        /// </summary>
        public ConsumerBase Parent
        {
            get { return _Parent; }
        }

        /// <summary>
        /// The namespace URI of this extension.
        /// </summary>
        public string Namespace
        {
            get { return "http://specs.openid.net/extensions/pape/1.0"; }
        }

        /// <summary>
        /// Name-Value data to be sent to Identity Provider during
        /// initial authentication request.
        /// </summary>
        public Dictionary<string, string> AuthorizationData
        {
            get
            {
                Dictionary<string, string> pms = new Dictionary<string, string>();
                pms["openid.ns.pape"] = Namespace;
                pms["openid.pape.max_auth_age"] = MaxAge.ToString();
                pms["openid.pape.preferred_auth_policies"] = String.Join(" ", PreferredPolicies.ToArray());
                return pms;
            }
        }

        /// <summary>
        /// Whether or not the validation completed per this extension.
        /// </summary>
        /// <returns>Always returns true.</returns>
        public bool Validation()
        {
            return true;
        }

        /// <summary>
        /// Returns data for use by OpenIDUser object.
        /// </summary>
        public Dictionary<string, string> ObjectUserData
        {
            get
            {
                Dictionary<string, string> ReturnData = new Dictionary<string, string>();
                NameValueCollection Request = HttpContext.Current.Request.QueryString;
                foreach (string key in Request.Keys)
                {
                    if (key.Contains("openid.pape."))
                    {
                        if (Request[key] != null)
                        {
                            ReturnData[key] = Request[key];
                        }
                    }
                }
                return ReturnData;
            }
        }

        #endregion
    }
}
