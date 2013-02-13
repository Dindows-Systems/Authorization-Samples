using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace ExtremeSwank.Authentication.OpenID.Plugins.Extensions
{
    /// <summary>
    /// Provides support for the Simple Registration extension.
    /// </summary>
    [Serializable]
    public class SimpleRegistration : IExtension
    {
        private string _Name = "OpenID Simple Registration Extension 1.1";
        private string _Namespace = "http://openid.net/extensions/sreg/1.1";
        private string _Prefix = "openid.sreg.";
        private List<string> _OptionalFields;
        private List<string> _RequiredFields;
        private string _PolicyURL;
        private ConsumerBase _Parent;

        /// <summary>
        /// Gets the name of the extension.
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }
        }
        /// <summary>
        /// Gets the extension's registered namespace.
        /// </summary>
        public string Namespace
        {
            get { return _Namespace; }
        }
        /// <summary>
        /// Gets or sets the parent OpenID object.
        /// </summary>
        public ConsumerBase Parent
        {
            get { return _Parent; }
            set
            {
                _Parent = value;
                if (!_Parent.PluginsExtension.Contains(this)) { _Parent.PluginsExtension.Add(this); }
            }
        }

        /// <summary>
        /// Comma-delimited list of optional fields to retrieve from Identity Provider.
        /// Valid values are: nickname, email, fullname, dob, gender, postcode,
        /// country, language, timezone
        /// </summary>
        /// <example>
        /// <code>
        /// OpenIDConsumer openid;
        /// SimpleRegistration sr = new SimpleRegistration(openid);
        /// sr.OptionalFields = "nickname,fullname,postcode";
        /// </code>
        /// </example>
        public string OptionalFields
        {
            get
            {
                return String.Join(",", _OptionalFields.ToArray());
            }
            set 
            {
                _OptionalFields = new List<string>(value.Split(','));
            }
        }
        /// <summary>
        /// Comma-delimited list of required fields to retrieve from Identity Provider.
        /// Valid values are: nickname, email, fullname, dob, gender, postcode,
        /// country, language, timezone
        /// </summary>
        /// <example>
        /// <code>
        /// OpenIDConsumer openid;
        /// SimpleRegistration sr = new SimpleRegistration(openid);
        /// sr.RequiredFields = "nickname,fullname,postcode";
        /// </code>
        /// </example>
        public string RequiredFields
        {
            get
            {
                return String.Join(",", _RequiredFields.ToArray());
            }
            set 
            {
                _RequiredFields = new List<string>(value.Split(','));
            }
        }
        /// <summary>
        /// Privacy policy URL to send to Identity Provider.
        /// </summary>
        public string PolicyURL
        {
            get
            {
                return _PolicyURL;
            }
            set { _PolicyURL = value; }
        }

        /// <summary>
        /// Dictionary&lt;string, string&gt; containing key-value pairs that will be passed
        /// during initial authentication request to Identity Provider.
        /// </summary>
        public Dictionary<string, string> AuthorizationData
        {
            get
            {
                Dictionary<string, string> pms = new Dictionary<string, string>();
                int fieldcount = _OptionalFields.Count + _RequiredFields.Count;
                if (!String.IsNullOrEmpty(PolicyURL))
                {
                    fieldcount++;
                }
                if (fieldcount > 0)
                {
                    pms["openid.ns.sreg"] = _Namespace;
                    if (RequiredFields != null)
                    {
                        pms[_Prefix + "required"] = RequiredFields;
                    }
                    if (OptionalFields != null)
                    {
                        pms[_Prefix + "optional"] = OptionalFields;
                    }
                    pms[_Prefix + "policy_url"] = HttpUtility.UrlEncode(PolicyURL);
                }
                return pms;
            }
        }
        /// <summary>
        /// Performs extension-specific validation functions once authentication response has been received.
        /// </summary>
        /// <returns>Returns boolean value, true if validation is successful, false if not.</returns>
        public bool Validation()
        {
            return true;
        }
        /// <summary>
        /// Dictionary&lt;string, string&gt; containing data received from authentication response.
        /// </summary>
        public Dictionary<string, string> ObjectUserData
        {
            get
            {
                Dictionary<string, string> Registration = new Dictionary<string, string>();
                NameValueCollection Request = HttpContext.Current.Request.QueryString;
                foreach (string key in Request.Keys)
                {
                    if (key.Contains(_Prefix))
                    {
                        if (Request[key] != null)
                        {
                            Registration[key] = Request[key];
                        }
                    }
                }
                return Registration;
            }
        }

        /// <summary>
        /// Add optional fields using members of the Fields class.
        /// </summary>
        /// <example>
        /// <code>
        /// OpenIDConsumer openid;
        /// SimpleRegistration sr = new SimpleRegistration(openid);
        /// sr.AddOptionalFields(Fields.Nickname, Fields.Email, Fields.PostalCode);
        /// </code>
        /// </example>
        /// <param name="fields">A list of parameters from the Fields class.</param>
        public void AddOptionalFields(params string[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].Replace("openid.sreg.", "");
                if (!_OptionalFields.Contains(fields[i]))
                {
                    _OptionalFields.Add(fields[i]);
                }
            }
        }

        /// <summary>
        /// Add required fields using members of the Fields class.
        /// </summary>
        /// <example>
        /// <code>
        /// OpenIDConsumer openid;
        /// SimpleRegistration sr = new SimpleRegistration(openid);
        /// sr.AddRequiredFields(Fields.Nickname, Fields.Email, Fields.PostalCode);
        /// </code>
        /// </example>
        /// <param name="fields">A list of parameters from the Fields class.</param>
        public void AddRequiredFields(params string[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].Replace("openid.sreg.", "");
                if (!_RequiredFields.Contains(fields[i]))
                {
                    _RequiredFields.Add(fields[i]);
                }
            }
        }

        /// <summary>
        /// Creates a new SimpleRegistration plugin and attaches it to a OpenID object.
        /// </summary>
        /// <param name="oid">OpenID object to attach.</param>
        public SimpleRegistration(ConsumerBase oid)
        {
            _RequiredFields = new List<string>();
            _OptionalFields = new List<string>();
            Parent = oid;
            oid.PluginsExtension.Add(this);
        }
    }

}
