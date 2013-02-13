
/* Title: AuthenticationDemo 
   Author: Matteo Slaviero 
   This work is absolutely free to use, copy, modify or redistribute for any purpose in any country. */

using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Web.UI;



namespace AuthenticationDemo
{
    /// <summary>
    /// Default Page
    /// </summary>
    public partial class PageDefault : Page
    {
        /// <summary>
        /// OnLoad Override
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            WritePrincipalAndIdentity();
            CanLoadResource();
        }

 
        /// <summary>
        /// Explore the authentication properties of the current thread.
        /// </summary>
        public void WritePrincipalAndIdentity()
        {

            IPrincipal p = Thread.CurrentPrincipal;
            IIdentity i = Thread.CurrentPrincipal.Identity;

            WriteToPage("Is Authenticate: " + i.IsAuthenticated);
            WriteToPage("Identity Name: " + i.Name);
            WriteToPage("Authentication Type: " + i.AuthenticationType);
           
            WriteToPage("&nbsp");
            WriteToPage("Is Administrator: " + p.IsInRole(@"BUILTIN\Administrators"));

        }

        /// <summary>
        /// 
        /// </summary>
        public void CanLoadResource()
        {

            FileStream stream = null;
            WindowsImpersonationContext imp = null;

            try
            {
                //IIdentity i = Thread.CurrentPrincipal.Identity;
                //imp = ((WindowsIdentity)i).Impersonate();

                stream = File.OpenRead(Server.MapPath("resource.txt"));

                WriteToPage("Access to file allowed.");
            }
            catch (UnauthorizedAccessException)
            {
                WriteException("Access to file denied.");
            }
            finally
            {
                if (imp != null)
                {
                    imp.Undo();
                    imp.Dispose();
                }

                if (stream != null) stream.Dispose(); 
            }
        }

        /// <summary>
        /// Write a string on the page
        /// </summary>
        /// <param name="text"></param>
        private void WriteToPage(string text)
        {
            output.InnerHtml += "<p>" + text + "</p>";
        }

        /// <summary>
        /// Write an exception message
        /// </summary>
        private void WriteException(string text)
        {
            exMessage.InnerHtml += "<p>" + text + "</p>";
        }
    }
}

