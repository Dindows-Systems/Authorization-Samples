
/* Title: ACLBrowser  
   Author: Matteo Slaviero 
   This work is absolutely free to use, copy, modify or redistribute for any purpose in any country. */

using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ACLBrowser
{
   
    class Program
    {
        
        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            GetACLInfo();
            Console.ReadKey();
        }

       
        /// <summary>
        /// Get the ACL info
        /// </summary>
        public static void GetACLInfo()
        {
            FileSecurity f = File.GetAccessControl(@"c:\resource.txt");
            AuthorizationRuleCollection acl = f.GetAccessRules(true, true, typeof(NTAccount));

            foreach (FileSystemAccessRule ace in acl)
            {
                Console.WriteLine("Identity: " + ace.IdentityReference.ToString());
                Console.WriteLine("Access Control Type: " + ace.AccessControlType);
                Console.WriteLine("Permissions: " + ace.FileSystemRights.ToString() + "\n");
            }
        }
    }
}
