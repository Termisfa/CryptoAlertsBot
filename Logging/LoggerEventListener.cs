using CryptoAlertsBot.ApiHandler.Models;
using GenericApiHandler;

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

        private void OnLogFired(LogEvent m, EventArgs e, Response response = default, Exception exc = default)
        {
            if (response != default)
                _ = _logger.Log(response: response);
            else if(exc != default)
                _ = _logger.Log(exception: exc);
        }
    }
}
