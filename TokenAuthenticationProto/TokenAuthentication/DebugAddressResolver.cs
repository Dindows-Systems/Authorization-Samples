using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// An address resolver that can be used for debugging.
	/// </summary>
	/// <remarks>
	/// This address resolver supports one setting: <c>Address</c> - This setting contains
	/// the address returned by the address resolver.
	/// </remarks>
	public sealed class DebugAddressResolver : SettingsContainer , IAddressResolver
	{

		string IAddressResolver.ResolveAddress()
		{
			if (this.Settings.ContainsKey("Address"))
				return this.Settings["Address"];

			return null;
		}

	}
}
