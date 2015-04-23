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
        public async Task<XElement> ApiGetAsync(string requestUriString)
        {
            HttpWebRequest request = ApiCreateRequest(requestUriString, "GET", null);
            return await ApiGetResponseElementAsync(request);
        }

        // overload
        public async Task<XElement> ApiGetAsync(string requestUriString, string id)
        {
            string requestUri = requestUriString + "/" + id;
            return await ApiGetAsync(requestUri);
        }

        public async Task ApiPostAsync(string requestUriString, XElement value)
        {
            HttpWebRequest request = ApiCreateRequest(requestUriString, "POST", ApiGetBytes(value));
            await ApiResponseEmptyAsync(request);
        }

        public async Task ApiPutAsync(string requestUriString, string id, XElement value)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = ApiCreateRequest(requestUri, "PUT", ApiGetBytes(value));
            await ApiResponseEmptyAsync(request);
        }

        public async Task ApiDeleteAsync(string requestUriString, string id)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = ApiCreateRequest(requestUri, "DELETE", null);
            await ApiResponseEmptyAsync(request);
        }

        //
        public async Task<XElement> ApiPostAsync(string requestUriString, string id, XElement value)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = ApiCreateRequest(requestUri, "POST", ApiGetBytes(value));
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

        protected async Task ApiResponseEmptyAsync(WebRequest request)
        {
            WebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = await CreateTask(request);
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
        public async Task<XElement> ApiLoginAsync(string requestUriString, string userName, string password)
        {
            XElement element = new XElement("Login");
            element.Add(new XElement("UserName", userName));
            element.Add(new XElement("Password", password));
            return await ApiPostAsync(requestUriString, "0", element);
        }

        public async Task ApiLogOffAsync(string requestUriString)
        {
            await ApiDeleteAsync(requestUriString, "0");
        }


    }
}
