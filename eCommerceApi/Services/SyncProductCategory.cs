using ApiCore;
using ApiCore.Services;
using eCommerce.Model.Entities;
using eCommerceApi.Helpers.Database;
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

        IRepository<ProductCategories> _categoryRepo;
        IRepository<TransactionSyncLog> _transLogRepo;
        IRepository<SyncTables> _syncRepo;

        public SyncProductCategory(IRepository<ProductCategories> categoryRepo, IRepository<TransactionSyncLog> transLogRepo, IRepository<SyncTables> syncRepo, RestAPI restAPI)
        {
            _restApi = restAPI;
            _wc = new WCObject(_restApi);

            _categoryRepo = categoryRepo;
            _transLogRepo = transLogRepo;
            _syncRepo = syncRepo;


        }


        public async Task<long> Sync()
        {
            var watch = new System.Diagnostics.Stopwatch();

            watch.Start();

            var db = AppConfig.Instance().Db;
            _categoryLastUpdateSync = db.GetLastUpdateDate(100, "ProductCategories");

            //getting the changes
            var syncRecords = _transLogRepo.Get(x => x.CreatedDate > _categoryLastUpdateSync && x.IsSynchronized == false).Count();
            var transactions = _transLogRepo.Get(t => t.CreatedDate > _categoryLastUpdateSync && t.IsSynchronized == false).ToList();



            if (syncRecords > 0)
            {
                var transInserted = _transLogRepo.Get(t => t.TableName == "ProductCategories" && t.Operation == "Insert" && t.CreatedDate > _categoryLastUpdateSync && t.IsSynchronized == false).ToList();
                var transUpdated =  _transLogRepo.Get(t => t.TableName == "ProductCategories" && t.Operation == "Update" && t.CreatedDate > _categoryLastUpdateSync && t.IsSynchronized == false).ToList();

             
                var insertedCategories = from i in _categoryRepo.GetAll()
                                       join inserted in transInserted on i.id equals inserted.TransId
                                       select i;

                var updatedCategories = from i in _categoryRepo.GetAll()
                                      join updated in transUpdated on i.id equals updated.TransId
                                      select i;




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
                         join x2 in transactions on x.TransId equals x2.TransId
                         select x).ToList();

                t.ForEach(t => { t.IsSynchronized = true; });
                DatabaseHelper.TransactionSyncLogBulkMerge(AppConfig.Instance().ConnectionString, t);




                watch.Stop();

                return watch.ElapsedMilliseconds;

                


            }

            return 0;


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
