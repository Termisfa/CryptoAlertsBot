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
                if ((await MostUsedApiCalls.GetUserById(Context.User.Id.ToString())) == default)
                {
                    _ = ReplyAsync("Error, para añadir una alerta primero debes darte de alta con el comando `!nuevousuario`");
                    return;
                }

                SocketTextChannel coinChannel = default;
                bool wrongChannel;

                coinChannelId = FormatChannelId(coinChannelId);

                if (ulong.TryParse(coinChannelId, out ulong coinChannelUlong))
                {
                    coinChannel = Context.Guild.GetTextChannel(coinChannelUlong);
                    wrongChannel = coinChannel == null;
                }
                else
                    wrongChannel = true;

                if (wrongChannel)
                {
                    _ = ReplyAsync("Error, debe especificar el canal de la moneda");
                    return;
                }

                string parsedAlertType = AlertsHandler.GetAlertType(alertTypeWord);
                if (parsedAlertType == default)
                {
                    _ = ReplyAsync(AlertsHandler.GetMessageWrongAlertTypeWord(alertTypeWord));
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

                _ = ReplyAsync("Alerta añadida con éxito");
                CommonFunctionality.UpdateAlertsResume(Context);
            }
            catch (Exception e)
            {
                _ = await ReplyAsync("Ha ocurrido un error");
            }
        }

        [Command("BORRARALERTA")]
        [Alias("ALERTABORRAR", "DELETEALERT", "ALERTDELETE", "REMOVEALERT", "ALERTREMOVE")]
        public async Task DeleteAlert(string coinChannelId, double price, string alertTypeWord)
        {
            try
            {
                SocketTextChannel coinChannel = default;
                bool wrongChannel;

                coinChannelId = FormatChannelId(coinChannelId);

                if (ulong.TryParse(coinChannelId, out ulong coinChannelUlong))
                {
                    coinChannel = Context.Guild.GetTextChannel(coinChannelUlong);
                    wrongChannel = coinChannel == null;
                }
                else
                    wrongChannel = true;

                if (wrongChannel)
                {
                    _ = ReplyAsync("Error, debe especificar el canal de la moneda");
                    return;
                }

                string parsedAlertType = AlertsHandler.GetAlertType(alertTypeWord);
                if (parsedAlertType == default)
                {
                    _ = ReplyAsync(AlertsHandler.GetMessageWrongAlertTypeWord(alertTypeWord));
                    return;
                }

                string pinnedMessage = (await coinChannel.GetPinnedMessagesAsync())?.FirstOrDefault()?.Content;

                string coinAddress = pinnedMessage.Replace(await MostUsedApiCalls.GetConstantTextByName(ConstantsNames.URL_POOCOIN), "");

                int deletedRows = await MostUsedApiCalls.DeleteAlert(Context.User.Id.ToString(), coinAddress, price.ToString(), parsedAlertType);

                if (deletedRows == 0)
                    _ = ReplyAsync("No se ha encontrado la alerta");
                else
                {
                    _ = ReplyAsync("Alerta eliminada");
                    CommonFunctionality.UpdateAlertsResume(Context);
                }
            }
            catch (Exception e)
            {
                _ = await ReplyAsync("Ha ocurrido un error");
            }
        }

        [Command("LISTALERTS")]
        [Alias("ALERTSLIST", "ALERTASLISTAR", "LISTADOALERTAS", "LISTARALERTAS")]
        public async Task ListAlerts()
        {
            try
            {
                _ = ReplyAsync(await CommonFunctionality.GetAlertsMsg(Context));
            }
            catch (Exception e)
            {
                _ = await ReplyAsync("Ha ocurrido un error");
            }
        }
    }
}