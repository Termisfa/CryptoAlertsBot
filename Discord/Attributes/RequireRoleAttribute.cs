using Discord.Commands;
using Discord.WebSocket;

namespace CryptoAlertsBot.Discord.Preconditions
{
    public class RequireRoleAttribute : PreconditionAttribute
    {
        private readonly string _name;

        public RequireRoleAttribute(string name) => _name = name;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            try
            {
                if (context.User is SocketGuildUser gUser)
                {
                    if (gUser.Roles.Any(r => r.Name == _name))
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    else
                        return Task.FromResult(PreconditionResult.FromError($"You must have a role named {_name} to run this command."));
                }
                else
                    return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
