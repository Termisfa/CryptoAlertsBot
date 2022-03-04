using CryptoAlertsBot.AlertsTypes;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Discord.Preconditions;
using CryptoAlertsBot.Models;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.Discord.Modules
{

    public class CoinCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ConstantsHandler _constantsHandler;
        private readonly Logger _logger;

        public CoinCommands(ConstantsHandler constantsHandler, Logger logger)
        {
            _constantsHandler = constantsHandler;
            _logger = logger;
        }

        [SlashCommand("nuevamoneda", "Añade una nueva moneda. Introducir la address")]
        public async Task NewCoin(
            [Summary("Address", "Dirección de la moneda. Ejemplo: 0xbb4cdb9cbd36b01bd1cbaebf2de08d9173bc095c")] string coinAddress
            )
        {
            try
            {
                coinAddress = coinAddress.ToLower().Trim();

                var coins = await BuildAndExeApiCall.GetWithOneArgument<Coins>("address", coinAddress);
                if (coins.Count != 0)
                {
                    await RespondAsync($"La moneda <#{coins[0].IdChannel}> ya existe");
                    return;
                }

                string urlApi = _constantsHandler.GetConstant(ConstantsNames.URL_API);
                ResultPancakeSwapApi coinInfo = await MostUsedApiCalls.GetFromPancakeSwapApi(urlApi, coinAddress);
                if (coinInfo == default)
                {
                    await RespondAsync($"La moneda '{coinAddress}' no existe en PancakeSwap");
                    return;
                }

                string dbCategoryChannelId = _constantsHandler.GetConstant(ConstantsNames.DB_CATEGORY_CHANNEL_ID);
                var coinChannel = await Context.Guild.CreateTextChannelAsync(coinInfo.Symbol, tcp => tcp.CategoryId = new Optional<ulong?>(ulong.Parse(dbCategoryChannelId)));

                string urlPooCoin = _constantsHandler.GetConstant(ConstantsNames.URL_POOCOIN);
                _ = (await coinChannel.SendMessageAsync(urlPooCoin + coinAddress)).PinAsync();

                Coins coin = new()
                {
                    Address = coinAddress,
                    Name = coinInfo.Name,
                    Symbol = coinInfo.Symbol,
                    IdChannel = coinChannel.Id.ToString()
                };
                _ = BuildAndExeApiCall.Post("coins", coin);

                await RespondAsync($"Moneda <#{coinChannel.Id}> añadida con éxito");
            }
            catch (Exception e)
            {
                _ = RespondAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
            }
        }

        [SlashCommand("borrarmoneda", "Elimina una moneda. Especificar el canal")]
        public async Task DeleteCoin(
            [Summary("Canal", "Canal de la moneda. Ejemplo: #WBNB")][IsCoinChannel] SocketTextChannel coinChannel
            )
        {
            try
            {
                var listAlerts = await BuildAndExeApiCall.GetWithOneArgument<Alerts>("coinAddress", $"$(select address from coins where idChannel = '{coinChannel.Id}')");
                if (listAlerts.Count != 0)
                {
                    await RespondAsync("No se puede eliminar una moneda que tiene alertas activas de algún usuario");
                    return;
                }

                _ = coinChannel.DeleteAsync();
                _ = BuildAndExeApiCall.DeleteWithOneArgument("coins", "idChannel", coinChannel.Id.ToString());

                await RespondAsync($"Moneda eliminada con éxito");
            }
            catch (Exception e)
            {
                _ = RespondAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
            }
        }


        //public async Task NewUser([Remainder][Summary("The text to echo")] string echo)

    }
}
