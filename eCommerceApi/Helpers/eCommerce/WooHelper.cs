using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.Base;
using WooCommerceNET.WooCommerce.v3;
namespace eCommerceApi.Helpers.eCommerce
{
    public class WooHelper
    {
        RestAPI _restApi;
        WCObject _wc;

        public WooHelper(RestAPI rest) 
        {
            _restApi = rest;
            _wc = new WCObject(_restApi);
        }

        public async  Task<List<Variation>> VariationBatch(int id ,List<Variation> i , List<Variation> u, List<string> opts = null)
        {
            WCObject wc = new WCObject(_restApi);
            BatchObject<Variation> batch = new BatchObject<Variation>();
            batch.create = i;
            batch.update = u;


            var r = await _wc.Product.Variations.UpdateRange(id, batch);

            var p = await _wc.Product.Get(Convert.ToInt32(id));

            if(opts == null)
            {
                opts = new List<string>();
                foreach (var item in i)
                {
                    opts.Add(item.attributes[0].option);
                }
            }
           

            if (p.attributes != null)
            {
                p.attributes.Add(new ProductAttributeLine()
                {
                    id = 1,
                    variation = true,
                    visible = true,
                    position = 0,
                    name = "Color",
                    options = opts

                });
            }

            foreach (var item in r.create)
            {
                p.variations.Add(Convert.ToInt32(item.id));
            }



            var z = await wc.Product.Update(id, p);
            return r.create; 
        }


    }
}
