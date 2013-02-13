using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ExtremeSwank.Authentication.OpenID
{

    /// <summary>
    /// Provides global application persistence, uses Application object.
    /// </summary>
    [Serializable]
    public class GlobalPersister
    {
        private const string prefix = "OpenIDConsumer_";
        /// <summary>
        /// Indexer, gets or sets objects into global persistence by key.
        /// </summary>
        /// <param name="key">String representing key index</param>
        /// <returns>Object stored in global persistence. If null, then object is not present.</returns>
        public object this[string key]
        {
            get
            {
                return HttpContext.Current.Application[prefix + key];
            }
            set
            {
                HttpContext.Current.Application[prefix + key] = value;
            }
        }

        /// <summary>
        /// Returns a string array of keys representing globally
        /// persisted objects.
        /// </summary>
        public static string[] SavedKeys
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (string key in HttpContext.Current.Application.Keys)
                {
                    if (key.StartsWith(prefix)) { ret.Add(key); }
                }
                return ret.ToArray();
            }
        }

        /// <summary>
        /// Cleans out any objects persisted Globally using the
        /// GlobalPersister object.
        /// </summary>
        public static void Cleanup()
        {
            foreach (string key in SavedKeys)
            {
                HttpContext.Current.Application[key] = null;
            }
        }
    }
}
