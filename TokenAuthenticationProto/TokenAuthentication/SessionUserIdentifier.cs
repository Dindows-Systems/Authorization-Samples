using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// A user identifier that returns the session id of the current session
	/// as user identifier.
	/// </summary>
	public sealed class SessionUserIdentifier : SettingsContainer , IUserIdentifier
	{

		string IUserIdentifier.GetUserIdentifier()
		{
			if (null == HttpContext.Current.Session)
			{
				throw new ApplicationException(string.Format("Session state is not enabled. Cannot use the user identifer '{0}'.", typeof(SessionUserIdentifier).FullName));
			}

			// The following line ensures that the session state is presisted
			// and that a new session ID is not generated for every request.
			HttpContext.Current.Session[typeof(SessionUserIdentifier).FullName] = "";

			return HttpContext.Current.Session.SessionID;
		}

	}
}
