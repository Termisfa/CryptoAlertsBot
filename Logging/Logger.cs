using CryptoAlertsBot.ApiHandler.Models;
using Discord.WebSocket;
using System.Diagnostics;

namespace CryptoAlertsBot
{
    public class Logger
    {
        private readonly DiscordSocketClient _client;
        private readonly ConstantsHandler _constantsHandler;

        public Logger(DiscordSocketClient client, ConstantsHandler constantsHandler)
        {
            _client = client;
            _constantsHandler = constantsHandler;
        }

        public Task Log(string msg = default, Exception exception = default, Response response = default)
        {
            msg = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + "\n" + msg;

            if (exception != default)
            {
                msg += exception.Message + "\n";
                msg += exception.StackTrace;
            }
            else if(response != default)
            {
                msg += response.ErrorInfo.Message + "\n";
                msg += "Query used: " + response.UsedQuery + "\n";
                msg += response.ErrorInfo.StackTrace;
            }

            msg += "\n" + new string('-', 50);

            Console.WriteLine(msg);

            var logChannel = ((SocketTextChannel)_client.GetChannel(ulong.Parse(_constantsHandler.GetConstant(ConstantsNames.LOG_CHANNEL_ID))));
            if (logChannel != null)
                logChannel.SendMessageAsync(msg);

            return Task.CompletedTask;
        }
    }
}
