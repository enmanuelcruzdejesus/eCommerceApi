using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCore;
using ApiCore.Services;
using eCommerceApi.Helpers.Database;
using eCommerceApi.Model;
using eCommerceApi.Services;
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
        SyncCustomer _sync;

        private readonly ILogger<CustomerController> _logger;
        IRepository<Customers> _repository;

        public CustomerController(IRepository<Customers> repository,ILogger<CustomerController> logger) {
            _logger = logger;
            _restApi = AppConfig.Instance().Service;
            //_sync = new SyncCustomer(_restApi);
            _repository = repository;


        }

        [HttpGet("getallwithrepo")]
        public IActionResult GetWithRepo()
        {
            try
            {


                var customers = _repository.GetAll();
                return Ok(customers);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return StatusCode(500, ex);

            }
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
                DatabaseHelper.CustomersBulkMerge(AppConfig.Instance().ConnectionString, customers);


                return Ok(customers);


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