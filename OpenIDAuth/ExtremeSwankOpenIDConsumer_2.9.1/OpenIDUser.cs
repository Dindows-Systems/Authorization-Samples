using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// Contains all information received about the authenticated user.
    /// </summary>
    public class OpenIDUser
    {
        private string _Identity;
        /// <summary>
        /// Gets or sets the claimed identifier.
        /// </summary>
        public string Identity
        {
            get { return _Identity; }
            set { _Identity = value; }
        }

        private string _BaseIdentity;
        /// <summary>
        /// Gets or sets the identifier validated by the Identity Provider.
        /// </summary>
        public string BaseIdentity
        {
            get { return _BaseIdentity; }
            set { _BaseIdentity = value; }
        }

        private Dictionary<string, string> _ExtensionData;

        /// <summary>
        /// Data returned by all loaded Extensions.
        /// </summary>
        public Dictionary<string, string> ExtensionData
        {
            get { return _ExtensionData; }
            set { _ExtensionData = value; }
        }

        /// <summary>
        /// Returns an empty OpenIDUser object.
        /// </summary>
        public OpenIDUser()
        {
            Init();
        }

        /// <summary>
        /// Retrieves extension data.
        /// </summary>
        /// <param name="key">Key of value to get</param>
        /// <returns>String containing value</returns>
        public string GetValue(string key)
        {
            if (ExtensionData.ContainsKey(key))
            {
                return ExtensionData[key];
            }
            return null;
        }

        private void Init()
        {
            _ExtensionData = new Dictionary<string, string>();
        }

        /// <summary>
        /// Returns a new OpenIDUser object with a pre-set claimed identity.
        /// </summary>
        /// <param name="identity">String containing the claimed identifier.</param>
        public OpenIDUser(string identity)
        {
            Init();
            Identity = identity;
        }

        /// <summary>
        /// Fill the object with information from the current response.
        /// </summary>
        /// <param name="openid">OpenIDConsumer object from which to retrieve the data.</param>
        public void Retrieve(ConsumerBase openid)
        {
            NameValueCollection Request = HttpContext.Current.Request.QueryString;
            if (Request["openid.identity"] != null) { BaseIdentity = Request["openid.identity"]; }

            foreach (IExtension e in openid.PluginsExtension)
            {
                foreach (string key in e.ObjectUserData.Keys)
                {
                    _ExtensionData[key] = e.ObjectUserData[key];
                }
            }
        }
    }

}
