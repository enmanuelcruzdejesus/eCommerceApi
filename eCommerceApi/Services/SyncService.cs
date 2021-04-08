using ApiCore;
using ApiCore.Services;
using eCommerceApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WCObject = WooCommerceNET.WooCommerce.v3.WCObject;
using WooCommerceNET.WooCommerce.v3;
using eCommerceApi.Helpers.Database;
using WooCommerceNET.Base;

namespace eCommerceApi.Services
{
    public class SyncService
    {
        RestAPI _restApi;
        WCObject _wc;

        SyncCustomer _syncCustomer;
        SyncProduct _syncProduct;
        SyncProductCategory _syncProductCategory;
        SyncOrder _syncOrder;


        IRepository<Customers> _customerRepo;
        IRepository<ProductCategories> _categoryRepo;
        IRepository<Products> _productRepo;
        IRepository<Orders> _orderRepo;
        IRepository<TransactionSyncLog> _transLogRepo;
        IRepository<SyncTables> _syncRepo;


        DateTime _customerLastUpdateSync;
        DateTime _categoryLastUpdateSync;
        DateTime _productLastUpdateSync;
        DateTime _orderLastUpdateSync;





        public SyncService(IRepository<Customers>  customerRepo,
                           IRepository<ProductCategories> categoryRepo,
                           IRepository<Products> productRepo,
                           IRepository<Orders> orderRepo,
                           IRepository<TransactionSyncLog>  transLogRepo,
                           IRepository<SyncTables> syncRepo )
        {

            _customerRepo = customerRepo;
            _categoryRepo = categoryRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _transLogRepo = transLogRepo;
            _syncRepo = syncRepo;

            _restApi = AppConfig.Instance().Service;

            _syncCustomer = new SyncCustomer(_customerRepo,_transLogRepo,_syncRepo,_restApi);
            _syncProductCategory = new SyncProductCategory(_categoryRepo, _transLogRepo, _syncRepo, _restApi);
            _syncProduct = new SyncProduct(_productRepo,_transLogRepo,_syncRepo,_restApi);
            _syncOrder = new SyncOrder(_orderRepo, _transLogRepo,_syncRepo,_restApi);

            _wc = new WCObject(_restApi);


        }


        public async Task Sync() 
        {
            //var task1 = _syncCustomer.Sync();
            //var task2 = _syncProductCategory.Sync();
            //var task3 = _syncProduct.Sync();
            //var task4 = _syncOrder.Sync();

            //await Task.WhenAll(task1, task2, task3, task4).ConfigureAwait(false);
            var db = AppConfig.Instance().Db;
            _customerLastUpdateSync = db.GetLastUpdateDate(100, "Customers");
            _categoryLastUpdateSync = db.GetLastUpdateDate(100, "ProductCategories");
            _productLastUpdateSync = db.GetLastUpdateDate(100, "Products");
            _orderLastUpdateSync = db.GetLastUpdateDate(100, "Orders"); 

          
            var customerTransactions = _transLogRepo.Get(t => t.TableName == "Customers" && t.CreatedDate > _customerLastUpdateSync && t.IsSynchronized == false).ToList();
            var syncCustomerRecords = customerTransactions.Count();

            var categoryTransactions = _transLogRepo.Get(t => t.TableName == "ProductCategories" && t.CreatedDate > _categoryLastUpdateSync && t.IsSynchronized == false).ToList();
            var syncCategoryRecords = categoryTransactions.Count();

            var productTransactions = _transLogRepo.Get(t => t.TableName == "Products" && t.CreatedDate > _productLastUpdateSync && t.IsSynchronized == false).ToList();
            var syncProductRecords = productTransactions.Count();


            var orderTransactions = _transLogRepo.Get(t => t.TableName == "Orders" && t.CreatedDate > _orderLastUpdateSync && t.IsSynchronized == false).ToList();
            var syncOrderRecords = orderTransactions.Count();

            if (syncCustomerRecords > 0)
            {

                var transInserted = _transLogRepo.Get(t => t.TableName == "Customers" && t.Operation == "Insert" && t.CreatedDate > _customerLastUpdateSync && t.IsSynchronized == false).ToList();
                var transUpdated = _transLogRepo.Get(t => t.TableName == "Customers" && t.Operation == "Update" && t.CreatedDate > _customerLastUpdateSync && t.IsSynchronized == false).ToList();

                var insertedIds = transInserted.Select(x => x.TransId).ToList();
                var updatedIds = transUpdated.Select(x => x.TransId).ToList();


                //Getting sales orders and payments

                var insertedCustomers = from i in _customerRepo.GetAll()
                                        join inserted in transInserted on i.id equals inserted.TransId
                                        select i;

                var updatedCustomers = from i in _customerRepo.GetAll()
                                       join updated in transUpdated on i.id equals updated.TransId
                                       select i;


                var create = new List<Customer>();
                var update = new List<Customer>();


                if (insertedCustomers.Count() > 0)
                {

                    foreach (var item in insertedCustomers)
                    {
                        var i = DatabaseHelper.GetECustomer(item);
                        create.Add(i);
                    }

                }

                if (updatedCustomers.Count() > 0)
                {

                    foreach (var item in updatedCustomers)
                    {
                        var x = DatabaseHelper.GetECustomer(item);
                        update.Add(x);
                    }


                }

                //commiting changes to server
                while (create.Count() > 0 || update.Count() > 0)
                {

                    var i = create.Take(100).ToList();
                    var u = update.Take(100).ToList();
                    var r = await SentCustomerData(i, u);

                    if (create.Count > 100)
                        create.RemoveRange(0, 100);
                    else
                        create.RemoveRange(0, create.Count);

                    if (update.Count > 100)
                        update.RemoveRange(0, 100);
                    else
                        update.RemoveRange(0, update.Count);


                    if (insertedCustomers.Count() > 0)
                    {
                        //updating reference
                        foreach (var item in r.create)
                        {
                            var p = insertedCustomers.SingleOrDefault(pro => pro.user_name == item.username);
                            if (p != null)
                                _customerRepo.Update(new Customers() { customerRef = item.id.ToString() }, c => c.id == p.id);
                        }
                    }




                }


                //updating last sync
                _syncRepo.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Customers");



                var t = (from x in _transLogRepo.Get(x => x.CreatedDate > _customerLastUpdateSync)
                         join x2 in customerTransactions on x.TransId equals x2.TransId
                         select x).ToList();

                t.ForEach(t => { t.IsSynchronized = true; });
                DatabaseHelper.TransactionSyncLogBulkMerge(AppConfig.Instance().ConnectionString, t);


               


            }



            if (syncCategoryRecords > 0)
            {

                var transInserted = _transLogRepo.Get(t => t.TableName == "ProductCategories" && t.Operation == "Insert" && t.CreatedDate > _categoryLastUpdateSync && t.IsSynchronized == false).ToList();
                var transUpdated = _transLogRepo.Get(t => t.TableName == "ProductCategories" && t.Operation == "Update" && t.CreatedDate > _categoryLastUpdateSync && t.IsSynchronized == false).ToList();

                var insertedIds = transInserted.Select(x => x.TransId).ToList();
                var updatedIds = transUpdated.Select(x => x.TransId).ToList();


                //Getting sales orders and payments

                var insertedCategories = from i in _categoryRepo.GetAll()
                                        join inserted in transInserted on i.id equals inserted.TransId
                                        select i;

                var updatedCategories = from i in _categoryRepo.GetAll()
                                       join updated in transUpdated on i.id equals updated.TransId
                                       select i;


                var create = new List<ProductCategory>();
                var update = new List<ProductCategory>();


                if (insertedCategories.Count() > 0)
                {

                    foreach (var item in insertedCategories)
                    {
                        var i = DatabaseHelper.GetECategory(item);
                        create.Add(i);
                    }

                }

                if (updatedCategories.Count() > 0)
                {

                    foreach (var item in updatedCategories)
                    {
                        var x = DatabaseHelper.GetECategory(item);
                        update.Add(x);
                    }


                }

                //commiting changes to server
                while (create.Count() > 0 || update.Count() > 0)
                {

                    var i = create.Take(100).ToList();
                    var u = update.Take(100).ToList();
                    var r = await SentCategoryData(i, u);

                    if (create.Count > 100)
                        create.RemoveRange(0, 100);
                    else
                        create.RemoveRange(0, create.Count);

                    if (update.Count > 100)
                        update.RemoveRange(0, 100);
                    else
                        update.RemoveRange(0, update.Count);


                    if (create.Count() > 0)
                    {
                        //updating reference
                        foreach (var item in r.create)
                        {
                            var p = insertedCategories.SingleOrDefault(pro => pro.slug == item.slug);
                            if (p != null)
                                _categoryRepo.Update(new ProductCategories() { categoryRef = Convert.ToInt32(item.id) }, product => product.id == p.id);
                        }
                    }




                }


                //updating last sync
                _syncRepo.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "ProductCategories");



                var t = (from x in _transLogRepo.Get(x => x.CreatedDate > _categoryLastUpdateSync)
                         join x2 in categoryTransactions on x.TransId equals x2.TransId
                         select x).ToList();

                t.ForEach(t => { t.IsSynchronized = true; });
                DatabaseHelper.TransactionSyncLogBulkMerge(AppConfig.Instance().ConnectionString, t);





            }




            if (syncProductRecords > 0)
            {

                var transInserted = _transLogRepo.Get(t => t.TableName == "Products" && t.Operation == "Insert" && t.CreatedDate > _productLastUpdateSync && t.IsSynchronized == false).ToList();
                var transUpdated = _transLogRepo.Get(t => t.TableName == "Products" && t.Operation == "Update" && t.CreatedDate > _productLastUpdateSync && t.IsSynchronized == false).ToList();

                var insertedIds = transInserted.Select(x => x.TransId).ToList();
                var updatedIds = transUpdated.Select(x => x.TransId).ToList();


                //Getting sales orders and payments

                var insertedProducts = from i in _productRepo.GetAll()
                                         join inserted in transInserted on i.id equals inserted.TransId
                                         select i;

                var updatedProducts = from i in _productRepo.GetAll()
                                        join updated in transUpdated on i.id equals updated.TransId
                                        select i;


                var create = new List<Product>();
                var update = new List<Product>();


                if (insertedProducts.Count() > 0)
                {

                    foreach (var item in insertedProducts)
                    {
                        var i = DatabaseHelper.GetEProduct(item);
                        create.Add(i);
                    }

                }

                if (updatedProducts.Count() > 0)
                {

                    foreach (var item in updatedProducts)
                    {
                        var x = DatabaseHelper.GetEProduct(item);
                        update.Add(x);
                    }


                }

                //commiting changes to server
                while (create.Count() > 0 || update.Count() > 0)
                {

                    var i = create.Take(100).ToList();
                    var u = update.Take(100).ToList();
                    var r = await SentProductData(i, u);

                    if (create.Count > 100)
                        create.RemoveRange(0, 100);
                    else
                        create.RemoveRange(0, create.Count);

                    if (update.Count > 100)
                        update.RemoveRange(0, 100);
                    else
                        update.RemoveRange(0, update.Count);


                    if (insertedProducts.Count() > 0)
                    {
                        //updating reference
                        foreach (var item in r.create)
                        {
                            var p = insertedProducts.SingleOrDefault(pro => pro.sku == item.sku);
                            if (p != null)
                                db.Products.Update(new Products() { productRef = Convert.ToInt32(item.id) }, product => product.id == p.id);
                        }
                    }




                }


                //updating last sync
                _syncRepo.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Products");



                var t = (from x in _transLogRepo.Get(x => x.CreatedDate > _productLastUpdateSync)
                         join x2 in productTransactions on x.TransId equals x2.TransId
                         select x).ToList();

                t.ForEach(t => { t.IsSynchronized = true; });
                DatabaseHelper.TransactionSyncLogBulkMerge(AppConfig.Instance().ConnectionString, t);





            }



            if (syncOrderRecords > 0)
            {

                //var transInserted = _transLogRepo.Get(t => t.TableName == "Orders" && t.Operation == "Insert" && t.CreatedDate > _orderLastUpdateSync && t.IsSynchronized == false).ToList();
                var transUpdated = _transLogRepo.Get(t => t.TableName == "Orders" && t.Operation == "Update" && t.CreatedDate > _orderLastUpdateSync && t.IsSynchronized == false).ToList();

                //var insertedIds = transInserted.Select(x => x.TransId).ToList();
                var updatedIds = transUpdated.Select(x => x.TransId).ToList();


                //Getting sales orders and payments

                //var insertedOrders = from i in _orderRepo.GetAll()
                //                       join inserted in transInserted on i.id equals inserted.TransId
                //                       select i;

                var updatedOrders = from i in _orderRepo.GetAll()
                                      join updated in transUpdated on i.id equals updated.TransId
                                      select i;


                var create = new List<Order>();
                var update = new List<Order>();


                //if (insertedOrders.Count() > 0)
                //{

                //    foreach (var item in insertedOrders)
                //    {
                //        var i = DatabaseHelper.GetEOrderFromOrder(item);
                //        create.Add(i);
                //    }

                //}

                if (updatedOrders.Count() > 0)
                {

                    foreach (var item in updatedOrders)
                    {
                        var x = DatabaseHelper.GetEOrderFromOrder(item);
                        update.Add(x);
                    }


                }

                //commiting changes to server
                while (create.Count() > 0 || update.Count() > 0)
                {

                    var i = create.Take(100).ToList();
                    var u = update.Take(100).ToList();
                    var r = await SentOrderData(i, u);

                    if (create.Count > 100)
                        create.RemoveRange(0, 100);
                    else
                        create.RemoveRange(0, create.Count);

                    if (update.Count > 100)
                        update.RemoveRange(0, 100);
                    else
                        update.RemoveRange(0, update.Count);


                    //if (insertedOrders.Count() > 0)
                    //{
                    //    //updating reference
                    //    foreach (var item in r.create)
                    //    {
                    //        var p = insertedOrders.SingleOrDefault(pro => pro.orderRef == item.id);
                    //        if (p != null)
                    //            db.Orders.Update(new Orders() { orderRef = Convert.ToInt32(item.id) }, product => product.id == p.id);
                    //    }
                    //}




                }


                //updating last sync
                _syncRepo.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Orders");



                var t = (from x in _transLogRepo.Get(x => x.CreatedDate > _orderLastUpdateSync)
                         join x2 in orderTransactions on x.TransId equals x2.TransId
                         select x).ToList();

                t.ForEach(t => { t.IsSynchronized = true; });
                DatabaseHelper.TransactionSyncLogBulkMerge(AppConfig.Instance().ConnectionString, t);





            }

        }


        private async Task<BatchObject<Customer>> SentCustomerData(List<Customer> i, List<Customer> u)
        {
            CustomerBatch batch = new CustomerBatch();

            batch.create = i;
            batch.update = u;


            var response = await _wc.Customer.UpdateRange(batch);

            return response;

        }


        private async Task<BatchObject<ProductCategory>> SentCategoryData(List<ProductCategory> i, List<ProductCategory> u)
        {
            ProductCategoryBatch batch = new ProductCategoryBatch();

            batch.create = i;
            batch.update = u;


            var response = await _wc.Category.UpdateRange(batch);

            return response;

        }


        private async Task<BatchObject<Product>> SentProductData(List<Product> i, List<Product> u)
        {
            ProductBatch batch = new ProductBatch();

            batch.create = i;
            batch.update = u;


            var response = await _wc.Product.UpdateRange(batch);

            return response;

        }


        private async Task<BatchObject<Order>> SentOrderData(List<Order> i, List<Order> u)
        {
            OrderBatch batch = new OrderBatch();

            batch.create = i;
            batch.update = u;


            var response = await _wc.Order.UpdateRange(batch);

            return response;

        }
    }
}
