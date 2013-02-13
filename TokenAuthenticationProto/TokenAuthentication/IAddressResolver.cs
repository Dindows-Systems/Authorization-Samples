using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// Defines the interface that all address resolvers must implement.
	/// </summary>
	public interface IAddressResolver : ISettingsContainer
	{
		/// <summary>
		/// Returns the address for the current user.
		/// </summary>
		string ResolveAddress();
	}
}
