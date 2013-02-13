using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// Static text for frequently occurring errors.
    /// </summary>
    public class Errors
    {
        /// <summary>
        /// No servers were found during the discovery process.
        /// </summary>
        public const string NoServersFound = "Unable to locate Identity Provider.";
        /// <summary>
        /// The signature received back from the IdP didn't decode to the
        /// expected value.
        /// </summary>
        public const string BadSignature = "Signature received from Identity Provider was invalid. Please try again.";
        /// <summary>
        /// An HTTP error occurred when attempting to contact the IdP.
        /// </summary>
        public const string HttpError = "Connection to Identity Provider failed.";
        /// <summary>
        /// Tried to create an association with DH-SHA1 or DH-SHA256 and received a secret
        /// in plaintext instead.
        /// </summary>
        public const string ReceivedPlaintext = "Requested and encrypted association, received plaintext secret.";
        /// <summary>
        /// The AuthorizationMode property has not been set.
        /// </summary>
        public const string AuthModeNotSet = "Authentication mode not set.";
        /// <summary>
        /// The window to complete the authentication request has expired.
        /// User should try the request again.
        /// </summary>
        public const string SessionTimeout = "Session timed out. Please try again.";
        /// <summary>
        /// Request has been actively refused by the IdP.
        /// </summary>
        public const string RequestRefused = "Identity Provider refused request.";
        /// <summary>
        /// The cached association handle has been invalidated.
        /// </summary>
        public const string HandleInvalidated = "Received association invalidation request from server.";
        /// <summary>
        /// The cached and received association handles do not match.
        /// </summary>
        public const string HandlesDoNotMatch = "Associated and received handles do not match.";
        /// <summary>
        /// Tried to look up the assocation record and failed.
        /// </summary>
        public const string AssociationNotFound = "Association not found in store.";
        /// <summary>
        /// The cached association record has expired.
        /// </summary>
        public const string AssociationExpired = "Association has expired.";
        /// <summary>
        /// No ID was specified.
        /// </summary>
        public const string NoIdSpecified = "Please specify an OpenID.";
        /// <summary>
        /// Attempted to perform an Immediate request while in Stateless mode.
        /// </summary>
        public const string NoStatelessImmediate = "Immediate-mode does not support Stateless authentication.";
        /// <summary>
        /// Failed to decode an XML document.
        /// </summary>
        public const string XmlDecodeFailed = "Unable to decode XML document.";
    }

}
