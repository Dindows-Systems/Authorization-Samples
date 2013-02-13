using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Web;

namespace TokenAuthentication.Web
{
	internal static class Extensions
	{

		public static string Duplicate(this string s, int count)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < count; i++)
				builder.Append(s);

			return builder.ToString();
		}

		public static string GetMobile(this UserPrincipal user)
		{
			try
			{
				return ((DirectoryEntry)user.GetUnderlyingObject()).InvokeGet("mobile") as string;
			}
			catch { }

			return null;
		}

		public static bool ImplementsInterface(this Type type, Type interfaceType)
		{
			if (null == interfaceType)
				throw new ArgumentNullException("interfaceType");

			if (!interfaceType.IsInterface)
				throw new ArgumentException("The argument must specify an interface.", "interfaceType");

			var intf = type.GetInterface(interfaceType.Name);
			return null != intf;
		}

		public static string UrlEncode(this string s)
		{
			StringBuilder builder = new StringBuilder();

			Action<char> charHandler = delegate(char c)
			{
				if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
				{
					builder.Append(c);
				}
				else
				{
					builder.Append(Uri.HexEscape(c));
				}
			};

			s.ToCharArray().ToList().ForEach(charHandler);

			return builder.ToString();
		}

	}
}
