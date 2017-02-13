using Newtonsoft.Json;
using RestSharp.Extensions.MonoHttp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Twilio;

namespace OTPManagerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //SendSms sms = new SendSms();
            //string status = sms.send("9092682006", "vivek123", "Hello!! How are you", "9092682006");
            //if (status == "1")
            //{
            //   Console.WriteLine("Message Send");
            //}
            //else if (status == "2")
            //{
            //    Console.WriteLine("No Internet Connection");
            //}
            //else
            //{
            //    Console.WriteLine("Invalid Login Or No Internet Connection");
            //}
            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://ultimatesmsapi.tk/sms.php?provider=way2sms&username=9092682006&password=vivek123&numbers9092682006&msg="+"Hello buddy!! Its working");
            //HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
            //System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
            //string responseString = respStreamReader.ReadToEnd();
            //respStreamReader.Close();
            //myResp.Close();


            //string username = "vivekgoyal923";
            //string password = "vivek123";
            ///*
            //* Your phone number, including country code, i.e. +44123123123 in this case:
            //*/
            //string msisdn = "+919092682006";



            /*
            * A 7-bit GSM SMS message can contain up to 160 characters (longer messages can be
            * achieved using concatenation).
            *
            * All non-alphanumeric 7-bit GSM characters are included in this example. Note that Greek characters,
            * and extended GSM characters (e.g. the caret "^"), may not be supported
            * to all networks. Please let us know if you require support for any characters that
            * do not appear to work to your network.
            */
            //string seven_bit_msg = "Hey Buddy!! Its working";

            //string data = "";
            //data += "MobileNumber=" + "9092682006";
            //data += "&FromEmailAddress=" + HttpUtility.UrlEncode("vivekgoyal923@gmail.com", System.Text.Encoding.GetEncoding("ISO-8859-1"));
            //data += "&Message=" + HttpUtility.UrlEncode(seven_bit_msg, System.Text.Encoding.GetEncoding("ISO-8859-1"));
            //
            string url = "https://railwaybot.pythonanywhere.com/otp";
            var data = new
            {
                username = "9092682006",
                password = "vivek123",
                phoneNumber = "9092682006",
                code = "123456"
            };
            var messageString = JsonConvert.SerializeObject(data);
            string sms_result = Post(url, messageString);
            Console.ReadLine();
        }
        
        public static string Post(string url, string data)
        {

            string result = null;
            try
            {
                byte[] buffer = Encoding.Default.GetBytes(data);

                HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(url);
                WebReq.Method = "POST";
                WebReq.ContentType = "application/x-www-form-urlencoded";
                WebReq.ContentLength = buffer.Length;
                Stream PostData = WebReq.GetRequestStream();

                PostData.Write(buffer, 0, buffer.Length);
                PostData.Close();
                HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
                Console.WriteLine(WebResp.StatusCode);

                Stream Response = WebResp.GetResponseStream();
                StreamReader _Response = new StreamReader(Response);
                result = _Response.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result + "\n";
        }
    }
}
