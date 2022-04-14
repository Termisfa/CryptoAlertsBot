using CryptoAlertsBot.ApiHandler.Models;
using Discord.WebSocket;

namespace CryptoAlertsBot
{
    public class Logger
    {
        private readonly DiscordSocketClient _client;
        private readonly ConstantsHandler _constantsHandler;

        private readonly Queue<string> _logs;
        private const int QUEUE_SIZE = 10;

        public Logger(DiscordSocketClient client, ConstantsHandler constantsHandler)
        {
            _client = client;
            _constantsHandler = constantsHandler;
            _logs = new();
        }

        public async Task Log(string msg = default, Exception exception = default, Response response = default)
        {
            if (exception != default)
            {
                msg += exception.Message + "\n";
                msg += exception.StackTrace;
            }
            else if (response != default)
            {
                msg += response.ErrorInfo.Message + "\n";
                msg += "Query used: " + response.UsedQuery + "\n";
                msg += response.ErrorInfo.StackTrace;
            }

            msg += "\n" + new string('-', 50);

            string finalMsg = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + "\n" + msg;

            Console.WriteLine(finalMsg);

            if (!_logs.Contains(finalMsg))
            {
                _logs.Enqueue(finalMsg);

                if (_logs.Count > QUEUE_SIZE)
                {
                    _ = _logs.Dequeue();
                }

                string logChannelID = await _constantsHandler.GetConstantAsync(ConstantsNames.LOG_CHANNEL_ID);
                if (!string.IsNullOrEmpty(logChannelID))
                {
                    var logChannel = (SocketTextChannel)_client.GetChannel(ulong.Parse(logChannelID));
                    if (logChannel != null)
                    {
                        _ = logChannel.SendMessageAsync(finalMsg);
                    }
                }
            }
        }
    }
}
