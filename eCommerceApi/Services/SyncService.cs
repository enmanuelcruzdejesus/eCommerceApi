using ApiCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WCObject = WooCommerceNET.WooCommerce.v3.WCObject;


namespace eCommerceApi.Services
{
    public class SyncService
    {
        RestAPI _restApi;
        WCObject _wc;

        SyncCustomer _syncCustomer;
        SyncProduct _syncProduct;
        SyncProductCategory _syncProductCategory;


        public SyncService()
        {
            _restApi = AppConfig.Instance().Service;

            _syncCustomer = new SyncCustomer(_restApi);
            _syncProduct = new SyncProduct(_restApi);
            _syncProductCategory = new SyncProductCategory(_restApi);
        }


        public async Task Sync() 
        {
            var task1 =  _syncCustomer.Sync();
            var task2 =  _syncProductCategory.Sync();
            var task3 = _syncProduct.Sync();


            await Task.WhenAll(task1, task2, task3);

        }
    }
}
