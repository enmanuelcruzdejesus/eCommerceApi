using ApiCore.Utils.Services;
using eCommerce.Model.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace eCommerceApi.ServiceClient.Services
{
    public class RestApiService
    {

        IHttpClient _service;
        //App _app = (App)App.Current;
        string _authorizationToken;
        string _authorizationMethod = "Bearer";

        public RestApiService(IHttpClient client, string authToken, string authMethod)
        {
            _service = client;
            _authorizationToken = authToken;
            _authorizationMethod = authMethod;
        }


        public async Task<HttpResponseMessage> Sync()
        {
            var response = await _service.PostAsync<List<Orders>>("app/sync", null, _authorizationToken, _authorizationMethod);
            return response;
        }

        public async Task<HttpResponseMessage> SyncOrders()
        {
            var response = await _service.PostAsync<List<Orders>>("order/download", null, _authorizationToken, _authorizationMethod);
            return response;

        }
        


        

    }
}
