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
            [Summary("PrecioPorcentaje", "Si es precio, en USD. Ejemplo: 1.23. Si es porcentaje, debe terminar con %. Ejemplo: 10%")] string priceString,
            [Summary("Tipo", "Tipo de la alerta")] AlertsEnum alertType,
            [Summary("Tiempo", "Tiempo mínimo entre alertas. Si es porcentual, también será el tiempo que se usa para comparar")] TimeEnum timeBetweenAlerts
            )
        {
            try
            {
                await DeferAsync();

                string userId = Context.User.Id.ToString();

                if ((await _mostUsedApiCalls.GetUserById(userId)) == default)
                {
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "Error, para añadir una alerta primero debes darte de alta con el comando `!nuevousuario`"; });
                    return;
                }

                bool isPorcentual = AlertsHelper.IsPorcentual(priceString);
                priceString = AlertsHelper.RemoveSymbolIfExists(priceString.Trim());

                if (!double.TryParse(priceString.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double price))
                {
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = $"Error, el número `{priceString}` no es válido"; });
                    return;
                }

                string pinnedMessage = (await coinChannel.GetPinnedMessagesAsync())?.FirstOrDefault()?.Content;

                string coinAddress = pinnedMessage.Replace(await _constantsHandler.GetConstantAsync(ConstantsNames.URL_POOCOIN), "");

                Alerts alert = new()
                {
                    UserId = userId,
                    CoinAddress = coinAddress,
                    PriceUsd = price,
                    AlertType = alertType.ToString() + (isPorcentual ? "%" : ""),
                    HoursBetweenAlerts = (int)timeBetweenAlerts
                };

                _ = await _buildAndExeApiCall.Post("alerts", alert);

                await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "Alerta añadida con éxito"; });
                await _commonFunctionality.UpdateAlertsResume(Context);
            }
            catch (Exception e)
            {
                await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "Ha ocurrido un error"; });
                await _logger.Log(exception: e);
            }
        }

        [SlashCommand("borraralerta", "Elimina una alerta existente")]
        public async Task DeleteAlert(
            [Summary("Canal", "Canal de la moneda. Ejemplo: #WBNB")][IsCoinChannel] SocketTextChannel coinChannel,
            [Summary("PrecioPorcentaje", "Si es precio, en USD. Ejemplo: 1.23. Si es porcentaje, debe terminar con %. Ejemplo: 10%")] string priceString,
            [Summary("Tipo", "Tipo de la alerta")] AlertsEnum alertType
            )
        {
            try
            {
                await DeferAsync();

                bool isPorcentual = AlertsHelper.IsPorcentual(priceString);
                priceString = AlertsHelper.RemoveSymbolIfExists(priceString.Trim());

                if (!double.TryParse(priceString.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double price))
                {
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = $"Error, el precio `{priceString}` no es válido"; });
                    return;
                }

                string pinnedMessage = (await coinChannel.GetPinnedMessagesAsync())?.FirstOrDefault()?.Content;

                string coinAddress = pinnedMessage.Replace(await _constantsHandler.GetConstantAsync(ConstantsNames.URL_POOCOIN), "");

                int deletedRows = await _mostUsedApiCalls.DeleteAlert(Context.User.Id.ToString(), coinAddress, price.ToString(), alertType.ToString() + (isPorcentual ? "%" : ""));

                if (deletedRows == 0)
                {
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "No se ha encontrado la alerta"; });
                }
                else
                {
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "Alerta eliminada"; });
                    await _commonFunctionality.UpdateAlertsResume(Context);
                }
            }
            catch (Exception e)
            {
                await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "Ha ocurrido un error"; });
                await _logger.Log(exception: e);
            }
        }

        [SlashCommand("listalerts", "Muestra un listado de tus alertas")]
        public async Task ListAlerts()
        {
            try
            {
                await DeferAsync();

                string msg = await _commonFunctionality.GetAlertsMsg(Context);

                await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = msg; });
            }
            catch (Exception e)
            {
                await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "Ha ocurrido un error"; });
                await _logger.Log(exception: e);
            }
        }
    }
}