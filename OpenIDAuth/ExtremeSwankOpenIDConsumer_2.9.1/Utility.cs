using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Net;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace ExtremeSwank.Authentication.OpenID
{
    /// <summary>
    /// Common functions used by main classes and plugins.
    /// </summary>
    public class Utility
    {
        /// <summary>
        /// Converts byte-array to Base64 string.
        /// </summary>
        /// <param name="inputBytes">Byte array to convert.</param>
        /// <returns>A Base64 string representing the input byte-array.</returns>
        static public string ToBase64String(byte[] inputBytes)
        {
            StringBuilder sb = new StringBuilder();
            ToBase64Transform base64Transform = new ToBase64Transform();
            byte[] outputBytes = new byte[base64Transform.OutputBlockSize];
            // Initializie the offset size.
            int inputOffset = 0;
            // Iterate through inputBytes transforming by blockSize.
            int inputBlockSize = base64Transform.InputBlockSize;
            while ((inputBytes.Length - inputOffset) > inputBlockSize)
            {
                base64Transform.TransformBlock(
                    inputBytes, inputOffset, inputBlockSize, outputBytes, 0);

                inputOffset += base64Transform.InputBlockSize;
                sb.Append(Encoding.UTF8.GetString(
                        outputBytes, 0, base64Transform.OutputBlockSize));
            }

            // Transform the final block of data.
            outputBytes = base64Transform.TransformFinalBlock(
                inputBytes, inputOffset, (inputBytes.Length - inputOffset));
            sb.Append(Encoding.UTF8.GetString(outputBytes, 0, outputBytes.Length));

            return sb.ToString();
        }
        /// <summary>
        /// Ensures that the byte array converts to a positive value. 
        /// </summary>
        /// <param name="inputBytes">Unsigned byte-array.</param>
        /// <returns>A corrected byte-array.</returns>
        public static byte[] EnsurePositive(byte[] inputBytes)
        {
            // XXX : if len < 1 : throw error
            int i = Convert.ToInt32(inputBytes[0].ToString());
            if (i > 127)
            {
                byte[] temp = new byte[inputBytes.Length + 1];
                temp[0] = 0;
                inputBytes.CopyTo(temp, 1);
                inputBytes = temp;
            }
            return inputBytes;
        }
        /// <summary>
        /// Positively-ensures an input byte-array and converts to a Base64 string.
        /// </summary>
        /// <param name="inputBytes">Unsigned byte-array.</param>
        /// <returns>A Base64 string representing the byte-array.</returns>
        public static string UnsignedToBase64(byte[] inputBytes)
        {
            return ToBase64String(EnsurePositive(inputBytes));
        }

        /// <summary>
        /// Converts HTTP response to key-value pairs.
        /// </summary>
        /// <param name="response">HTTP response.</param>
        /// <returns>Dictionary&lt;string, string&gt; object representing information in response.</returns>
        public static Dictionary<string, string> SplitResponse(string response)
        {
            string[] rarr = response.Split('\n');
            Dictionary<string, string> sd = new Dictionary<string, string>();

            foreach (string l in rarr)
            {
                string line = l.Trim();
                if (line != "")
                {
                    char[] delimiter = { ':' };
                    string[] keyval = line.Split(delimiter, 2);
                    if (keyval.Length == 2)
                    {
                        sd[keyval[0].Trim()] = keyval[1].Trim();
                    }
                }
            }
            return sd;
        }
        /// <summary>
        /// Converts a Dictionary&lt;string, string&gt; to a URL string.
        /// </summary>
        /// <param name="arr">Dictionary&lt;string, string&gt; to convert.</param>
        /// <returns>A URL string.</returns>
        public static string Keyval2URL(Dictionary<string, string> arr)
        {
            if (arr == null) { return null; }
            string qstr = "";
            foreach (string key in arr.Keys)
            {
                qstr += key + "=" + arr[key] + "&";
            }
            return qstr;
        }

        /// <summary>
        /// Performs an HTTP request and returns the response.
        /// </summary>
        /// <param name="url">URL to make request to.</param>
        /// <param name="method">Request type, either "GET" or "POST".</param>
        /// <param name="pms">Dictionary&lt;string, string&gt; containing key-value pairs to send.</param>
        /// <returns>String containing HTTP response.</returns>
        public static string MakeRequest(string url, string method, Dictionary<string, string> pms)
        {
            Uri uri;
            string arguments = Keyval2URL(pms);

            if (method == null) { method = "GET"; }
            if (method == "GET" && pms != null)
            {
                uri = new Uri(url + "?" + arguments);
            }
            else
            {
                uri = new Uri(url);
            }
            HttpWebRequest hwp = (HttpWebRequest)WebRequest.Create(uri);
            hwp.UserAgent = "ExtremeSwank OpenID Consumer 2.6.1";
            hwp.Method = method;
            Stream s;

            // Open connection, write request to server
            if (method == "POST" && pms != null)
            {
                s = hwp.GetRequestStream();
                StreamWriter sw = new StreamWriter(s, Encoding.UTF8);
                sw.Write(arguments);
            }

            // Get response from server
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)hwp.GetResponse();
            }
            catch (WebException we)
            {
                HandleHttpError((HttpWebResponse)we.Response);
                return null;
            }

            s = response.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            string body = sr.ReadToEnd();
            string headers = "";

            foreach (string header in response.Headers)
            {
                headers += header + ":" + response.Headers[header] + "\n";
            }

            // Close connection
            response.Close();

            string ret = headers + body;
            return ret;
        }
        /// <summary>
        /// Performs an HTTP request and returns the response.
        /// </summary>
        /// <param name="url">URL to make request to.</param>
        /// <returns>String containing HTTP response.</returns>
        public static string MakeRequest(string url)
        {
            return MakeRequest(url, null, null);
        }
        /// <summary>
        /// Performs an HTTP request and returns the response.
        /// </summary>
        /// <param name="url">URL to make request to.</param>
        /// <param name="method">Request type, either "GET" or "POST".</param>
        /// <returns>String containing HTTP response.</returns>
        public static string MakeRequest(string url, string method)
        {
            return MakeRequest(url, method, null);
        }
        /// <summary>
        /// Remove HTML comments from string.
        /// </summary>
        /// <param name="content">String containing HTML.</param>
        /// <returns>String with HTML comments removed.</returns>
        public static string RemoveHtmlComments(string content)
        {
            Regex rx_comments = new Regex(@"((<!-- )((?!<!-- ).)*( -->))(\r\n)*", RegexOptions.Singleline);

            string newcontent = rx_comments.Replace(content, string.Empty);
            return newcontent;
        }
        /// <summary>
        /// Processes errors received during HTTP requests.
        /// </summary>
        /// <param name="response"></param>
        public static void HandleHttpError(HttpWebResponse response)
        {
            Stream s;
            StreamReader sr;
            string body = "";
            if (response == null)
            {
                ErrorStore.Store(Errors.HttpError);
                return;
            }
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    s = response.GetResponseStream();
                    sr = new StreamReader(s);
                    body = sr.ReadToEnd();
                    if (body != null)
                    {
                        Dictionary<string, string> data = SplitResponse(body);
                        if (data["error"] != null)
                        {
                            ErrorStore.Store(data["error"]);
                            return;
                        }
                    }
                    ErrorStore.Store(Errors.HttpError);
                    break;
            }
        }
    }
}
