using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XData.Net.Http
{
    public partial class HttpClient
    {
        public async Task<XElement> ApiGetAsync(string relativeUri)
        {
            HttpWebRequest request = ApiCreateRequest(relativeUri, "GET", null);
            return await ApiGetResponseElementAsync(request);
        }

        public async Task<XElement> ApiPostAsync(string relativeUri, XElement value)
        {
            HttpWebRequest request = ApiCreateRequest(relativeUri, "POST", ApiGetBytes(value));
            return await ApiGetResponseElementAsync(request);
        }

        public async Task<XElement> ApiPutAsync(string relativeUri, XElement value)
        {
            HttpWebRequest request = ApiCreateRequest(relativeUri, "PUT", ApiGetBytes(value));
            return await ApiGetResponseElementAsync(request);
        }

        public async Task<XElement> ApiDeleteAsync(string relativeUri, XElement value)
        {
            HttpWebRequest request = (value == null)
                ? ApiCreateRequest(relativeUri, "DELETE", null)
                : ApiCreateRequest(relativeUri, "DELETE", ApiGetBytes(value));

            return await ApiGetResponseElementAsync(request);
        }

        protected async Task<XElement> ApiGetResponseElementAsync(HttpWebRequest request)
        {
            WebResponse response = null;
            try
            {
                response = await CreateTask(request);
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

        protected XElement GetElement(WebResponse response)
        {
            Stream responseStream = null;
            try
            {
                responseStream = response.GetResponseStream();
                XmlReader reader = null;
                try
                {
                    reader = XmlReader.Create(responseStream);
                    XElement element = XElement.Load(reader);
                    return element;
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            finally
            {
                if (responseStream != null) responseStream.Close();
            }
        }

        //
        public async Task<XElement> ApiLoginAsync(string relativeUri, string userName, string password, bool rememberMe)
        {
            XElement element = new XElement("Login");
            element.Add(new XElement("UserName", userName));
            element.Add(new XElement("Password", password));
            element.Add(new XElement("RememberMe", rememberMe.ToString()));
            return await ApiPostAsync(relativeUri, element);
        }

        public async Task<XElement> ApiLogOffAsync(string relativeUri)
        {
            return await ApiDeleteAsync(relativeUri, null);
        }


    }
}
