using CryptoAlertsBot.Models;
using System.Text.Json;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.ApiHandler
{
    public static class MostUsedApiCalls
    {
        public static async Task<Users> GetUserByIdForcingOne(string userId)
        {
            List<Users> users = await BuildAndExeApiCall.GetWithOneArgument<Users>("id", userId);

            if (users.Count != 1)
            {
                await Logger.Log("Error in GetUserById. UserIdProvided: " + userId);
                throw new Exception();
            }

            return users[0];
        }

        public static async Task<Users> GetUserById(string userId)
        {
            List<Users> users = await BuildAndExeApiCall.GetWithOneArgument<Users>("id", userId);

            if (users.Count == 1)
                return users[0];
            else if (users.Count == 0)
                return default;
            else
            {
                await Logger.Log("Error in GetUserById. UserIdProvided: " + userId);
                throw new Exception();
            }
        }

        public static async Task UpdateUserById(string userId, Users user)
        {
            int affectedRows = await BuildAndExeApiCall.PutWithOneArgument("users", user, "id", userId);

            if (affectedRows != 1)
            {
                await Logger.Log("Error in UpdateUserById. UserIdProvided: " + userId);
                throw new Exception();
            }
        }

        public static async Task DeleteUserById(string userId)
        {
            int affectedRows = await BuildAndExeApiCall.DeleteWithOneArgument("users", "id", userId);

            if (affectedRows != 1)
            {
                await Logger.Log("Error in DeleteUserById. UserIdProvided: " + userId);
                throw new Exception();
            }
        }

        public static async Task<string> GetConstantTextByName(string name)
        {
            var constants = await BuildAndExeApiCall.GetWithOneArgument<Constants>("name", name);

            if (constants.Count != 1)
            {
                await Logger.Log("Error in GetConstantByName. Name provided: " + name);
                throw new Exception();
            }

            return constants[0].Text;
        }


        public static async Task<ResultPancakeSwapApi> GetFromPancakeSwapApi(string baseAddress, string coin)
        {
            var httpResponse = await ApiCalls.Get(coin, baseAddress);

            if (!httpResponse.IsSuccessStatusCode)
                return default;

            string httpResponseMessage = await httpResponse.Content.ReadAsStringAsync();

            ResultPancakeSwapApiPreParsed resultPancakeSwapApiPreParsed = JsonSerializer.Deserialize<ResultPancakeSwapApiPreParsed>(httpResponseMessage);

            int priceLength = int.Parse(await GetConstantTextByName(ConstantsNames.PRICE_LENGTH));

            ResultPancakeSwapApi result = new ResultPancakeSwapApi()
            {
                Updated_at = UnixTimeStampToDateTime(resultPancakeSwapApiPreParsed.updated_at),
                Name = resultPancakeSwapApiPreParsed.data.name,
                Symbol = resultPancakeSwapApiPreParsed.data.symbol,
                Price = StringPriceToDouble(resultPancakeSwapApiPreParsed.data.price, priceLength),
                Price_BNB = StringPriceToDouble(resultPancakeSwapApiPreParsed.data.price_BNB, priceLength + 3)
            };

            return result;
        }

        public static async Task<string> GetAlertsList(string userId)
        {
            string result = "Resumen de alertas actuales: \n";

            var alerts = await BuildAndExeApiCall.GetWithOneArgument<Alerts>("userId", userId);


            return result;
        }
    }
}
