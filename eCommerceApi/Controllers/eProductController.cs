
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
    public class eProductController : ControllerBase
    {
        public async  Task<IActionResult> Get()
        {
            var restApi = AppConfig.Instance().Service;
     //       RestAPI rest = new RestAPI("http://52.71.3.144/wp-json/wc/v3/", "ck_e3763a72e7b9458a6540fdf48d42097db87106cd", "cs_1f204be5002a0d673d7d91e78a6121cfdb908919");
            WCObject wc = new WCObject(restApi);

            //Get all products
            var products = await wc.Product.GetAll();

            return Ok(products);
        }
    }
}