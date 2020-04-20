using System;
using System.Collections.Generic;
using System.IO;
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
    public class AppController : ControllerBase
    {
        RestAPI _restApi;

        public AppController() { _restApi = AppConfig.Instance().Service; }


        [HttpPost("webhooks")]
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
                    jsonData = reader.ReadToEnd();

                    // Do something


                }

                return Ok(jsonData);






                //    if (System.Web.HttpContext.Current.Request.InputStream.CanSeek)
                //    {
                //        //Move the cursor to beginning of stream if it has already been by json process
                //        System.Web.HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
                //        jsonData = new StreamReader(HttpContext.Current.Request.InputStream).ReadToEnd();
                //        //Get the value of webhooks header's signature
                //        hmacHeaderSignature = System.Web.HttpContext.Current.Request.Headers["intuit-signature"];
                //    }

                //    //Validate webhooks response by hading it with HMACSHA256 algo and comparing it with Intuit's header signature
                //    bool isRequestvalid = ProcessNotificationData.Validate(jsonData, hmacHeaderSignature);

                //    //If request is valid, send 200 Status to webhooks sever
                //    if (isRequestvalid == true)
                //    {
                //        WebhooksNotificationdto.WebhooksData webhooksData = JsonConvert.DeserializeObject<WebhooksNotificationdto.WebhooksData>(jsonData);
                //        return Request.CreateResponse(HttpStatusCode.OK, webhooksData);
                //    }

                //    return Request.CreateResponse(HttpStatusCode.Conflict, "Error");

                //    //Defult pgae displayed will be the Index view page when application is running




                //}
                //catch (Exception ex)
                //{

                //    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                //}


            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }

        }
    }
}