using CryptoAlertsBot.AlertsTypes;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Discord.Preconditions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.Discord.Modules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireAdminCategory(Group = "Permission")]
    public class AdminCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ConstantsHandler _constantsHandler;
        public AdminCommands(ConstantsHandler constantsHandler)
        {
            _constantsHandler = constantsHandler;
        }

        [Command("CLEAR")]
        [Alias("BORRAR")]
        public async Task Clear(int amount = 100)
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
            }
        }

        [Command("NEWCONSTANT")]
        [Alias("ADDCONSTANT")]
        public async Task NewConstant(string key, [Remainder] string value)
        {
            try
            {
                if (_constantsHandler.AddConstant(key, value))
                    _ = ReplyAsync("Constante añadida con éxito");
                else
                    _ = ReplyAsync("La constante ya existe");

            }
            catch (Exception e)
            {
                _ = ReplyAsync("Ha ocurrido un error");
            }
        }

        [Command("LISTCONSTANTS")]
        [Alias("CONSTANTSLIST")]
        public async Task ConstantsList()
        {
            try
            {
                _ = ReplyAsync(_constantsHandler.ListConstants());

            }
            catch (Exception e)
            {
                _ = ReplyAsync("Ha ocurrido un error");
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
            }
        }

        [Command("MODIFYCONSTANT")]
        [Alias("UPDATECONSTANT", "CONSTANTUPDATE")]
        public async Task UpdateConstant(string key, [Remainder] string value)
        {
            try
            {
                if (_constantsHandler.UpdateConstant(key, value))
                    _ = ReplyAsync("Constante actualizada con éxito");
                else
                    _ = ReplyAsync("La constante no existe");

            }
            catch (Exception e)
            {
                _ = ReplyAsync("Ha ocurrido un error");
            }
        }


        //ADD COMMAND TO DELETE USER ONLY FOR ADMIN. IT SHOULD DELETE ROLE, CHANNELS AND ALL HIS INFO IN THE DATABASE

    }
}