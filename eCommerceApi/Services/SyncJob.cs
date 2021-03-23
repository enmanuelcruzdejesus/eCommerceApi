using ApiCore;
using eCommerceApi.Helpers.Database;
using eCommerceApi.Model;
using Microsoft.Extensions.Logging;
using Quartz;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace eCommerceApi.Services
{
    [DisallowConcurrentExecution]
    public class SyncJob : IJob
    {
        RestAPI _restApi;
        WCObject _wc;
        SyncService _syncService;
   
        private readonly ILogger<SyncJob> _logger;

        public SyncJob(ILogger<SyncJob> logger)
        {
            _restApi = AppConfig.Instance().Service;
            _wc = new WCObject(_restApi);

            _syncService = new SyncService();
           
        }


        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(async () =>
            {

                await _syncService.Sync();
   

            });



        }
    }
}
