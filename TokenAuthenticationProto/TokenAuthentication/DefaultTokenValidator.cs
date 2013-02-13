using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// The default token validator used if no other is configured.
	/// </summary>
	public sealed class DefaultTokenValidator : SettingsContainer , ITokenValidator
	{
		/// <summary>
		/// Validates the given token against the pending token.
		/// </summary>
		/// <param name="token">The token to validate.</param>
		/// <returns>Returns <c>true</c> if the given token matches the pending token.</returns>
		bool ITokenValidator.ValidateToken(string token)
		{
			var pendingToken = AuthenticationManager.GetPendingToken();
			HttpContext.Current.Trace.Write(string.Format("Validating token. Given Token: {0}.", token));

			return string.Equals(token, pendingToken, StringComparison.Ordinal);
		}
	}
}
