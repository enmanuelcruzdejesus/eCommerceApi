using ApiCore;
using ApiCore.Services;
using eCommerce.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WCObject = WooCommerceNET.WooCommerce.v3.WCObject;
using WooCommerceNET.WooCommerce.v3;
using eCommerceApi.Helpers.Database;
using eCommerceApi.Helpers.eCommerce;

namespace eCommerceApi.Services
{
    public class SyncProductVariation
    {
        RestAPI _restApi;
        WCObject _wc;
        WooHelper _wch;



        IRepository<ProductVariations> _productVaritionsRepo;
        IRepository<TransactionSyncLog> _transLogRepo;
        IRepository<SyncTables> _syncRepo;
        IRepository<Products> _productRepo;

        DateTime _productVariationLastUpdateSync;
        DateTime _productLastUpdateSync;

        public SyncProductVariation(IRepository<ProductVariations> productVaritionsRepo, IRepository<Products> productRepo, IRepository<TransactionSyncLog> transLogRepo, IRepository<SyncTables> syncRepo, RestAPI restAPI)
        {
            _restApi = restAPI;
            _wc = new WCObject(_restApi);
            _productVaritionsRepo = productVaritionsRepo;
            _transLogRepo = transLogRepo;
            _syncRepo = syncRepo;
            _productRepo = productRepo;

            _wch = new WooHelper(_restApi,_productRepo);

        }

        public async Task<long> Sync()
        {
            var watch = new System.Diagnostics.Stopwatch();

            watch.Start();
            var db = AppConfig.Instance().Db;


            _productVariationLastUpdateSync = db.GetLastUpdateDate(100, "ProductVariations");



        
            var productVariationTransactions = db.TransSyncLog.Get(t => t.TableName == "ProductVariations" && t.CreatedDate > _productVariationLastUpdateSync && t.IsSynchronized == false).ToList();
            var syncProductVariationsRecords = productVariationTransactions.Count();


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


            watch.Stop();

            return watch.ElapsedMilliseconds;


        }

    }
}
