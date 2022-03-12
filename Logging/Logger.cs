using CryptoAlertsBot.ApiHandler.Models;
using Discord;
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

        public async Task Log(string msg = default, Exception exception = default, Response response = default)
        {
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

            string finalMsg = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + "\n" + msg;

            Console.WriteLine(finalMsg);

            string logChannelID = _constantsHandler.GetConstant(ConstantsNames.LOG_CHANNEL_ID);
            if(!string.IsNullOrEmpty(logChannelID))
            {
                var logChannel = (SocketTextChannel)_client.GetChannel(ulong.Parse(logChannelID));
                if (logChannel != null && !await logChannel.GetMessagesAsync(10).Flatten().AnyAsync(w => w.Content.Contains(msg)))
                {
                    _ = logChannel.SendMessageAsync(finalMsg);
                }
            }
        }
    }
}
