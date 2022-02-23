using CryptoAlertsBot.AlertsTypes;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using Discord.Commands;
using Discord.WebSocket;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.Discord.Modules
{
    public class AlertCommands : ModuleBase<SocketCommandContext>
    {
        [Command("NUEVAALERTA")]
        [Alias("ALERTANUEVA", "NEWALERT", "AÑADIRALERTA", "ALERTAAÑADIR", "ADDALERT")]
        public async Task NewAlert(string coinChannelId, double price, string alertTypeWord)
        {
            try
            {
                if((await MostUsedApiCalls.GetUserById(Context.User.Id.ToString())) == default)
                {
                    ReplyAsync("Error, para añadir una alerta primero debes darte de alta con el comando `!nuevousuario`");
                    return;
                }

                SocketTextChannel coinChannel = default;

                coinChannelId = FormatChannelId(coinChannelId);
                bool wrongChannel;

                if (ulong.TryParse(coinChannelId, out ulong coinChannelUlong))
                {
                    coinChannel = Context.Guild.GetTextChannel(coinChannelUlong);
                    wrongChannel = coinChannel == null;
                }
                else
                    wrongChannel = true;

                if (wrongChannel)
                {
                    ReplyAsync("Error, debe especificar el canal de la moneda");
                    return;
                }

                string parsedAlertType = AlertsHandler.GetAlertType(alertTypeWord);
                if (parsedAlertType == default)
                {
                    string answer = AlertsHandler.GetMessageWrongAlertTypeWord(alertTypeWord);
                    ReplyAsync(answer);
                    return;
                }

                string pinnedMessage = (await coinChannel.GetPinnedMessagesAsync())?.FirstOrDefault()?.Content;

                string coinAddress = pinnedMessage.Replace(await MostUsedApiCalls.GetConstantTextByName(ConstantsNames.URL_POOCOIN), "");

                Alerts alert = new()
                {
                    UserId = Context.User.Id.ToString(),
                    CoinAddress = coinAddress,
                    PriceUsd = price,
                    AlertType = parsedAlertType
                };

                _ = await BuildAndExeApiCall.Post("alerts", alert);

                ReplyAsync("Alerta añadida con éxito");
                CommonFunctionality.UpdateAlertsResume(Context);

            }
            catch (Exception e)
            {
                await ReplyAsync("Ha ocurrido un error");
            }
        }




        //public async Task NewUser([Remainder][Summary("The text to echo")] string echo)

    }
}
