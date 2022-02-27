namespace CryptoAlertsBot
{
    public static class Logger
    {
        public static Task Log(string msg, string method = default)
        {
            if(method != default)
                Console.WriteLine($"This error was fired in: {method}");

            Console.WriteLine(msg);
            return Task.CompletedTask;
        }
    }
}
