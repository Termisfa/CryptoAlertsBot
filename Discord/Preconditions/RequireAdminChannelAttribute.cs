using CryptoAlertsBot.ApiHandler;
using Discord.Commands;
using Discord.WebSocket;

namespace CryptoAlertsBot.Discord.Preconditions
{
    public class RequireAdminChannelAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User is SocketGuildUser gUser)
            {
                string adminChannelId = await MostUsedApiCalls.GetConstantTextByName(ConstantsNames.ADMIN_CATEGORY_ID);
                if (context.Channel.Id.ToString() == adminChannelId)
                //if (gUser.Roles.Any(r => r.Name == _name))
                    return PreconditionResult.FromSuccess();
                else
                    return PreconditionResult.FromError($"You must be in admin channel.");
            }
            else
                return PreconditionResult.FromError("You must be in a guild to run this command.");
        }
    }
}
