using Discord.WebSocket;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using CryptoAlertsBot.Discord;
using CryptoAlertsBot.Models.MergedModels;
using CryptoAlertsBot.AlertsTypes;
using Discord;
using GenericApiHandler.Models;
using System.Globalization;

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
                var timer = new System.Timers.Timer(1000 * 60 * 1); //Debería ser 1000 * 60 * 1  (1 min)
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
            try
            {
                CultureInfo culture = new CultureInfo("es-ES");
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                ResultPancakeSwapApi coinInfo = await _mostUsedApiCalls.GetFromPancakeSwapApi(_constantsHandler.GetConstant(ConstantsNames.URL_API), coin.Address);

                Prices price = await UpdateDatabase(coin, coinInfo);

                if (price != default)
                {
                    CheckAlerts(coin, coinInfo.Price);
                    UpdateResume(coin, price);
                }
            }
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
            }
        }

        private async Task<Prices> UpdateDatabase(Coins coin, ResultPancakeSwapApi coinInfo)
        {
            try
            {
                var categoryChannelDb = _client.Guilds.First().CategoryChannels.First(w => w.Id == ulong.Parse(_constantsHandler.GetConstant(ConstantsNames.DB_CATEGORY_CHANNEL_ID)));

                var coinChannel = categoryChannelDb.Channels.FirstOrDefault(w => w.Id == ulong.Parse(coin.IdChannel));

                if (coinChannel == null)
                {
                    throw new Exception($"El canal de la moneda `{coin.Name}:{coin.Address}` no existe");
                }

                List<HttpParameter> parameters = new();
                parameters.Add(HttpParameter.DefaultParameter("coinAddress", coin.Address));
                parameters.Add(HttpParameter.ParameterWithoutApostrophes("priceDate", $"(select max(priceDate) from prices where coinAddress = '{coin.Address}')"));

                var prices = await _buildAndExeApiCall.GetWithMultipleParameters<Prices>(parameters);
                var previousPrice = prices.FirstOrDefault();

                if (previousPrice != null && coinInfo.Updated_at <= previousPrice.PriceDate)
                {
                    return default;
                }

                Prices price = new()
                {
                    CoinAddress = coin.Address,
                    PriceUsd = coinInfo.Price,
                    PriceDate = coinInfo.Updated_at,
                };

                if(await _buildAndExeApiCall.Post("prices", price) != 1)
                {
                    return default;
                }

                _ = (coinChannel as SocketTextChannel).SendMessageAsync(await _commonFunctionality.FormatPriceToDatabaseChannelAsync(price, previousPrice));

                return price;
            }
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
                return default;
            }
        }

        private async void UpdateResume(Coins coin, Prices price)
        {
            try
            {
                var alerts = (await _buildAndExeApiCall.GetWithOneParameter<Alerts>(HttpParameter.DefaultParameter("coinAddress", coin.Address))).DistinctBy(w => w.UserId).ToList();

                foreach (var alert in alerts)
                {
                    var categoryChannelId = await _commonFunctionality.GetCategoryChannelIdFromUserId(alert.UserId);
                    var resumeChannel = (SocketTextChannel)_client.Guilds.First().GetCategoryChannel(categoryChannelId).Channels?.FirstOrDefault(w => w.Name.Contains("resumen"));

                    await resumeChannel.DeleteMessagesAsync(await resumeChannel.GetMessagesAsync().Flatten().Where(w => w.Content.Contains(coin.Name)).ToListAsync());

                    _ = resumeChannel.SendMessageAsync(await _commonFunctionality.FormatPriceToResumeChannelAsync(coin, price, _constantsHandler.GetConstant(ConstantsNames.URL_POOCOIN)));
                }
            }
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
            }
        }

        private async void CheckAlerts(Coins coin, double price)
        {
            try
            {
                List<HttpParameter> parameters = new();
                parameters.Add(HttpParameter.DefaultParameter("coinAddress", coin.Address));
                parameters.Add(HttpParameter.ParameterWithoutApostrophes("userId", "users.Id"));
                parameters.Add(HttpParameter.ParameterWithoutApostrophes("active", "true"));

                var alertsUsersList = await _buildAndExeApiCall.GetWithMultipleParameters<AlertsUsers>(parameters, "alerts,users");

                foreach (var alertUser in alertsUsersList)
                {
                    if (Helpers.Helpers.IsGreaterOrLesserHandler(AlertsHelper.GetAlertSign(alertUser.Alert.AlertType), price, alertUser.Alert.PriceUsd))
                    {
                        if (alertUser.Alert.LastAlert == null)
                            _ = NotifyAlert(alertUser, price, coin.IdChannel);
                        else
                        {
                            double alertsCooldown = alertUser.Alert.HoursBetweenAlerts;

                            if ((DateTime.Now - alertUser.Alert.LastAlert.Value).TotalHours > alertsCooldown)
                            {
                                List<Prices> prices = (await _buildAndExeApiCall.GetWithOneParameter<Prices>(HttpParameter.DefaultParameter("coinAddress", coin.Address))).Where(w => w.PriceDate >= alertUser.Alert.LastAlert.Value.AddHours(alertsCooldown)).ToList();

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
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
            }
        }

        private Task NotifyAlert(AlertsUsers alertUser, double price, string coinChannelId)
        {
            try
            {
                alertUser.Alert.LastAlert = DateTime.Now;
                _ = _buildAndExeApiCall.PutWithOneParameter("alerts", alertUser.Alert, HttpParameter.DefaultParameter("id", alertUser.Alert.Id.ToString()));

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
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
                return Task.CompletedTask;
            }
        }
    }
}
