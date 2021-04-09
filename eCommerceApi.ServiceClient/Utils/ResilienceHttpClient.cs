using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiCore.Utils.Services
{
    public class ResilienceHttpClient : IHttpClient
    {
        HttpClient _client;
        Policy _policy;
        string _url;

        public ResilienceHttpClient(string BaseUrl)
        {
            _url = BaseUrl;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Connection", "close");

            _policy = Policy.Handle<HttpRequestException>()
              // Policy 1: wait and retry policy (bypasses internet connectivity issues)
              .WaitAndRetryAsync(
                  // number of retries
                  6,
                  // exponential backofff
                  retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                  // on retry
                  (exception, timeSpan, retryCount, context) =>
                  {
                      //var msg = $"Retry {retryCount} implemented with Polly's RetryPolicy " +
                      //    $"of {context.PolicyKey} " +
                      //    $"at {context.ExecutionKey}, " +
                      //    $"due to: {exception}.";
                  });

        }

        public Task<HttpResponseMessage> GetAsync(string controller, string authorizationToken = null, string authorizationMethod = "Bearer")
        {
            return _policy.ExecuteAsync(async () =>
            {
                var uri = new Uri(String.Format(_url, controller, string.Empty));
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                   authorizationMethod,
                   authorizationToken
               );
                var response = await _client.GetAsync(uri);
                return response;
            });


        }

        public Task<HttpResponseMessage> GetAsync(Uri uri, string authorizationToken = null, string authorizationMethod = "Bearer")
        {
            return _policy.ExecuteAsync(async () =>
            {

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                   authorizationMethod,
                   authorizationToken
               );
                var response = await _client.GetAsync(uri);
                return response;
            });


        }




        public Task<HttpResponseMessage> GetByIdAsync(string controller, string id, string authorizationToken = null, string authorizationMethod = "Bearer")
        {


            return _policy.ExecuteAsync(async () =>
            {
                var uri = new Uri(String.Format(_url, controller, id));
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                       authorizationMethod,
                       authorizationToken
                   );
                var response = await _client.GetAsync(uri);
                return response;
            });



        }




        public Task<HttpResponseMessage> PutAsync<T>(string controller, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            return _policy.ExecuteAsync(async () =>
            {
                var uri = new Uri(string.Format(_url, controller, string.Empty));
                var json = JsonConvert.SerializeObject(item);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                   authorizationMethod,
                   authorizationToken
               );
                var response = await _client.PutAsync(uri, content);
                return response;
            });

        }

        public Task<HttpResponseMessage> PutAsync<T>(Uri uri, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            return _policy.ExecuteAsync(async () =>
            {

                var json = JsonConvert.SerializeObject(item);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                   authorizationMethod,
                   authorizationToken
               );
                var response = await _client.PutAsync(uri, content);
                return response;
            });

        }

        public Task<HttpResponseMessage> PostAsync<T>(string controller, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            return _policy.ExecuteAsync(async () =>
            {
                var uri = new Uri(string.Format(_url, controller, string.Empty));
                var json = JsonConvert.SerializeObject(item);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                   authorizationMethod,
                   authorizationToken
               );
                var response = await _client.PostAsync(uri, content);
                return response;
            });


        }


        public Task<HttpResponseMessage> PostAsync<T>(Uri uri, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            return _policy.ExecuteAsync(async () =>
            {

                var json = JsonConvert.SerializeObject(item);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                   authorizationMethod,
                   authorizationToken
               );
                var response = await _client.PostAsync(uri, content);
                return response;
            });


        }

        public Task<HttpResponseMessage> DeleteAsync(string controller, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            throw new NotImplementedException();
        }
    }
}