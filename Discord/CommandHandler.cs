using CryptoAlertsBot.Discord.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace CryptoAlertsBot.Discord
{
    public class CommandHandler : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }

        public async Task SetupAsync()
        {
            _commands.CommandExecuted += OnCommandExecutedAsync;

            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModuleAsync<UserCommands>(null);
            await _commands.AddModuleAsync<CoinCommands>(null);
            await _commands.AddModuleAsync<AlertCommands>(null);
            await _commands.AddModuleAsync<AdminCommands>(null);
            await _commands.AddModuleAsync<TestCommands>(null);
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // We have access to the information of the command executed,
            // the context of the command, and the result returned from the
            // execution in this event.

            // We can tell the user what went wrong
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }

            // ...or even log the result (the method used should fit into
            // your existing log handler)
            var commandName = command.IsSpecified ? command.Value.Name : "A command";
            Logger.Log("CommandExecution", $"{commandName} was executed at {DateTime.UtcNow}.");
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
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

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);
        }
    }
}
