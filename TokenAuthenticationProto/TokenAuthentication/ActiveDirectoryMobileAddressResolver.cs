using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// An address resolver that returns the mobile phone number for
	/// the currently logged on Windows account.
	/// </summary>
	public sealed class ActiveDirectoryMobileAddressResolver : SettingsContainer, IAddressResolver
	{
		string IAddressResolver.ResolveAddress()
		{
			UserPrincipal user = UserPrincipal.Current;
			return user.GetMobile();
		}
	}
}
