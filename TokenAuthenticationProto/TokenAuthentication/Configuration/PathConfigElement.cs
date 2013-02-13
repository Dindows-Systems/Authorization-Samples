using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace TokenAuthentication.Web.Configuration
{
	/// <summary>
	/// A configuration element that allows you to configure a path.
	/// </summary>
	public sealed class PathConfigElement : ConfigurationElement
	{
		/// <summary>
		/// Specifies the configured path.
		/// </summary>
		/// <remarks>
		/// A path always specifies the beginning of a path that is targeted.
		/// For instance, the path <c>/secure/</c> specifies that any resource
		/// whose relative path starts with <c>/secure/</c> is targeted. To specify
		/// single files, specify the path as <c>/folder/file.aspx</c>, for
		/// example.
		/// </remarks>
		[ConfigurationProperty("path", IsKey = true, IsRequired = true)]
		public string Path
		{
			get { return (string)base["path"]; }
		}
	}
}
