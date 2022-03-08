using CryptoAlertsBot.AlertsTypes;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Discord.Preconditions;
using CryptoAlertsBot.Models;
using Discord.Interactions;
using Discord.WebSocket;
using System.Globalization;

namespace CryptoAlertsBot.Discord.Modules
{
    public class AlertCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ConstantsHandler _constantsHandler;
        private readonly Logger _logger;
        private readonly BuildAndExeApiCall _buildAndExeApiCall;
        private readonly MostUsedApiCalls _mostUsedApiCalls;
        private readonly CommonFunctionality _commonFunctionality;

        public AlertCommands(ConstantsHandler constantsHandler, Logger logger, BuildAndExeApiCall buildAndExeApiCall, MostUsedApiCalls mostUsedApiCalls, CommonFunctionality commonFunctionality)
        {
            _constantsHandler = constantsHandler;
            _logger = logger;
            _buildAndExeApiCall = buildAndExeApiCall;
            _mostUsedApiCalls = mostUsedApiCalls;
            _commonFunctionality = commonFunctionality;
        }

        [SlashCommand("nuevaalerta", "Añade una nueva alerta")]
        public async Task NewAlert(
            [Summary("Canal", "Canal de la moneda. Ejemplo: #WBNB")][IsCoinChannel] SocketTextChannel coinChannel,
            [Summary("Precio", "Precio en USD para la alerta. Ejemplo: 1.23")] string priceString,
            [Summary("Tipo", "Tipo de la alerta")] AlertsEnum alertType,
            [Summary("Tiempo", "Tiempo mínimo entre alertas")] TimeEnum timeBetweenAlerts
            )
        {
            try
            {
                string userId = Context.User.Id.ToString();

                if ((await _mostUsedApiCalls.GetUserById(userId)) == default)
                {
                    await RespondAsync("Error, para añadir una alerta primero debes darte de alta con el comando `!nuevousuario`");
                    return;
                }

                if (!double.TryParse(priceString.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double price))
                {
                    await RespondAsync($"Error, el precio `{priceString}` no es válido");
                    return;
                }

                string pinnedMessage = (await coinChannel.GetPinnedMessagesAsync())?.FirstOrDefault()?.Content;

                string coinAddress = pinnedMessage.Replace(_constantsHandler.GetConstant(ConstantsNames.URL_POOCOIN), "");

                Alerts alert = new()
                {
                    UserId = userId,
                    CoinAddress = coinAddress,
                    PriceUsd = price,
                    AlertType = alertType.ToString(),
                    HoursBetweenAlerts = (int)timeBetweenAlerts
                };

                _ = await _buildAndExeApiCall.Post("alerts", alert);

                await RespondAsync("Alerta añadida con éxito");
                _commonFunctionality.UpdateAlertsResume(Context);
            }
            catch (Exception e)
            {
                _ = RespondAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
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
                if (!double.TryParse(priceString.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double price))
                {
                    await RespondAsync($"Error, el precio `{priceString}` no es válido");
                    return;
                }

                string pinnedMessage = (await coinChannel.GetPinnedMessagesAsync())?.FirstOrDefault()?.Content;

                string coinAddress = pinnedMessage.Replace(_constantsHandler.GetConstant(ConstantsNames.URL_POOCOIN), "");

                int deletedRows = await _mostUsedApiCalls.DeleteAlert(Context.User.Id.ToString(), coinAddress, price.ToString(), alertType.ToString());

                if (deletedRows == 0)
                    await RespondAsync("No se ha encontrado la alerta");
                else
                {
                    await RespondAsync("Alerta eliminada");
                    await _commonFunctionality.UpdateAlertsResume(Context);
                }
            }
            catch (Exception e)
            {
                _ = RespondAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
            }
        }

        [SlashCommand("listalerts", "Muestra un listado de tus alertas")]
        public async Task ListAlerts()
        {
            try
            {
                _ = RespondAsync(await _commonFunctionality.GetAlertsMsg(Context));
            }
            catch (Exception e)
            {
                _ = RespondAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
            }
        }
    }
}