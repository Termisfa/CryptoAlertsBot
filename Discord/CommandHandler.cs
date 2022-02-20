﻿using CryptoAlertsBot.Discord.Modules;
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

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModuleAsync<UserCommands>(null);
            await _commands.AddModuleAsync<CoinCommands>(null);
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