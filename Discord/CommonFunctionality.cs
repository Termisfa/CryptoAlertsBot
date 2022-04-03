using CryptoAlertsBot.AlertsTypes;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using CryptoAlertsBot.Models.MergedModels;
using Discord.Interactions;
using Discord.WebSocket;
using GenericApiHandler.Models;

namespace CryptoAlertsBot.Discord
{
    public class CommonFunctionality
    {

        private readonly BuildAndExeApiCall _buildAndExeApiCall;
        private readonly MostUsedApiCalls _mostUsedApiCalls;

        public CommonFunctionality(BuildAndExeApiCall buildAndExeApiCall, MostUsedApiCalls mostUsedApiCalls)
        {
            _buildAndExeApiCall = buildAndExeApiCall;
            _mostUsedApiCalls = mostUsedApiCalls;
        }

        public async Task UpdateAlertsResume(SocketInteractionContext context)
        {
            try
            {
                string alertsMsg = await GetAlertsMsg(context);

                var categoryChannelId = await GetCategoryChannelIdFromUserId(context.User.Id.ToString());

                var resumeChannel = (SocketTextChannel)context.Guild.GetCategoryChannel(categoryChannelId).Channels?.FirstOrDefault(w => w.Name.Contains("resumen"));

                (await resumeChannel?.GetPinnedMessagesAsync())?.FirstOrDefault(w => w.Content.Contains("Resumen de alertas actuales"))?.DeleteAsync();

                (await resumeChannel.SendMessageAsync(alertsMsg))?.PinAsync();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<ulong> GetCategoryChannelIdFromUserId(string userId)
        {
            try
            {
                var user = await _mostUsedApiCalls.GetUserById(userId);

                return ulong.Parse(user.IdCategoryChannel);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<string> GetAlertsMsg(SocketInteractionContext context)
        {
            try
            {
                List<HttpParameter> parameters = new();
                parameters.Add(HttpParameter.ParameterWithoutApostrophes( "coinAddress", "address"));
                parameters.Add(HttpParameter.DefaultParameter("userId", context.User.Id.ToString()));

                var queryResult = await _buildAndExeApiCall.GetWithMultipleParameters<AlertsCoins>(parameters, "alerts,coins");

                string msg = "Resumen de alertas actuales: \n";

                if (queryResult.Count == 0)
                    msg += "No tienes ninguna alerta";
                else
                {
                    foreach (AlertsCoins alertCoin in queryResult)
                    {
                        bool isPorcentual = AlertsHelper.IsPorcentual(alertCoin.Alert.AlertType);
                        string alertType = AlertsHelper.RemoveSymbolIfExists(alertCoin.Alert.AlertType);
                        string priceOrPercent = isPorcentual ? "Porcentaje" : "PrecioUSD";
                        string symbol = isPorcentual ? "%" : "USD";

                        msg += $"Moneda: <#{alertCoin.Coin.IdChannel}>." 
                            + $" {priceOrPercent}: `{alertCoin.Alert.PriceUsd}`{symbol}." 
                            + $" Tipo de alerta: `{alertType}`." 
                            + $" Tiempo entre alertas: `{((TimeEnum)alertCoin.Alert.HoursBetweenAlerts).ToString()}`\n";
                    }
                }

                return msg;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<string> FormatPriceToDatabaseChannelAsync(Prices price, Prices previousPrice)
        {
            try
            {
                string emote = string.Empty;
                if (previousPrice != null)
                    emote = price.PriceUsd > previousPrice.PriceUsd ? ":point_up_2:" : ":point_down:";
                string result = new string('-', 60) + "\n";
                result += $"Actualizado: `{price.PriceDate.Value.ToString("dd MMM yyyy HH:mm:ss")}`\n";
                result += $" {emote} Precio USD: {await GetRoundedPriceAsync(price.PriceUsd)}";
                return result;
            }
            catch (Exception e) { throw; }
        }

        public async Task<string> FormatPriceToResumeChannelAsync(Coins coin, Prices price, string urlPooCoin)
        {
            try
            {
                string result = new string('-', 60) + "\n";
                result += $"Nombre: `{coin.Name}`\n";
                result += $"Símbolo: `{coin.Symbol}`\n";
                result += $"Canal: {Helpers.Helpers.FormatChannelIdToDiscordFormat(coin.IdChannel)}\n";
                result += $"Actualizado: `{price.PriceDate.Value.ToString("dd MMM yyyy HH:mm:ss")}`\n";
                result += $"Precio USD: `{price.PriceUsd}`\n";
                result += urlPooCoin + coin.Address;
                return result;
            }
            catch (Exception e) { throw; }
        }

        public async Task<string> GetRoundedPriceAsync(double price)
        {
            try
            {
                int priceLength = int.Parse(await _mostUsedApiCalls.GetConstantTextByName(ConstantsNames.PRICE_LENGTH));
                string result = price.ToString();
                if (result.Length > priceLength)
                    result = result.Substring(0, priceLength);
                return result;
            }
            catch (Exception e) { throw; }
        }
    }
}
