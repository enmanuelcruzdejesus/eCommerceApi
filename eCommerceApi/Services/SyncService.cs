using ApiCore;
using ApiCore.Services;
using eCommerce.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WCObject = WooCommerceNET.WooCommerce.v3.WCObject;
using WooCommerceNET.WooCommerce.v3;
using eCommerceApi.Helpers.Database;
using WooCommerceNET.Base;
using eCommerce.Model.Entities;
using eCommerceApi.Helpers.eCommerce;

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
        WooHelper _wch;



        IRepository<Customers> _customerRepo;
        IRepository<ProductCategories> _categoryRepo;
        IRepository<Products> _productRepo;
        IRepository<ProductVariations> _productVaritionsRepo;
        IRepository<Orders> _orderRepo;
        IRepository<TransactionSyncLog> _transLogRepo;
        IRepository<SyncTables> _syncRepo;



        DateTime _customerLastUpdateSync;
        DateTime _categoryLastUpdateSync;
        DateTime _productLastUpdateSync;
        DateTime _productVariationLastUpdateSync;
        DateTime _orderLastUpdateSync;





        public SyncService(IRepository<Customers>  customerRepo,
                           IRepository<ProductCategories> categoryRepo,
                           IRepository<Products> productRepo,
                           IRepository<ProductVariations> productVarRepo,
                           IRepository<Orders> orderRepo,
                           IRepository<TransactionSyncLog>  transLogRepo,
                           IRepository<SyncTables> syncRepo )
        {

            _customerRepo = customerRepo;
            _categoryRepo = categoryRepo;
            _productRepo = productRepo;
            _productVaritionsRepo = productVarRepo;
            _orderRepo = orderRepo;
            _transLogRepo = transLogRepo;
            _syncRepo = syncRepo;

            _restApi = AppConfig.Instance().Service;

            _syncCustomer = new SyncCustomer(_customerRepo,_transLogRepo,_syncRepo,_restApi);
            _syncProductCategory = new SyncProductCategory(_categoryRepo, _transLogRepo, _syncRepo, _restApi);
            _syncProduct = new SyncProduct(_productRepo,_transLogRepo,_syncRepo,_restApi);
            _syncOrder = new SyncOrder(_orderRepo, _transLogRepo,_syncRepo,_restApi);

            _wc = new WCObject(_restApi);

            _wch = new WooHelper(_restApi,_productRepo);

       


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
            _productVariationLastUpdateSync = db.GetLastUpdateDate(100, "ProductVariations");
            _orderLastUpdateSync = db.GetLastUpdateDate(100, "Orders"); 

          
            var customerTransactions = _transLogRepo.Get(t => t.TableName == "Customers" && t.CreatedDate > _customerLastUpdateSync && t.IsSynchronized == false).ToList();
            var syncCustomerRecords = customerTransactions.Count();

            var categoryTransactions = _transLogRepo.Get(t => t.TableName == "ProductCategories" && t.CreatedDate > _categoryLastUpdateSync && t.IsSynchronized == false).ToList();
            var syncCategoryRecords = categoryTransactions.Count();

            var productTransactions = _transLogRepo.Get(t => t.TableName == "Products" && t.CreatedDate > _productLastUpdateSync && t.IsSynchronized == false).ToList();
            var syncProductRecords = productTransactions.Count();

            var productVariationTransactions = _transLogRepo.Get(t => t.TableName == "ProductVariations" && t.CreatedDate > _productVariationLastUpdateSync && t.IsSynchronized == false).ToList();
            var syncProductVariationsRecords = productVariationTransactions.Count();

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

                var updatedIds = transUpdated.Select(x => x.TransId).ToList();


                var insertedProducts = from i in _productRepo.GetAll()
                                       join inserted in transInserted on i.id equals inserted.TransId
                                       where i.isChild == false
                                       select i;

                var updatedProducts = from i in _productRepo.GetAll()
                                      join updated in transUpdated on i.id equals updated.TransId
                                      where i.isChild == false
                                      select i;

                var updateProductVariations = (from i in _productRepo.GetAll()
                                               join updated in transUpdated on i.id equals updated.TransId
                                               where i.isChild == true
                                               select i).ToList();


                var parentUpdateVar = (from i in updateProductVariations
                                       join p in _productVaritionsRepo.GetAll() on i.id equals p.productidvariation
                                       select p).Select(x => x.productid).Distinct();

                var childUpdateVariations = new List<ProductVariations>();
                var listOfVars = new List<Variation>();
                var hashUpdate = new Dictionary<int, List<Variation>>();
                foreach (var item in parentUpdateVar)
                {
                    var childs = (from x in _productVaritionsRepo.GetAll()
                                  join j in updateProductVariations on x.productidvariation equals j.id
                                  where x.productid == item
                                  select x).ToList();



                    var pchild = (from j in db.Products.GetAll()
                                  join c in childs on j.id equals c.productidvariation
                                  select j).ToList();

                    foreach (var i in pchild)
                    {


                        listOfVars.Add(DatabaseHelper.GetEVariationFromProduct(i));

                    }


                    //looking for woocomerce product id
                    var wi = db.Products.GetById(item).productRef;

                    hashUpdate.Add(wi, listOfVars);
                }

                //ProductBatch productBatch = new ProductBatch();
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
                while (create.Count() > 0 || update.Count() > 0 || hashUpdate.Count() > 0)
                {

                    var i = create.Take(100).ToList();
                    var u = update.Take(100).ToList();
                    var uv = hashUpdate.Take(100).ToList();

                    var r = await SentProductData(i, u);

                    foreach (var item in uv)
                    {
                        var a = await _wch.VariationBatch(item.Key, null, item.Value);
                    }

                    //removing the first 100

                    if (create.Count > 100)
                        create.RemoveRange(0, 100);
                    else
                        create.RemoveRange(0, create.Count);

                    if (update.Count > 100)
                        update.RemoveRange(0, 100);
                    else
                        update.RemoveRange(0, update.Count);

                   

                    foreach (var item in uv)
                    {
                        hashUpdate.Remove(item.Key);
                    }


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


            if (syncProductVariationsRecords > 0)
            {

                var transInserted = db.TransSyncLog.Get(t => t.TableName == "ProductVariations" && t.Operation == "Insert" && t.CreatedDate > _productVariationLastUpdateSync && t.IsSynchronized == false).ToList();
                var transUpdated = db.TransSyncLog.Get(t => t.TableName == "ProductVariations" && t.Operation == "Update" && t.CreatedDate > _productVariationLastUpdateSync && t.IsSynchronized == false).ToList();



                var insertedProducts = from i in db.ProductVariations.GetAll()
                                       join inserted in transInserted on i.id equals inserted.TransId
                                       select i;

                var updatedProducts = from i in db.ProductVariations.GetAll()
                                      join updated in transUpdated on i.id equals updated.TransId
                                      select i;



                var create = new List<Variation>();
                var update = new List<Variation>();



                if (insertedProducts.Count() > 0)
                {

                    foreach (var item in insertedProducts)
                    {
                        var i = DatabaseHelper.GetEVariation(item);
                        create.Add(i);
                    }

                }

                if (updatedProducts.Count() > 0)
                {

                    foreach (var item in updatedProducts)
                    {
                        var x = DatabaseHelper.GetEVariation(item);
                        update.Add(x);
                    }

                }

                WCObject wc = new WCObject(_restApi);
                var parentInsert = insertedProducts.Select(j => j.productid).Distinct();
                var childInsertVariations = new List<ProductVariations>();
                var hashInsert = new Dictionary<int, List<Variation>>();


                foreach (var i in parentInsert)
                {
                    var childs = (from x in insertedProducts
                                  where x.productid == i
                                  select x).ToList();

                    var pchild = (from j in db.Products.GetAll()
                                  join c in childs on j.id equals c.productidvariation
                                  select j).ToList();

                    var childVar = (from z in create.ToList()
                                    join cv in pchild on z.sku equals cv.sku
                                    select z).ToList();


                    //looking for woocomerce product id
                    var wi = db.Products.GetById(i).productRef;

                    hashInsert.Add(wi, childVar);

                }


                var parentUpdate = updatedProducts.Select(j => j.productid).Distinct();
                var childUpdateVariations = new List<ProductVariations>();
                var hashUpdate = new Dictionary<int, List<Variation>>();
                foreach (var i in parentUpdate)
                {
                    var childs = (from x in updatedProducts
                                  where x.productid == i
                                  select x).ToList();

                    var pchild = (from j in db.Products.GetAll()
                                  join c in childs on j.id equals c.id
                                  select j).ToList();

                    var childVar = (from z in update.ToList()
                                    join cv in pchild on z.sku equals cv.sku
                                    select z).ToList();


                    //looking for woocomerce product id
                    var wi = db.Products.GetById(i).productRef;

                    hashUpdate.Add(wi, childVar);

                }



                var resultInsert = new List<Variation>();
                foreach (var item in hashInsert)
                {

                    var id = db.Products.Get(o => o.productRef == item.Key).FirstOrDefault().id;
                    var optionsC = db.ProductVariations.Get(o => o.productid == id).Select(x => x.color).ToList();
                    var r = await _wch.VariationBatch(item.Key, item.Value, null, optionsC);
                    resultInsert.AddRange(r);

                }

                foreach (var item in hashUpdate)
                {
                    var r = await _wch.VariationBatch(item.Key, item.Value, null);
                }



                //updating last sync of product variation
                db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "ProductVariations");

                var t = (from x in db.TransSyncLog.Get(x => x.CreatedDate > _productVariationLastUpdateSync)
                         join x2 in productVariationTransactions on x.TransId equals x2.TransId
                         select x).ToList();

                t.ForEach(t => { t.IsSynchronized = true; });
                DatabaseHelper.TransactionSyncLogBulkMerge(AppConfig.Instance().ConnectionString, t);





                //updating product ref
                foreach (var item in resultInsert)
                {
                    var p = db.Products.GetAll().FirstOrDefault(pr => pr.sku.Trim() == item.sku.Trim());
                    if (p != null)
                        db.Products.Update(new Products() { productRef = Convert.ToInt32(item.id) }, product => product.id == p.id);
                    else
                        Console.WriteLine("NULL POINTER EXCEPT");
                }


                //marking the updated produtc rows as synchronized
                var trans = db.TransSyncLog.Get(t => t.IsSynchronized == false).ToList();
                db.TransSyncLog.Update(new TransactionSyncLog() { IsSynchronized = true }, trans => trans.IsSynchronized == false);


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

        public async Task<BatchObject<Variation>> SendProductVariationData(int parent, List<Variation> i, List<Variation> u)
        {
            BatchObject<Variation> batch = new BatchObject<Variation>();
            batch.create = i;
            batch.update = u;

            var r = await _wc.Product.Variations.UpdateRange(parent,batch);


            return r;
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
