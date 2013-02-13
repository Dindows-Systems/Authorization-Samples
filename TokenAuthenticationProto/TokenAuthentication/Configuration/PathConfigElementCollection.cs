using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace TokenAuthentication.Web.Configuration
{
	/// <summary>
	/// A collection of <see cref="PathConfigElement"/> items.
	/// </summary>
	public sealed class PathConfigElementCollection : ConfigurationElementCollection
	{

		private IEnumerable<string> Paths
		{
			get
			{
				foreach (PathConfigElement item in this)
				{
					yield return item.Path;
				}
			}
		}



		internal bool ContainsUrl(Uri url)
		{
			if (null == url)
			{
				return false;
			}

			string s = url.AbsolutePath;
			if (null != this.Paths.FirstOrDefault(p=>s.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}
			return false;
		}



		protected override ConfigurationElement CreateNewElement()
		{
			return new PathConfigElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((PathConfigElement)element).Path;
		}

	}
}
