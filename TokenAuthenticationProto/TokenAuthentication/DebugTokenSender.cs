using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// A token sender that can be used for debugging. It stores sent tokens in
	/// text files in a specific directory.
	/// </summary>
	/// <remarks>
	/// This token sender supports one setting: <c>Path</c> - This setting specifies
	/// the full path to the directory where to store the text files containing
	/// the tokens.
	/// </remarks>
	public sealed class DebugTokenSender : SettingsContainer , ITokenSender
	{

		void ITokenSender.SendToken(string address, string site, string token, Uri loginUrl)
		{
			string path = null;
			if (this.Settings.ContainsKey("Path"))
			{
				path = this.Settings["Path"];

				using (FileStream strm = new FileStream(string.Format("{0}\\{1}.txt", path, Guid.NewGuid()), FileMode.CreateNew))
				{
					using (StreamWriter writer = new StreamWriter(strm))
					{
						writer.WriteLine("Address: {0}", address);
						writer.WriteLine("Site: {0}", site);
						writer.WriteLine("Token: {0}", token);
						writer.WriteLine("Login URL: {0}", loginUrl);
					}
				}
			}
		}

	}
}
