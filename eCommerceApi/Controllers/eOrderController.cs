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
    public class eOrderController : ControllerBase
    {
        RestAPI _restApi;
        private readonly ILogger<eOrderController> _logger;
        public eOrderController(ILogger<eOrderController> logger)
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
                var order = await wc.Order.GetAll();

                return Ok(order);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }

        }

        [HttpPost("Create")]
        public async Task<IActionResult> Post(Order order)
        {
            try
            {
                WCObject wc = new WCObject(_restApi);


                var result = await wc.Order.Add(order);

                return Ok(result);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }
          
        }






    }
}