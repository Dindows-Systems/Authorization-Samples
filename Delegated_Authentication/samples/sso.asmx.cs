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
using System.Collections;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Text;

namespace samples.sforce.com
{
	/// <summary>
	/// This is a more complex implementation that does single sign on to salesforce.com from an intranet page
	/// note that in this example, the users corporate password is never sent to salesforce.com
	///
	/// 	i) an intranet web page uses the CreateToken method on this class to get an authentication token
	///		   that can be passed out to salesforce.com
	///    ii) the authenticate call that salesforce.com makes back to this service simplies verifies the
	///        validity of the token (passed via the password field).
	/// 
	/// In this case we use a simple manufactured token consisting of token #, a time stamp and the users login
	/// which we then encrypt and digitally sign. The checks in authenticate verify the digital signature and
	/// encryption, then check the login matches the passed username, and that the token # and timestamp are
	/// reasonable (the rolling token # and timestamp help prevent replay attacks)
	/// alternatives could be to send a kerberos ticket, or SAML assertion in the password field.
	/// 
	/// The token generation code assumes that it can authenticate the user, this might not be the case
	/// if deployed in the DMZ. In which case, you'll need to put the code on both your internal intranet
	/// and DMZ server, and make sure they are both using the same RSA keys. (the WebService should only
	/// need the publics key to verify the signature & encryption)
	/// </summary>
	public class SingleSignOn : System.Web.Services.WebService
	{
		// this creates a new token for the currently authenticated user
		// this would be called from an authenticated intranet page that want to
		// show a "login to salesforce.com" link. (see gotosfdc.aspx)
		public static string CreateToken(string userName)
		{
			AuthToken t = new AuthToken(userName);
			MemoryStream ms = new MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(ms, t);
			return Convert.ToBase64String(EncryptAndSign(ms.ToArray()));
		}

        // this helper function will take a serialized token and decode it back to a AuthToken
        // object, verifying the signature & encryption along the way		
		internal static AuthToken VerifyAndDecryptToken(string serializedToken)
		{
			// unpack the string into the data & sig
			byte [] data, sig;
			if(!UnpackSerializedString(serializedToken, out data, out sig))
				return null;
			// verify the signature
			if(!GetSigningRsa().VerifyData(data, new SHA1CryptoServiceProvider(), sig))
				return null;
			// decrypt the data
			byte [] token = Decrypt(GetEncRsa(), data);
			AuthToken t = DeserializeToken(token);
			if(!t.Expired)
			    return t;
			return null;
		}
	
		static AuthToken DeserializeToken(byte [] data)
		{
			MemoryStream ms = new MemoryStream(data);
			BinaryFormatter bf = new BinaryFormatter();
			return (AuthToken)bf.Deserialize(ms);
		}

        // this takes a base64 serialized token, and splits it out into the data and signature		
		static bool UnpackSerializedString(string serializedToken, out byte [] data, out byte [] sig)
		{
			data = null;
			sig = null;
			byte [] serializedData = Convert.FromBase64String(serializedToken);
			// first 4 bytes say how much data there is
			int dataLen = BitConverter.ToInt32(serializedData,0);
			// sanity check, dataLen should never be more than 4k
			if(dataLen > 4096)
				return false; 
			data = new byte[dataLen];
			Buffer.BlockCopy(serializedData, 4, data, 0, dataLen);
			sig = new byte[serializedData.Length - 4 - dataLen];
			Buffer.BlockCopy(serializedData, 4 + dataLen, sig, 0, sig.Length);
			return true;
		}

        // takes the source array of bytes, encrypts it and signs it.
		static byte [] EncryptAndSign(byte [] src)
		{
			// encrypt it
			byte [] data = Encrypt(GetEncRsa(), src);
			// sign it
			byte [] sig = GetSigningRsa().SignData(data, new SHA1CryptoServiceProvider());
			byte [] res = new byte[data.Length + sig.Length + 4];
			Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, res, 0, 4);
			Buffer.BlockCopy(data, 0, res, 4, data.Length);
			Buffer.BlockCopy(sig,  0, res, 4 + data.Length, sig.Length);
			return res;
		}
	
		// create a symetrical key, encrypt that with RSA, and encrypt the data with the symertrical key
		// see http://pages.infinit.net/ctech/20031101-0151.html
		static byte[] Encrypt (RSA rsa, byte[] input) 
		{
		     // by default this will create a 128 bits AES (Rijndael) object
		     SymmetricAlgorithm sa = SymmetricAlgorithm.Create ();
		     ICryptoTransform ct = sa.CreateEncryptor ();
		     byte[] encrypt = ct.TransformFinalBlock (input, 0, input.Length);
		
		     RSAPKCS1KeyExchangeFormatter fmt = new RSAPKCS1KeyExchangeFormatter (rsa);
		     byte[] keyex = fmt.CreateKeyExchange (sa.Key);
		
		     // return the key exchange, the IV (public) and encrypted data
		     byte[] result = new byte [keyex.Length + sa.IV.Length + encrypt.Length];
		     Buffer.BlockCopy (keyex, 0, result, 0, keyex.Length);
		     Buffer.BlockCopy (sa.IV, 0, result, keyex.Length, sa.IV.Length);
		     Buffer.BlockCopy (encrypt, 0, result, keyex.Length + sa.IV.Length, encrypt.Length);
		     return result;
		}

        // does the reverse of Encrypt		
		static byte[] Decrypt (RSA rsa, byte[] input) 
		{
		     // by default this will create a 128 bits AES (Rijndael) object
		     SymmetricAlgorithm sa = SymmetricAlgorithm.Create ();
		
		     byte[] keyex = new byte [rsa.KeySize >> 3];
		     Buffer.BlockCopy (input, 0, keyex, 0, keyex.Length);
		
		     RSAPKCS1KeyExchangeDeformatter def = new RSAPKCS1KeyExchangeDeformatter (rsa);
		     byte[] key = def.DecryptKeyExchange (keyex);
		
		     byte[] iv = new byte [sa.IV.Length];
		     Buffer.BlockCopy (input, keyex.Length, iv, 0, iv.Length);
		
		     ICryptoTransform ct = sa.CreateDecryptor (key, iv);
		     byte[] decrypt = ct.TransformFinalBlock (input, keyex.Length + iv.Length, input.Length - (keyex.Length + iv.Length));
		     return decrypt;
		}	

        // gets an RSA key for doing encyption from the keystore			
		private static RSACryptoServiceProvider GetEncRsa()
		{
			CspParameters cp = new CspParameters();
	    	cp.KeyContainerName = "samples.sforce.com:encrypt";
       		RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp);
			return rsa;
		}

        // gets an RSA key for signing from the keystore
		private static RSACryptoServiceProvider GetSigningRsa()
		{
			CspParameters cp = new CspParameters();
	    	cp.KeyContainerName = "samples.sforce.com:signing";
       		RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp);
			return rsa;
		}

        // this is the web service entry point, the password we get back from salesforce.com
        // will be what we generated in the CreateToken call above
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
		
			AuthToken t = VerifyAndDecryptToken(password);
			if ((t== null) || (t.Username != username))
			    return false;

			return usedids.IsNewId(t.TokenNumber, t.Expires);			    
		}
		
		private static UsedIds usedids = new UsedIds();
	}

    // a helper class that keeps track of the set of token number's we've seen
    // and removes them from the list once they expire	
	internal class UsedIds
	{
	    private Hashtable ids, sync_ids;
        private ReaderWriterLock rwl;
        internal UsedIds()
        {
            ids = new Hashtable();
            sync_ids = Hashtable.Synchronized(ids);
            rwl = new ReaderWriterLock();
            Thread cleanupThread = new Thread(new ThreadStart(this.Cleanup));
            cleanupThread.IsBackground = true;
            cleanupThread.Start();
        }
    	    
	    // returns true if this id hasn't been used before, false if it has
	    internal bool IsNewId(long id, DateTime expires)
	    {
	        try
	        {
	            rwl.AcquireReaderLock(Timeout.Infinite);
	            if(sync_ids.ContainsKey(id))
	                return false;
	            sync_ids[id] = expires;
	            return true;
	        }
	        finally
	        {
	            rwl.ReleaseReaderLock();
	        }
	    }

        internal void Cleanup()
        {
            ArrayList expired = new ArrayList();
            while(true)
            {
                Thread.Sleep(60000);
                DateTime now = DateTime.Now;
                try
                {
                    rwl.AcquireWriterLock(Timeout.Infinite);
                    foreach ( DictionaryEntry e in ids )
                    {
                        if ( now > ((DateTime)e.Value) )
                            expired.Add(e.Key);
                    }
                }
                finally
                {
                    rwl.ReleaseWriterLock();
                }
                foreach ( long id in expired )
                    sync_ids.Remove(id);
                expired.Clear();
            }
        }	    
	}
	
	[Serializable]
	internal class AuthToken
	{
		internal AuthToken(string username)
		{
			this.token_number = Interlocked.Increment(ref m_next_token_number);
			this.expires      = DateTime.Now.AddMinutes(5);
			this.username     = username;
		}
		
		public string Username
		{
		    get { return username; }
		}
		
		public DateTime Expires
		{
		    get { return expires; }
		}
		public bool Expired
		{
		    get { return DateTime.Now > expires ; }
		}
		public long TokenNumber
		{
		    get { return token_number; }
		}
		
		private long 		token_number;
		private DateTime 	expires;
		private string		username;
			
		private static long m_next_token_number = 42;
	}
}
