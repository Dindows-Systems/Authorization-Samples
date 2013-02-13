using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// Defines the interface that token senders must implement.
	/// </summary>
	public interface ITokenSender : ISettingsContainer
	{

		/// <summary>
		/// Sends the given token to the current user.
		/// </summary>
		/// <param name="address">The address to send the token to.</param>
		/// <param name="site">The site from where the token is sent.</param>
		/// <param name="token">The token to send to the current user.</param>
		/// <param name="loginUrl">
		/// A URL that can be embedded as a link in the message that is sent
		/// to a user and that would allow users to log in by clicking the link.
		/// </param>
		void SendToken(string address, string site, string token, Uri loginUrl);

	}
}
