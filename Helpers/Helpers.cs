using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoAlertsBot.Helpers
{
    public static class Helpers
    {
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp/1000).ToLocalTime();
            return dateTime;
        }

        public static double StringPriceToDouble(string price, int maxLength = default)
        {
            price = price.Substring(0, maxLength);

            double priceDouble = double.Parse(price);

            return priceDouble;
        }

        public static string FormatChannelId(string rawId)
        {
            string result = rawId.Replace("<", "").Replace(">", "").Replace("#", "");
            return result;
        }
    }
}
