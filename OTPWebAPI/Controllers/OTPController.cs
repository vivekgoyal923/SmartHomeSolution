using OTPWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace OTPWebAPI.Controllers
{
    public class OTPController : ApiController
    {
        private static Random rng = new Random();
        private static List<OTPCode> OTPCodes = new List<OTPCode>();
        [HttpGet]
        public async Task<HttpResponseMessage> RequestOTP(string phoneNumber)
        {
            int value = rng.Next(100, 9999); //1
            string code = value.ToString("0000");
            OTPCodes.Add(new OTPCode { PhoneNumber = phoneNumber, Code = code });//2 and 3

            //4 send an SMS with the code
            try
            {
                // SMS client will throw an error if something goes wrong 
                var message = string.Format("Your code:{0} for verifying your number with me", code);
                var number = phoneNumber.Trim();
                Client smsClient = new Client("key", "secret");
                await smsClient.SendSMS(number, message);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                // handle error here, see https://www.sinch.com/docs/rest-apis/api-documentation/#messagingapi for possible errors
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
        public HttpResponseMessage VerifyOTP(string phoneNumber, string code)
        {
            if (OTPCodes.Any(otp => otp.PhoneNumber == phoneNumber && otp.Code == code))
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }
    }
}