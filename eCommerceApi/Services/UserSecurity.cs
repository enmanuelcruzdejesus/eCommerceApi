using ApiCore;
using eCommerce.Model.Entities;
using eCommerceApi.Model;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerceApi.Services
{
    public class UserSecurity
    {

        public static Users Login(string userName, string password)
        {
            var db = AppConfig.Instance().Db;
            var exist = db.Users.Get(user => user.username == userName && user.password == password && user.active == true).First();
            return exist;
       


        }
    }
}
