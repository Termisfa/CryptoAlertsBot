using System.Globalization;

namespace CryptoAlertsBot.Helpers
{
    public static class Helpers
    {
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            try
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
                dateTime = dateTime.AddSeconds(unixTimeStamp / 1000);
                return dateTime;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static double StringPriceToDouble(string price, int maxLength)
        {
            try
            {
                if (price.Length > maxLength)
                    price = price.Substring(0, maxLength);

                double priceDouble = double.Parse(price.Replace(',', '.'), CultureInfo.InvariantCulture);

                return priceDouble;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static string FormatChannelIdToNumberFormat(string formatedChannelId)
        {
            try
            {
                string result = formatedChannelId.Replace("<", "").Replace(">", "").Replace("#", "");
                return result;
            }
            catch (Exception e) { throw; }
        }

        public static string FormatChannelIdToDiscordFormat(string rawId)
        {
            try
            {
                string result = $"<#{rawId}>";
                return result;
            }
            catch (Exception e) { throw; }
        }

        public static bool IsGreaterOrLesserHandler(string option, double amount1, double amount2)
        {
            try
            {
                switch (option)
                {
                    case ">": return amount1 > amount2;
                    case ">=": return amount1 >= amount2;
                    case "=": return amount1 == amount2;
                    case "!=": return amount1 != amount2;
                    case "<": return amount1 < amount2;
                    case "<=": return amount1 <= amount2;

                    default:
                        return true;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static bool IsDebug()
        {
#if DEBUG
            return true;
#else
                return false;
#endif
        }
    }
}
