using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// Defines the interface that all user identifiers must implement.
	/// </summary>
	public interface IUserIdentifier : ISettingsContainer
	{
		/// <summary>
		/// Returns a string that represents the current user.
		/// </summary>
		string GetUserIdentifier();
	}
}
