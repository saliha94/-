using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sundy.Net.Http
{
    public partial class WebClient
    {
        protected async Task<string> GetResponseStringAsync(WebRequest request)
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

        public async Task<string> LoginAsync(string requestUriString, string userName, string password)
        {
            HttpWebRequest request = CreateLoginRequest(requestUriString, userName, password);
            return await GetResponseStringAsync(request);
        }

        public async Task<string> LogOffAsync(string requestUriString)
        {
            HttpWebRequest request = CreateLogOffRequest(requestUriString);
            return await GetResponseStringAsync(request);
        }

        //
        public async Task<string> GetAsync(string requestUriString)
        {
            HttpWebRequest request = base.CreateRequest(requestUriString, "GET", "text/plain,application/plain", null, null);
            return await GetResponseStringAsync(request);
        }

        // overload
        public async Task<string> GetAsync(string requestUriString, string id)
        {
            string requestUri = requestUriString + "/" + id;
            return await GetAsync(requestUri);
        }

        public async Task<string> PostAsync(string requestUriString, NameValueCollection collection)
        {
            HttpWebRequest request = CreatePostRequest(requestUriString, collection);
            return await GetResponseStringAsync(request);
        }

        // overload
        public async Task<string> PostAsync(string requestUriString, string id, NameValueCollection collection)
        {
            string requestUri = requestUriString + "/" + id;
            return await PostAsync(requestUri, collection);
        }


    }
}
