namespace CryptoAlertsBot
{
    public static class Logger
    {
        public static Task Log(string msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }
    }
}
