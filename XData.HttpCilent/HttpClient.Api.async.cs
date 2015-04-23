using System;
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
            Stream responseStream = null;
            try
            {
                response = await CreateTask(request);
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
            catch (Exception e)
            {
                string msg = e.Message;
                throw e;
            }
            finally
            {
                if (responseStream != null) responseStream.Close();
                if (response != null) response.Close();
            }
        }

        //
        public async Task<XElement> ApiLoginAsync(string relativeUri, string userName, string password)
        {
            XElement element = new XElement("Login");
            element.Add(new XElement("UserName", userName));
            element.Add(new XElement("Password", password));
            return await ApiPostAsync(relativeUri, element);
        }

        public async Task<XElement> ApiLogOffAsync(string relativeUri)
        {
            return await ApiDeleteAsync(relativeUri, null);
        }


    }
}
