using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// Defines the interface that all token validators must implement.
	/// </summary>
	/// <remarks>
	/// <para>
	/// By creating your custom token validator and configuring your web application
	/// to use that instead of the default token validator, you could for instance
	/// create authentication schemes where your users have been sent unique lists
	/// of one-time passwords, and they would use those passwords as tokens.
	/// </para>
	/// <para>
	/// 
	/// </para>
	/// </remarks>
	public interface ITokenValidator : ISettingsContainer
	{
		/// <summary>
		/// Validates the given token for the current user.
		/// </summary>
		/// <param name="token">The token to validate.</param>
		/// <returns>Returns <c>true</c> if the token is valid. <c>False</c> if the token is invalid.</returns>
		bool ValidateToken(string token);
	}
}
