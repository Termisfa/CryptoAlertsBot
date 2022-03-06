using Discord.WebSocket;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using CryptoAlertsBot.Discord;
using CryptoAlertsBot.Models.MergedModels;
using CryptoAlertsBot.AlertsTypes;
using Discord;

namespace CryptoAlertsBot
{
    public class FillPricesDB
    {
        private readonly DiscordSocketClient _client;
        private readonly ConstantsHandler _constantsHandler;
        private readonly Logger _logger;
        private readonly BuildAndExeApiCall _buildAndExeApiCall;
        private readonly MostUsedApiCalls _mostUsedApiCalls;
        private readonly CommonFunctionality _commonFunctionality;

        public FillPricesDB(DiscordSocketClient client, ConstantsHandler constantsHandler, Logger logger, BuildAndExeApiCall buildAndExeApiCall, MostUsedApiCalls mostUsedApiCalls, CommonFunctionality commonFunctionality)
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
                var timer = new System.Timers.Timer(1000 * 10 * 1); //Debería ser 1000 * 60 * 1  (1 min)
                timer.Start();
                timer.Elapsed += FillPricesTimerCallbackAsync;
            }
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
            }
        }

        private async void FillPricesTimerCallbackAsync(object? sender, System.Timers.ElapsedEventArgs elapsed)
        {
            try
            {
                var coinsList = await _buildAndExeApiCall.GetAllTable<Coins>();

                coinsList.AsParallel().ForAll(coin => _ = FillPrice(coin));
            }
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
            }
        }

        private async Task FillPrice(Coins coin)
        {
            ResultPancakeSwapApi coinInfo = await _mostUsedApiCalls.GetFromPancakeSwapApi(_constantsHandler.GetConstant(ConstantsNames.URL_API), coin.Address);

            Prices price = await UpdateDatabase(coin, coinInfo);

            if (price != default)
            {
                CheckAlerts(coin, coinInfo.Price);
                UpdateResume(coin, price);
            }

        }

        private async Task<Prices> UpdateDatabase(Coins coin, ResultPancakeSwapApi coinInfo)
        {
            var categoryChannelDb = _client.Guilds.First().CategoryChannels.First(w => w.Id == ulong.Parse(_constantsHandler.GetConstant(ConstantsNames.DB_CATEGORY_CHANNEL_ID)));

            var coinChannel = categoryChannelDb.Channels.FirstOrDefault(w => w.Id == ulong.Parse(coin.IdChannel));

            if (coinChannel == null)
                throw new Exception($"El canal de la moneda `{coin.Name}:{coin.Address}` no existe");

            Dictionary<string, string> parameters = new();
            parameters.Add("coinAddress", coin.Address);
            parameters.Add("priceDate", $"$(select max(priceDate) from prices where coinAddress = '{coin.Address}')");

            var previousPrice = (await _buildAndExeApiCall.GetWithMultipleArguments<Prices>(parameters)).FirstOrDefault();

            if (previousPrice != null && coinInfo.Updated_at == previousPrice.PriceDate)
                return default;

            Prices price = new()
            {
                CoinAddress = coin.Address,
                PriceUsd = coinInfo.Price,
                PriceDate = coinInfo.Updated_at,
            };

            _ = _buildAndExeApiCall.Post("prices", price);

            _ = (coinChannel as SocketTextChannel).SendMessageAsync(await _commonFunctionality.FormatPriceToDatabaseChannelAsync(price, previousPrice));

            return price;
        }

        private async void UpdateResume(Coins coin, Prices price)
        {
            var alerts = (await _buildAndExeApiCall.GetWithOneArgument<Alerts>("coinAddress", coin.Address)).DistinctBy(w => w.UserId).ToList();

            foreach (var alert in alerts)
            {
                var categoryChannelId = await _commonFunctionality.GetCategoryChannelIdFromUserId(alert.UserId);
                var resumeChannel = (SocketTextChannel)_client.Guilds.First().GetCategoryChannel(categoryChannelId).Channels?.FirstOrDefault(w => w.Name.Contains("resumen"));

                await resumeChannel.DeleteMessagesAsync(await resumeChannel.GetMessagesAsync().Flatten().Where(w => w.Content.Contains(coin.Name)).ToListAsync());

                _ = resumeChannel.SendMessageAsync(await _commonFunctionality.FormatPriceToResumeChannelAsync(coin, price, _constantsHandler.GetConstant(ConstantsNames.URL_POOCOIN)));
            }
        }

        private async void CheckAlerts(Coins coin, double price)
        {
            Dictionary<string, string> arguments = new();
            arguments.Add("coinAddress", coin.Address);
            arguments.Add("userId", "$users.Id");
            arguments.Add("active", "$true");

            var alertsUsersList = await _buildAndExeApiCall.GetWithMultipleArguments<AlertsUsers>(arguments, "alerts,users");

            foreach (var alertUser in alertsUsersList)
            {
                if (Helpers.Helpers.IsGreaterOrLesserHandler(AlertsHelper.GetAlertSign(alertUser.Alert.AlertType), price, alertUser.Alert.PriceUsd))
                {
                    if (alertUser.Alert.LastAlert == null)
                        _ = NotifyAlert(alertUser, price, coin.IdChannel);
                    else
                    {
                        double alertsCooldown = double.Parse(_constantsHandler.GetConstant(ConstantsNames.ALERTS_COOLDOWN));

                        if ((DateTime.Now - alertUser.Alert.LastAlert.Value).TotalHours > alertsCooldown)
                        {
                            List<Prices> prices = (await _buildAndExeApiCall.GetWithOneArgument<Prices>("coinAddress", coin.Address)).Where(w => w.PriceDate >= alertUser.Alert.LastAlert.Value.AddHours(alertsCooldown)).ToList();

                            foreach (var priceRow in prices)
                            {
                                if (Helpers.Helpers.IsGreaterOrLesserHandler(AlertsHelper.GetAlertSign(alertUser.Alert.AlertType), alertUser.Alert.PriceUsd, priceRow.PriceUsd))
                                {
                                    await NotifyAlert(alertUser, price, coin.IdChannel);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private Task NotifyAlert(AlertsUsers alertUser, double price, string coinChannelId)
        {
            alertUser.Alert.LastAlert = DateTime.Now;
            _ = _buildAndExeApiCall.PutWithOneArgument("alerts", alertUser.Alert, "id", alertUser.Alert.Id.ToString());


            var categoryChannel = _client.Guilds.First().CategoryChannels.First(w => w.Id == ulong.Parse(alertUser.User.IdCategoryChannel));

            var alertsChannel = categoryChannel.Channels.FirstOrDefault(w => w.Name.Contains("alertas"));

            if (alertsChannel == null)
                throw new Exception($"El canal de alertas del usuario '{alertUser.User.Name}' no existe");

            var priceLength = int.Parse(_constantsHandler.GetConstant(ConstantsNames.PRICE_LENGTH));
            string trimmedPrice = price.ToString();
            if (trimmedPrice.Length > priceLength)
                trimmedPrice = trimmedPrice.Substring(0, priceLength);

            string upOrDown = alertUser.Alert.AlertType == AlertsEnum.Sube.ToString() ? $"subido" : "bajado";

            _ = (alertsChannel as SocketTextChannel).SendMessageAsync($"<@{alertUser.Alert.UserId}> La moneda {Helpers.Helpers.FormatChannelIdToDiscordFormat(coinChannelId)} ha {upOrDown} de `{alertUser.Alert.PriceUsd}`. Está en  `{trimmedPrice}` USD");
            return Task.CompletedTask;
        }
    }
}
