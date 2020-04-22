using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiCore;
using eCommerceApi.Helpers.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;


namespace eCommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        RestAPI _restApi;

        public AppController() { _restApi = AppConfig.Instance().Service; }


        [HttpPost("webhooks/order")]
        public async Task<IActionResult> Post()
        {
            try
            {

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
                            var list = new List<eCommerceApi.Model.Orders>();
                            list.Add(order);
                            db.Orders.BulkMerge(list);
                            var idorder = DatabaseHelper.GetOrderByRef(order.orderRef).id;
                            var detail = order.Detail;
                            foreach (var item in detail)
                            {
                                item.orderId = idorder;
                            }
                            db.OrderDetails.BulkMerge(detail);


                        }

                    }

                  

                    // Do something


                }

                return Ok(jsonData);



        
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }
    }
}