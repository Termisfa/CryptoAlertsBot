using CryptoAlertsBot.Models;
using System.Text.Json;
using static CryptoAlertsBot.Helpers.Helpers;

namespace CryptoAlertsBot.ApiHandler
{
    public static class MostUsedApiCalls
    {
        public static async Task<Users> GetUserById(string userId)
        {
            List<Users> users = await BuildAndExeApiCall.GetWithOneArgument<Users>("id", userId);

            if (users.Count == 1)
                return users[0];
            else if (users.Count == 0)
                return default;
            else
            {
                throw new Exception("Error in GetUserById. UserIdProvided: " + userId);
            }
        }

        public static async Task UpdateUserById(string userId, Users user)
        {
            int affectedRows = await BuildAndExeApiCall.PutWithOneArgument("users", user, "id", userId);

            if (affectedRows != 1)
            {
                throw new Exception("Error in UpdateUserById. UserIdProvided: " + userId);
            }
        }

        public static async Task DeleteUserById(string userId)
        {
            int affectedRows = await BuildAndExeApiCall.DeleteWithOneArgument("users", "id", userId);

            if (affectedRows != 1)
            {
                throw new Exception("Error in DeleteUserById. UserIdProvided: " + userId);
            }
        }

        public static async Task<Coins> GetCoinByAddress(string address)
        {
            List<Coins> coins = await BuildAndExeApiCall.GetWithOneArgument<Coins>("address", address);

            if (coins.Count == 1)
                return coins[0];
            else if (coins.Count == 0)
                return default;
            else
            {
                throw new Exception("Error in GetCoinById. AddressProvided: " + address);
            }
        }

        public static async Task<int> DeleteAlert(string userId, string coinAddress, string priceUsd, string alertType)
        {
            Dictionary<string, string> parameters = new();
            parameters.Add("userId", userId);
            parameters.Add("coinAddress", coinAddress);
            parameters.Add("priceUsd", priceUsd);
            parameters.Add("alertType", alertType);

            int result = await BuildAndExeApiCall.DeleteWithMultipleArguments("Alerts", parameters);

            return result;
        }

        public static async Task<string> GetConstantTextByName(string name)
        {
            var constants = await BuildAndExeApiCall.GetWithOneArgument<Constants>("name", name);

            if (constants.Count != 1)
            {
                throw new Exception("Error in GetConstantByName. Name provided: " + name);
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
    }
}
