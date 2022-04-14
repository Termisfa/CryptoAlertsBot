using System.Globalization;
using System.Reflection;

namespace CryptoAlertsBot.Helpers
{
    public static class GenericHelpers
    {
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            try
            {
                DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
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

        public static string FormatUserIdToNumberFormat(string userID)
        {
            try
            {
                string result = userID.Replace("<", "").Replace(">", "").Replace("@", "").Replace("!", "");
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
                return option switch
                {
                    ">" => amount1 > amount2,
                    ">=" => amount1 >= amount2,
                    "=" => amount1 == amount2,
                    "!=" => amount1 != amount2,
                    "<" => amount1 < amount2,
                    "<=" => amount1 <= amount2,
                    _ => true,
                };
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static List<Type> GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            try
            {
                return assembly.GetTypes()
                                .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                                .ToList();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static bool IsRelease()
        {
#if DEBUG
            return false;
#else
                return true;
#endif
        }
    }
}
