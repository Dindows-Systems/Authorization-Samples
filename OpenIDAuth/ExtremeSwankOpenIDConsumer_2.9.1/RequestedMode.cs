using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// OpenID modes that can be remotely requested.
    /// </summary>
    public enum RequestedMode
    {
        /// <summary>
        /// No OpenID mode was requested.
        /// </summary>
        None,
        /// <summary>
        /// ID Resolution mode was requested.
        /// </summary>
        IdResolution,
        /// <summary>
        /// Operation was cancelled by user.
        /// </summary>
        CancelledByUser,
    }
}
