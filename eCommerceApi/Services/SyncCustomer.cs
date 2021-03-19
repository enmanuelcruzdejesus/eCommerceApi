using ApiCore;
using eCommerceApi.Helpers.Database;
using eCommerceApi.Model;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
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
            if (AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > _customerLastUpdateSync).Count() > 0)
            {
                var insertedRecords = db.TransSyncLog.Get(t => t.TableName == "Customers" && t.Operation == "Insert" && t.CreatedDate > _customerLastUpdateSync).Select(x => x.TransId);
                var updatedRecords = db.TransSyncLog.Get(t => t.TableName == "Customers" && t.Operation == "Update" && t.CreatedDate > _customerLastUpdateSync).Select(x => x.TransId);


                //Getting sales orders and payments
                var insertedCustomers = db.Customers.GetAll().Where(o => Sql.In(o.id, insertedRecords));
                var updatedCustomers = db.Customers.GetAll().Where(o => Sql.In(o.id, updatedRecords));


                CustomerBatch customerBatch = new CustomerBatch();

                if (insertedCustomers.Count() > 0)
                {
                    var create = new List<Customer>();
                    foreach (var item in insertedCustomers)
                    {
                        var i = DatabaseHelper.GetECustomer(item);
                        create.Add(i);
                    }
                    customerBatch.create = create;
                }

                if (updatedCustomers.Count() > 0)
                {
                    var update = new List<Customer>();
                    foreach (var item in updatedCustomers)
                    {
                        var x = DatabaseHelper.GetECustomer(item);
                        update.Add(x);
                    }
                    customerBatch.update = update;

                }

                var response = await _wc.Customer.UpdateRange(customerBatch);
                if (response.create != customerBatch.create)
                {
                    //updating sync table
                    db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Customers");

                    foreach (var i in response.create)
                    {

                        var cust = insertedCustomers.SingleOrDefault(customer => customer.user_name == i.username);
                        if (cust != null)
                            db.Customers.Update(new Customers() { customerRef = Convert.ToString(i.id) }, c => c.id == cust.id);


                    }


                }

                if (response.update != customerBatch.update)
                    db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Customers");




            }
        }

    }
}
