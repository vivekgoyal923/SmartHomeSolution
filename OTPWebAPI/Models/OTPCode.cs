using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OTPWebAPI.Models
{
    public class OTPCode
    {
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
    }
}