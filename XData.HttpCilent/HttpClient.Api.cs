using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace XData.Net.Http
{
    public partial class HttpClient
    {
        public XElement ApiGet(string requestUriString)
        {
            HttpWebRequest request = ApiCreateRequest(requestUriString, "GET", null);
            return ApiGetResponseElement(request);
        }

        public XElement ApiPost(string requestUriString, XElement value)
        {
            HttpWebRequest request = ApiCreateRequest(requestUriString, "POST", ApiGetBytes(value));
            return ApiGetResponseElement(request);
        }

        public XElement ApiPut(string requestUriString, XElement value)
        {
            HttpWebRequest request = ApiCreateRequest(requestUriString, "PUT", ApiGetBytes(value));
            return ApiGetResponseElement(request);
        }

        public XElement ApiDelete(string requestUriString, XElement value)
        {
            HttpWebRequest request = (value == null)
                ? ApiCreateRequest(requestUriString, "DELETE", null)
                : ApiCreateRequest(requestUriString, "DELETE", ApiGetBytes(value));

            return ApiGetResponseElement(request);
        }

        protected HttpWebRequest ApiCreateRequest(string requestUriString, string method, byte[] content)
        {
            return CreateRequest(requestUriString, method, "text/xml,application/xml", content, "application/xml");
        }

        protected byte[] ApiGetBytes(XElement element)
        {
            MemoryStream stream = new MemoryStream();
            element.Save(stream);
            byte[] buffer = stream.ToArray();
            stream.Close();
            return buffer;
        }

        protected XElement ApiGetResponseElement(WebRequest request)
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

        //
        public XElement ApiLogin(string requestUriString, string userName, string password)
        {
            XElement element = new XElement("Login");
            element.Add(new XElement("UserName", userName));
            element.Add(new XElement("Password", password));
            return ApiPost(requestUriString, element);
        }

        public XElement ApiLogOff(string requestUriString)
        {
            return ApiDelete(requestUriString, null);
        }


    }
}
