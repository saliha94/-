using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Sundy.Net.Http
{
    public partial class HttpClient
    {

        private int _timeout = 90000; // 90 secs
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        // Allow null
        private string _userAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)";
        public string UserAgent
        {
            get { return _userAgent; }
            set { _userAgent = value; }
        }

        protected CookieContainer CookieContainer = new CookieContainer();

        protected HttpWebRequest CreateRequest(string requestUriString)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);
            request.CookieContainer = CookieContainer;
            return request;
        }

        protected HttpWebRequest CreateRequest(string requestUriString, string method, string accept, byte[] content, string contentType)
        {
            HttpWebRequest request = CreateRequest(requestUriString);
            request.AllowAutoRedirect = false;
            request.KeepAlive = true;
            request.Timeout = Timeout;
            request.Method = method;
            request.Accept = accept;
            request.UserAgent = UserAgent;
            if (content != null)
            {
                request.ContentType = contentType;
                request.ContentLength = content.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(content, 0, content.Length);
                requestStream.Close();
            }
            return request;
        }

        protected string GetBaseUriString(string uriString)
        {
            string baseUriString;
            int index = uriString.IndexOf('/');
            if (index == -1 || index == uriString.Length - 1)
            {
                baseUriString = uriString;
            }
            else
            {
                if (uriString[index + 1] == '/')
                {
                    index = uriString.IndexOf('/', index + 2);
                }
                baseUriString = uriString.Substring(0, index);
            }
            return baseUriString;
        }


        // 
        protected readonly string VerificationTokenName = "__RequestVerificationToken";

        protected string GetVerificationTokenValue(string requestUriString)
        {
            HttpWebRequest request = CreateRequest(requestUriString, "GET", null, "application/html");
            HttpWebResponse response = null;
            Stream responseStream = null;
            string text;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                text = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                throw ex;
            }
            finally
            {
                if (responseStream != null) responseStream.Close();
                if (response != null) response.Close();
            }

            //
            Regex regex = new Regex("\"" + VerificationTokenName + "\"" + @".+value\s*=\s*"".+?""");
            // name="__RequestVerificationToken" contentType="hidden" source="VuT-QxOhABS6H1K24Bl15iH40QeHY6gTgP0qIu_avXSJ4XMD_h2x05-r2qfJYiAx3szYgMKQkhE91yveHr_PPsj58CxprT2y_2l3EVmA-sI1"
            string s = regex.Match(text).Value;
            string[] ss = s.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            string result = ss[ss.Length - 1];
            result = result.TrimStart('"').TrimEnd('"');
            return result;
        }

        protected HttpWebRequest CreateLoginRequest(string requestUriString, string userName, string password)
        {
            string tokenValue = GetVerificationTokenValue(requestUriString);

            string contentString = VerificationTokenName + "=" + tokenValue + "&";
            contentString += string.Format("UserName={0}&Password={1}&RememberMe=false", userName, password);
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string contentType = "application/x-www-form-urlencoded";

            HttpWebRequest request = CreateRequest(requestUriString, "POST", content, contentType);
            return request;
        }

        public string Login(string requestUriString, string userName, string password)
        {
            HttpWebRequest request = CreateLoginRequest(requestUriString, userName, password);
            return GetResponseString(request);
        }

        protected HttpWebRequest CreateLogOffRequest(string requestUriString)
        {
            string tokenValue = GetVerificationTokenValue(GetBaseUriString(requestUriString));
            string contentString = VerificationTokenName + "=" + tokenValue;
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string contentType = "application/x-www-form-urlencoded";

            HttpWebRequest request = CreateRequest(requestUriString, "POST", content, contentType);
            return request;
        }

        public string LogOff(string requestUriString)
        {
            HttpWebRequest request = CreateLogOffRequest(requestUriString);
            return GetResponseString(request);
        }

        protected HttpWebRequest CreateRequest(string requestUriString, string method, byte[] content, string contentType)
        {
            return CreateRequest(requestUriString, method, "text/plain,application/plain", content, contentType);
        }

        protected string GetResponseString(WebRequest request)
        {
            WebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = request.GetResponse();
                responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string text = reader.ReadToEnd();
                reader.Close();
                return text;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                throw ex;
            }
            finally
            {
                if (responseStream != null) responseStream.Close();
                if (response != null) response.Close();
            }
        }

        //
        public string Get(string requestUriString)
        {
            HttpWebRequest request = CreateRequest(requestUriString, "GET", null, null);
            return GetResponseString(request);
        }

        // overload
        public string Get(string requestUriString, string id)
        {
            string requestUri = requestUriString + "/" + id;
            return Get(requestUri);
        }

        protected HttpWebRequest CreatePostRequest(string requestUriString, NameValueCollection collection)
        {
            string contentString = string.Empty;
            foreach (string key in collection.AllKeys)
            {
                string value = collection[key];
                contentString += string.Format("&{0}={1}", key, value);
            }
            contentString = contentString.TrimStart('&');
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string contentType = "application/x-www-form-urlencoded";
            HttpWebRequest request = CreateRequest(requestUriString, "POST", content, contentType);
            return request;
        }

        public string Post(string requestUriString, NameValueCollection collection)
        {
            HttpWebRequest request = CreatePostRequest(requestUriString, collection);
            return GetResponseString(request);
        }

        // overload
        public string Post(string requestUriString, string id, NameValueCollection collection)
        {
            string requestUri = requestUriString + "/" + id;
            return Post(requestUri, collection);
        }


    }
}
