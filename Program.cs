using CryptoAlertsBot;
using CryptoAlertsBot.Discord;
using CryptoAlertsBot.Discord.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Configuration;

public partial class Program
{

    private DiscordSocketClient _client;
    private CommandService _commands;
    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        

        _client = new DiscordSocketClient();

        _client.Log += new Func<LogMessage, Task>((LogMessage msg) => { return Logger.Log(msg.ToString()); });

        //  You can assign your bot token to a string, and pass that in to connect.
        //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
        var token = ConfigurationManager.AppSettings["DiscordKeyProduction"];

        // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
        // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
        // var token = File.ReadAllText("token.txt");
        // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

        _commands = new CommandService(new CommandServiceConfig
        {
            // Again, log level:
            LogLevel = LogSeverity.Info,

            // There's a few more properties you can set,
            // for example, case-insensitive commands.
            CaseSensitiveCommands = false,
        });

        CommandHandler commandHandler = new(_client, _commands);
        await commandHandler.InstallCommandsAsync();

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);


    }
}

