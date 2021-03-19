﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCore;
using eCommerceApi.Helpers.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace eCommerceApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        RestAPI _restApi;

        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ILogger<CustomerController> logger) {
            _logger = logger;
            _restApi = AppConfig.Instance().Service;  
        
        }


        [HttpGet("getall")]
        public IActionResult Get() 
        {
            try
            {

                var db = AppConfig.Instance().Db;
                var customers = db.Customers.GetAll();
                return Ok(customers);

            }catch(Exception ex)
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

                var ecustomers = await wc.Customer.GetAll();

                var db = AppConfig.Instance().Db;



                //adapting data
                List<eCommerceApi.Model.Customers> customers = new List<Model.Customers>();
                foreach (var item in ecustomers)
                {
                    var c = DatabaseHelper.GetCustomerFromECustomer(item);
                    customers.Add(c);
                }


                //       db.Customers.BulkMerge(customers);
                DatabaseHelper.CustomerBulkMerge(AppConfig.Instance().ConnectionString, customers);


                return Ok(customers);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);
            }

        }



    }
}