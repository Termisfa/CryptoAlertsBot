
namespace CryptoAlertsBot.AlertsTypes
{
    public static class AlertsHelper
    {
        public static string GetAlertSign(string alertWord)
        {
            if (AlertsEnum.Sube.ToString() == alertWord)
                return ">=";

            if (AlertsEnum.Baja.ToString() == alertWord)
                return "<=";

            return default;
        }
    }
}
