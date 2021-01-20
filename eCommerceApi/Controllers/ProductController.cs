using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCore;
using eCommerceApi.Helpers.Database;
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
        private readonly ILogger<ProductController> _logger;

        public ProductController(ILogger<ProductController> logger) {
            _logger = logger;
            _restApi = AppConfig.Instance().Service;
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