using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models.MergedModels;
using Discord.Commands;
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
        public static async void UpdateAlertsResume(ICommandContext context)
        {
            string alertsMsg = await GetAlertsMsg(context);

            var categoryChannelId = await GetCategoryChannelIdFromUserId(context.User.Id.ToString());

            var resumeChannel = (SocketTextChannel)((SocketCategoryChannel)(await context.Guild.GetCategoriesAsync())?.FirstOrDefault(w => w.Id == categoryChannelId))?.Channels?.FirstOrDefault(w => w.Name.Contains("resumen"));

            (await resumeChannel?.GetPinnedMessagesAsync())?.FirstOrDefault(w => w.Content.Contains("Resumen de alertas actuales"))?.DeleteAsync();

            (await resumeChannel.SendMessageAsync(alertsMsg))?.PinAsync();

        }

        public static async Task<ulong> GetCategoryChannelIdFromUserId(string userId)
        {
            var user = await MostUsedApiCalls.GetUserByIdForcingOne(userId);

            return ulong.Parse(user.IdCategoryChannel);
        }

        public static async Task<string> GetAlertsMsg(ICommandContext context)
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
    }
}
