
namespace CryptoAlertsBot.AlertsTypes
{
    public static class AlertsHelper
    {
        public static string GetAlertSign(string alertWord)
        {
            try
            {
                if (AlertsEnum.Sube.ToString() == alertWord)
                    return ">=";
                if (AlertsEnum.Baja.ToString() == alertWord)
                    return "<=";
                return default;
            }
            catch (Exception e) { throw; }
        }

        public static int GetAlertMultiplier(string alertWord)
        {
            try
            {
                if (AlertsEnum.Sube.ToString() == alertWord)
                    return 1;
                if (AlertsEnum.Baja.ToString() == alertWord)
                    return -1;
                return default;
            }
            catch (Exception e) { throw; }
        }

        public static bool IsPorcentual(string alertWord)
        {
            bool result;

            result = alertWord.ElementAt(alertWord.Length - 1) == '%';

            return result;
        }

        public static string RemoveSymbolIfExists(string alertWord)
        {
            if (!IsPorcentual(alertWord))
                return alertWord;

            return alertWord.Substring(0, alertWord.Length - 1);
        }
    }
}
