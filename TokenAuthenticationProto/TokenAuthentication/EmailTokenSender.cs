using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Resources;
using System.Reflection;
using System.Web;
using System.Globalization;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// A token sender that sends an authentication token by e-mail.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This token sender uses the default e-mail configuration information that
	/// is specified in the <c>system.net/mailSettings</c> configuration section
	/// when sending e-mails.
	/// </para>
	/// <para>
	/// This token sender uses the following settings that can be configured for
	/// the token sender.
	/// </para>
	/// <list>
	/// <item>
	/// <term>Subject</term>
	/// <description>The subject of the e-mail.</description>
	/// </item>
	/// <item>
	/// <term>Body</term>
	/// <description>
	/// The body of the e-mail to send. The body can contain three
	/// formatting place holders ({0}, {1} and {2}) that will be replaced
	/// with the token ({0}), the name of the site ({1}) that is
	/// sending the token and the login URL {2} that users can use to log in
	/// directly just by clicking the URL.
	/// </description>
	/// </item>
	/// <item>
	/// <term>GlobalResourceName</term>
	/// <description>
	/// The name of a global resource file that is used to get the subject and body from.
	/// If this setting is specified, 'Body' and 'Subject' settings are ignored. Instead,
	/// a file in the App_GlobalResources directory must exist with the given name. This
	/// resource file must contain the resources 'Subject' and 'Body' that contain
	/// the actual subject and body.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	public sealed class EmailTokenSender : SettingsContainer, ITokenSender
	{

		private string Body
		{
			get
			{
				if (null != this.GlobalResourceName)
				{
					return HttpContext.GetGlobalResourceObject(this.GlobalResourceName, "Body", CultureInfo.CurrentUICulture) as string;
				}

				if (this.Settings.ContainsKey("Body"))
				{
					return this.Settings["Body"];
				}

				return "The website at '{1}' has sent you an authentication token. If you have not accessed that site, you should discard this e-mail.\n\nThe token is: {0}\n\nUse this token to log in to the site.";
			}
		}

		private string GlobalResourceName
		{
			get
			{
				if (this.Settings.ContainsKey("GlobalResourceName"))
				{
					return this.Settings["GlobalResourceName"];
				}

				return null;
			}
		}

		private string Subject
		{
			get
			{
				if (null != this.GlobalResourceName)
				{
					return HttpContext.GetGlobalResourceObject(this.GlobalResourceName, "Subject", CultureInfo.CurrentUICulture) as string;
				}

				if (this.Settings.ContainsKey("Subject"))
				{
					return this.Settings["Subject"];
				}

				return "Authentication Token";
			}
		}

		void ITokenSender.SendToken(string address, string site, string token, Uri loginUrl)
		{
			
			using (MailMessage msg = new MailMessage())
			{
				msg.To.Add(address);
				msg.Subject = this.Subject;
				msg.IsBodyHtml = false;
				msg.Body = string.Format(CultureInfo.CurrentUICulture, this.Body, token, site, loginUrl);

				SmtpClient client = new SmtpClient();
				client.Send(msg);
			}
		}

	}
}
