using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ExtremeSwank.Authentication.OpenID.Plugins.Discovery
{
    /// <summary>
    /// XRDS Discovery Plugin.  Provides everything needed to
    /// discover OpenIDs using XRDS documents.
    /// </summary>
    [Serializable]
    public class XRDS : IDiscovery
    {
        string _Name = "XRDS Discovery Plugin";
        ProtocolVersion _PV;
        private ConsumerBase _Parent;

        /// <summary>
        /// Name of plugin.
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

        private string[] Prefixes = { "=", "@", "+", "$", "!", "xri://" };
        private string[] IgnorePrefixes = { "xri://$dns*", "xri://$ip*" };
        /// <summary>
        /// Accepts a claimed identifier and returns
        /// the normalized identifier, and an end-point URL.
        /// </summary>
        /// <param name="openid">String containing claimed identifier.</param>
        /// <returns>Returns a string array with two fields, first field contains a normalized version
        /// of the claimed identifier, and an end-point URL.  Returns null if this plugin does not support
        /// the identifier type.</returns>
        public string[] ProcessID(string openid)
        {
            string tid = openid;
            string url_id = "";

            foreach (string prefix in Prefixes)
            {
                if (tid.StartsWith(prefix))
                {
                    if (prefix.Length == 1)
                    {
                        url_id = "http://xri.net/" + tid;
                    }
                    else
                    {
                        string a = tid;
                        bool ignore = false;
                        foreach (string ip in IgnorePrefixes)
                        {
                            if (a.StartsWith(ip))
                            {
                                ignore = true;
                            }
                        }
                        if (!ignore)
                        {
                            a = a.Substring(prefix.Length, a.Length - prefix.Length);
                            tid = a;
                            url_id = "http://xri.net/" + tid;
                        }
                    }
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
        /// Consumes the contents of the XRDS document. Locates all OpenID-related data.
        /// </summary>
        /// <param name="content">HTTP response output from request.</param>
        /// <returns>String array containing to two string arrays, first contains list of server URLs,
        /// second contains list of delegates (local identifiers).</returns>
        public string[][] Discover(string content)
        {
            if (!content.Contains("<?xml"))
            {
                return null;
            }
            int xmlbegin = content.IndexOf("<?xml");
            string fixedcontent = content.Substring(xmlbegin);

            XmlDocument xd = new XmlDocument();
            try
            {
                xd.LoadXml(fixedcontent);
            }
            catch
            {
                ErrorStore.Store(Errors.XmlDecodeFailed);
                return null;
            }

            foreach (IExtension ext in Parent.PluginsExtension)
            {
                if (ext is IXRDSConsumer)
                {
                    IXRDSConsumer ixc = (IXRDSConsumer)ext;
                    ixc.ProcessXRDS(xd);
                }
            }

            XmlNamespaceManager nsmanager = new XmlNamespaceManager(xd.NameTable);
            nsmanager.AddNamespace("openid", "http://openid.net/xmlns/1.0");
            nsmanager.AddNamespace("xrds", "xri://$xrds");
            nsmanager.AddNamespace("xrd", "xri://$xrd*($v*2.0)");

            XmlNode rootnode = xd.DocumentElement;

            List<string> servers = new List<string>();
            List<string> delegates = new List<string>();
            List<XmlNode> openidNodes = new List<XmlNode>();
            List<string> supportedVersions = new List<string>();

            XmlNodeList xmlServices = rootnode.SelectNodes("/xrds:XRDS/xrd:XRD/xrd:Service", nsmanager);
            foreach (XmlNode node in xmlServices)
            {
                foreach (XmlNode servicenode in node.ChildNodes)
                {
                    string val = "";
                    if (servicenode.HasChildNodes)
                    {
                        val = servicenode.ChildNodes[0].Value;
                    }
                    if (servicenode.Name == "Type")
                    {
                        if (val.Contains("http://specs.openid.net/auth/2."))
                        {
                            openidNodes.Add(node);
                            supportedVersions.Add("2.0");
                        }
                        else if (val.Contains("http://openid.net/signon/1."))
                        {
                            openidNodes.Add(node);
                            supportedVersions.Add("1.x");
                        }
                    }
                }
            }

            foreach (XmlNode oidn in openidNodes)
            {
                foreach (XmlNode onode in oidn.ChildNodes)
                {
                    string val = "";
                    if (onode.HasChildNodes)
                    {
                        val = onode.ChildNodes[0].Value;
                    }
                    switch (onode.Name)
                    {
                        case "URI":
                            servers.Add(val);
                            break;
                        case "openid:Delegate":
                            delegates.Add(val);
                            break;
                        case "LocalID":
                            delegates.Add(val);
                            break;
                    }
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
        /// Returns the newest OpenID protocol version supported
        /// by the Identity Provider.
        /// </summary>
        public ProtocolVersion Version
        {
            get { return _PV; }
        }
        /// <summary>
        /// Creates is XRDS object, automatically attaches
        /// to specified OpenID object.
        /// </summary>
        /// <param name="oid">Parent OpenID object</param>
        public XRDS(ConsumerBase oid)
        {
            Parent = oid;
            oid.PluginsDiscovery.Add(this);
        }
    }
}
