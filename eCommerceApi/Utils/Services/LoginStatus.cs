using System;
using System.Collections.Generic;
using System.Text;

namespace DevCore.Utils.Services
{
   public  class LoginStatus
    {

        public bool IsValid { get; set; }
        public string Message { get; set; }


        public LoginStatus(bool isvalid, string message)
        {
            this.IsValid = isvalid;
            this.Message = message;

        }
    }
}
