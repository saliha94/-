using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Sundy.Net.Http
{
    public partial class WebApiClient
    {
        public async Task<XElement> GetAsync(string requestUriString)
        {
            HttpWebRequest request = CreateRequest(requestUriString, "GET", null);
            return await GetResponseElementAsync(request);
        }

        // overload
        public async Task<XElement> GetAsync(string requestUriString, string id)
        {
            string requestUri = requestUriString + "/" + id;
            return await GetAsync(requestUri);
        }

        public async Task PostAsync(string requestUriString, XElement value)
        {
            HttpWebRequest request = CreateRequest(requestUriString, "POST", GetBytes(value));
            await ResponseEmptyAsync(request);
        }

        public async Task PutAsync(string requestUriString, string id, XElement value)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = CreateRequest(requestUri, "PUT", GetBytes(value));
            await ResponseEmptyAsync(request);
        }

        public async Task DeleteAsync(string requestUriString, string id)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = CreateRequest(requestUri, "DELETE", null);
            await ResponseEmptyAsync(request);
        }

        //
        public async Task<XElement> PostAsync(string requestUriString, string id, XElement value)
        {
            string requestUri = requestUriString + "/" + id;
            HttpWebRequest request = CreateRequest(requestUri, "POST", GetBytes(value));
            return await GetResponseElementAsync(request);
        }

        protected async Task<XElement> GetResponseElementAsync(HttpWebRequest request)
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

        protected async Task ResponseEmptyAsync(WebRequest request)
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
        public async Task<XElement> LoginAsync(string requestUriString, string userName, string password)
        {
            XElement element = new XElement("Login");
            element.Add(new XElement("UserName", userName));
            element.Add(new XElement("Password", password));
            return await PostAsync(requestUriString, "0", element);
        }

        public async Task LogOffAsync(string requestUriString)
        {
            await DeleteAsync(requestUriString, "0");
        }


    }
}
