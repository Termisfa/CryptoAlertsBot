using CryptoAlertsBot.AlertsTypes;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Discord.Preconditions;
using CryptoAlertsBot.Helpers;
using CryptoAlertsBot.Models;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GenericApiHandler.Models;

namespace CryptoAlertsBot.Discord.Modules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireAdminCategory(Group = "Permission")]
    public class AdminCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ConstantsHandler _constantsHandler;
        private readonly Logger _logger;
        private readonly MostUsedApiCalls _mostUsedApiCalls;
        private readonly BuildAndExeApiCall _buildAndExeApiCall;

        public AdminCommands(ConstantsHandler constantsHandler, Logger logger, MostUsedApiCalls mostUsedApiCalls, BuildAndExeApiCall buildAndExeApiCall)
        {
            _constantsHandler = constantsHandler;
            _logger = logger;
            _mostUsedApiCalls = mostUsedApiCalls;
            _buildAndExeApiCall = buildAndExeApiCall;
        }

        [Command("CLEAR")]
        [Alias("BORRAR")]
        public async Task Clear(int amount = 99)
        {
            try
            {
                var messages = await Context.Channel.GetMessagesAsync(amount + 1).Flatten().ToListAsync();

                await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
                const int delay = 5000;
                var m = await this.ReplyAsync($"Purge completed. _This message will be deleted in {delay / 1000} seconds._");
                await Task.Delay(delay);
                await m.DeleteAsync();
            }
            catch (Exception e)
            {
                _ = ReplyAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
            }
        }

        [Command("NEWCONSTANT")]
        [Alias("ADDCONSTANT")]
        public async Task NewConstant(string key, [Remainder] string value)
        {
            try
            {
                if (await _constantsHandler.AddConstantAsync(key, value))
                    _ = ReplyAsync("Constante añadida con éxito");
                else
                    _ = ReplyAsync("La constante ya existe");
            }
            catch (Exception e)
            {
                _ = ReplyAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
            }
        }

        [Command("LISTCONSTANTS")]
        [Alias("CONSTANTSLIST")]
        public async Task ConstantsList()
        {
            try
            {
                _ = ReplyAsync(await _constantsHandler.ListConstantsAsync());
            }
            catch (Exception e)
            {
                _ = ReplyAsync("Ha ocurrido un error");
                _ = _logger.Log(e.Message);
            }
        }

        [Command("DELETECONSTANT")]
        [Alias("REMOVECONSTANT")]
        public async Task DeleteConstant(string key)
        {
            try
            {
                if (await _constantsHandler.DeleteConstantAsync(key))
                    _ = ReplyAsync("Constante eliminada con éxito");
                else
                    _ = ReplyAsync("La constante no existe");
            }
            catch (Exception e)
            {
                _ = ReplyAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
            }
        }

        [Command("MODIFYCONSTANT")]
        [Alias("UPDATECONSTANT", "CONSTANTUPDATE")]
        public async Task UpdateConstant(string key, [Remainder] string value)
        {
            try
            {
                if (await _constantsHandler.UpdateConstantAsync(key, value))
                    _ = ReplyAsync("Constante actualizada con éxito");
                else
                    _ = ReplyAsync("La constante no existe");
            }
            catch (Exception e)
            {
                _ = ReplyAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
            }
        }

        [Command("DELETEUSER")]
        [Alias("BORRARUSUARIO", "USERDELETE")]
        public async Task DeleteUser(string user)
        {
            try
            {
                string userId = GenericHelpers.FormatUserIdToNumberFormat(user);
                Users userInfo = await _mostUsedApiCalls.GetUserById(userId);

                if (userInfo == null)
                {
                    _ = ReplyAsync("El usuario no existe");
                    return;
                }

                var categoryChannel = Context.Guild.CategoryChannels.First(w => w.Id == ulong.Parse(userInfo.IdCategoryChannel));

                ulong roleId = categoryChannel.PermissionOverwrites.FirstOrDefault(w => !Context.Guild.GetRole(w.TargetId).IsEveryone).TargetId;
                _ = Context.Guild.GetRole(roleId).DeleteAsync();

                foreach (var channel in categoryChannel.Channels)
                {
                    await channel.DeleteAsync();
                }
                _ = categoryChannel.DeleteAsync();

                _ = _buildAndExeApiCall.DeleteWithOneParameter("users", HttpParameter.DefaultParameter("Id", userId));
                _ = _buildAndExeApiCall.DeleteWithOneParameter("alerts", HttpParameter.DefaultParameter("UserId", userId));

                _ = ReplyAsync("Usuario completamente eliminado con éxito");
            }
            catch (Exception e)
            {
                _ = ReplyAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
            }
        }

        //[Command("TEST")]
        //[Alias("T")]
        //public async Task Test([Remainder] string value)
        //{
        //    try
        //    {
        //        var x = await ApiCalls.ExeCall(GenericApiHandler.Data.Enums.ApiCallTypesEnum.Get, "/v1/currencies/ticker?key=c4b9d2f6d5933295ccce939ae2ff00abf10349d6&ids=BTC,ETH,XRP&interval=1d,30d&convert=EUR", baseAddress: "https://api.nomics.com");

        //    }
        //    catch (Exception e)
        //    {
        //        _ = ReplyAsync("Ha ocurrido un error");
        //        _ = _logger.Log(exception: e);
        //    }
        //}
    }
}