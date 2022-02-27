using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using Discord.Commands;

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
                messageParam.DeleteAsync();
                return;
            }

            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) ||
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
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        _ = await context.Channel.SendMessageAsync(result.ErrorReason);
                        break;
                    case InteractionCommandError.UnknownCommand:
                        break;
                    case InteractionCommandError.BadArgs:
                        break;
                    case InteractionCommandError.Exception:
                        break;
                    case InteractionCommandError.Unsuccessful:
                        break;
                    default:
                        break;
                }
            }
        }



        private async Task NormalCommandExecuted(Optional<CommandInfo> command, ICommandContext context, global::Discord.Commands.IResult result)
        {
            {
                if (!result.IsSuccess)
                {
                    switch (result.Error)
                    {
                        case CommandError.UnmetPrecondition:
                            _ = await context.Channel.SendMessageAsync(result.ErrorReason);
                            break;
                        case CommandError.UnknownCommand:
                            break;
                        case CommandError.BadArgCount:
                            break;
                        case CommandError.Exception:
                            break;
                        case CommandError.Unsuccessful:
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
