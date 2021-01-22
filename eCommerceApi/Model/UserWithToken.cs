using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerceApi.Model
{
    public class UserWithToken
    {
        private Users _user;
        public Users User
        {
            get
            {
                return _user;
            }
            set
            {
                _user = value;
            }
        }
        public string Token { get; set; }

        public UserWithToken(Users user)
        {
            _user = user;
        }
    }
}
