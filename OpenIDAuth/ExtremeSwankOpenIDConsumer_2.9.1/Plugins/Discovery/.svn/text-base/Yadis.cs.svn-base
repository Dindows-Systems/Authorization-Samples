using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtremeSwank.Authentication.OpenID.Plugins.Discovery
{
    /// <summary>
    /// Yadis Discovery Plugin.
    /// Provides everything needed to perform Yadis discovery.
    /// Depends on XRDS plugin to decode resulting XRDS document.
    /// </summary>
    [Serializable]
    public class Yadis : IDiscovery
    {
        string _Name = "Yadis Discovery Plugin";
        private ConsumerBase _Parent;

        /// <summary>
        /// Human-readable name of plugin.
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }
        }
        /// <summary>
        /// Parent OpenID object.
        /// </summary>
        public ConsumerBase Parent
        {
            get { return _Parent; }
            set { _Parent = value; }
        }
        ProtocolVersion _PV;
        /// <summary>
        /// Not used.  Always returns null.
        /// </summary>
        /// <param name="openid">Claimed identifier.</param>
        /// <returns>Null</returns>
        public string[] ProcessID(string openid)
        {
            return null;
        }
        /// <summary>
        /// Perform Yadis discovery on a HTTP response.
        /// Checks both HTTP headers and response body.
        /// </summary>
        /// <param name="content">HTTP response</param>
        /// <returns>String array containing two string arrays, first list is the discovered Identity Providers,
        /// second list is the discovered local IDs (delegates).</returns>
        public string[][] Discover(string content)
        {
            string xrds_url = GetURL(content);
            if (xrds_url != "")
            {
                XRDS x = new XRDS(Parent);
                string xrdsdoc = Utility.MakeRequest(xrds_url);
                if (xrdsdoc != null)
                {
                    string[][] ret = x.Discover(xrdsdoc);
                    _PV = x.Version;
                    return ret;
                }
                return null;
            }
            return null;
        }
        private string GetURL(string content)
        {
            string[] arrcon = content.Split('\n');
            int i = 0;
            while (i < arrcon.Length)
            {
                if (arrcon[i].StartsWith("X-XRDS-Location:"))
                {
                    char[] delimiter = { ':' };
                    string[] keyval = arrcon[i].Split(delimiter, 2);
                    string newurl = keyval[1].Trim();
                    return newurl;
                }
                i++;
            }

            string newcontent = Utility.RemoveHtmlComments(content);

            Regex xrds1 = new Regex("<meta[^>]*http-equiv=\"X-XRDS-Location\"[^>]*content=\"([^\"]+)\"[^>]*");
            Regex xrds2 = new Regex("<meta[^>]*content=\"([^\"]+)\"[^>]*http-equiv=\"X-XRDS-Location\"[^>]*");
            Match matches1 = xrds1.Match(newcontent);
            Match matches2 = xrds2.Match(newcontent);

            string url = "";

            if (matches1.Groups[1].Value != "") { url = matches1.Groups[1].Value; }
            else if (matches2.Groups[1].Value != "") { url = matches2.Groups[1].Value; }

            if (url != "")
            {
                return url;
            }
            return "";
        }
        /// <summary>
        /// Highest version of OpenID protocol supported by the discovered Identity Provider.
        /// </summary>
        public ProtocolVersion Version
        {
            get
            {
                return _PV;
            }
        }
        /// <summary>
        /// Creates a new Yadis object, automatically attaches to supplied
        /// OpenID object.
        /// </summary>
        /// <param name="oid"></param>
        public Yadis(ConsumerBase oid)
        {
            Parent = oid;
            oid.PluginsDiscovery.Add(this);
        }
    }
}
