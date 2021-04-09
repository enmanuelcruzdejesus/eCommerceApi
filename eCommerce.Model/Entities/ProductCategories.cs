using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.Model.Entities
{
    public class ProductCategories
    {

        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }
        public int categoryRef { get; set; }

        public string name { get; set; }
        public string descrip { get; set; }

        public int parent { get; set; }
        public string slug { get; set; }



        public DateTime created { get; set; }
        public DateTime lastupdate { get; set; }
    }
}
