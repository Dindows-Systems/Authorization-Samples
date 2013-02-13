using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Microsoft.Interop.Security.AzRoles;
using System.Runtime.InteropServices;


/// <summary>
/// Summary description for CheckAccess
/// </summary>

namespace BusinessComponent
{
    public class CheckAccess
    {
		# region Constants
    	    public enum ScopeName
		    {
			    GENERAL = 0,
			    ENGINEERING = 1,
                BUSINESSINTELLIGENCE = 2,
                BPO = 3
		    }
		#endregion

		# region Methods

		public bool CheckAccessPermissions(HandleRef token, string name, ScopeName scopeName, int operationID)
		{
			AzAuthorizationStore authorizationStore = new AzAuthorizationStoreClass();
			authorizationStore.Initialize(0, ConfigurationSettings.AppSettings["AuthorizationStorePath"], null);
			IAzApplication iazApplication = authorizationStore.OpenApplication("SatheeshApp", null);
			IAzClientContext context = iazApplication.InitializeClientContextFromToken((UInt64)token.Handle, 0);
			object results = new object[1];
			Object[] operation = new Object[1];
			Object[] scope = new Object[1];
			Object[] names = new Object[1];
			Object[] values = new Object[1];
			operation[0] = operationID;
			if (scopeName == ScopeName.GENERAL)
			{
				scope[0] = "GENERAL";
			}
			names[0] = "";
			values[0] = "";
			try
			{
				results = context.AccessCheck(name, scope, operation, names, values, null, null, null);
				object[] returnResults = (object[])results;
				if (returnResults[0].ToString() == "0")
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception ex)
			{

				// Handle Exception
			}
		}
		#endregion
    }
}