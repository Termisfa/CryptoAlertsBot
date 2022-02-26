using CryptoAlertsBot.AlertsTypes;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Discord.Preconditions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.Discord.Modules
{
    [RequireAdminChannel]
    //[RequireRole("Admin")]
    //[RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    //[RequireOwner]
    public class AdminCommands :  ModuleBase<SocketCommandContext>
    {
        [Command("CLEAR")]
        [Alias("BORRAR")]
        public async Task Clear()
        {
            try
            {
                _ = await ReplyAsync("test");

            }
            catch (Exception e)
            {
                _ = await ReplyAsync("Ha ocurrido un error");
            }
        }
        
    }
}