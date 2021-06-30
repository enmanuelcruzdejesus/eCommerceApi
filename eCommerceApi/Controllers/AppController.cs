using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiCore;
using ApiCore.Services;
using eCommerceApi.Helpers.Database;
using eCommerce.Model;
using eCommerceApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using eCommerce.Model.Entities;

namespace eCommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        RestAPI _restApi;
        WCObject _wc;
        SyncService _syncService;
       
        IRepository<Customers> _customerRepo;
        IRepository<ProductCategories> _categoryRepo;
        IRepository<Products> _productRepo;
        IRepository<ProductVariations> _productVaritionsRepo;
        IRepository<Orders> _orderRepo;
        IRepository<TransactionSyncLog> _transLogRepo;
        IRepository<SyncTables> _syncRepo;

        private readonly ILogger<AppController> _logger;

        public AppController(IRepository<Customers> customerRepo,
                           IRepository<ProductCategories> categoryRepo,
                           IRepository<Products> productRepo,
                           IRepository<ProductVariations> productVarRepo,

                           IRepository<Orders> orderRepo,
                           IRepository<TransactionSyncLog> transLogRepo,
                           IRepository<SyncTables> syncRepo, ILogger<AppController> logger) 
        {
            _logger = logger;
            _restApi = AppConfig.Instance().Service;
            _customerRepo = customerRepo;
            _categoryRepo = categoryRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _transLogRepo = transLogRepo;
            _syncRepo = syncRepo;
            _productVaritionsRepo = productVarRepo;


            _syncService = new SyncService(_customerRepo,categoryRepo,productRepo, _productVaritionsRepo, orderRepo,transLogRepo,syncRepo);

          
        }




        [HttpPost("webhooks/order")]
        public async Task<IActionResult> Post()
        {
            try
            {
                 _logger.LogInformation("order webhook");

                //Get webhooks response payload
                string jsonData = null;
                object hmacHeaderSignature = null;

                HttpRequestRewindExtensions.EnableBuffering(this.Request);

                HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(Request.Body))
                {
                    jsonData = await reader.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        var eOrder = JsonConvert.DeserializeObject<Order>(jsonData);
                        if (eOrder != null)
                        {
                            var order = DatabaseHelper.GetOrderFromEOrder(eOrder);

                            var db = AppConfig.Instance().Db;
                            var list = new List<Orders>();
                            list.Add(order);
                            //  db.Orders.BulkMerge(list);
                            DatabaseHelper.OrderBulkMerge(AppConfig.Instance().ConnectionString, list);

                            var idorder = DatabaseHelper.GetOrderByRef(order.orderRef).id;
                            var detail = order.Detail;
                            foreach (var item in detail)
                            {
                                item.orderId = idorder;
                            }
                            //  db.OrderDetails.BulkMerge(detail);
                            DatabaseHelper.OrderDetailBulkMerge(AppConfig.Instance().ConnectionString, detail);



                        }

                    }

    
                    // Do something


                }

                return Ok(jsonData);



        
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.ToString());
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
                await  _syncService.Sync();
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