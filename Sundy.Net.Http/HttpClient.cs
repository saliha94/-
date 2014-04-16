using System.IO;
using System.Net;

namespace Sundy.Net.Http
{
    public abstract partial class HttpClient
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


    }
}
