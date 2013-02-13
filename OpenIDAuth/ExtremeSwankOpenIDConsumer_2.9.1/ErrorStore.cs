using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// Posts an error.
    /// </summary>
    public class ErrorStore
    {
        static string _Error;
        /// <summary>
        /// String array containing last stored error.  First field is the description, second is the code.
        /// </summary>
        static public string Error
        {
            get
            {
                return _Error;
            }
        }
        /// <summary>
        /// Stores a single error, and holds into a memory cache for later retrieval.
        /// </summary>
        /// <param name="desc">Description of error. Typically pulled from the Errors class.</param>
        static public void Store(string desc)
        {
            _Error = desc;
        }
    }
}
