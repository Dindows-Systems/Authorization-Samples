﻿/*
===========================================================================
Copyright (c) 2010 BrickRed Technologies Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sub-license, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
===========================================================================

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Security.Principal;
using System.Web.Security;
using System.Web.UI;
using Brickred.SocialAuth.NET.Core.BusinessObjects;

namespace Brickred.SocialAuth.NET.Core
{
    /// <summary>
    /// Class with extension methods to increase productivity
    /// </summary>
    static class Extensions
    {

        public static void SetQueryparameter(this UriBuilder ub, string key, string value)
        {
            string result = "";
            var queryparams = HttpUtility.ParseQueryString(ub.Query);
            queryparams.Set(key, value);
            queryparams.AllKeys.ToList().ForEach(k => result += k + "=" + queryparams[k] + "&");
            ub.Query = result.Substring(0, result.Length - 1);
        }


        public static string GetBaseURL(this HttpRequest request)
        {
            string baseUrlInConfig = Utility.GetSocialAuthConfiguration().BaseURL.Domain;
            StringBuilder url = new StringBuilder();
            url.Append(request.Url.Scheme);
            url.Append("://");

            if (string.IsNullOrEmpty(baseUrlInConfig))
            {
                url.Append(request.Url.Host);
                if (request.Url.Port != 80)
                {
                    url.Append(":");
                    url.Append(request.Url.Port);
                }


                url.Append(request.ApplicationPath);

            }
            else
            {
                //if port is not default 80 and there is no information about port in web.config as well
                if (request.Url.Port != 80 && !baseUrlInConfig.Contains(":"))
                {
                    url.Append(":");
                    url.Append(request.Url.Port);
                }
                url.Append(baseUrlInConfig);

            }
            if (!url.ToString().EndsWith("/"))
                url.Append("/");
            return url.ToString();

        }
        public static int IndexOfNthOrLast(this string input, string delimeter, int n, int startIndex)
        {
            int position = -1;
            while ((position = (input.IndexOf(delimeter, startIndex))) > -1 && (n-- > 0))
                startIndex = position + 1;
            return startIndex;
        }

        
        public static string Get(this Newtonsoft.Json.Linq.JObject input, string key)
        {
            if (input.SelectToken(key) == null)
                return "";
            else
            {
                string value = input.SelectToken(key).ToString();
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    value = value.Substring(1, value.Length - 2);
                return value;
            }
        }
    }



}

public static class IdentityExtensions
{

    public static bool IsSTSaware(this HttpApplication app)
    {
        HttpModuleCollection activeModules = app.Modules;
        if (activeModules["ClaimsPrincipalHttpModule"] != null)
        {
            if (activeModules["ClaimsPrincipalHttpModule"].ToString() == "Microsoft.IdentityModel.Web.ClaimsPrincipalHttpModule")
                return true;
        }
        return false;
    }


    public static Brickred.SocialAuth.NET.Core.BusinessObjects.UserProfile GetProfile(this IIdentity identity)
    {
        if (SocialAuthUser.IsLoggedIn())
            return SocialAuthUser.GetCurrentUser().GetProfile();
        else
            return null;
    }

    public static List<Brickred.SocialAuth.NET.Core.BusinessObjects.Contact> GetContacts(this IIdentity identity)
    {
        if (SocialAuthUser.IsLoggedIn())
            return SocialAuthUser.GetCurrentUser().GetContacts();
        else
            return null;
    }

    public static string GetProvider(this IIdentity identity)
    {
        if (SocialAuthUser.IsLoggedIn())
            return SocialAuthUser.CurrentConnection.ProviderType.ToString();
        else
            return "";
    }

}