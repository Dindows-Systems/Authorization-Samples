using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TokenAuthentication.Web
{
	public abstract class SettingsContainer : ISettingsContainer
	{
		private Dictionary<string, string> _Settings;
		public IDictionary<string, string> Settings
		{
			get
			{
				if (null == _Settings)
					_Settings = new Dictionary<string, string>();

				return _Settings;
			}
		}
	}
}
