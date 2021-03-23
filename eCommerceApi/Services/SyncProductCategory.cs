using ApiCore;
using eCommerceApi.Helpers.Database;
using eCommerceApi.Model;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.Base;
using WooCommerceNET.WooCommerce.v3;
using WCObject = WooCommerceNET.WooCommerce.v3.WCObject;


namespace eCommerceApi.Services
{
    public class SyncProductCategory
    {
        RestAPI _restApi;
        WCObject _wc;

        DateTime _categoryLastUpdateSync;

        public SyncProductCategory(RestAPI restAPI)
        {
            _restApi = restAPI;
            _wc = new WCObject(_restApi);
        }


        public async Task Sync()
        {
            var db = AppConfig.Instance().Db;
            _categoryLastUpdateSync = db.GetLastUpdateDate(100, "ProductCategories");

            //getting the changes
            var syncRecords = AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > _categoryLastUpdateSync && x.IsSynchronized == false).Count();
            var transIds = db.TransSyncLog.Get(t => t.CreatedDate > _categoryLastUpdateSync && t.IsSynchronized == false).Select(t => t.TransId).ToList();



            if (syncRecords > 0)
            {
                var transInserted = db.TransSyncLog.Get(t => t.TableName == "ProductCategories" && t.Operation == "Insert" && t.CreatedDate > _categoryLastUpdateSync && t.IsSynchronized == false).ToList();
                var transUpdated = db.TransSyncLog.Get(t => t.TableName == "ProductCategories" && t.Operation == "Update" && t.CreatedDate > _categoryLastUpdateSync && t.IsSynchronized == false).ToList();

                var insertedIds = transInserted.Select(x => x.TransId).ToList();
                var updatedIds = transUpdated.Select(x => x.TransId).ToList();

                //Getting sales orders and payments
                var insertedCategories = db.ProductCategories.GetAll().Where(o => Sql.In(o.id, insertedIds));
                var updatedCategories = db.ProductCategories.GetAll().Where(o => Sql.In(o.id, updatedIds));



                //ProductBatch productBatch = new ProductBatch();
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
                        var p = insertedCategories.SingleOrDefault(pro => pro.slug == item.slug);
                        if (p != null)
                            db.ProductCategories.Update(new ProductCategories() { categoryRef = Convert.ToInt32(item.id) }, product => product.id == p.id);
                    }


                }
                //updating last sync
                db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "ProductCategories");

                var transLog = AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate >  _categoryLastUpdateSync && Sql.In(x.TransId, transIds)).ToList();
                transLog.ForEach(t => { t.IsSynchronized = true; });
                DatabaseHelper.TransactionSyncLogBulkMerge(AppConfig.Instance().ConnectionString, transLog);



                //var response = await _wc.Category.UpdateRange(categoryBatch);
                //if (response.create != categoryBatch.create)
                //{
                //    //updating sync table
                //    db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "ProductCategories");

                //    //updating productRef

                //    foreach (var i in response.create)
                //    {

                //        var category = insertedCategories.SingleOrDefault(cat => cat.descrip == i.description);
                //        if (category != null)
                //        {
                //            db.ProductCategories.Update(new ProductCategories() { categoryRef = Convert.ToInt32(i.id) }, c => c.id == category.id);


                //        }




                //    }
                //}

                //if (response.update != categoryBatch.update)
                //    //updating sync table
                //    db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "ProductCategories");





            }


        }


        //this method will 
        private async Task<BatchObject<ProductCategory>> CommitingData(List<ProductCategory> i, List<ProductCategory> u)
        {
            ProductCategoryBatch batch = new ProductCategoryBatch();

            batch.create = i;
            batch.update = u;


            var response = await _wc.Category.UpdateRange(batch);

            return response;

        }



    }
}
