using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace Sundy.Net.Http
{
    public partial class WebApiClient : HttpClient
    {
        public XElement Get(string requestUriString)
        {
            HttpWebRequest request = CreateRequest(requestUriString, "GET", null);
            return GetResponseElement(request);
        }

        // overload
        public XElement Get(string requestUriString, string id)
        {
            string requestUri = requestUriString + "/" + id;
            return Get(requestUri);
        }

        public void Post(string requestUriString, XElement value)
        {
            HttpWebRequest request = CreateRequest(requestUriString, "POST", GetBytes(value));
            ResponseEmpty(request);
        }

        public void Put(string requestUriString, string id, XElement value)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = CreateRequest(requestUri, "PUT", GetBytes(value));
            ResponseEmpty(request);
        }

        public void Delete(string requestUriString, string id)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = CreateRequest(requestUri, "DELETE", null);
            ResponseEmpty(request);
        }

        //
        public XElement Post(string requestUriString, string id, XElement value)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = CreateRequest(requestUri, "POST", GetBytes(value));
            return GetResponseElement(request);
        }

        protected HttpWebRequest CreateRequest(string requestUriString, string method, byte[] content)
        {
            return base.CreateRequest(requestUriString, method, "text/xml,application/xml", content, "application/xml");
        }

        protected byte[] GetBytes(XElement element)
        {
            MemoryStream stream = new MemoryStream();
            element.Save(stream);
            byte[] buffer = stream.ToArray();
            stream.Close();
            return buffer;
        }

        protected XElement GetResponseElement(WebRequest request)
        {
            WebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = request.GetResponse();
                responseStream = response.GetResponseStream();
                XmlReader reader = XmlReader.Create(responseStream);
                XElement element = XElement.Load(reader);
                if (element.Name.LocalName == "Error")
                {
                    throw new WebException("The remote server returned error: (500) internal server error.",
                        new Exception(element.Element("ExceptionMessage").Value, new Exception(element.ToString())));
                }
                return element;
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

        protected void ResponseEmpty(WebRequest request)
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
                if (text == string.Empty) return;
                XElement element = XElement.Parse(text);
                if (element.Name.LocalName == "Error")
                {
                    throw new WebException("The remote server returned error: (500) internal server error.",
                        new Exception(element.Element("ExceptionMessage").Value, new Exception(element.ToString())));
                }
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
        public XElement Login(string requestUriString, string userName, string password)
        {
            XElement element = new XElement("Login");
            element.Add(new XElement("UserName", userName));
            element.Add(new XElement("Password", password));
            return Post(requestUriString, "0", element);
        }

        public void LogOff(string requestUriString)
        {
            Delete(requestUriString, "0");
        }


    }
}
