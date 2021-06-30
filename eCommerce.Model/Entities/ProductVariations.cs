using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.Model.Entities
{

    
    [Alias("ProductVariations")]
    public class ProductVariations
    {
        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }
        public int productid { get; set; }

        public int productidvariation { get; set; }

        public string color { get; set; }

        public string size { get; set; }


        public string status { get; set; }

        public DateTime created { get; set; }

        public DateTime lastupdate { get; set; }


    }
}
