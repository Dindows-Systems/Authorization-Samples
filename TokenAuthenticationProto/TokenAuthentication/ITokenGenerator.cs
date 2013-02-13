using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// Defines the interface that a token generator must implement.
	/// </summary>
	public interface ITokenGenerator : ISettingsContainer
	{
		/// <summary>
		/// Generates a token.
		/// </summary>
		string GenerateToken();
	}
}
