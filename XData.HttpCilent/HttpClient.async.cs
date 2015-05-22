﻿using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace XData.Net.Http
{
    public partial class HttpClient
    {
        protected Task<WebResponse> CreateTask(WebRequest request)
        {
            Task<WebResponse> task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
            return task;
        }

        // 
        protected async Task<string> GetResponseStringAsync(WebRequest request)
        {
            WebResponse response = null;
            try
            {
                response = await CreateTask(request);
                string text = GetString(response);
                return text;
            }
            catch (WebException ex)
            {
                response = ex.Response;
                string text = GetString(response);
                throw new WebException(ex.Message, new Exception(text));
            }
            finally
            {
                if (response != null) response.Close();
            }
        }

        public async Task<string> LoginAsync(string relativeUri, string userName, string password, bool rememberMe)
        {
            string requestUriString = Origin + relativeUri;

            HttpWebRequest request = CreateLoginRequest(requestUriString, userName, password, rememberMe);
            return await GetResponseStringAsync(request);
        }

        public async Task<string> LogOffAsync(string relativeUri)
        {
            string requestUriString = Origin + relativeUri;

            HttpWebRequest request = CreateLogOffRequest(requestUriString);
            return await GetResponseStringAsync(request);
        }

        //
        public async Task<string> GetAsync(string relativeUri)
        {
            string requestUriString = Origin + relativeUri;

            HttpWebRequest request = CreateRequest(requestUriString, "GET", "text/plain,application/plain", null, null);
            return await GetResponseStringAsync(request);
        }

        public async Task<string> PostAsync(string relativeUri, NameValueCollection collection)
        {
            string requestUriString = Origin + relativeUri;

            HttpWebRequest request = CreatePostRequest(requestUriString, collection);
            return await GetResponseStringAsync(request);
        }


    }
}
