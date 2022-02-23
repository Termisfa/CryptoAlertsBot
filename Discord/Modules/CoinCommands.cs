using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using Discord;
using Discord.Commands;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.Discord.Modules
{

    // Create a module with no prefix
    public class CoinCommands : ModuleBase<SocketCommandContext>
    {
        [Command("NUEVAMONEDA")]
        [Alias("MONEDANUEVA", "AÑADIRMONEDA", "ADDCOIN", "COINADD", "NEWCOIN")]
        public async Task NewCoin(string coinAddress)
        {
            try
            {
                coinAddress = coinAddress.ToLower().Trim();

                var coins = await BuildAndExeApiCall.GetWithOneArgument<Coins>("address", coinAddress);
                if (coins.Count != 0)
                {
                    ReplyAsync($"La moneda <#{coins[0].IdChannel}> ya existe");
                    return;
                }

                string urlApi = await MostUsedApiCalls.GetConstantTextByName(ConstantsNames.URL_API);
                ResultPancakeSwapApi coinInfo = await MostUsedApiCalls.GetFromPancakeSwapApi(urlApi, coinAddress);
                if(coinInfo == default)
                {
                    ReplyAsync($"La moneda '{coinAddress}' no existe en PancakeSwap");
                    return;
                }

                string dbCategoryChannelId = await MostUsedApiCalls.GetConstantTextByName(ConstantsNames.DB_CATEGORY_CHANNEL_ID);
                var coinChannel = await Context.Guild.CreateTextChannelAsync(coinInfo.Symbol, tcp => tcp.CategoryId = new Optional<ulong?>(ulong.Parse(dbCategoryChannelId)));

                string urlPooCoin = await MostUsedApiCalls.GetConstantTextByName(ConstantsNames.URL_POOCOIN);
                (await coinChannel.SendMessageAsync(urlPooCoin + coinAddress)).PinAsync();

                Coins coin = new()
                {
                    Address = coinAddress,
                    Name = coinInfo.Name,
                    Symbol = coinInfo.Symbol,
                    IdChannel = coinChannel.Id.ToString()
                };
                BuildAndExeApiCall.Post("coins", coin);

                await ReplyAsync($"Moneda <#{coinChannel.Id}> añadida con éxito");
            }
            catch (Exception e)
            {
                await ReplyAsync("Ha ocurrido un error");
            }
        }

        [Command("OLVIDARMONEDA")]
        [Alias("DELETECOIN", "COINDELETE", "BORRARMONEDA", "SUPRIMIRMONEDA")]
        public async Task DeleteCoin(string coinChannelId)
        {
            try
            {
                coinChannelId = FormatChannelId(coinChannelId);
                var coinChannel = Context.Guild.GetTextChannel(ulong.Parse(coinChannelId));

                if(coinChannel == null)
                {
                    await ReplyAsync("Error, debe especificar el canal de la moneda");
                    return;
                }

                var listAlerts = await BuildAndExeApiCall.GetWithOneArgument<Alerts>("coinAddress", $"(select address from coins where idChannel = '{coinChannel.Id}')");
                if(listAlerts.Count != 0)
                {
                    await ReplyAsync("No se puede eliminar una moneda que tiene alertas activas de algún usuario");
                    return;
                }

                coinChannel.DeleteAsync();
                BuildAndExeApiCall.DeleteWithOneArgument("coins", "idChannel", coinChannelId);

                await ReplyAsync($"Moneda eliminada con éxito");
            }
            catch (Exception e)
            {
                await ReplyAsync("Ha ocurrido un error");
            }
        }


        //public async Task NewUser([Remainder][Summary("The text to echo")] string echo)

    }
}
