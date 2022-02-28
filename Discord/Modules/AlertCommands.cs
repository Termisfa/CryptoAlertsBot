using CryptoAlertsBot.AlertsTypes;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using Discord.Interactions;
using Discord.WebSocket;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.Discord.Modules
{
    public class AlertCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ConstantsHandler _constantsHandler;
        public AlertCommands(ConstantsHandler constantsHandler)
        {
            _constantsHandler = constantsHandler;
        }

        [SlashCommand("alertanueva", "Añade una nueva alerta")]
        public async Task NewAlert(string coinChannelId, double price, string alertTypeWord)
        {
            try
            {
                if ((await MostUsedApiCalls.GetUserById(Context.User.Id.ToString())) == default)
                {
                    await RespondAsync("Error, para añadir una alerta primero debes darte de alta con el comando `!nuevousuario`");
                    return;
                }

                SocketTextChannel coinChannel = default;
                bool wrongChannel;

                coinChannelId = FormatChannelIdToNumberFormat(coinChannelId);

                if (ulong.TryParse(coinChannelId, out ulong coinChannelUlong))
                {
                    coinChannel = Context.Guild.GetTextChannel(coinChannelUlong);
                    wrongChannel = coinChannel == null;
                }
                else
                    wrongChannel = true;

                if (wrongChannel)
                {
                    await RespondAsync("Error, debe especificar el canal de la moneda");
                    return;
                }

                string parsedAlertType = AlertsHandler.GetAlertType(alertTypeWord);
                if (parsedAlertType == default)
                {
                    await RespondAsync(AlertsHandler.GetMessageWrongAlertTypeWord(alertTypeWord));
                    return;
                }

                string pinnedMessage = (await coinChannel.GetPinnedMessagesAsync())?.FirstOrDefault()?.Content;

                string coinAddress = pinnedMessage.Replace(_constantsHandler.GetConstant(ConstantsNames.URL_POOCOIN), "");

                Alerts alert = new()
                {
                    UserId = Context.User.Id.ToString(),
                    CoinAddress = coinAddress,
                    PriceUsd = price,
                    AlertType = parsedAlertType
                };

                _ = await BuildAndExeApiCall.Post("alerts", alert);

                await RespondAsync("Alerta añadida con éxito");
                CommonFunctionality.UpdateAlertsResume(Context);
            }
            catch (Exception e)
            {
                await RespondAsync("Ha ocurrido un error");
            }
        }

        [SlashCommand("borraralerta", "Elimina una alerta existente")]
        public async Task DeleteAlert(string coinChannelId, double price, string alertTypeWord)
        {
            try
            {
                SocketTextChannel coinChannel = default;
                bool wrongChannel;

                coinChannelId = FormatChannelIdToNumberFormat(coinChannelId);

                if (ulong.TryParse(coinChannelId, out ulong coinChannelUlong))
                {
                    coinChannel = Context.Guild.GetTextChannel(coinChannelUlong);
                    wrongChannel = coinChannel == null;
                }
                else
                    wrongChannel = true;

                if (wrongChannel)
                {
                    await RespondAsync("Error, debe especificar el canal de la moneda");
                    return;
                }

                string parsedAlertType = AlertsHandler.GetAlertType(alertTypeWord);
                if (parsedAlertType == default)
                {
                    await RespondAsync(AlertsHandler.GetMessageWrongAlertTypeWord(alertTypeWord));
                    return;
                }

                string pinnedMessage = (await coinChannel.GetPinnedMessagesAsync())?.FirstOrDefault()?.Content;

                string coinAddress = pinnedMessage.Replace(_constantsHandler.GetConstant(ConstantsNames.URL_POOCOIN), "");

                int deletedRows = await MostUsedApiCalls.DeleteAlert(Context.User.Id.ToString(), coinAddress, price.ToString(), parsedAlertType);

                if (deletedRows == 0)
                    await RespondAsync("No se ha encontrado la alerta");
                else
                {
                    await RespondAsync("Alerta eliminada");
                    CommonFunctionality.UpdateAlertsResume(Context);
                }
            }
            catch (Exception e)
            {
                await RespondAsync("Ha ocurrido un error");
            }
        }

        [SlashCommand("listalerts", "Muestra un listado de tus alertas")]
        public async Task ListAlerts()
        {
            try
            {
                _ = RespondAsync(await CommonFunctionality.GetAlertsMsg(Context));
            }
            catch (Exception e)
            {
                _ = RespondAsync("Ha ocurrido un error");
            }
        }
    }
}