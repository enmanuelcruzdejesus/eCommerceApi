using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCore;
using eCommerceApi.Helpers.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace eCommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        RestAPI _restApi;
        public CustomerController() { _restApi = AppConfig.Instance().Service;  }



        [HttpPost("download")]
        public async Task<IActionResult> Post()
        {
            try
            {

                WCObject wc = new WCObject(_restApi);

                var ecustomers = await wc.Customer.GetAll();

                var db = AppConfig.Instance().Db;



                //adapting data
                List<eCommerceApi.Model.Customers> customers = new List<Model.Customers>();
                foreach (var item in ecustomers)
                {
                    var c = DatabaseHelper.GetCustomerFromECustomer(item);
                    customers.Add(c);
                }


                db.Customers.BulkMerge(customers);

                return Ok(customers);


            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }
    }
}