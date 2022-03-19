using Discord.WebSocket;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using CryptoAlertsBot.Discord;
using CryptoAlertsBot.Models.MergedModels;
using CryptoAlertsBot.AlertsTypes;
using Discord;
using GenericApiHandler.Models;
using GenericApiHandler.Data.Enums;

namespace CryptoAlertsBot
{
    public class ClearPricesTable
    {
        private readonly DiscordSocketClient _client;
        private readonly ConstantsHandler _constantsHandler;
        private readonly Logger _logger;
        private readonly BuildAndExeApiCall _buildAndExeApiCall;
        private readonly MostUsedApiCalls _mostUsedApiCalls;
        private readonly CommonFunctionality _commonFunctionality;

        public ClearPricesTable(DiscordSocketClient client, ConstantsHandler constantsHandler, Logger logger, BuildAndExeApiCall buildAndExeApiCall, MostUsedApiCalls mostUsedApiCalls, CommonFunctionality commonFunctionality)
        {
            _client = client;
            _constantsHandler = constantsHandler;
            _logger = logger;
            _buildAndExeApiCall = buildAndExeApiCall;
            _mostUsedApiCalls = mostUsedApiCalls;
            _commonFunctionality = commonFunctionality;
        }

        public void Initialize()
        {
            try
            {
                var timer = new System.Timers.Timer(1000 * 60 * 60 * 24 * 7); //It should be 1000 * 60 * 60 * 24 * 7  (1 week)
                timer.Start();
                timer.Elapsed += ClearTable;
            }
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
            }
        }

        private async void ClearTable(object? sender, System.Timers.ElapsedEventArgs elapsed)
        {
            try
            {
                int affectedRows = await _buildAndExeApiCall.DeleteWithOneParameter("prices",HttpParameter.DefaultParameter("PriceDate", DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss"), ComparatorsEnum.lowerThan));
                _ = _logger.Log("Deleted prices older than a week. Rows deleted: " + affectedRows);
            }
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
            }
        }

       
    }
}
