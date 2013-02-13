using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Runtime.InteropServices;
using System.Configuration;
using BusinessComponent;

public partial class _Default : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
            CheckAccess accessCheckObj;
			bool hasAccess = false;
			accessCheckObj = new CheckAccess();
			//Get user Token
			HandleRef token = new HandleRef(this, ((HttpWorkerRequest)((IServiceProvider)Context).GetService(typeof(HttpWorkerRequest))).GetUserToken());
            // This code will check the access permission for the logged in user for Modifying Employee Information
			try
			{
	    		hasAccess = accessCheckObj.CheckAccessPermissions(token
    								, Resources.Operations.ModifyEmployeeInformation.ToString()
									, AccessCheck.ScopeName.GENERAL
									, Convert.ToInt32(Resources.Operations.ModifyEmployeeInformationID.ToString()));
                // if has Access is true then the user has the permission to proceed and perform the operation 
                // else he/she should be redirected to the access denied page 
			}
            catch (Exception ex)
			{
				// Handle Exception
			}
    }
}
