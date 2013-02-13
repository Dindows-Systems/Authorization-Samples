using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TokenAuthentication.Web.Configuration;
using System.Web;
using System.Web.Caching;
using System.Globalization;

namespace TokenAuthentication.Web
{
	/// <summary>
	/// A class that assists in handling token authentication.
	/// </summary>
	/// <remarks>
	/// If you are developing your own token login page, you should
	/// primarily use the methods <see cref="Authenticate(string)"/>
	/// and <see cref="SendToken"/>. You can also make use of the
	/// <see cref="IsTokenPending"/> method to check whether a token
	/// has already been sent to the user.
	/// </remarks>
	public static class AuthenticationManager
	{

		/// <summary>
		/// Authenticates the current user using the given token. If authentication is
		/// successful, the user is redirected away from the login page, to the originally
		/// requested page. If authentication is unsuccessful, the method returns false.
		/// </summary>
		/// <param name="token">The token to authenticate with.</param>
		public static bool Authenticate(string token)
		{
			return Authenticate(token, true);
		}

		/// <summary>
		/// Authenticates the current user using the given token. If authentication is
		/// successful, the user is redirected away from the login page, to the originally
		/// requested page. If authentication is unsuccessful, the method returns false.
		/// </summary>
		/// <param name="token">The token to authenticate with.</param>
		/// <param name="redirect">
		/// If this parameter is set to <c>true</c> and authentication is successful,
		/// the user is automatically redirected to the originally requested resource.
		/// </param>
		public static bool Authenticate(string token, bool redirect)
		{
			HttpContext.Current.Trace.Write(string.Format(CultureInfo.InvariantCulture, "Authenticating... Token: {0}", token));
			
			if (ValidateToken(token))
			{
				HttpContext.Current.Cache.Remove(GeneratePendingTokenKey());
				HttpContext.Current.Cache.Insert(GenerateAuthenticationKey(), "", null, DateTime.MaxValue, TokenAuthenticationConfigSection.Current.Timeout, CacheItemPriority.NotRemovable, null);

				if (redirect)
				{
					RedirectFromLoginPage();
				}
				return true;
			}

			return false;
		}

		/// <summary>
		/// Ensures that the current user has performed a token authentication
		/// successfully.
		/// </summary>
		/// <remarks>
		/// This method usually called only from <see cref="TokenAuthenticationModule"/>. It is therefore
		/// not necessary to call from application code.
		/// </remarks>
		public static void EnsureAuthentication()
		{
			if (IsLoginPage(HttpContext.Current.Request.Url) && "login" == HttpContext.Current.Request.QueryString["action"])
			{
				if (Authenticate(HttpContext.Current.Request.QueryString["token"], false))
				{
					HttpContext.Current.Response.Write("OK");
				}
				else
				{
					HttpContext.Current.Response.Write("Fail");
				}

				HttpContext.Current.Response.End();
			}

			if (TokenAuthenticationConfigSection.Current.Enabled && IsProtectedUrl(HttpContext.Current.Request.Url) && !IsAuthenticated())
			{
				if (!IsLoginPage(HttpContext.Current.Request.Url) && !IsTokenPending())
				{
					SendToken();
				}
				RedirectToLoginPage();
			}
		}

		/// <summary>
		/// On a login page, returns the originally requested URL that caused the
		/// user to be redirected to the login page.
		/// </summary>
		public static string GetOriginalUrl()
		{
			var url = HttpContext.Current.Request.QueryString["ReturnUrl"];
			if (string.IsNullOrEmpty(url))
			{
				url = HttpContext.Current.Request.ApplicationPath;
			}

			return url;
		}

		/// <summary>
		/// Returns the currently pending token or null if no token is pending.
		/// </summary>
		public static string GetPendingToken()
		{
			return HttpContext.Current.Cache.Get(GeneratePendingTokenKey()) as string;
		}

		/// <summary>
		/// Returns the time left before the currently pending token
		/// expires.
		/// </summary>
		/// <remarks>
		/// Returns <see cref="TimeSpan.Zero"/> if no token is pending.
		/// </remarks>
		public static TimeSpan GetPendingTimeRemaining()
		{
			var timeStampObject = HttpContext.Current.Cache.Get(GeneratePendingTokenTimestampKey());
			if (timeStampObject is DateTime)
			{
				DateTime dt = (DateTime)timeStampObject;
				TimeSpan time = TokenAuthenticationConfigSection.Current.PendingTimeout.Subtract(DateTime.UtcNow.Subtract(dt));
				if (time > TimeSpan.Zero)
				{
					return time;
				}
			}

			return TimeSpan.Zero;
		}

		/// <summary>
		/// Returns true if the current user has authenticated using a token.
		/// </summary>
		/// <remarks>
		/// It is not necessary to call this method from code. This method is primarily
		/// to be used internally.
		/// </remarks>
		public static bool IsAuthenticated()
		{
			var key = GenerateAuthenticationKey();
			var b = null != HttpContext.Current.Cache.Get(key);
			return b;
		}

		/// <summary>
		/// Returns true if the given URL is protected by token authentication.
		/// </summary>
		/// <param name="url">The URL to check.</param>
		public static bool IsProtectedUrl(Uri url)
		{
			if (IsLoginPage(url))
			{
				return false;
			}

			if (TokenAuthenticationConfigSection.Current.ExcludePaths.ContainsUrl(url))
			{
				return false;
			}

			if (TokenAuthenticationConfigSection.Current.IncludePaths.ContainsUrl(url))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns <c>true</c> if a token has been sent to the current user
		/// and it is still pending authentication.
		/// </summary>
		public static bool IsTokenPending()
		{
			var key = GeneratePendingTokenKey();
			var b = null != HttpContext.Current.Cache.Get(key);
			return b;
		}

		/// <summary>
		/// Generates a new token and sends it to the current user as specified
		/// in the configuration information for the application.
		/// </summary>
		public static void SendToken()
		{
			var tokenGenerator = TokenAuthenticationConfigSection.Current.TokenGenerator.CreateInstance<ITokenGenerator>();
			var token = tokenGenerator.GenerateToken();

			var pendingKey = GeneratePendingTokenKey();
			HttpContext.Current.Cache.Remove(GenerateAuthenticationKey());
			HttpContext.Current.Cache.Insert(pendingKey, token, null, DateTime.UtcNow.Add(TokenAuthenticationConfigSection.Current.PendingTimeout), TimeSpan.Zero, CacheItemPriority.NotRemovable, null);
			HttpContext.Current.Cache.Insert(GeneratePendingTokenTimestampKey(), DateTime.UtcNow, new CacheDependency(null, new string[] { pendingKey }));

			string address = null;
			var addressResolver = TokenAuthenticationConfigSection.Current.AddressResolver.CreateInstance<IAddressResolver>();
			if (null != addressResolver)
			{
				address = addressResolver.ResolveAddress();
			}

			UriBuilder builder = new UriBuilder(GetLoginUrl(null));
			builder.Query = string.Format(CultureInfo.InvariantCulture, "action=login&token={0}", token.UrlEncode());

			var sender = TokenAuthenticationConfigSection.Current.TokenSender.CreateInstance<ITokenSender>();
			if (null != sender)
			{
				sender.SendToken(address, HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority), token, builder.Uri);
			}
		}

		/// <summary>
		/// Signs the current user out so that the user must sign in with
		/// a new token again.
		/// </summary>
		public static void SignOut()
		{
			HttpContext.Current.Cache.Remove(GenerateAuthenticationKey());

			if (IsProtectedUrl(HttpContext.Current.Request.Url))
			{
				RedirectToLoginPage();
			}
		}



		/// <summary>
		/// Generates the key to a cached item that is used as a marker that token authentication
		/// has been successfully performed. If a cached item with this key exists and its value
		/// is anything else but <c>null</c>, the current user is considered to have successfully
		/// passed token authentication.
		/// </summary>
		private static string GenerateAuthenticationKey()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}|{1}|authenticated", typeof(AuthenticationManager).FullName, GetUserIdentifier());
		}

		/// <summary>
		/// Generates a key that is used to store the generated token in the cache with.
		/// </summary>
		private static string GeneratePendingTokenKey()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}|{1}|pending-token", typeof(AuthenticationManager).FullName, GetUserIdentifier());
		}

		/// <summary>
		/// Generates a key that is used to store the timestamp for the pending token with.
		/// </summary>
		private static string GeneratePendingTokenTimestampKey()
		{
			return GeneratePendingTokenKey() + "|timestamp";
		}

		/// <summary>
		/// Returns an absolute URL that represents the token login page.
		/// </summary>
		/// <param name="returnUrl">
		/// The URL to use as return URL. This URL is added as query string
		/// parameter to the login page URL. If this parameter is null, no
		/// return URL is added to the login page URL.
		/// </param>
		private static Uri GetLoginUrl(Uri returnUrl)
		{
			string loginPage = Configuration.TokenAuthenticationConfigSection.Current.LoginUrl.Replace("\\", "/").Replace("~", HttpContext.Current.Request.ApplicationPath).Replace("//", "/");
			UriBuilder builder = new UriBuilder(new Uri(HttpContext.Current.Request.Url, loginPage));
			if (null != returnUrl)
			{
				builder.Query = string.Format(CultureInfo.InvariantCulture, "ReturnUrl={0}", HttpUtility.UrlEncode(returnUrl.PathAndQuery));
			}
			return builder.Uri;
		}

		/// <summary>
		/// Returns the identifier returned by the currently configured user identifier.
		/// </summary>
		private static string GetUserIdentifier()
		{
			var identifier = TokenAuthenticationConfigSection.Current.UserIdentifier.CreateInstance<IUserIdentifier>();
			var id = identifier.GetUserIdentifier();
			if (string.IsNullOrEmpty(id))
			{
				throw new ApplicationException("The currently configured user identifer returned an empty string or null as the user identifier. This is not allowed.");
			}

			return id;
		}

		/// <summary>
		/// Returns true if the given URL points to the login page.
		/// </summary>
		/// <param name="url">The URL to check.</param>
		private static bool IsLoginPage(Uri url)
		{
			var loginPage = GetLoginUrl(null).GetLeftPart(UriPartial.Path);
			var currentPage = url.GetLeftPart(UriPartial.Path);

			return string.Equals(loginPage, currentPage, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Redirects the user to the originally requested page (which is
		/// specified in the 'ReturnUrl' query string parameter), or to the
		/// front page of the site, if the originally requested page is
		/// not available.
		/// </summary>
		private static void RedirectFromLoginPage()
		{
			HttpContext.Current.Response.Redirect(GetOriginalUrl(), true);
		}

		/// <summary>
		/// Redirects the user to the configured token login page.
		/// </summary>
		/// <remarks>
		/// If the current request is to the login page, no redirection is performed.
		/// </remarks>
		private static void RedirectToLoginPage()
		{
			HttpContext.Current.Response.Redirect(GetLoginUrl(HttpContext.Current.Request.Url).ToString(), true);
		}

		/// <summary>
		/// Returns true if the given token is a valid pending token.
		/// </summary>
		/// <param name="token">The token to validate.</param>
		private static bool ValidateToken(string token)
		{
			if (GetPendingTimeRemaining() > TimeSpan.Zero)
			{
				ITokenValidator validator = TokenAuthenticationConfigSection.Current.TokenValidator.CreateInstance<ITokenValidator>();
				return validator.ValidateToken(token);
			}
			return false;
		}

	}
}
