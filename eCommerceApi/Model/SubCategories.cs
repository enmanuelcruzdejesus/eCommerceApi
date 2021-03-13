using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace eCommerceApi.Model
{

    public class SubCategories
    {

        [PrimaryKey]
        [AutoIncrement]
        [DataMember]

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
