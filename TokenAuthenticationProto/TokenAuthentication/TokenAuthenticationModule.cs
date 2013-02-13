using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.DirectoryServices.AccountManagement;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// A module that makes sure that the current user has authenticated also
	/// using a token.
	/// </summary>
	public sealed class TokenAuthenticationModule : IHttpModule
	{

		void IHttpModule.Dispose() { }

		void IHttpModule.Init(HttpApplication context)
		{
			context.AcquireRequestState += this.OnAcquireRequestState;
		}

		private void OnAcquireRequestState(object sender, EventArgs e)
		{
			AuthenticationManager.EnsureAuthentication();
		}

	}
}
