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

            if (AppConfig.Instance().Db.TransSyncLog.Get(x => x.CreatedDate > _categoryLastUpdateSync).Count() > 0)
            {
                var insertedRecords = db.TransSyncLog.Get(t => t.TableName == "ProductCategories" && t.Operation == "Insert" && t.CreatedDate > _categoryLastUpdateSync).Select(x => x.TransId);
                var updatedRecords = db.TransSyncLog.Get(t => t.TableName == "ProductCategories" && t.Operation == "Update" && t.CreatedDate > _categoryLastUpdateSync).Select(x => x.TransId);


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
                        {
                            db.ProductCategories.Update(new ProductCategories() { categoryRef = Convert.ToInt32(i.id) }, c => c.id == category.id);


                        }




                    }
                }

                if (response.update != categoryBatch.update)
                    //updating sync table
                    db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "ProductCategories");





            }


        }


    }
}
