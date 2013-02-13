using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// The default user identifier that is used if no other user
	/// identifier is configured in the application.
	/// </summary>
	/// <remarks>
	/// This user identifier returns identity name of the currently logged on user.
	/// </remarks>
	public sealed class DefaultUserIdentifier : SettingsContainer , IUserIdentifier
	{

		string IUserIdentifier.GetUserIdentifier()
		{
			return HttpContext.Current.User.Identity.Name;
		}

	}
}
