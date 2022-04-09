using CryptoAlertsBot.ApiHandler;
using Discord.Commands;
using Discord.WebSocket;

namespace CryptoAlertsBot.Discord.Preconditions
{
    public class RequireAdminCategoryAttribute : PreconditionAttribute
    {
        public RequireAdminCategoryAttribute() { }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            try
            {
                ConstantsHandler constantsHandler = (ConstantsHandler)services.GetService(typeof(ConstantsHandler));

                if (context.User is SocketGuildUser gUser)
                {
                    string adminCategoryId = await constantsHandler.GetConstantAsync(ConstantsNames.ADMIN_CATEGORY_ID);
                    var adminCategory = (SocketCategoryChannel)(await context.Guild.GetCategoriesAsync())?.FirstOrDefault(w => w.Id.ToString() == adminCategoryId);

                    if ((bool)adminCategory?.Channels?.Any(channel => channel.Id == context.Channel.Id))
                        return PreconditionResult.FromSuccess();
                    else
                        return PreconditionResult.FromError($"You must be in admin category.");
                }
                else
                    return PreconditionResult.FromError("You must be in a guild to run this command.");
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
