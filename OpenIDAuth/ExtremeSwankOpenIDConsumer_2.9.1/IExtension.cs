using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// Interface used for Extension plugins. 
    /// Extension plugins extend the OpenID consumer to support additional data-passing specifications.
    /// </summary>
    public interface IExtension
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
        /// Advertised namespace this plugins supports.
        /// </summary>
        string Namespace { get; }
        /// <summary>
        /// Data to be passed to Identity Provider during initial
        /// authenticaton request.
        /// </summary>
        Dictionary<string, string> AuthorizationData { get; }
        /// <summary>
        /// Checked by OpenIDConsumer object during Validation.
        /// </summary>
        /// <remarks>If the extension should not perform validation, always return true.</remarks>
        /// <returns>Boolean - returns true if validation is successful, false if not.</returns>
        bool Validation();
        /// <summary>
        /// Key-value formatted information returned from successful authentication request.
        /// Information is specific to this Extension's namespace.
        /// </summary>
        Dictionary<string, string> ObjectUserData { get; }
    }
}
