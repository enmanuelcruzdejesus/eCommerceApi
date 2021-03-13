using ApiCore;
using eCommerceApi.Helpers.Database;
using eCommerceApi.Model;
using Quartz;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace eCommerceApi.Services
{
    [DisallowConcurrentExecution]
    public class SyncJob : IJob
    {
        RestAPI _restApi;
        WCObject _wc;

        public SyncJob()
        {
            _restApi = AppConfig.Instance().Service;
            _wc = new WCObject(_restApi);
        }


        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(async () =>
            {

                Console.WriteLine("WOOCOMMERCE JOB EXECUTING!!!");
                var db = AppConfig.Instance().Db;

                var customerLastUpdateSync = db.GetLastUpdateDate(100, "Customers");
                var categoryLastUpdateSync = db.GetLastUpdateDate(100, "ProductCategories");
                var subCategoriesLastUpdateSync = db.GetLastUpdateDate(100, "SubCategories");
                var productLastUpdateSync = db.GetLastUpdateDate(100, "Products");




                if (AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > customerLastUpdateSync).Count() > 0)
                {
                    var insertedRecords = db.TransSyncLog.Get(t => t.TableName == "Customers" && t.Operation == "Insert" && t.CreatedDate > customerLastUpdateSync).Select(x => x.TransId);
                    var updatedRecords = db.TransSyncLog.Get(t => t.TableName == "Customers" && t.Operation == "Update" && t.CreatedDate > customerLastUpdateSync).Select(x => x.TransId);


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
                                db.Customers.Update(new Customers() { customerRef = Convert.ToString(i.id)}, c => c.id == cust.id);


                        }


                    }

                    if(response.update != customerBatch.update)
                        db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Customers");




                }



                if (AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > categoryLastUpdateSync).Count() > 0)
                {
                    var insertedRecords = db.TransSyncLog.Get(t => t.TableName == "ProductCategories" && t.Operation == "Insert" && t.CreatedDate > categoryLastUpdateSync).Select(x => x.TransId);
                    var updatedRecords = db.TransSyncLog.Get(t => t.TableName == "ProductCategories" && t.Operation == "Update" && t.CreatedDate > categoryLastUpdateSync).Select(x => x.TransId);


                    //Getting sales orders and payments
                    var insertedCategories = db.ProductCategories.GetAll().Where(o => Sql.In(o.id, insertedRecords));
                    var updatedCategories = db.ProductCategories.GetAll().Where(o => Sql.In(o.id, updatedRecords));



                    ProductCategoryBatch categoryBatch = new ProductCategoryBatch();

                    if (insertedCategories.Count() > 0)
                    {
                        var create = new List<ProductCategory>();
                        foreach (var item in insertedCategories)
                        {
                            var i = DatabaseHelper.GetECategory(item);
                            create.Add(i);
                        }


                        categoryBatch.create = create;
                    }

                    if (updatedCategories.Count() > 0)
                    {
                        var update = new List<ProductCategory>();
                        foreach (var item in updatedCategories)
                        {
                            var x = DatabaseHelper.GetECategory(item);
                            update.Add(x);
                        }
                        categoryBatch.update = update;

                    }

                    var response = await _wc.Category.UpdateRange(categoryBatch);
                    if (response.create != categoryBatch.create)
                    {
                        //updating sync table
                        db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "ProductCategories");

                        //updating productRef

                        foreach (var i in response.create)
                        {

                            var category = insertedCategories.SingleOrDefault(cat => cat.descrip == i.description);
                            if (category != null)
                                db.ProductCategories.Update(new ProductCategories() { categoryRef = Convert.ToInt32(i.id) }, c => c.id == category.id);


                        }
                    }

                    if(response.update!= categoryBatch.update)
                        //updating sync table
                        db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "ProductCategories");





                }


                if (AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > subCategoriesLastUpdateSync).Count() > 0)
                {
                    var insertedRecords = db.TransSyncLog.Get(t => t.TableName == "SubCategories" && t.Operation == "Insert" && t.CreatedDate > subCategoriesLastUpdateSync).Select(x => x.TransId);
                    var updatedRecords = db.TransSyncLog.Get(t => t.TableName == "SubCategories" && t.Operation == "Update" && t.CreatedDate > subCategoriesLastUpdateSync).Select(x => x.TransId);


                    //Getting sales orders and payments
                    var insertedSubCategories = db.SubCategories.GetAll().Where(o => Sql.In(o.id, insertedRecords));
                    var updatedSubCategories = db.SubCategories.GetAll().Where(o => Sql.In(o.id, updatedRecords));


                    ProductCategoryBatch categoryBatch = new ProductCategoryBatch();

                    if (insertedSubCategories.Count() > 0)
                    {
                        var create = new List<ProductCategory>();
                        foreach (var item in insertedSubCategories)
                        {
                            var i = DatabaseHelper.GetESubCategory(item);
                            create.Add(i);
                        }
                        categoryBatch.create = create;
                    }

                    if (updatedSubCategories.Count() > 0)
                    {
                        var update = new List<ProductCategory>();
                        foreach (var item in updatedSubCategories)
                        {
                            var x = DatabaseHelper.GetESubCategory(item);
                            update.Add(x);
                        }
                        categoryBatch.update = update;

                    }

                    var response = await _wc.Category.UpdateRange(categoryBatch);
                    if (response.create != categoryBatch.create)
                    {
                        //updating sync table
                        db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "SubCategories");

                        //updating productRef

                        foreach (var i in response.create)
                        {

                            var category = insertedSubCategories.SingleOrDefault(cat => cat.descrip == i.description);
                            if (category != null)
                                db.SubCategories.Update(new SubCategories() { categoryRef = Convert.ToInt32(i.id) }, c => c.id == category.id);


                        }
                    }

                    if (response.update != categoryBatch.update)
                        //updating sync table
                        db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "SubCategories");





                }


                //PRODUCTOS
                if (AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > productLastUpdateSync).Count() > 0)
                {
                    var insertedRecords = db.TransSyncLog.Get(t => t.TableName == "Products" && t.Operation == "Insert" && t.CreatedDate > productLastUpdateSync).Select(x => x.TransId);
                    var updatedRecords = db.TransSyncLog.Get(t => t.TableName == "Products" && t.Operation == "Update" && t.CreatedDate > productLastUpdateSync).Select(x => x.TransId);


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

                        //updating productRef

                        foreach (var i in response.create)
                        {

                            var p = insertedProducts.SingleOrDefault(pro => pro.description == i.description);
                            if (p != null)
                                db.Products.Update(new Products() { productRef = Convert.ToInt32(i.id) }, product => product.id == p.id);


                        }
                    }


                    if (response.update != productBatch.update)
                        db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Products");

                }





            });



        }
    }
}
