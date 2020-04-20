using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace eCommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class eOrderController : ControllerBase
    {
        RestAPI _restApi;

       public eOrderController()
        {
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

                return StatusCode(500, ex);
            }

        }

        [HttpPost("Create")]
        public async Task<IActionResult> Post(Order order)
        {
            WCObject wc = new WCObject(_restApi);

           
            var result = await wc.Order.Add(order);

            return Ok(result);
        }






    }
}