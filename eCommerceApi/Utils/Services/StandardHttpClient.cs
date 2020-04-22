
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace DevCore.Utils.Services
{
    public class StandardHttpClient : IHttpClient
    {
        private HttpClient _client;
        private string Url { get; set; }

        public StandardHttpClient(string url)
        {
            Url = url;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Connection", "close");

        }

        public async Task<HttpResponseMessage> GetAsync(string controller, string action,  string authorizationToken = null, string authorizationMethod = "Bearer")
        {
            var uri = new Uri(String.Format(Url, controller, action ,  string.Empty));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                authorizationMethod,
                authorizationToken
            );
            var response = await _client.GetAsync(uri);
            return response;
        }

        public async Task<HttpResponseMessage> GetAsync(Uri uri, string authorizationToken = null, string authorizationMethod = "Bearer")
        {

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                authorizationMethod,
                authorizationToken
            );
            var response = await _client.GetAsync(uri);
            return response;
        }

        public async Task<HttpResponseMessage> GetByIdAsync(string controller,string action , string id, string authorizationToken = null, string authorizationMethod = "Bearer")
        {
            var uri = new Uri(String.Format(Url, controller, action, id));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
              authorizationMethod,
              authorizationToken
               );
            var response = await _client.GetAsync(uri);
            return response;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string controller, string action , T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            var uri = new Uri(string.Format(Url, controller, action , string.Empty));
            var json = JsonConvert.SerializeObject(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
              authorizationMethod,
              authorizationToken
          );
            var response = await _client.PostAsync(uri, content);
            return response;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(Uri uri, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {

            var json = JsonConvert.SerializeObject(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
              authorizationMethod,
              authorizationToken
          );
            var response = await _client.PostAsync(uri, content);
            return response;
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string controller,string action , T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {

            var uri = new Uri(string.Format(Url, controller, action, string.Empty));
            var json = JsonConvert.SerializeObject(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
             authorizationMethod,
             authorizationToken
             );
            var response = await _client.PutAsync(uri, content);
            return response;
        }


        public async Task<HttpResponseMessage> PutAsync<T>(Uri uri, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {


            var json = JsonConvert.SerializeObject(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
             authorizationMethod,
             authorizationToken
             );
            var response = await _client.PutAsync(uri, content);
            return response;
        }

        public Task<HttpResponseMessage> DeleteAsync(string controller, string action, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            throw new NotImplementedException();
        }



        public void Dispose()
        {
            _client.Dispose();
            Url = null;
        }

    }
}
