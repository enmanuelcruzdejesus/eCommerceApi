﻿using ApiCore;
using ApiCore.Services;
using eCommerce.Model.Entities;
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
        IRepository<Products> _productRepo;
        public WooHelper(RestAPI rest, IRepository<Products> productRepo) 
        {
            _restApi = rest;
            _wc = new WCObject(_restApi);

            _productRepo = productRepo;
        }

        public async  Task<List<Variation>> VariationBatch(int id ,List<Variation> i , List<Variation> u, List<string> opts = null)
        {
            WCObject wc = new WCObject(_restApi);
            BatchObject<Variation> batch = new BatchObject<Variation>();
            batch.create = i;
            batch.update = u;

            var db = AppConfig.Instance().Db;

            var r = await _wc.Product.Variations.UpdateRange(id, batch);

            if (i != null && i.Count() > 0)
            {
                var p = await _wc.Product.Get(Convert.ToInt32(id));

                if (opts == null )
                {
                    opts = new List<string>();
                    foreach (var item in i)
                    {
                        opts.Add(item.attributes[0].option);
                    }
                }


                if (p.attributes != null )
                {
                    p.attributes.Add(new ProductAttributeLine()
                    {
                        id = 1,
                        variation = true,
                        visible = true,
                        name = "Color",
                        options = opts

                    });
                }
                else
                {
                    p.attributes = new List<ProductAttributeLine>();
                    p.attributes.Add(new ProductAttributeLine()
                    {
                        id = 1,
                        variation = true,
                        visible = true,
                        name = "Color",
                        options = opts

                    });
                }

                foreach (var item in r.create)
                {
                    p.variations.Add(Convert.ToInt32(item.id));
                    //updating product reference

                    //var v = _productRepo.Get(x => x.sku == item.sku).FirstOrDefault();
                    // _productRepo.Update(new Products() { productRef = Convert.ToInt32(item.id) }, product => product.id == v.id);

                }

                var z = await wc.Product.Update(id, p);
                return r.create;

            }


            return  r.update;


        }



    }
}
