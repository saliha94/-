using System.Net;
using System.Threading.Tasks;

namespace Sundy.Net.Http
{
    public abstract partial class HttpClient
    {
        protected Task<WebResponse> CreateTask(WebRequest request)
        {
            Task<WebResponse> task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
            return task;
        }


    }
}
