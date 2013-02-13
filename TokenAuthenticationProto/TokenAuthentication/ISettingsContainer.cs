using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// Defines the interface for types that contain settings.
	/// </summary>
	public interface ISettingsContainer
	{
		/// <summary>
		/// Returns a collection of settings.
		/// </summary>
		IDictionary<string, string> Settings { get; }
	}
}
