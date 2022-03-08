
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
    }
}
