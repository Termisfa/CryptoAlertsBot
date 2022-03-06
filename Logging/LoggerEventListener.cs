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
  
    public class LoggerEventListener
    {
        private readonly Logger _logger;
        private readonly LogEvent _logEvent;

        public LoggerEventListener(Logger logger, LogEvent logEvent)
        {
            _logger = logger;
            _logEvent = logEvent;
        }

        public void Initialize()
        {
            _logEvent.FireEvent += new LogEvent.LogEventHandler(OnLogFired);
        }

        private void OnLogFired(LogEvent m, EventArgs e, Response response)
        {
            _logger.Log(response: response);
        }
    }


    //public class Test
    //{
    //    private DiscordSocketClient _client;

    //    public Test(DiscordSocketClient client)
    //    {
    //        _client = client;
    //    }

    //    public void Main()
    //    {
    //        LogEvent m = new LogEvent();
    //        LoggerEventListener l = new LoggerEventListener();
    //        l.Subscribe(m, _client);
    //    }
    //}
}
