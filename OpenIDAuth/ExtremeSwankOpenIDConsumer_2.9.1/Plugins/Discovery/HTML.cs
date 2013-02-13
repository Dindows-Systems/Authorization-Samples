using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtremeSwank.Authentication.OpenID.Plugins.Discovery
{
    /// <summary>
    /// HTML Discovery Plugin.  Provides everything needed to
    /// discover OpenIDs using HTML documents.
    /// </summary>
    [Serializable]
    public class HTML : IDiscovery
    {
        string _Name = "HTML Discovery Plugin";
        private ConsumerBase _Parent;

        /// <summary>
        /// Human-readable plugin name.
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
        string[] _IndirectPrefixes = { "xri://$ip*", "xri://$dns*" };
        string[] _Prefixes = { "http://", "https://" };

        /// <summary>
        /// Processes a claimed identifier and returns a normalized ID and an endpoint URL.
        /// </summary>
        /// <param name="openid">Claimed identifier.</param>
        /// <returns>String-array containing a normalized ID and an endpoint URL.</returns>
        public string[] ProcessID(string openid)
        {
            string tid = openid;
            string url_id = "";

            foreach (string prefix in _IndirectPrefixes)
            {
                if (tid.StartsWith(prefix))
                {
                    string a = tid;
                    a = a.Substring(prefix.Length, a.Length - prefix.Length);
                    tid = a;
                    url_id = "http://" + tid;
                }
            }
            foreach (string prefix in _Prefixes)
            {
                if (tid.StartsWith(prefix))
                {
                    string a = tid;
                    a = a.Substring(prefix.Length, a.Length - prefix.Length);
                    tid = a;
                    url_id = prefix + tid;
                }
            }
            if (url_id == "")
            {
                return null;
            }
            string[] ret = { tid, url_id };
            return ret;
        }
        /// <summary>
        /// Parse HTTP response for OpenID Identity Providers.
        /// </summary>
        /// <param name="content">HTTP response content</param>
        /// <returns>String array containing two string arrays, first lists server endpoints, the second
        /// lists local identifiers (delegates).</returns>
        public string[][] Discover(string content)
        {
            List<string> servers = new List<string>();
            List<string> delegates = new List<string>();
            List<string> supportedVersions = new List<string>();

            string newcontent = Utility.RemoveHtmlComments(content);

            Regex rx_linktag1 = new Regex("<link[^>]*rel=\"([^\"]+)\"[^>]*href=\"([^\"]+)\"[^>]*");
            Regex rx_linktag2 = new Regex("<link[^>]*href=\"([^\"]+)\"[^>]*rel=\"([^\"]+)\"[^>]*");

            MatchCollection m1 = rx_linktag1.Matches(newcontent);
            MatchCollection m2 = rx_linktag2.Matches(newcontent);
            List<string[]> links = new List<string[]>();

            foreach (Match m in m1)
            {
                string[] l = { m.Groups[1].Value, m.Groups[2].Value };
                links.Add(l);
            }
            foreach (Match m in m2)
            {
                string[] l = { m.Groups[2].Value, m.Groups[1].Value };
                links.Add(l);
            }

            foreach (string[] link in links)
            {
                string relattr = link[0];
                string hrefattr = link[1];

                if (relattr.Contains("openid2.provider"))
                {
                    if (!servers.Contains(hrefattr)) { servers.Add(hrefattr); }
                    supportedVersions.Add("2.0");
                }
                else if (relattr.Contains("openid.server"))
                {
                    if (!servers.Contains(hrefattr)) { servers.Add(hrefattr); }
                    supportedVersions.Add("1.x");
                }
                else if (relattr.Contains("openid2.local_id"))
                {
                    if (!delegates.Contains(hrefattr)) { delegates.Add(hrefattr); }
                    supportedVersions.Add("2.0");
                }
                else if (relattr.Contains("openid.delegate"))
                {
                    if (!delegates.Contains(hrefattr)) { delegates.Add(hrefattr); }
                    supportedVersions.Add("1.x");
                }
            }

            if (supportedVersions.Contains("2.0"))
            {
                _PV = ProtocolVersion.V2_0;
            }
            else if (supportedVersions.Contains("1.x"))
            {
                _PV = ProtocolVersion.V1_1;
            }

            List<string[]> ret = new List<string[]>();
            if (servers.Count != 0)
            {
                ret.Add(servers.ToArray());
                ret.Add(delegates.ToArray());
                return ret.ToArray();
            }
            return null;
        }
        /// <summary>
        /// Highest version of OpenID protocol supported by discovered Identity Provider.
        /// </summary>
        public ProtocolVersion Version
        {
            get
            {
                return _PV;
            }
        }
        /// <summary>
        /// Creates a new HTML object and automatically attaches it to
        /// the supplied OpenID object.
        /// </summary>
        /// <param name="oid"></param>
        public HTML(ConsumerBase oid)
        {
            Parent = oid;
            oid.PluginsDiscovery.Add(this);
        }
    }
}
