using Discord.WebSocket;
using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Discord;
using System.Reflection;
using GenericApiHandler.Helpers;
using System.Text;

namespace CryptoAlertsBot.RepetitiveTasks
{
    public class DbBackup
    {
        private readonly Logger _logger;
        private readonly DataBaseBackup _dataBaseBackup;
        private readonly ConstantsHandler _constantsHandler;
        private readonly DiscordSocketClient _client;

        public DbBackup(Logger logger, DataBaseBackup dataBaseBackup, ConstantsHandler constantsHandler, DiscordSocketClient client)
        {
            _logger = logger;
            _dataBaseBackup = dataBaseBackup;
            _constantsHandler = constantsHandler;
            _client = client;
        }

        public void Initialize()
        {
            try
            {
                var timer = new System.Timers.Timer(1000 * 60 * 60 * 24); //It should be 1000 * 60 * 60 * 24 (1 day)
                timer.Start();
                timer.Elapsed += ExeBackup;
            }
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
            }
        }

        private async void ExeBackup(object? sender, System.Timers.ElapsedEventArgs elapsed)
        {
            try
            {
                List<Type> tableTypes = Helpers.Helpers.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "CryptoAlertsBot.Models");

                string backupResult = await _dataBaseBackup.GetDataBaseBackup(tableTypes);



                var dbBackupChannel = _client.Guilds.First().GetChannel(ulong.Parse(_constantsHandler.GetConstant(ConstantsNames.BACKUP_DB_CHANNEL_ID)));

                using (var stream = GenerateStreamFromString(backupResult))
                {
                    await (dbBackupChannel as SocketTextChannel).SendFileAsync(stream, "testFileName.sql", "test");
                }
            }
            catch (Exception e)
            {
                _ = _logger.Log(exception: e);
            }
        }
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }

}
