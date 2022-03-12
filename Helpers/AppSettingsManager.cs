using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoAlertsBot
{
    public static class AppSettingsManager
    {
        public static string GetDiscordBotKey()
        {
            return ConfigurationManager.AppSettings.Get("DiscordBotKey");
        }

        public static string GetConnectionString()
        {
            return ConfigurationManager.AppSettings.Get("ConnectionString");
        }

        public static string GetCryptoAllertsDiscordServerId()
        {
            return ConfigurationManager.AppSettings.Get("CryptoAllertsDiscordServerId");
        }

        public static string GetApiBaseAddress()
        {
            return ConfigurationManager.AppSettings.Get("ApiBaseAddress");
        }

        public static string GetApiBaseUri()
        {
            return ConfigurationManager.AppSettings.Get("ApiBaseUri");
        }

        public static string GetApiDefaultSchema()
        {
            return ConfigurationManager.AppSettings.Get("ApiDefaultSchema");
        }
    }
}
