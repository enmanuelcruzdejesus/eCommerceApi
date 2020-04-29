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
    public class CategoryController : ControllerBase
    {
        RestAPI _restApi;

        public CategoryController() { _restApi = AppConfig.Instance().Service; }



        [HttpPost("download")]
        public async Task<IActionResult> Post()
        {
            try
            {

                WCObject wc = new WCObject(_restApi);

                //Get all products
                var ecategories = await wc.Category.GetAll();

                var db = AppConfig.Instance().Db;



                //adapting data
                List<eCommerceApi.Model.ProductCategories> categories = new List<Model.ProductCategories>();
                foreach (var item in ecategories)
                {
                    var cat = DatabaseHelper.GetCategoryFromEProductCategory(item);
                    categories.Add(cat);
                }


                db.ProductCategories.BulkMerge(categories);

                return Ok(categories);


            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }

    }
}