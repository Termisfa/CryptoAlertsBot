using CryptoAlertsBot.AlertsTypes;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Discord.Preconditions;
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
        
        [SlashCommand("nuevaalerta", "Añade una nueva alerta")]
        public async Task NewAlert(
            [Summary("Canal", "Canal de la moneda. Ejemplo: #WBNB")][IsCoinChannel] SocketTextChannel coinChannel,
            [Summary("Precio", "Precio en USD para la alerta. Ejemplo: 1.23")] string priceString,
            [Summary("Tipo", "Tipo de la alerta")] AlertsEnum alertType
            )
        {
            try
            {
                if ((await MostUsedApiCalls.GetUserById(Context.User.Id.ToString())) == default)
                {
                    await RespondAsync("Error, para añadir una alerta primero debes darte de alta con el comando `!nuevousuario`");
                    return;
                }

                if (!double.TryParse(priceString.Replace('.', ','), out double price))
                {
                    await RespondAsync($"Error, el precio `{priceString}` no es válido");
                    return;
                }

                string pinnedMessage = (await coinChannel.GetPinnedMessagesAsync())?.FirstOrDefault()?.Content;

                string coinAddress = pinnedMessage.Replace(_constantsHandler.GetConstant(ConstantsNames.URL_POOCOIN), "");

                Alerts alert = new()
                {
                    UserId = Context.User.Id.ToString(),
                    CoinAddress = coinAddress,
                    PriceUsd = price,
                    AlertType = alertType.ToString()
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
        public async Task DeleteAlert(
            [Summary("Canal", "Canal de la moneda. Ejemplo: #WBNB")][IsCoinChannel] SocketTextChannel coinChannel,
            [Summary("Precio", "Precio en USD para la alerta. Ejemplo: 1.23")] string priceString,
            [Summary("Tipo", "Tipo de la alerta")] AlertsEnum alertType
            )
        {
            try
            {
                if (!double.TryParse(priceString.Replace('.', ','), out double price))
                {
                    await RespondAsync($"Error, el precio `{priceString}` no es válido");
                    return;
                }

                string pinnedMessage = (await coinChannel.GetPinnedMessagesAsync())?.FirstOrDefault()?.Content;

                string coinAddress = pinnedMessage.Replace(_constantsHandler.GetConstant(ConstantsNames.URL_POOCOIN), "");

                int deletedRows = await MostUsedApiCalls.DeleteAlert(Context.User.Id.ToString(), coinAddress, price.ToString(), alertType.ToString());

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