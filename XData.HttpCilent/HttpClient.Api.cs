using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace XData.Net.Http
{
    public partial class HttpClient
    {
        public XElement ApiGet(string relativeUri)
        {
            HttpWebRequest request = ApiCreateRequest(relativeUri, "GET", null);
            return ApiGetResponseElement(request);
        }

        public XElement ApiPost(string relativeUri, XElement value)
        {
            HttpWebRequest request = ApiCreateRequest(relativeUri, "POST", ApiGetBytes(value));
            return ApiGetResponseElement(request);
        }

        public XElement ApiPut(string relativeUri, XElement value)
        {
            HttpWebRequest request = ApiCreateRequest(relativeUri, "PUT", ApiGetBytes(value));
            return ApiGetResponseElement(request);
        }

        public XElement ApiDelete(string relativeUri, XElement value)
        {
            HttpWebRequest request = (value == null)
                ? ApiCreateRequest(relativeUri, "DELETE", null)
                : ApiCreateRequest(relativeUri, "DELETE", ApiGetBytes(value));

            return ApiGetResponseElement(request);
        }

        protected HttpWebRequest ApiCreateRequest(string relativeUri, string method, byte[] content)
        {
            string requestUriString = Origin + relativeUri;

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
            try
            {
                response = request.GetResponse();
                XElement element = GetElement(response);
                return element;
            }
            catch (WebException ex)
            {
                response = ex.Response;
                XElement element = GetElement(response);

                Debug.Assert(element.Name.LocalName == "Error");

                throw new WebException(ex.Message,
                    new Exception(element.Element("ExceptionMessage").Value, new Exception(element.ToString())));
            }
            finally
            {
                if (response != null) response.Close();
            }
        }

        //
        public XElement ApiLogin(string relativeUri, string userName, string password, bool rememberMe)
        {
            XElement element = new XElement("Login");
            element.Add(new XElement("UserName", userName));
            element.Add(new XElement("Password", password));
            element.Add(new XElement("RememberMe", rememberMe.ToString()));
            return ApiPost(relativeUri, element);
        }

        public XElement ApiLogOff(string relativeUri)
        {
            return ApiDelete(relativeUri, null);
        }


    }
}
