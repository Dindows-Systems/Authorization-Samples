using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// Interface used for Extension plugins that utilize XRDS data directly.
    /// </summary>
    public interface IXRDSConsumer
    {
        /// <summary>
        /// Parent consumer object.
        /// </summary>
        ConsumerBase Parent { get; }
        /// <summary>
        /// Process the XRDS data provided by the XRDS Discovery plugin.
        /// </summary>
        /// <param name="xrdsdoc">XmlDocument object containing XRDS document.</param>
        void ProcessXRDS(XmlDocument xrdsdoc);
    }
}
