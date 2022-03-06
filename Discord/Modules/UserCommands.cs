using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.Discord.Modules
{
    public class UserCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly Logger _logger;
        private readonly BuildAndExeApiCall _buildAndExeApiCall;
        private readonly MostUsedApiCalls _mostUsedApiCalls;

        public UserCommands(Logger logger, BuildAndExeApiCall buildAndExeApiCall, MostUsedApiCalls mostUsedApiCalls)
        {
            _logger = logger;
            _buildAndExeApiCall = buildAndExeApiCall;
            _mostUsedApiCalls = mostUsedApiCalls;
        }

        [SlashCommand("nuevousuario", "Es necesario para utilizar las alertas")]
        public async Task NewUser()
        {
            try
            {
                var userId = Context.User.Id.ToString();
                Users user = await _mostUsedApiCalls.GetUserById(userId);

                if (user != default)
                {
                    if (user.Active)
                        await RespondAsync($"El usuario <@{userId}> ya estaba activo");
                    else
                    {
                        user.Active = true;
                        await _mostUsedApiCalls.UpdateUserById(userId, user);
                        await RespondAsync($"Usuario <@{userId}> reactivado");
                    }
                    return;
                }

                var role = await Context.Guild.CreateRoleAsync(Context.User.Username + " Rol", isMentionable: false);
                _ = (Context.User as IGuildUser).AddRoleAsync(role);

                var categoryChannel = await Context.Guild.CreateCategoryChannelAsync("Zona personal " + Context.User.Username);
                _ = categoryChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, OverwritePermissions.DenyAll(categoryChannel));
                _ = categoryChannel.AddPermissionOverwriteAsync(role, OverwritePermissions.AllowAll(categoryChannel));

                _ =Context.Guild.CreateTextChannelAsync("alertas-" + Context.User.Username, tcp => tcp.CategoryId = categoryChannel.Id);
                _ = Context.Guild.CreateTextChannelAsync("resumen-" + Context.User.Username, tcp => tcp.CategoryId = categoryChannel.Id);

                user = new()
                {
                    Id = userId,
                    Name = Context.User.Username,
                    Active = true,
                    IdCategoryChannel = categoryChannel.Id.ToString()
                };

                var affectedRows = await _buildAndExeApiCall.Post("users", user);

                if (affectedRows == 1)
                    await RespondAsync($"Usuario <@{userId}> añadido con éxito");
                else
                    await RespondAsync("Ha ocurrido un error");
            }
            catch (Exception e)
            {
                _ = RespondAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
            }
        }

        [SlashCommand("bajausuario", "Desactiva todas las alertas")]
        public async Task DeleteUser()
        {
            try
            {
                var userId = Context.User.Id.ToString();

                Users user = new()
                {
                    Active = false
                };

                var affectedRows = await _buildAndExeApiCall.PutWithOneArgument("users", user, "id", userId);

                if (affectedRows != 1)
                    await RespondAsync($"El usuario <@{userId}> no estaba dado de alta");
                else
                    await RespondAsync($"Usuario <@{userId}> dado de baja con éxito");
            }
            catch (Exception e)
            {
                _ = RespondAsync("Ha ocurrido un error");
                _ = _logger.Log(exception: e);
            }
        }



        //public async Task NewUser([Remainder][Summary("The text to echo")] string echo)

    }

 //   // Create a module with the 'sample' prefix
 //   [Group("sample")]
	//public class SampleModule : ModuleBase<SocketCommandContext>
	//{
	//	// ~sample square 20 -> 400
	//	[Command("square")]
	//	[Summary("Squares a number.")]
	//	public async Task SquareAsync(
	//		[Summary("The number to square.")]
	//	int num)
	//	{
	//		// We can also access the channel from the Command Context.
	//		await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
	//	}

	//	// ~sample userinfo --> foxbot#0282
	//	// ~sample userinfo @Khionu --> Khionu#8708
	//	// ~sample userinfo Khionu#8708 --> Khionu#8708
	//	// ~sample userinfo Khionu --> Khionu#8708
	//	// ~sample userinfo 96642168176807936 --> Khionu#8708
	//	// ~sample whois 96642168176807936 --> Khionu#8708
	//	[Command("userinfo")]
	//	[Summary
	//	("Returns info about the current user, or the user parameter, if one passed.")]
	//	[Alias("user", "whois")]
	//	public async Task UserInfoAsync(
	//		[Summary("The (optional) user to get info from")]
	//	SocketUser user = null)
	//	{
	//		var userInfo = user ?? Context.Client.CurrentUser;
	//		await await RespondAsync($"{userInfo.Username}#{userInfo.Discriminator}");
	//	}
	//}
}
