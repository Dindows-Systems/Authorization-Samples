using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

/// <summary>
/// Authenticates user agains the data source
/// </summary>
public class UserAuthentication
{
    public static UserAuthentication Instance
    {
        get
        {
            return new UserAuthentication();
        }
    }
    public UserAuthentication()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    /// <summary>
    /// Authenticates user agains a data source and populates the user roles
    /// in the out parameter
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="commaSeperatedRoles"></param>
    /// <returns></returns>
    public bool AuthenticateUser(string userName, string password, out string commaSeperatedRoles)
    {
        bool success = false;
        commaSeperatedRoles = string.Empty;

        //The user credential check is hard coded here. This should be done
        //against a user database in real projects
        if (string.Compare(userName,"Administrator",true) == 0 && password == "123")
        {
            commaSeperatedRoles = "Admin";
            success = true;
        }

        if (string.Compare(userName,"John",true) == 0 && password == "123")
        {
            commaSeperatedRoles = "User";
            success = true;
        }

        return success;
    }
}
