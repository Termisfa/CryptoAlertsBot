using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GenericApiHandler.Models;
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
                await DeferAsync();

                var userId = Context.User.Id.ToString();
                Users user = await _mostUsedApiCalls.GetUserById(userId);

                if (user != default)
                {
                    if (user.Active)
                    {
                        await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = $"El usuario <@{userId}> ya estaba activo"; });
                    }
                    else
                    {
                        user.Active = true;
                        await _mostUsedApiCalls.UpdateUserById(userId, user);
                        await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = $"Usuario <@{ userId}> reactivado"; });
                    }
                    return;
                }

                var role = await Context.Guild.CreateRoleAsync(Context.User.Username + " Rol", isMentionable: false);
                _ = (Context.User as IGuildUser).AddRoleAsync(role);

                var categoryChannel = await Context.Guild.CreateCategoryChannelAsync("Zona personal " + Context.User.Username);
                _ = categoryChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, OverwritePermissions.DenyAll(categoryChannel));
                _ = categoryChannel.AddPermissionOverwriteAsync(role, OverwritePermissions.AllowAll(categoryChannel));

                _ = Context.Guild.CreateTextChannelAsync("alertas-" + Context.User.Username, tcp => tcp.CategoryId = categoryChannel.Id);
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
                {
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = $"Usuario <@{ userId}> añadido con éxito"; });
                }
                else
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "Ha ocurrido un error"; });
            }
            catch (Exception e)
            {
                await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "Ha ocurrido un error"; });
                _ = _logger.Log(exception: e);
            }
        }

        [SlashCommand("bajausuario", "Desactiva todas las alertas")]
        public async Task DeleteUser()
        {
            try
            {
                await DeferAsync();

                var userId = Context.User.Id.ToString();

                Users user = new()
                {
                    Active = false
                };

                var affectedRows = await _buildAndExeApiCall.PutWithOneParameter("users", user, HttpParameter.DefaultParameter("id", userId));

                if (affectedRows != 1)
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = $"El usuario <@{userId}> no estaba dado de alta"; });
                else
                    await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = $"Usuario <@{userId}> dado de baja con éxito"; });
            }
            catch (Exception e)
            {
                await ModifyOriginalResponseAsync((responseMsg) => { responseMsg.Content = "Ha ocurrido un error"; });
                _ = _logger.Log(exception: e);
            }
        }
    }
}
