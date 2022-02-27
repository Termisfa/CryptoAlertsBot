using CryptoAlertsBot.Discord.Preconditions;
using Discord;
using Discord.Commands;

namespace CryptoAlertsBot.Discord.Modules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireAdminCategory(Group = "Permission")]

    //[RequireRole("Admin", Group = "Permission")]
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