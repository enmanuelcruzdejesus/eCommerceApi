using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCore;
using eCommerce.Model.Entities;
using eCommerceApi.DAL.Services;
using eCommerceApi.Helpers.Database;

using eCommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace eCommerceApi.Controllers
{
  
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        RestAPI _restApi;
        WCObject _wc;
        Database _db;
        SyncProductCategory _sync;


        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ILogger<CategoryController> logger) 
        {
            _logger = logger;
           
            _restApi = AppConfig.Instance().Service;
            _db = AppConfig.Instance().Db;

            _wc = new WCObject(_restApi);

            //_sync = new SyncProductCategory(_restApi);
        }


        [HttpPost("uploadAll")]
        public async Task<IActionResult> Upload()
        {

            try
            {
                var categories = _db.ProductCategories.Get(x => x.categoryRef == 0).Take(100);
                ProductCategoryBatch categoryBatch = new ProductCategoryBatch();
                if (categories.Count() > 0)
                {
                    var create = new List<ProductCategory>();
                    foreach (var item in categories)
                    {
                        var i = DatabaseHelper.GetECategory(item);
                        create.Add(i);
                    }
                    categoryBatch.create = create;
                    var response = await _wc.Category.UpdateRange(categoryBatch);

                    if (response.create != categoryBatch.create)
                    {
                        //updating sync table
                        _db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "ProductCategories");

                        //updating productRef
                        foreach (var i in response.create)
                        {

                            var category = categories.SingleOrDefault(cat => cat.descrip == i.description);
                         

                            if (category != null)
                                _db.ProductCategories.Update(new ProductCategories() { categoryRef = Convert.ToInt32(i.id) }, c => c.id == category.id);


                        }
                    }


                    return Ok("the data was uploaded!");

                }


                return NoContent();

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }

          
        }



        [HttpPost("download")]
        public async Task<IActionResult> Post()
        {
            try
            {

                WCObject wc = new WCObject(_restApi);

                //Get all products
                var ecategories = await wc.Category.GetAll();

                var db = AppConfig.Instance().Db;



                //adapting data
                List<ProductCategories> categories = new List<ProductCategories>();
                foreach (var item in ecategories)
                {
                    var cat = DatabaseHelper.GetCategoryFromEProductCategory(item);
                    categories.Add(cat);
                }


                //   db.ProductCategories.BulkMerge(categories);
                DatabaseHelper.ProductCategoriesBulkMerge(AppConfig.Instance().ConnectionString, categories);


                return Ok(categories);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }

        }



        [HttpPost("sync")]
        public async Task<IActionResult> Sync()
        {
            try
            {
                var watch = new System.Diagnostics.Stopwatch();

                watch.Start();
                await _sync.Sync();

                watch.Stop();
                return Ok("Execution Time " + watch.ElapsedMilliseconds.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }

        }

    }
}