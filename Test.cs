using CryptoAlertsBot.ApiHandler.Models;
using Discord.WebSocket;
using GenericApiHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoAlertsBot
{
  
    public class Listener
    {
        private DiscordSocketClient _client;

        public void Subscribe(LogEvent m, DiscordSocketClient client)
        {
            _client = client;
            m.FireEvent += new LogEvent.LogEventHandler(OnLogFired);
        }
        private void OnLogFired(LogEvent m, EventArgs e, ErrorInfo errorInfo)
        {
            (_client.GetChannel(924669579496665138) as SocketTextChannel).SendMessageAsync(errorInfo.Message);
            System.Console.WriteLine("HEARD IT");
        }

    }
    public class Test
    {
        private DiscordSocketClient _client;

        public Test(DiscordSocketClient client)
        {
            _client = client;
        }

        public void Main()
        {
            LogEvent m = new LogEvent();
            Listener l = new Listener();
            l.Subscribe(m, _client);
        }
    }
}
