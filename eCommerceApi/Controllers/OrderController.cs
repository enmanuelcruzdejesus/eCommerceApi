using ApiCore;
using eCommerceApi.DAL.Services;
using eCommerceApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace eCommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        RestAPI _restApi;
        WCObject _wc;
        Database _db;
        private readonly ILogger<OrderController> _logger;
        SyncOrder _sync;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
            _restApi = AppConfig.Instance().Service;
            _db = AppConfig.Instance().Db;

            _wc = new WCObject(_restApi);

            _sync = new SyncOrder(_restApi);

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
