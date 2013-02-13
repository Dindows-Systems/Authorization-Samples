using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace OpenId
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

        }



        protected void Application_Errort(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError().GetBaseException();
            System.Diagnostics.Debug.WriteLine(ex.ToString());

        }



        protected void Application_End(object sender, EventArgs e)
        {

        }




        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
           
        }
    }
}