using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.Base;
using WCObject = WooCommerceNET.WooCommerce.v3.WCObject;
using WooCommerceNET.WooCommerce.v3;
using ApiCore;
using eCommerceApi.Helpers.Database;

using ApiCore.Services;
using eCommerce.Model.Entities;

namespace eCommerceApi.Services
{
    public class SyncOrder
    {
        RestAPI _restApi;
        WCObject _wc;


        DateTime _syncLastUpdateSync;

        IRepository<Orders> _orderRepo;
        IRepository<TransactionSyncLog> _transLogRepo;
        IRepository<SyncTables> _syncRepo;


        public SyncOrder(IRepository<Orders> orderRepo,
                         IRepository<TransactionSyncLog> transLogRepo,
                         IRepository<SyncTables> syncRepo,
                         RestAPI restAPI)
        {
            _restApi = restAPI;
            _wc = new WCObject(_restApi);
            _orderRepo = orderRepo;
            _transLogRepo = transLogRepo;
            _syncRepo = syncRepo;
        }


        public async Task<long> Sync()
        {

            var watch = new System.Diagnostics.Stopwatch();

            watch.Start(); 

            var db = AppConfig.Instance().Db;
            _syncLastUpdateSync = db.GetLastUpdateDate(100, "Orders");

            //getting the changes
            var syncRecords = _transLogRepo.Get(x => x.CreatedDate > _syncLastUpdateSync && x.IsSynchronized == false).Count();
            var transactions = _transLogRepo.Get(t => t.CreatedDate > _syncLastUpdateSync && t.IsSynchronized == false).ToList();

            if (syncRecords > 0)
            {


                //var transInserted = db.TransSyncLog.Get(t => t.TableName == "Orders" && t.Operation == "Insert" && t.CreatedDate > _syncLastUpdateSync && t.IsSynchronized == false).ToList();
                var transUpdated = _transLogRepo.Get(t => t.TableName == "Orders" && t.Operation == "Update" && t.CreatedDate > _syncLastUpdateSync && t.IsSynchronized == false).ToList();



                //var insertedOrders = from i in db.Orders.GetAll()
                //                       join inserted in transInserted on i.id equals inserted.TransId
                //                       select i;

                var updatedOrders = from i in _orderRepo.GetAll()
                                      join updated in transUpdated on i.id equals updated.TransId
                                      select i;

                //ProductBatch productBatch = new ProductBatch();
                var create = new List<Order>();
                var update = new List<Order>();


                //if (insertedOrders.Count() > 0)
                //{

                //    foreach (var item in insertedOrders)
                //    {
                //        var i = DatabaseHelper.GetEOrderFromOrder(item);
                //        create.Add(i);
                //    }
                //    //productBatch.create = create;
                //}

                if (updatedOrders.Count() > 0)
                {

                    foreach (var item in updatedOrders)
                    {
                        var x = DatabaseHelper.GetEOrderFromOrder(item);
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

                    //if (create.Count > 100)
                    //    create.RemoveRange(0, 100);
                    //else
                    //    create.RemoveRange(0, create.Count);

                    if (update.Count > 100)
                        update.RemoveRange(0, 100);
                    else
                        update.RemoveRange(0, update.Count);


                    //if (create.Count() > 0)
                    //{
                    //    //updating reference
                    //    foreach (var item in r.create)
                    //    {
                    //        var p = insertedOrders.SingleOrDefault(pro => pro.sku == item.sku);
                    //        if (p != null)
                    //            db.Products.Update(new Products() { productRef = Convert.ToInt32(item.id) }, product => product.id == p.id);
                    //    }
                    //}



                }
                //updating last sync
                _syncRepo.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Orders");



                var t = (from x in  _transLogRepo.Get(x => x.CreatedDate > _syncLastUpdateSync)
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
        private async Task<BatchObject<Order>> CommitingData(List<Order> i, List<Order> u)
        {
            OrderBatch batch = new OrderBatch();

            batch.create = i;
            batch.update = u;


            var response = await _wc.Order.UpdateRange(batch);

            return response;

        }
    }
}
