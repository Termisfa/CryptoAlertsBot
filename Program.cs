using CryptoAlertsBot;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Discord;
using CryptoAlertsBot.Discord.Modules;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using GenericApiHandler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Configuration;

public partial class Program
{

    private DiscordSocketClient _client;
    private InteractionService _commands;
    private ServiceProvider _services;
    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync(string[] args) { }

    private async Task MainAsync()
    {

        using (_services = ConfigureServices())
        {
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commands = _services.GetRequiredService<InteractionService>();

            _client.Log += LogAsync;
            _commands.Log += LogAsync;
            _client.Ready += ReadyAsync;

            await _client.LoginAsync(TokenType.Bot, AppSettingsManager.GetDiscordBotKey());
            await _client.StartAsync();

            await _services.GetRequiredService<CommandHandler>().InitializeAsync();

            await Task.Delay(-1);
        }
    }

    private ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<Logger>()
            .AddSingleton<LogEvent>()
            .AddSingleton<BuildAndExeApiCall>()
            .AddSingleton<ConstantsHandler>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<FillPricesDB>()
            .AddSingleton<MostUsedApiCalls>()
            .AddSingleton<CommonFunctionality>()
            .BuildServiceProvider();
    }

    private async Task ReadyAsync()
    {
        if (IsDebug())
        {
            await _commands.RegisterCommandsToGuildAsync(ulong.Parse(AppSettingsManager.GetCryptoAllertsDiscordServerId()));
        }
        else
        {
            // this method will add commands globally, but can take around an hour
            await _commands.RegisterCommandsGloballyAsync(true);
        }
        Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");

        _services.GetRequiredService<FillPricesDB>().Initialize();

    }

    private async Task LogAsync(LogMessage log)
    {
        await _services.GetRequiredService<Logger>().Log(log.ToString());
    }

    static bool IsDebug()
    {
#if DEBUG
        return true;
#else
                return false;
#endif
    }
}





