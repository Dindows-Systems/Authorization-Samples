using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Xml;

/// <summary>
/// An utility class that allows to use Forms authorization with Roles
/// Author : M.M.Al-Farooque Shubho
/// </summary>
public class FormsAuthenticationUtil
{
    private FormsAuthenticationUtil()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    /// <summary>
    /// Creates a Forms authentication ticket and sets it within Uri or Cookie using the SetAuthCookieMain()
    /// private method, and redirects to the originally requested page 
    /// </summary>
    /// <param name="userName">User name</param>
    /// <param name="commaSeperatedRoles">Comma seperated roles for the users</param>
    /// <param name="createPersistentCookie">True or false whether to create persistant cookie</param>
    /// <param name="strCookiePath">Path for which the authentication ticket is valid</param>
    /// <param name="ExpirationTimeInMinutes">Time in minutes after which the authentication ticket will expire</param>
    private static void RedirectFromLoginPageMain(string userName, string commaSeperatedRoles, bool createPersistentCookie, string strCookiePath)
    {
        SetAuthCookieMain(userName, commaSeperatedRoles, createPersistentCookie, strCookiePath);
        HttpContext.Current.Response.Redirect(FormsAuthentication.GetRedirectUrl(userName, createPersistentCookie));
    }

    /// <summary>
    /// Creates Forms authentication ticket and redirects to the originally requested page. Uses the 
    /// RedirectFromLoginPageMain() private method
    /// </summary>
    /// <param name="userName">User name</param>
    /// <param name="commaSeperatedRoles">Comma seperated roles for the users</param>
    /// <param name="createPersistentCookie">True or false whether to create persistant cookie</param>
    /// <param name="strCookiePath">Path for which the authentication ticket is valid</param>
    public static void RedirectFromLoginPage(string userName, string commaSeperatedRoles, bool createPersistentCookie, string strCookiePath)
    {
        RedirectFromLoginPageMain(userName, commaSeperatedRoles, createPersistentCookie, strCookiePath);
    }

    /// <summary>
    /// Creates Forms authentication ticket and redirects to the originally requested page. Uses the 
    /// RedirectFromLoginPageMain() private method
    /// </summary>
    /// <param name="userName">User name</param>
    /// <param name="commaSeperatedRoles">Comma seperated roles for the users</param>
    /// <param name="createPersistentCookie">True or false whether to create persistant cookie</param>
    public static void RedirectFromLoginPage(string userName, string commaSeperatedRoles, bool createPersistentCookie)
    {
        RedirectFromLoginPageMain(userName, commaSeperatedRoles, createPersistentCookie, null);
    }


    /// <summary>
    /// Creates and returns the Forms authentication ticket 
    /// </summary>
    /// <param name="userName">User name</param>
    /// <param name="commaSeperatedRoles">Comma seperated roles for the users</param>
    /// <param name="createPersistentCookie">True or false whether to create persistant cookie</param>
    /// <param name="strCookiePath">Path for which the authentication ticket is valid</param>
    private static FormsAuthenticationTicket CreateAuthenticationTicket(string userName, string commaSeperatedRoles, bool createPersistentCookie, string strCookiePath)
    {
        string cookiePath = strCookiePath == null ? FormsAuthentication.FormsCookiePath : strCookiePath;

        //Determine the cookie timeout value from web.config if specified
        int expirationMinutes = GetCookieTimeoutValue();

        //Create the authentication ticket
        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
        1,                      //A dummy ticket version

        userName,               //User name for whome the ticket is issued

        DateTime.Now,           //Current date and time

        DateTime.Now.AddMinutes(expirationMinutes), //Expiration date and time

        createPersistentCookie, //Whether to persist coolkie on client side. If true, 
            //The authentication ticket will be issued for new sessions from
            //the same client PC    

        commaSeperatedRoles,    //Comma seperated user roles

        cookiePath);            //Path cookie valid for

        return ticket;
    }

    /// <summary>
    /// Retrieves cookie timeout value in the <forms></forms> section in the web.config file as this
    /// value is not accessable via the FormsAuthentication or any other built in class
    /// </summary>
    /// <returns></returns>
    private static int GetCookieTimeoutValue()
    {
        int timeout = 30; //Default timeout is 30 minutes
        XmlDocument webConfig = new XmlDocument();
        webConfig.Load(HttpContext.Current.Server.MapPath(@"~\web.config"));
        XmlNode node = webConfig.SelectSingleNode("/configuration/system.web/authentication/forms");
        if (node != null && node.Attributes["timeout"] != null)
        {
            timeout = int.Parse(node.Attributes["timeout"].Value);
        }

        return timeout;
    }

    /// <summary>
    /// Creates a Forms authentication ticket and writes it in Url or embeds it within Cookie. Uses the             
    /// SetAuthCookieMain() private method 
    /// </summary>
    /// <param name="userName">User name</param>
    /// <param name="commaSeperatedRoles">Comma seperated roles for the users</param>
    /// <param name="createPersistentCookie">True or false whether to create persistant cookie</param>
    public static void SetAuthCookie(string userName, string commaSeperatedRoles, bool createPersistentCookie)
    {
        SetAuthCookieMain(userName, commaSeperatedRoles, createPersistentCookie, null);
    }

    /// <summary>
    /// Creates a Forms authentication ticket and writes it in Url or embeds it within Cookie. Uses the             
    /// SetAuthCookieMain() private method 
    /// </summary>
    /// <param name="userName">User name</param>
    /// <param name="commaSeperatedRoles">Comma seperated roles for the users</param>
    /// <param name="createPersistentCookie">True or false whether to create persistant cookie</param>
    /// <param name="strCookiePath">Path for which the authentication ticket is valid</param>
    public static void SetAuthCookie(string userName, string commaSeperatedRoles, bool createPersistentCookie, string strCookiePath)
    {
        SetAuthCookieMain(userName, commaSeperatedRoles, createPersistentCookie, strCookiePath);
    }

    /// <summary>
    /// Creates Forms authentication ticket using the private method CreateAuthenticationTicket() and writes 
    /// it in Url or embeds it within Cookie 
    /// </summary>
    /// <param name="userName">User name</param>
    /// <param name="commaSeperatedRoles">Comma seperated roles for the users</param>
    /// <param name="createPersistentCookie">True or false whether to create persistant cookie</param>
    /// <param name="strCookiePath">Path for which the authentication ticket is valid</param>
    private static void SetAuthCookieMain(string userName, string commaSeperatedRoles, bool createPersistentCookie, string strCookiePath)
    {
        FormsAuthenticationTicket ticket = CreateAuthenticationTicket(userName, commaSeperatedRoles, createPersistentCookie, strCookiePath);
        //Encrypt the authentication ticket
        string encrypetedTicket = FormsAuthentication.Encrypt(ticket);

        if (!FormsAuthentication.CookiesSupported)
        {
            //If the authentication ticket is specified not to use cookie, set it in the Uri
            FormsAuthentication.SetAuthCookie(encrypetedTicket, createPersistentCookie);
        }
        else
        {
            //If the authentication ticket is specified to use a cookie, wrap it within a cookie.
            //The default cookie name is .ASPXAUTH if not specified 
            //in the <forms> element in web.config
            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypetedTicket);

            //Set the cookie's expiration time to the tickets expiration time
            if (ticket.IsPersistent) authCookie.Expires = ticket.Expiration;
            ////Set the cookie in the Response
            HttpContext.Current.Response.Cookies.Add(authCookie);
        }
    }


    /// <summary>
    /// Adds roles to the current User in HttpContext after forms authentication authenticates the user
    /// so that, the authorization mechanism can authorize user based on the groups/roles of the user
    /// </summary>
    public static void AttachRolesToUser()
    {
        if (HttpContext.Current.User != null)
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                if (HttpContext.Current.User.Identity is FormsIdentity)
                {
                    FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;

                    FormsAuthenticationTicket ticket = (id.Ticket);

                    if (!FormsAuthentication.CookiesSupported)
                    {
                        //If cookie is not supported for forms authentication, then the 
                        //authentication ticket is stored in the Url, which is encrypted.
                        //So, decrypt it
                        ticket = FormsAuthentication.Decrypt(id.Ticket.Name);
                    }

                    // Get the stored user-data, in this case, user roles
                    if (!string.IsNullOrEmpty(ticket.UserData))
                    {
                        string userData = ticket.UserData;

                        string[] roles = userData.Split(',');
                        //Roles were put in the UserData property in the authentication ticket
                        //while creating it

                        HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(id, roles);
                    }
                }
            }
        }
    }
}
