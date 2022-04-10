using CryptoAlertsBot;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Charts;
using CryptoAlertsBot.Discord;
using CryptoAlertsBot.Helpers;
using CryptoAlertsBot.RepetitiveTasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using GenericApiHandler;
using GenericApiHandler.Authentication;
using GenericApiHandler.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

public partial class Program
{
    private DiscordSocketClient _client;
    private InteractionService _commands;
    private ServiceProvider _services;
    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync(string[] args) { }

    private async Task MainAsync()
    {
        CultureInfo culture = new CultureInfo("es-ES");
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        using (_services = ConfigureServices())
        {
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commands = _services.GetRequiredService<InteractionService>();

            _client.Log += LogAsync;
            _commands.Log += LogAsync;
            _client.Ready += ReadyAsync;

            string apiKey = Helpers.IsRelease() ? AppSettingsManager.GetDiscordBotKey() : AppSettingsManager.DiscordTestBotKey();
            //string apiKey = AppSettingsManager.GetDiscordBotKey();
            await _client.LoginAsync(TokenType.Bot, apiKey);

            await _client.StartAsync();

            _services.GetRequiredService<LoggerEventListener>().Initialize();
            await _services.GetRequiredService<AuthToken>().InitializeAsync();
            await _services.GetRequiredService<ConstantsHandler>().InitializeAsync();
            await _services.GetRequiredService<CommandHandler>().InitializeAsync();

            await Task.Delay(-1);
        }
    }

    private ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton<LogEvent>()
            .AddSingleton<AuthToken>()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<Logger>()
            .AddSingleton<LoggerEventListener>()
            .AddSingleton<BuildAndExeApiCall>()
            .AddSingleton<ConstantsHandler>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<FillPricesDB>()
            .AddSingleton<MostUsedApiCalls>()
            .AddSingleton<CommonFunctionality>()
            .AddSingleton<ClearPricesTable>()
            .AddSingleton<DbBackupExecute>()
            .AddSingleton<DbBackupPrepare>()
            .BuildServiceProvider();
    }

    private async Task ReadyAsync()
    {
        //if (Helpers.IsRelease())
        //{
        // this method will add commands globally, but can take around an hour
        await _commands.RegisterCommandsGloballyAsync(true);
        //}
        //else
        //{
        //    await _commands.RegisterCommandsToGuildAsync(ulong.Parse(_services.GetRequiredService<ConstantsHandler>().GetConstant(ConstantsNames.SERVER_ID)));
        //}
        Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");

        _services.GetRequiredService<FillPricesDB>().Initialize();
        _services.GetRequiredService<ClearPricesTable>().Initialize();
        _services.GetRequiredService<DbBackupPrepare>().Initialize();
    }

    private async Task LogAsync(LogMessage log)
    {
        await _services.GetRequiredService<Logger>().Log(log.ToString());
    }
}





