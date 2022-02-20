using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.Discord.Modules
{

    // Create a module with no prefix
    public class UserCommands : ModuleBase<SocketCommandContext>
    {
        [Command("NUEVOUSUARIO")]
        [Alias("USUARIONUEVO", "ADDUSER", "AÑADIRUSUARIO", "USERADD", "NEWUSER")]
        //[Summary("Probando.")]
        public async Task NewUser()
        {
            try
            {
                var userId = Context.User.Id.ToString();
                Users user = await MostUsedApiCalls.GetUserById(userId);

                if (user != default)
                {
                    if (user.Active)
                        ReplyAsync($"El usuario <@{userId}> ya estaba activo");
                    else
                    {
                        user.Active = true;
                        await MostUsedApiCalls.UpdateUserById(userId, user);
                        ReplyAsync($"Usuario <@{userId}> reactivado");
                    }
                    return;
                }

                var role = await Context.Guild.CreateRoleAsync(Context.User.Username + " Rol", isMentionable: false);
                (Context.User as IGuildUser).AddRoleAsync(role);

                var categoryChannel = await Context.Guild.CreateCategoryChannelAsync("Zona personal " + Context.User.Username);
                categoryChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, OverwritePermissions.DenyAll(categoryChannel));
                categoryChannel.AddPermissionOverwriteAsync(role, OverwritePermissions.AllowAll(categoryChannel));

                Context.Guild.CreateTextChannelAsync("alertas-" + Context.User.Username, tcp => tcp.CategoryId = categoryChannel.Id);
                Context.Guild.CreateTextChannelAsync("resumen-" + Context.User.Username, tcp => tcp.CategoryId = categoryChannel.Id);

                user = new()
                {
                    Id = userId,
                    Name = Context.User.Username,
                    Active = true,
                    IdCategoryChannel = categoryChannel.Id.ToString()
                };

                var affectedRows = await BuildAndExeApiCall.Post("users", user);

                if (affectedRows == 1)
                    ReplyAsync($"Usuario <@{userId}> añadido con éxito");
                else
                    ReplyAsync("Ha ocurrido un error");
            }
            catch (Exception e)
            {
                await ReplyAsync("Ha ocurrido un error");
            }
        }

        [Command("BAJAUSUARIO")]
        [Alias("USUARIOBAJA", "DELETEUSER", "USERDELETE", "BORRARUSUARIO")]
        public async Task DeleteUser()
        {
            try
            {
                var userId = Context.User.Id.ToString();

                Users user = new()
                {
                    Active = false
                };

                var affectedRows = await BuildAndExeApiCall.PutWithOneArgument("users", user, "id", userId);

                if (affectedRows != 1)
                    ReplyAsync($"El usuario <@{userId}> no estaba dado de alta");
                else
                    ReplyAsync($"Usuario <@{userId}> dado de baja con éxito");
            }
            catch (Exception e)
            {
                await ReplyAsync("Ha ocurrido un error");
            }
        }



        //public async Task NewUser([Remainder][Summary("The text to echo")] string echo)

    }

    // Create a module with the 'sample' prefix
    [Group("sample")]
	public class SampleModule : ModuleBase<SocketCommandContext>
	{
		// ~sample square 20 -> 400
		[Command("square")]
		[Summary("Squares a number.")]
		public async Task SquareAsync(
			[Summary("The number to square.")]
		int num)
		{
			// We can also access the channel from the Command Context.
			await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
		}

		// ~sample userinfo --> foxbot#0282
		// ~sample userinfo @Khionu --> Khionu#8708
		// ~sample userinfo Khionu#8708 --> Khionu#8708
		// ~sample userinfo Khionu --> Khionu#8708
		// ~sample userinfo 96642168176807936 --> Khionu#8708
		// ~sample whois 96642168176807936 --> Khionu#8708
		[Command("userinfo")]
		[Summary
		("Returns info about the current user, or the user parameter, if one passed.")]
		[Alias("user", "whois")]
		public async Task UserInfoAsync(
			[Summary("The (optional) user to get info from")]
		SocketUser user = null)
		{
			var userInfo = user ?? Context.Client.CurrentUser;
			await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
		}
	}
}
