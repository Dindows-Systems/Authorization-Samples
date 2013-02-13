using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// An address resolver that returns the e-mail address of the currently
	/// logged on Windows account.
	/// </summary>
	public sealed class ActiveDirectoryEmailAddressResolver : SettingsContainer, IAddressResolver
	{
		string IAddressResolver.ResolveAddress()
		{
			return UserPrincipal.Current.EmailAddress;
		}
	}
}
