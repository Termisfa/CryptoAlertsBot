using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using CryptoAlertsBot.Discord;

namespace CryptoAlertsBot
{
    public class FillPricesDB
    {
        private readonly DiscordSocketClient _client;
        private readonly ConstantsHandler _constantsHandler;
        private readonly SocketGuild _guild;

        public FillPricesDB(DiscordSocketClient client, ConstantsHandler constantsHandler)
        {
            _client = client;
            _constantsHandler = constantsHandler;
            _guild = _client.Guilds.First();
        }

        public void Initialize()
        {
            int timeStepInMilliseconds = 1000 * 60;
            Timer timer = new(TimerCallbackAsync, null, 0, timeStepInMilliseconds);
        }

        private async void TimerCallbackAsync(Object? stateInfo)
        {
            var coinsList = await BuildAndExeApiCall.GetAllTable<Coins>();

            coinsList.AsParallel().ForAll(coin => _ = FillPrice(coin));
        }

        private async Task FillPrice(Coins coin)
        {
            try
            {
                var categoryChannelDb = _guild.CategoryChannels.First(w => w.Id == ulong.Parse(_constantsHandler.GetConstant(ConstantsNames.DB_CATEGORY_CHANNEL_ID)));

                var coinChannel = categoryChannelDb.Channels.FirstOrDefault(w => w.Id == ulong.Parse(coin.IdChannel));

                if (coinChannel == null)
                    throw new Exception($"El canal de la moneda `{coin.Name}:{coin.Address}` no existe");

                Dictionary<string, string> parameters = new();
                parameters.Add("coinAddress", coin.Address);
                parameters.Add("priceDate", $"%(select max(priceDate) from prices where coinAddress = '{coin.Address}')");

                var previousPrice = (await BuildAndExeApiCall.GetWithMultipleArguments<Prices>(parameters)).FirstOrDefault();

                ResultPancakeSwapApi coinInfo = await MostUsedApiCalls.GetFromPancakeSwapApi(_constantsHandler.GetConstant(ConstantsNames.URL_API), coin.Address);

                if (previousPrice != null && coinInfo.Updated_at == previousPrice.PriceDate)
                    return;

                Prices price = new()
                {
                    CoinAddress = coin.Address,
                    PriceUsd = coinInfo.Price,
                    PriceDate = coinInfo.Updated_at
                };

                _ = BuildAndExeApiCall.Post("prices", price);

                _ = (coinChannel as SocketTextChannel).SendMessageAsync(await CommonFunctionality.FormatPriceToDatabaseChannelAsync(price, previousPrice));

                //TODO:
                //CheckAlerts(coinAddress, dataFromHttps.data.price)
                //UpdateResume(coinAddress, dataFromHttps)
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
                Logger.Log(e.StackTrace);
            }

        }
    }
}
