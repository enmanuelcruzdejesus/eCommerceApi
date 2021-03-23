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
    public class SyncCustomer
    {
        RestAPI _restApi;
        WCObject _wc;

        DateTime _customerLastUpdateSync;

        public SyncCustomer(RestAPI restAPI)
        {
            _restApi = restAPI;
            _wc = new WCObject(_restApi);
        }

        public async Task Sync()
        {
            var db = AppConfig.Instance().Db;
            _customerLastUpdateSync = db.GetLastUpdateDate(100, "Customers");
            //getting the changes
            var syncRecords = AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > _customerLastUpdateSync && x.IsSynchronized == false).Count();
            var transIds = db.TransSyncLog.Get(t => t.CreatedDate > _customerLastUpdateSync && t.IsSynchronized == false).Select(t => t.TransId).ToList();


            if (syncRecords > 0)
            {

                var transInserted = db.TransSyncLog.Get(t => t.TableName == "Customers" && t.Operation == "Insert" && t.CreatedDate > _customerLastUpdateSync && t.IsSynchronized == false).ToList();
                var transUpdated = db.TransSyncLog.Get(t => t.TableName == "Customers" && t.Operation == "Update" && t.CreatedDate > _customerLastUpdateSync && t.IsSynchronized == false).ToList();

                var insertedIds = transInserted.Select(x => x.TransId).ToList();
                var updatedIds = transUpdated.Select(x => x.TransId).ToList();


                //Getting sales orders and payments
                var insertedCustomers = db.Customers.GetAll().Where(o => Sql.In(o.id, insertedIds));
                var updatedCustomers = db.Customers.GetAll().Where(o => Sql.In(o.id, updatedIds));


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

                    //updating reference
                    foreach (var item in r.create)
                    {
                        var p = insertedCustomers.SingleOrDefault(pro => pro.user_name == item.username);
                        if (p != null)
                            db.Customers.Update(new Customers() { customerRef = item.id.ToString() }, c => c.id == p.id);
                    }


                }


                //updating last sync
                db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Customers");


                var transLog = AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > _customerLastUpdateSync && Sql.In(x.TransId, transIds)).ToList();
                transLog.ForEach(t => { t.IsSynchronized = true; });
                DatabaseHelper.TransactionSyncLogBulkMerge(AppConfig.Instance().ConnectionString, transLog);


                //var response = await _wc.Customer.UpdateRange(customerBatch);
                //if (response.create != customerBatch.create)
                //{
                //    //updating sync table
                //    db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Customers");

                //    foreach (var i in response.create)
                //    {

                //        var cust = insertedCustomers.SingleOrDefault(customer => customer.user_name == i.username);
                //        if (cust != null)
                //            db.Customers.Update(new Customers() { customerRef = Convert.ToString(i.id) }, c => c.id == cust.id);


                //    }


                //}

                //if (response.update != customerBatch.update)
                //    db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Customers");




            }
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
