using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using Discord;
using Discord.Interactions;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.Discord.Modules
{

    public class CoinCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ConstantsHandler _constantsHandler;
        public CoinCommands(ConstantsHandler constantsHandler)
        {
            _constantsHandler = constantsHandler;
        }

        [SlashCommand("nuevamoneda", "Añade una nueva moneda. Introducir la address")]
        public async Task NewCoin(string coinAddress)
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
                await RespondAsync("Ha ocurrido un error");
            }
        }

        [SlashCommand("olvidarmoneda", "Elimina una moneda. Especificar el canal")]
        public async Task DeleteCoin(string coinChannel)
        {
            try
            {
                coinChannel = FormatChannelId(coinChannel);
                var channel = Context.Guild.GetTextChannel(ulong.Parse(coinChannel));

                if (channel == null)
                {
                    await RespondAsync("Error, debe especificar el canal de la moneda");
                    return;
                }

                var listAlerts = await BuildAndExeApiCall.GetWithOneArgument<Alerts>("coinAddress", $"%(select address from coins where idChannel = '{channel.Id}')");
                if (listAlerts.Count != 0)
                {
                    await RespondAsync("No se puede eliminar una moneda que tiene alertas activas de algún usuario");
                    return;
                }

                _ = channel.DeleteAsync();
                _ = BuildAndExeApiCall.DeleteWithOneArgument("coins", "idChannel", coinChannel);

                await RespondAsync($"Moneda eliminada con éxito");
            }
            catch (Exception e)
            {
                await RespondAsync("Ha ocurrido un error");
            }
        }


        //public async Task NewUser([Remainder][Summary("The text to echo")] string echo)

    }
}
