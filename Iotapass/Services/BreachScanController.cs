using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Iotapass.Services
{
    
    // Class containing methods to interact with the haveibeenpwned API 3.0, using a HttpClient
    internal class BreachScanController
    {
        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://api.pwnedpasswords.com/range/")
        };
        /// <summary>
        /// Takes a SHA1 hash of a password and returns the number of occurrences found in breaches. Returns -1 if error found.
        /// </summary>
        /// <param name="hashed"></param>
        /// <returns></returns>
        internal static int FindOccurrences(string hashed)
        {
            string httpResponse;
            try
            {
                // Here is the HTTP response message, shortened
                httpResponse = client.GetAsync(hashed.Substring(0, 5)).Result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                return 0;
            }
            
            // hashed is the hashed password, passed as the only parameter. If it (the remaining 35 chars) does not exist in our response string, return 0.
            if (!httpResponse.Contains(hashed.Substring(5)))
            {
                return 0;
            }
            // The initial check that the response contains the hash should be true. This gets the substring of the response, skipping over the hash we want and the colon, so the string starts with the number we need.
            httpResponse = httpResponse.Substring(httpResponse.IndexOf(hashed.Substring(5)) + 36);
            bool scanningNums = true;
            char ltr = ' ';
            string myNum = "";
            while (scanningNums)
            {
                ltr = httpResponse[0];
                if (ltr == '0' || ltr == '1' || ltr == '2' || ltr == '3' || ltr == '4' || ltr == '5' || ltr == '6' || ltr == '7' || ltr == '8' || ltr == '9')
                {
                    myNum += ltr;
                    if (httpResponse.Length > 1)
                    {
                        httpResponse = httpResponse.Substring(1);
                    }
                    else
                    {
                        scanningNums = false;
                    }
                }
                else
                {
                    scanningNums = false;
                }
            }
            // myNum is the # of occurrences that password has had in breaches.
            try
            {
                int x = int.Parse(myNum);
                return x;
            }
            catch (Exception _)
            {
                return -1;
            }
        }
    }
}
