
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace eCommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class eProductController : ControllerBase
    {
        RestAPI _restApi;
        private readonly ILogger<eProductController> _logger;
        public eProductController(ILogger<eProductController> logger)
        {
            _logger = logger;
            _restApi = AppConfig.Instance().Service;
        }


        [HttpGet("GetAll")]
        public async  Task<IActionResult> Get()
        {

            try
            {
                WCObject wc = new WCObject(_restApi);

                //Get all products
                var products = await wc.Product.GetAll();

                return Ok(products);

            }
            catch (Exception ex )
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
                WCObject wc = new WCObject(_restApi);

               
                var products = await wc.Product.Get(id);

                return Ok(products);

            }
            catch (Exception ex)
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
                WCObject wc = new WCObject(_restApi);

                product.images.Add( new ProductImage() { });


                //Get all products
                var result = await wc.Product.Add(product);

                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }

        }



        [HttpPut("Update")]
        public async Task<IActionResult> Update(Product product)
        {

            try
            {
                WCObject wc = new WCObject(_restApi);

                 

                var id = Convert.ToInt32(product.id);


                //Get all products
                var result = await wc.Product.Update(id,product);

                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }

        }








    }
}