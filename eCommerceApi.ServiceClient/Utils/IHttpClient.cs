using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApiCore.Utils.Services
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string controller, string authorizationToken = null, string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> GetByIdAsync(string controller, string id, string authorizationToken = null, string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> PostAsync<T>(string controller, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> PutAsync<T>(string controller, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> DeleteAsync(string controller, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer");
    }
}
