using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using Discord.Commands;
using CryptoAlertsBot.Helpers;

namespace CryptoAlertsBot.Discord
{
    public class CommandHandler : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _slashCommands;
        private readonly CommandService _normalCommands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, InteractionService slashCommands, CommandService normalCommands, IServiceProvider services)
        {
            _client = client;
            _slashCommands = slashCommands;
            _normalCommands = normalCommands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            await _slashCommands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _normalCommands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.InteractionCreated += HandleInteraction;
            _client.MessageReceived += HandleMsgAsync;

            _slashCommands.SlashCommandExecuted += SlashCommandExecuted;
            _normalCommands.CommandExecuted += NormalCommandExecuted;
        }


        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, arg);
                await _slashCommands.ExecuteCommandAsync(ctx, _services);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                if (arg.Type == InteractionType.ApplicationCommand)
                {
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }
            }
        }

        private async Task HandleMsgAsync(SocketMessage messageParam)
        {
            //To remove pinned messages notifications
            if (messageParam.Type == MessageType.ChannelPinnedMessage)
            {
                await messageParam.DeleteAsync();
                return;
            }

            // Don't process the command if it was a system message
            if (messageParam is not SocketUserMessage message) return;

            int argPos = 0;

            char botPrefix = GenericHelpers.IsRelease() ? '!' : '%';

            if (!(message.HasCharPrefix(botPrefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);

            await _normalCommands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);
        }

        private async Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, global::Discord.Interactions.IResult result)
        {
            await CommandExecutedAsync(context, result);
        }

        private async Task NormalCommandExecuted(Optional<CommandInfo> command, ICommandContext context, global::Discord.Commands.IResult result)
        {
            await CommandExecutedAsync(context, result);
        }

        private static async Task CommandExecutedAsync(dynamic context, dynamic result)
        {
            if (!result.IsSuccess)
            {
                string errorMsg = (dynamic)result.Error switch
                {
                    CommandError.UnknownCommand or InteractionCommandError.UnknownCommand => "Ese comando no existe",
                    CommandError.BadArgCount or InteractionCommandError.BadArgs => "Número erróneo de parámetros",
                    _ => result.ErrorReason,
                };
                _ = await context.Channel.SendMessageAsync(errorMsg);
            }
        }
    }
}

