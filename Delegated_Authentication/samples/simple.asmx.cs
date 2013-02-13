// Copyright (c) 2004 salesforce.com
//
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

using System;
using System.DirectoryServices;

namespace samples.sforce.com
{
	/// <summary>
	/// This is about the simpliest implemention of the sforce authentication service you can write
	/// It simply trys to connect to your Active Directory server using the passed in credentials
	/// If there's a bad username/password combo it throws an exception and we return false
	/// otherwise the credentials are ok and we return true. 
	/// Note that DirectoryEntry might not goto AD until we do something that actually requires it
	/// that's why we read a property from the created DirectoryEntry object.
	/// </summary>
	public class SimpleAdAuth : System.Web.Services.WebService
	{
		[System.Web.Services.WebMethodAttribute()]
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("", 
				RequestNamespace="urn:authentication.soap.sforce.com", 
				ResponseElementName="AuthenticateResult", 
				ResponseNamespace="urn:authentication.soap.sforce.com", 
				Use=System.Web.Services.Description.SoapBindingUse.Literal, 
				ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
		[return: System.Xml.Serialization.XmlElementAttribute("Authenticated")]
		public bool Authenticate ( string username, 
															 string password, 
															 string sourceIp, 
															 [System.Xml.Serialization.XmlAnyElementAttribute()] System.Xml.XmlElement[] Any)
		{
			if(username.IndexOf("@")==-1)
				return false;

			// try and bind to an AD entry, this will authenticate the username & password we supply
			// TODO: you'll need to change this to match your AD name
			const string root = "LDAP://DC=sample,DC=org";
			try
			{
				DirectoryEntry de = new DirectoryEntry(root, username, password);
				// retrieve a property
				string tempName = de.Name;
				return true;
			}
    		catch(Exception)
    		{
				return false;
			}
		}
	}
}
