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

        public Task Log(string msg = default, Exception exception = default, string method = default)
        {
            method ??= new StackTrace().GetFrame(1).GetMethod().Name;
            if (method == "MoveNext")
                method = "discord logger";

            msg = $"This log was fired in: `{method}`. \n" + msg;

            if (exception != default)
            {
                msg += exception.Message + "\n";
                msg += exception.StackTrace;
            }

            Console.WriteLine(msg);

            var logChannel = ((SocketTextChannel)_client.GetChannel(ulong.Parse(_constantsHandler.GetConstant(ConstantsNames.LOG_CHANNEL_ID))));
            if (logChannel != null)
                logChannel.SendMessageAsync(msg);

            return Task.CompletedTask;
        }
    }
}
