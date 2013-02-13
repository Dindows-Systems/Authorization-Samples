using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// Represents the mode used for authentication.
    /// </summary>
    public enum AuthorizationMode
    {
        /// <summary>
        /// Represents Stateful (smart) authentication.
        /// </summary>
        /// <remarks>In Stateful authentication, OpenIDConsumer will first create a 
        /// cached shared secret with the Identity Provider. Then, the authentication
        /// request to the Identity Provider through the end-user's User Agent.
        /// Once the user has been authenticated at the Identity Provider, a response is
        /// sent once again through the User Agent. The Consumer will then verify the
        /// validity of the response using the cached pre-shared secret.
        /// Stateful mode requires less processing at the Identity Provider
        /// and gives faster response to the user.</remarks>
        Stateful,
        /// <summary>
        /// Represents Stateless (dumb) authentication.
        /// </summary>
        /// <remarks>In Stateless authentication, the authentication request is
        /// sent immediately to the Identity Provider through the end-user's User Agent.
        /// The Identity Provider will authenticate the user and will eventually
        /// respond with the requested information, also passing the data through the 
        /// User Agent. The Consumer will then communicate with the Identity Provider 
        /// directly to confirm the validity of the data.</remarks>
        Stateless
    };
}
