using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// Provides session-specific persistence, uses Session object.
    /// </summary>
    [Serializable]
    public class SessionPersister
    {
        private const string prefix = "OpenIDConsumer_";
        /// <summary>
        /// Indexer, gets or sets objects into session persistence by key.
        /// </summary>
        /// <param name="key">String representing key index</param>
        /// <returns>Object stored in session persistence. If null, then object is not present.</returns>
        public object this[string key]
        {
            get
            {
                return HttpContext.Current.Session[prefix + key];
            }
            set
            {
                HttpContext.Current.Session[prefix + key] = value;
            }
        }
        /// <summary>
        /// Returns a string array of keys representing session
        /// persisted objects.
        /// </summary>
        public static string[] SavedKeys
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (string key in HttpContext.Current.Session.Keys)
                {
                    if (key.StartsWith(prefix)) { ret.Add(key); }
                }
                return ret.ToArray();
            }
        }

        /// <summary>
        /// Removes all objects from Session persistence that were
        /// inserted using the SessionPersister object.
        /// </summary>
        public static void Cleanup()
        {
            foreach (string key in SavedKeys)
            {
                HttpContext.Current.Session[key] = null;
            }
        }
    }
}
