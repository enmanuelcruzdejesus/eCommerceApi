using ApiCore;
using eCommerceApi.Helpers.Database;
using eCommerceApi.Model;
using Microsoft.Extensions.Logging;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.Base;
using WooCommerceNET.WooCommerce.Legacy;
using WooCommerceNET.WooCommerce.v3;
using Product = WooCommerceNET.WooCommerce.v3.Product;
using WCObject = WooCommerceNET.WooCommerce.v3.WCObject;

namespace eCommerceApi.Services
{
    public class SyncProduct
    {

        RestAPI _restApi;
        WCObject _wc;

    
        DateTime _productLastUpdateSync;

        public SyncProduct(RestAPI  restAPI)
        {
            _restApi = restAPI;
            _wc = new WCObject(_restApi);
 
        }


        public async Task Sync()
        {
           
            var db = AppConfig.Instance().Db;
            _productLastUpdateSync = db.GetLastUpdateDate(100, "Products");

            //getting the changes
            var syncRecords = AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > _productLastUpdateSync && x.IsSynchronized == false).Count();
            var transactions = db.TransSyncLog.Get(t => t.CreatedDate > _productLastUpdateSync && t.IsSynchronized == false).ToList();

            if (syncRecords > 0)
            {
                

                var transInserted = db.TransSyncLog.Get(t => t.TableName == "Products" && t.Operation == "Insert" && t.CreatedDate > _productLastUpdateSync && t.IsSynchronized == false).ToList();
                var transUpdated = db.TransSyncLog.Get(t => t.TableName == "Products" && t.Operation == "Update" && t.CreatedDate > _productLastUpdateSync && t.IsSynchronized == false).ToList();

          

                var insertedProducts = from i in db.Products.GetAll()
                                       join inserted in transInserted on i.id equals inserted.TransId
                                       select i;

                var updatedProducts = from i in db.Products.GetAll()
                                       join updated in transUpdated on i.id equals updated.TransId
                                       select i;

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
                    //productBatch.create = create;
                }

                if (updatedProducts.Count() > 0)
                {
                   
                    foreach (var item in updatedProducts)
                    {
                        var x = DatabaseHelper.GetEProduct(item);
                        update.Add(x);
                    }
                    //productBatch.update = update;

                }



                //commiting changes to server
                while (create.Count() > 0 || update.Count() > 0)
                {
                    
                    var i = create.Take(100).ToList();
                    var u = update.Take(100).ToList();
                    var r = await CommitingData(i, u);

                    if (create.Count > 100)
                        create.RemoveRange(0, 100);
                    else
                        create.RemoveRange(0, create.Count);

                    if (update.Count > 100)
                        update.RemoveRange(0, 100);
                    else
                        update.RemoveRange(0, update.Count);

                    //updating reference
                    foreach (var item in r.create)
                    {
                        var p = insertedProducts.SingleOrDefault(pro => pro.sku == item.sku);
                        if (p != null)
                            db.Products.Update(new Products() { productRef = Convert.ToInt32(item.id) }, product => product.id == p.id);
                    }


                }
                //updating last sync
                db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Products");

       
           
                var t = (from x in AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > _productLastUpdateSync)
                         join x2 in transactions on x.TransId equals x2.TransId
                         select x ).ToList();

                t.ForEach(t => { t.IsSynchronized = true; });
                DatabaseHelper.TransactionSyncLogBulkMerge(AppConfig.Instance().ConnectionString, t);






            }


        }


        //this method will 
        private async Task<BatchObject<Product>> CommitingData(List<Product> iProducts, List<Product> uProducts)
        {
            ProductBatch productBatch = new ProductBatch();

            productBatch.create = iProducts;
            productBatch.update = uProducts;


            var response = await _wc.Product.UpdateRange(productBatch);

            return response;

        }
       

    }
}
