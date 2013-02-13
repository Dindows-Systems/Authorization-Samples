using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace OAuth4Client.OAuth
{
    public class OAuthException: Exception
    {
        public OAuthException(string text): base(text)
        {
        }
        public OAuthException(string text, Exception innerException): base(text, innerException)
        {
        }        
    }

    public class OAuthNetworkException : OAuthException
    {
        public string Url { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ResponseDataString { get; set; }
        public string OAuthHeader { get; set; }

        public OAuthNetworkException(string text, string url, HttpStatusCode statusCode, string responseDataString, string oAuthHeader)
            : base(text)
        {
            this.Url = url;
            this.StatusCode = statusCode;
            this.ResponseDataString = responseDataString;
            this.OAuthHeader = oAuthHeader;
        }
    }
}