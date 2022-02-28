
namespace CryptoAlertsBot.AlertsTypes
{
    public static class AlertsHandler
    {
        public static string GetAlertType(string alertTypeWord)
        {
            alertTypeWord = alertTypeWord.Trim().ToUpper();

            if (IsValidWord<AlertsUpEnum>(alertTypeWord))
                return AlertsUpEnum.UP.ToString();

            if (IsValidWord<AlertsDownEnum>(alertTypeWord))
                return AlertsDownEnum.DOWN.ToString();

            return default;
        }     

        public static string GetMessageWrongAlertTypeWord(string alertTypeWord)
        {
            string answer = "La palabra `" + alertTypeWord + "` no es válida \n";
            answer += "Palabras válidas de subida: `" + GetEnumList<AlertsDownEnum>() + "` \n";
            answer += "Palabras válidas de bajada: `" + GetEnumList<AlertsUpEnum>() + "`";

            return answer;
        }

        public static string GetEnumList<T>()
        {
            string result = String.Join(", ", Enum.GetValues(typeof(T)).Cast<T>().ToList());

            return result;
        }

        public static string GetAlertSign(string alertWord)
        {
            if (IsValidWord<AlertsUpEnum>(alertWord))
                return ">=";

            if (IsValidWord<AlertsDownEnum>(alertWord))
                return "<=";

            return default;
        }

        private static bool IsValidWord<T>(string alertTypeWord)
        {
            bool result = Enum.IsDefined(typeof(T), alertTypeWord);
            return result;
        }
    }
}
