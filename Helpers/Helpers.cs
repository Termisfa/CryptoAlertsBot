using System.Globalization;

namespace CryptoAlertsBot.Helpers
{
    public static class Helpers
    {
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp / 1000).ToLocalTime();
            return dateTime;
        }

        public static double StringPriceToDouble(string price, int maxLength)
        {
            if (price.Length > maxLength)
                price = price.Substring(0, maxLength);

            double priceDouble = double.Parse(price.Replace(',', '.'), CultureInfo.InvariantCulture);

            return priceDouble;
        }

        public static string FormatChannelIdToNumberFormat(string formatedChannelId)
        {
            string result = formatedChannelId.Replace("<", "").Replace(">", "").Replace("#", "");
            return result;
        }

        public static string FormatChannelIdToDiscordFormat(string rawId)
        {
            string result = $"<#{rawId}>";
            return result;
        }

        public static bool IsGreaterOrLesserHandler(string option, double amount1, double amount2)
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
    }
}
