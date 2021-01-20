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
    public class eCategoryController : ControllerBase
    {
        RestAPI _restApi;
        private readonly ILogger<eCategoryController> _logger;

        public eCategoryController(ILogger<eCategoryController> logger)
        {
            _logger = logger;
            _restApi = AppConfig.Instance().Service;
        }



        [HttpGet("GetAll")]
        public async Task<IActionResult> Get()
        {

            try
            {
                WCObject wc = new WCObject(_restApi);

                //Get all products
                var categories = await wc.Category.GetAll();

                return Ok(categories);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }

        }

        [HttpPost("Create")]
        public async Task<IActionResult> Post(ProductCategory  productCategory)
        {

            try
            {
                WCObject wc = new WCObject(_restApi);

                //Get all products
                var result = await wc.Category.Add(productCategory);

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