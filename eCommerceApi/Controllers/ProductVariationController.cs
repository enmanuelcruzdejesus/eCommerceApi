using ApiCore;
using ApiCore.Services;
using eCommerce.Model.Entities;
using eCommerceApi.Helpers.Database;
using eCommerceApi.Helpers.eCommerce;
using eCommerceApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.Base;
using WooCommerceNET.WooCommerce.v3;

namespace eCommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVariationController : ControllerBase
    {
        RestAPI _restApi;
        SyncProductVariation _sync;
        IRepository<ProductVariations> _repo;
        IRepository<Products> _productRepo;

        private readonly ILogger<ProductVariationController> _logger;

        public ProductVariationController(ILogger<ProductVariationController> logger, IRepository<Products> productRepo ,IRepository<ProductVariations> _productVarRepo,IRepository<TransactionSyncLog> tranLog, IRepository<SyncTables> syncTable, RestAPI restAPI)
        {
            _logger = logger;
            _restApi = AppConfig.Instance().Service;
            _repo = _productVarRepo;
            _productRepo = productRepo;
            _sync = new SyncProductVariation(_repo,_productRepo, tranLog, syncTable, restAPI);



        }


        [HttpGet("GetAllAttributes")]
        public async Task<IActionResult> Get()
        {
            try
            {
                
                var r = await _restApi.GetRestful("products/attributes", null);

                return Ok(r);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }



        }


        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var p = new Dictionary<string, string>(){
                 {"product_id", id.ToString()}};
                var r = await _restApi.GetRestful("products/"+id.ToString()+"/variations",null);

                return Ok(r);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }




        }

        [HttpGet("GetVarId/{id}/{parent}")]
        public async Task<IActionResult> GetById(int id,int parent)
        {
            try
            {
                var r2 = await _restApi.GetRestful("products/" + id.ToString() + "/variations/"+parent, null);
                return Ok(r2);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }
        }


        [HttpPost("Create")]
        public async Task<IActionResult> Post(Product product)
        {

            try
            {

              
                var id = Convert.ToInt32(product.id);
                WCObject wc = new WCObject(_restApi);

                var a = await wc.Product.Variations.Add(new Variation()
                {
                    regular_price = 21,
                    on_sale = true,
                    purchasable = true,
                   

                    attributes = new List<VariationAttribute>()
                    {
                        new VariationAttribute()
                        {
                             id = 1,
                             name = "Color",
                             option = "Red",
 
                        }
                    },



                }, id, null);


                 

                var p = await wc.Product.Get(Convert.ToInt32(id));

                p.attributes.Add(new ProductAttributeLine()
                {
                    id = 1,
                    variation = true,
                    visible = true,
                    name = "Color",
                    options = new List<string>() { "Blue" ,"Red"}


            });

                p.variations.Add(Convert.ToInt32(a.id));

                var z = await wc.Product.Update(id,p);
             

                return Ok(z);


                
            

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }

        }

        [HttpPost("SyncCreate/{id}")]
        public async Task<IActionResult> PostSync(int id, List<Variation> v)
        {
           

            WooHelper wch = new WooHelper(_restApi,_productRepo);

            var r = await wch.VariationBatch(id, v,null);


            return Ok(r);
        }

        [HttpPost("Sync2")]
        public async Task<IActionResult> PostSync2()
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



        [HttpPut]
        public async Task<IActionResult> Update(Variation product)
        {
            WCObject wc = new WCObject(_restApi);

            //looking for his father
            var products = (await wc.Product.GetAll()).Where(p => p.type == "variable");

            var parent = Convert.ToInt32(product.id);


            var response = await wc.Product.Variations.Update(Convert.ToInt32(product.id), product, parent, null);

            return Ok(response);



        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            WCObject wc = new WCObject(_restApi);
            var list = new List<int>() {66253,
                                        66267,
                                        66269,
                                        66276,
                                        66282,
                                        66283,
                                        66284,
                                        66285,
                                        66286,
                                        66297,
                                        66319,
                                        66324 };


            var vars = new List<int>();
            List<Variation> vs = new List<Variation>();
            Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>();

            foreach (var item in list)
            {
               var p = await wc.Product.Get(item);
                vars.AddRange(p.variations);
                dic.Add(item, vars);

            }

            foreach (var item in dic)
            {
                foreach (var x in item.Value)
                {
                    var v = await  wc.Product.Variations.Get(x,item.Key);
                    vs.Add(v);
                }
            }

            var db = AppConfig.Instance().Db;
            var q = (from i in db.Products.GetAll()
                    join x in vs on i.sku equals x.sku
                    select i).ToList();


            foreach (var item in vs)
            {
                var p = q.FirstOrDefault(z => z.sku == item.sku);
                db.Products.Update(new Products() { productRef = Convert.ToInt32(item.id) }, product => product.id == p.id);
            }

            

            return Ok(q);
            

        }



    }
}
