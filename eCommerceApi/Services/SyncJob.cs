using ApiCore;
using ApiCore.Services;
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

        IRepository<Customers> _customerRepo;
        IRepository<ProductCategories> _categoryRepo;
        IRepository<Products> _productRepo;
        IRepository<Orders> _orderRepo;
        IRepository<TransactionSyncLog> _transLogRepo;
        IRepository<SyncTables> _syncRepo;

        private readonly ILogger<SyncJob> _logger;

        public SyncJob(IRepository<Customers> customerRepo,
                           IRepository<ProductCategories> categoryRepo,
                           IRepository<Products> productRepo,
                           IRepository<Orders> orderRepo,
                           IRepository<TransactionSyncLog> transLogRepo,
                           IRepository<SyncTables> syncRepo,ILogger<SyncJob> logger)
        {
            _restApi = AppConfig.Instance().Service;
            _wc = new WCObject(_restApi);

            _customerRepo = customerRepo;
            _categoryRepo = categoryRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _transLogRepo = transLogRepo;
            _syncRepo = syncRepo;



            _syncService = new SyncService(_customerRepo, categoryRepo, productRepo, orderRepo, transLogRepo, syncRepo);

        }


        public async  Task Execute(IJobExecutionContext context)
        {

            await _syncService.Sync();


        }
    }
}
