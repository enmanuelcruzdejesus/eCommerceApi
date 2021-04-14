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
    public class SyncCustomer
    {
        RestAPI _restApi;
        WCObject _wc;

        DateTime _customerLastUpdateSync;
        IRepository<Customers> _customerRepo;
        IRepository<TransactionSyncLog> _transLogRepo;
        IRepository<SyncTables> _syncRepo;


        public SyncCustomer(IRepository<Customers> customerRepo, IRepository<TransactionSyncLog> transLogRepo, IRepository<SyncTables> syncRepo, RestAPI restAPI)
        {
            _restApi = restAPI;
            _wc = new WCObject(_restApi);
            _customerRepo = customerRepo;
            _transLogRepo = transLogRepo;
            _syncRepo = syncRepo;
        }

        public async Task<long> Sync()
        {
            var watch = new System.Diagnostics.Stopwatch();

            watch.Start();

            var db = AppConfig.Instance().Db;
            _customerLastUpdateSync = db.GetLastUpdateDate(100, "Customers");
            //getting the changes
            var syncRecords = _transLogRepo.Get(x => x.TableName =="Customers" &&  x.CreatedDate > _customerLastUpdateSync && x.IsSynchronized == false ).Count();
            var transactions = _transLogRepo.Get(t => t.TableName == "Customers" &&  t.CreatedDate > _customerLastUpdateSync && t.IsSynchronized == false).ToList();


            if (syncRecords > 0)
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
                            var p = insertedCustomers.SingleOrDefault(pro => pro.user_name == item.username);
                            if (p != null)
                                _customerRepo.Update(new Customers() { customerRef = item.id.ToString() }, c => c.id == p.id);
                        }
                    }

                 


                }


                //updating last sync
                _syncRepo.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Customers");



                var t = (from x in _transLogRepo.Get(x => x.CreatedDate > _customerLastUpdateSync)
                         join x2 in transactions on x.TransId equals x2.TransId
                         select x).ToList();

                t.ForEach(t => { t.IsSynchronized = true; });
                DatabaseHelper.TransactionSyncLogBulkMerge(AppConfig.Instance().ConnectionString, t);


                watch.Stop();

                return watch.ElapsedMilliseconds;


            }

            return 0;
        }


        private async Task<BatchObject<Customer>> CommitingData(List<Customer> i, List<Customer> u)
        {
            CustomerBatch batch = new CustomerBatch();

            batch.create = i;
            batch.update = u;


            var response = await _wc.Customer.UpdateRange(batch);

            return response;

        }


    }
}
