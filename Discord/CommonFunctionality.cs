using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using CryptoAlertsBot.Models.MergedModels;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoAlertsBot.Discord
{
    public static class CommonFunctionality
    {
        public static async void UpdateAlertsResume(SocketInteractionContext context)
        {
            string alertsMsg = await GetAlertsMsg(context);

            var categoryChannelId = await GetCategoryChannelIdFromUserId(context.User.Id.ToString());

            var resumeChannel = (SocketTextChannel)context.Guild.GetCategoryChannel(categoryChannelId).Channels?.FirstOrDefault(w => w.Name.Contains("resumen"));

            (await resumeChannel?.GetPinnedMessagesAsync())?.FirstOrDefault(w => w.Content.Contains("Resumen de alertas actuales"))?.DeleteAsync();

            (await resumeChannel.SendMessageAsync(alertsMsg))?.PinAsync();
        }

        public static async Task<ulong> GetCategoryChannelIdFromUserId(string userId)
        {
            var user = await MostUsedApiCalls.GetUserByIdForcingOne(userId);

            return ulong.Parse(user.IdCategoryChannel);
        }

        public static async Task<string> GetAlertsMsg(SocketInteractionContext context)
        {
            Dictionary<string, string> arguments = new();
            arguments.Add("coinAddress", "%address");
            arguments.Add("userId", context.User.Id.ToString());

            var queryResult = await BuildAndExeApiCall.GetWithMultipleArguments<AlertsCoins>(arguments, "alerts,coins");

            string msg = "Resumen de alertas actuales: \n";

            if (queryResult.Count == 0)
                msg += "No tienes ninguna alerta";
            else
            {
                foreach (AlertsCoins alertCoin in queryResult)
                    msg += "Moneda: <#" + alertCoin.Coin.IdChannel + "> PrecioUSD: `" + alertCoin.Alert.PriceUsd + "` Tipo de alerta: `" + alertCoin.Alert.AlertType + "`\n";
            }

            return msg;
        }

        public static async Task<string> FormatPriceToDatabaseChannelAsync(Prices price, Prices previousPrice)
        {
            string emote = string.Empty;

            if (previousPrice != null)
                emote = price.PriceUsd > previousPrice.PriceUsd ? ":point_up_2:" : ":point_down:";

            string result = "-----------------------------------------------------------\n";

            result += $"Actualizado: `{price.PriceDate.Value.ToString("dd MMM yyyy HH:mm:ss")}`\n";
            result += $" {emote} Precio USD: {await GetRoundedPriceAsync(price.PriceUsd)}";

            return result;
        }

        public static async Task<string> GetRoundedPriceAsync(double price)
        {
            int priceLength = int.Parse(await MostUsedApiCalls.GetConstantTextByName(ConstantsNames.PRICE_LENGTH));

            string result = price.ToString().Substring(0, priceLength);
            return result;
        }
    }
}
