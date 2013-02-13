using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// The default token generator type. This type is used by default if no
	/// token generator has been configured.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This token generator recognizes the following settings that can be configured
	/// for the generator in the configuration file.
	/// </para>
	/// <list type="bullet">
	/// <item>
	/// <term>Chars</term>
	/// <description>A string that contains all the characters that can be included in the token. If not specified, a default set of characters will be used.</description>
	/// </item>
	/// <item>
	/// <term>MinTokenLength</term>
	/// <description>The minimum length of the token. Defaults to 8.</description>
	/// </item>
	/// <item>
	/// <term>MaxTokenLength</term>
	/// <description>The maximum length of the token. Defaults to 8.</description>
	/// </item>
	/// </list>
	/// </remarks>
	public sealed class DefaultTokenGenerator : SettingsContainer, ITokenGenerator
	{

		private string Chars
		{
			get
			{
				if (this.Settings.ContainsKey("Chars"))
				{
					return this.Settings["Chars"];
				}
				return "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			}
		}

		private int MaxTokenLength
		{
			get
			{
				if (this.Settings.ContainsKey("MaxTokenLength"))
				{
					int i = 0;
					if (int.TryParse(this.Settings["MaxTokenLength"], out i))
					{
						return i;
					}
				}
				return 8;
			}
		}

		private int MinTokenLength
		{
			get
			{
				if (this.Settings.ContainsKey("MinTokenLength"))
				{
					int i = 0;
					if (int.TryParse(this.Settings["MinTokenLength"], out i))
					{
						return i;
					}
				}
				return 8;
			}
		}



		string ITokenGenerator.GenerateToken()
		{
			Random rnd = new Random();
			int length = rnd.Next(this.MinTokenLength, this.MaxTokenLength);
			string chars = this.Chars.Duplicate(length);
			
			// Randomize the order of the chars and take the first chars from
			// the randomized string. The number of chars to take is specified
			// in the length variable.
			var s1 = new string(chars.ToCharArray().OrderBy(c => rnd.Next()).Take(length).ToArray());
			return s1;
		}

	}
}
