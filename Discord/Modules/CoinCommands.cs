using CryptoAlertsBot.AlertsTypes;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Discord.Preconditions;
using CryptoAlertsBot.Models;
using CryptoAlertsBot.Models.PancakeSwap;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GenericApiHandler.Models;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.Discord.Modules
{

    public class CoinCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ConstantsHandler _constantsHandler;
        private readonly Logger _logger;
        private readonly BuildAndExeApiCall _buildAndExeApiCall;
        private readonly MostUsedApiCalls _mostUsedApiCalls;

        public CoinCommands(ConstantsHandler constantsHandler, Logger logger, BuildAndExeApiCall buildAndExeApiCall, MostUsedApiCalls mostUsedApiCalls)
        {
            _constantsHandler = constantsHandler;
            _logger = logger;
            _buildAndExeApiCall = buildAndExeApiCall;
            _mostUsedApiCalls = mostUsedApiCalls;
        }

        [SlashCommand("nuevamoneda", "Añade una nueva moneda. Introducir la address")]
        public async Task NewCoin(
            [Summary("Address", "Dirección de la moneda. Ejemplo: 0xbb4cdb9cbd36b01bd1cbaebf2de08d9173bc095c")] string coinAddress
            )
        {
            try
            {
                await DeferAsync();

                coinAddress = coinAddress.ToLower().Trim();

                var coins = await _buildAndExeApiCall.GetAllTable<Coins>();
                if (coins.Any(w => w.Address == coinAddress))
                {
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = $"La moneda <#{coins[0].IdChannel}> ya existe"; });
                    return;
                }

                string urlApi = await _constantsHandler.GetConstantAsync(ConstantsNames.URL_API);
                ResultPancakeSwapApi coinInfo = await _mostUsedApiCalls.GetFromPancakeSwapApi(urlApi, coinAddress);
                if (coinInfo == default)
                {
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = $"La moneda '{coinAddress}' no existe en PancakeSwap"; });
                    return;
                }

                string dbCategoryChannelId = await _constantsHandler.GetConstantAsync(ConstantsNames.DB_CATEGORY_CHANNEL_ID);
                var coinChannel = await Context.Guild.CreateTextChannelAsync(coinInfo.Symbol, tcp => tcp.CategoryId = new Optional<ulong?>(ulong.Parse(dbCategoryChannelId)));

                string urlPooCoin = await _constantsHandler.GetConstantAsync(ConstantsNames.URL_POOCOIN);
                _ = (await coinChannel.SendMessageAsync(urlPooCoin + coinAddress)).PinAsync();

                Coins coin = new()
                {
                    Address = coinAddress,
                    Name = coinInfo.Name,
                    Symbol = coinInfo.Symbol,
                    IdChannel = coinChannel.Id.ToString()
                };
                _ = _buildAndExeApiCall.Post("coins", coin);

                await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = $"Moneda <#{coinChannel.Id}> añadida con éxito"; });
            }
            catch (Exception e)
            {
                await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "Ha ocurrido un error"; });
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
                await DeferAsync();

                var listAlerts = await _buildAndExeApiCall.GetWithOneParameter<Alerts>(HttpParameter.ParameterWithoutApostrophes("coinAddress", $"(select address from coins where idChannel = '{coinChannel.Id}')"));
                if (listAlerts.Count != 0)
                {
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "No se puede eliminar una moneda que tiene alertas activas de algún usuario"; });
                    return;
                }

                _ = coinChannel.DeleteAsync();
                _ = _buildAndExeApiCall.DeleteWithOneParameter("coins", HttpParameter.DefaultParameter("idChannel", coinChannel.Id.ToString()));

                await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = $"Moneda eliminada con éxito"; });
            }
            catch (Exception e)
            {
                await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "Ha ocurrido un error"; });
                _ = _logger.Log(exception: e);
            }
        }
    }
}
