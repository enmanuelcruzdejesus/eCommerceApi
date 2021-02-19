using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCore;
using eCommerceApi.DAL.Services;
using eCommerceApi.Helpers.Database;
using eCommerceApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace eCommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        RestAPI _restApi;
        WCObject _wc;
        Database _db;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ILogger<ProductController> logger) {
            _logger = logger;
            _restApi = AppConfig.Instance().Service;
            _db = AppConfig.Instance().Db;

            _wc = new WCObject(_restApi);
        }

        [HttpPost("uploadAll")]
        public async Task<IActionResult> Upload()
        {
            try
            {

                var products = _db.Products.Get(x => x.productRef == 0).Take(100);
                ProductBatch batch = new ProductBatch();
                if (products.Count() > 0)
                {
                    var create = new List<Product>();
                    foreach (var item in products)
                    {
                        var i = DatabaseHelper.GetEProduct(item);
                        create.Add(i);
                    }
                    batch.create = create;
                    var response = await _wc.Product.UpdateRange(batch);

                    if (response.create != batch.create)
                    {
                        //updating sync table
                        _db.SyncTables.Update(new SyncTables() { UserId = 100, LastUpdateSync = DateTime.Now }, s => s.UserId == 100 && s.TableName == "Product");

                        //updating productRef
                        foreach (var i in response.create)
                        {

                            var p = products.SingleOrDefault(pro => pro.description == i.description);
                            if (p != null)
                                _db.Products.Update(new Products() { productRef = Convert.ToInt32(i.id) }, product => product.id == p.id);


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
                var eproducts = await wc.Product.GetAll();

                var db = AppConfig.Instance().Db;



                //adapting data
                List<eCommerceApi.Model.Products> products = new List<Model.Products>();
                foreach (var item in eproducts)
                {
                    var p = DatabaseHelper.GetProductFromEProduct(item);
                    products.Add(p);
                }


                //  db.Products.BulkMerge(products);
                DatabaseHelper.ProductsBulkMerge(AppConfig.Instance().ConnectionString, products);


                return Ok(products);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }

        }
    }
}