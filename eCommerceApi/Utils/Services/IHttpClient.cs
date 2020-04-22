using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DevCore.Utils.Services
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string controller, string action,  string authorizationToken = null, string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> GetByIdAsync(string controller, string action, string id, string authorizationToken = null, string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> PostAsync<T>(string controller, string action, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> PutAsync<T>(string controller, string action , T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> DeleteAsync(string controller, string action , string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer");
    }
}
