﻿using System;
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
    public class ProductController : ControllerBase
    {
        RestAPI _restApi;

        public ProductController() { _restApi = AppConfig.Instance().Service; }


        [HttpPost("download")]
        public async Task<IActionResult> Post()
        {
            try
            {

                WCObject wc = new WCObject(_restApi);

                //Get all products
                var eproducts = await wc.Product.GetAll();

                var db = AppConfig.Instance().Db;



                //adapting data
                List<eCommerceApi.Model.Products> products = new List<Model.Products>();
                foreach (var item in eproducts)
                {
                    var p = DatabaseHelper.GetProductFromEProduct(item);
                    products.Add(p);
                }


                db.Products.BulkMerge(products);

                return Ok(products);


            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }
    }
}