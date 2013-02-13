using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Globalization;

namespace TokenAuthentication.Web.Configuration
{
	/// <summary>
	/// The class that represents the token authentication configuration section
	/// in the configuration file for your web application.
	/// </summary>
	/// <example>
	/// <para>
	/// The following sample shows how to register the configuration section in
	/// your web application's configuration file, under the <c>configuration/configSections</c>
	/// configuration element.
	/// </para>
	/// <code>
	/// &lt;section name="tokenAuthentication" type="TokenAuthentication.Web.Configuration.TokenAuthenticationConfigSection, TokenAuthentication"/&gt;
	/// </code>
	/// </example>
	public sealed class TokenAuthenticationConfigSection : ConfigurationSection
	{

		/// <summary>
		/// Returns an instance of the current configuration section.
		/// </summary>
		public static TokenAuthenticationConfigSection Current
		{
			get
			{
				var webConfig = WebConfigurationManager.OpenWebConfiguration(string.Format(CultureInfo.InvariantCulture, "{0}", HttpContext.Current.Request.ApplicationPath));
				ConfigurationSection section = new List<object>(webConfig.Sections.Cast<object>()).FirstOrDefault(s => s.GetType() == typeof(TokenAuthenticationConfigSection)) as ConfigurationSection;
				if (null != section)
				{
					return (TokenAuthenticationConfigSection)ConfigurationManager.GetSection(section.SectionInformation.Name);
				}

				return null;
			}
		}



		/// <summary>
		/// Returns the address resolver configuration element. This allows you
		/// to customize how an address for a user is resolved.
		/// </summary>
		/// <remarks>
		/// An address resolver is optional, but if it is not configured, no address
		/// can be delivered to the token sender configured in <see cref="TokenSender"/>.
		/// </remarks>
		[ConfigurationProperty("addressResolver", IsRequired = false)]
		public TypeConfigElement AddressResolver
		{
			get { return (TypeConfigElement)base["addressResolver"]; }
		}

		/// <summary>
		/// Returns whether token authentication is enabled.
		/// </summary>
		[ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
		public bool Enabled
		{
			get { return (bool)base["enabled"]; }
		}

		/// <summary>
		/// Returns a collection of path configuration elements that specify
		/// paths that are excluded from token authentication.
		/// </summary>
		[ConfigurationProperty("excludePaths", IsRequired = false)]
		public PathConfigElementCollection ExcludePaths
		{
			get { return (PathConfigElementCollection)base["excludePaths"]; }
		}

		/// <summary>
		/// Returns a collection of path configuration elements that specify
		/// paths that are included in token authentication, i.e. the specified
		/// included paths are protected by token authentication.
		/// </summary>
		/// <remarks>
		/// A resource is protected by token authentication if its path is included
		/// in this collection and if its path is not included in the
		/// <see cref="ExcludedPaths"/> collection.
		/// </remarks>
		[ConfigurationProperty("includePaths", IsRequired = false)]
		public PathConfigElementCollection IncludePaths
		{
			get { return (PathConfigElementCollection)base["includePaths"]; }
		}

		/// <summary>
		/// Specifies the URL to the token login page.
		/// </summary>
		/// <remarks>
		/// The default value is <c>~/tokenlogin.aspx</c>
		/// </remarks>
		[ConfigurationProperty("loginUrl", IsRequired = false, DefaultValue = "~/tokenlogin.aspx")]
		public string LoginUrl
		{
			get { return (string)base["loginUrl"]; }
		}

		/// <summary>
		/// Returns the timeout for pending tokens, i.e. the time between
		/// a token is generated and sent to a user and when it is valid
		/// for authentication. After a pending token has expired, a new
		/// token must be sent to the user.
		/// </summary>
		/// <remarks>
		/// The default value is 5 minutes.
		/// </remarks>
		[ConfigurationProperty("pendingTimeout", IsRequired = false, DefaultValue = "00:05:00")]
		public TimeSpan PendingTimeout
		{
			get { return (TimeSpan)base["pendingTimeout"]; }
		}

		/// <summary>
		/// Specifies the authentication timeout, i.e. the allowed time between
		/// requests to resources protected by token authentication. After the
		/// authentication has expired, the user is directed to the token login
		/// page for reauthentication.
		/// </summary>
		/// <remarks>
		/// The default value is 20 minutes.
		/// </remarks>
		[ConfigurationProperty("timeout", IsRequired = false, DefaultValue = "00:20:00")]
		public TimeSpan Timeout
		{
			get { return (TimeSpan)base["timeout"]; }
		}

		/// <summary>
		/// Returns the token generator configuration element. This allows you to customize
		/// how a token is generated.
		/// </summary>
		/// <remarks>
		/// A token generator is optional. If no token generator is configured, a default token
		/// generator is used instead. The default token generator use is
		/// <see cref="DefaultTokenGenerator"/>.
		/// </remarks>
		[ConfigurationProperty("tokenGenerator", IsRequired = false)]
		public TypeConfigElement TokenGenerator
		{
			get
			{
				TypeConfigElement elem = (TypeConfigElement)base["tokenGenerator"];
				if (null == elem.Type)
				{
					elem = new TypeConfigElement(typeof(DefaultTokenGenerator));
				}

				return elem;
			}
		}

		/// <summary>
		/// Returns the token sender configuration element. This allows you to customize
		/// how a token is sent to a user.
		/// </summary>
		/// <remarks>
		/// A token sender is optional. If no token sender is configured a generated token is
		/// never sent to a user.
		/// </remarks>
		[ConfigurationProperty("tokenSender", IsRequired = false)]
		public TypeConfigElement TokenSender
		{
			get { return (TypeConfigElement)base["tokenSender"]; }
		}

		/// <summary>
		/// Returns the token validator configuration element. This allows you to customize
		/// how a token is validated.
		/// </summary>
		/// <remarks>
		/// A token validator is optional. If no token validator is configured, the default
		/// <see cref="DefaultTokenValidator"/> is used instead.
		/// </remarks>
		[ConfigurationProperty("tokenValidator", IsRequired = false)]
		public TypeConfigElement TokenValidator
		{
			get
			{
				TypeConfigElement elem = (TypeConfigElement)base["tokenValidator"];
				if (null == elem.Type)
				{
					elem = new TypeConfigElement(typeof(DefaultTokenValidator));
				}

				return elem;
			}
		}

		/// <summary>
		/// Returns the user identifier configuration element. This allows you to customize
		/// how users are identified, thus customizing the scope in which a token authentication
		/// is valid.
		/// </summary>
		/// <remarks>
		/// A user identifier is optional. If no user identifier is configured, the default
		/// <see cref="DefaultUserIdentifier"/> is used instead.
		/// </remarks>
		[ConfigurationProperty("userIdentifier", IsRequired = false)]
		public TypeConfigElement UserIdentifier
		{
			get
			{
				TypeConfigElement elem = (TypeConfigElement)base["userIdentifier"];
				if (null == elem.Type)
				{
					elem = new TypeConfigElement(typeof(DefaultUserIdentifier));
				}

				return elem;
			}
		}


		protected override void PostDeserialize()
		{
			base.PostDeserialize();
			
			if (null != this.AddressResolver.Type && !this.AddressResolver.Type.ImplementsInterface(typeof(IAddressResolver)))
			{
				throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "The 'type' attribute in the 'addressResolver' element must implement the '{0}' interface.", typeof(IAddressResolver).FullName));
			}
			if (!this.TokenGenerator.Type.ImplementsInterface(typeof(ITokenGenerator)))
			{
				throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "The 'type' attribute in the 'tokenGenerator' element must implement the '{0}' interface.", typeof(ITokenGenerator).FullName));
			}
			if (null != this.TokenSender.Type && !this.TokenSender.Type.ImplementsInterface(typeof(ITokenSender)))
			{
				throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "The 'type' attribute in the 'tokenSender' element must implement the '{0}' interface.", typeof(ITokenSender).FullName));
			}
			if (!this.TokenValidator.Type.ImplementsInterface(typeof(ITokenValidator)))
			{
				throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "The 'type' attribute in the 'tokenValidator' element must implement the '{0}' interface.", typeof(ITokenValidator).FullName));
			}
			if (!this.UserIdentifier.Type.ImplementsInterface(typeof(IUserIdentifier)))
			{
				throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "The 'type' attribute in the 'userIdentifier' element must implement the '{0}' interface.", typeof(IUserIdentifier).FullName));
			}

		}

	}
}
