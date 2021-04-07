using ApiCore;
using eCommerceApi.DAL.Services;
using eCommerceApi.Helpers.Database;
using eCommerceApi.Model;
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

            //_sync = new SyncOrder(_restApi);

        }






        [HttpPost("downloadById/{id}")]
        public async Task<IActionResult> Post(int id)
        {
            try
            {

                WCObject wc = new WCObject(_restApi);


                //Get all products

                var order = await wc.Order.Get(id);


                if (order != null)
                {
                    var db = AppConfig.Instance().Db;

                    if(db.Orders.Get(o => o.orderRef == order.id).Count() > 0)
                    {
                        var o = DatabaseHelper.GetOrderFromEOrder(order);
                        DatabaseHelper.OrderBulkMerge(AppConfig.Instance().ConnectionString, new List<Model.Orders>() { o });

                    }


                    return Ok(order);
                }
                  
                else
                    return NoContent();

   

            }
            catch (Exception ex)
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

                var orders = await wc.Order.GetAll();


                if (orders != null)
                {
                    if(orders.Count()> 0)
                    {
                      
                        
                        //left join between ecommerce orders and orders db 

                        var query = orders.GroupJoin(AppConfig.Instance().Db.Orders.GetAll(), i => i.id, j => j.orderRef,
                            (i, j) => new { i, j }).SelectMany(o => o.j.DefaultIfEmpty(), (k, v) => new { k.i, v }).Where(p => p.v == null);

                        var leftOrders = query.Select(x => x.i).ToList();

                        var dbOrders = new List<Orders>();

                        //converting ecommerce order to db order
                        foreach (var item in leftOrders)
                        {

                            var i = DatabaseHelper.GetOrderFromEOrder(item);
                            dbOrders.Add(i);
                        }



                        DatabaseHelper.OrdersWithItemsBulkMerge(AppConfig.Instance().ConnectionString, dbOrders);

                      


                        return  Ok(leftOrders);
                    }
                }


                return NoContent();
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
