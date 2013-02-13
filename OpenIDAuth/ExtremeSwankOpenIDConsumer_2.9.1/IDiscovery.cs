using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// Interface used for Discovery plugins.
    /// Discovery plugins extend the OpenID consumer to support additional identifier discovery methods.
    /// </summary>
    public interface IDiscovery
    {
        /// <summary>
        /// Human-readable name of plugin.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Parent OpenIDConsumer object.
        /// </summary>
        ConsumerBase Parent { get; }
        /// <summary>
        /// Method called during discovery process.
        /// </summary>
        /// <param name="content">HTTP response output from request.</param>
        /// <returns>String array containing two string arrays, first contains list of server URLs,
        /// second contains list of delegates (local identifiers).</returns>
        string[][] Discover(string content);
        /// <summary>
        /// Method called prior to discovery process.  Accepts a claimed identifier and returns
        /// the normalized identifier, and an end-point URL.
        /// </summary>
        /// <param name="openid">String containing claimed identifier.</param>
        /// <returns>Returns a string array with two fields, first field contains a normalized version
        /// of the claimed identifier, and an end-point URL.  Returns null if this plugin does not support
        /// the identifier type.</returns>
        string[] ProcessID(string openid);
        /// <summary>
        /// Based on discovery, returns highest protocol version supported by endpoint. Used by consumer
        /// to determine which version of protocol to use when connecting to Identity Provider.
        /// </summary>
        ProtocolVersion Version { get; }
    }

}
