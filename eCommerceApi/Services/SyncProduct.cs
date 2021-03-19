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
            if (AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > _productLastUpdateSync).Count() > 0)
            {


                var insertedRecords = db.TransSyncLog.Get(t => t.TableName == "Products" && t.Operation == "Insert" && t.CreatedDate > _productLastUpdateSync).Select(x => x.TransId);
                var updatedRecords = db.TransSyncLog.Get(t => t.TableName == "Products" && t.Operation == "Update" && t.CreatedDate > _productLastUpdateSync).Select(x => x.TransId);


                //Getting sales orders and payments
                var insertedProducts = db.Products.GetAll().Where(o => Sql.In(o.id, insertedRecords));
                var updatedProducts = db.Products.GetAll().Where(o => Sql.In(o.id, updatedRecords));


                ProductBatch productBatch = new ProductBatch();

                if (insertedProducts.Count() > 0)
                {
                    var create = new List<Product>();
                    foreach (var item in insertedProducts)
                    {
                        var i = DatabaseHelper.GetEProduct(item);
                        create.Add(i);
                    }
                    productBatch.create = create;
                }

                if (updatedProducts.Count() > 0)
                {
                    var update = new List<Product>();
                    foreach (var item in updatedProducts)
                    {
                        var x = DatabaseHelper.GetEProduct(item);
                        update.Add(x);
                    }
                    productBatch.update = update;

                }

                var response = await _wc.Product.UpdateRange(productBatch);


                //updating sync table
                if (response.create != productBatch.create)
                {
                    db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Products");

                    //updating product reference
                    foreach (var i in response.create)
                    {
                        var p = insertedProducts.SingleOrDefault(pro => pro.sku == i.sku);
                        if (p != null)
                            db.Products.Update(new Products() { productRef = Convert.ToInt32(i.id) }, product => product.id == p.id);
                    }
                }


                if (response.update != productBatch.update)
                    db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Products");

            }


        }

    }
}
