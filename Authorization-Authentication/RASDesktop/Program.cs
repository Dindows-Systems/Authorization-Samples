
/* Title: DesktopRAS  
   Author: Matteo Slaviero 
   This work is absolutely free to use, copy, modify or redistribute for any purpose in any country. */

using System;
using System.Threading;

namespace DesktopRAS
{
    class Program
    {
        /// <summary>
        /// Entry Point
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            Console.WriteLine("Type of Identity: " + Thread.CurrentPrincipal.Identity.GetType());
            Console.WriteLine("Identity Name: " + Thread.CurrentPrincipal.Identity.Name);
            Console.Read();
        }
    }
}
