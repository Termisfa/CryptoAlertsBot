using CryptoAlertsBot.Models;
using System.Text.Json;
using CryptoAlertsBot.ApiHandler;
using GenericApiHandler.Data.Enums;

namespace CryptoAlertsBot
{
    public class MostUsedApiCalls
    {
        private readonly BuildAndExeApiCall _buildAndExeApiCall;

        public MostUsedApiCalls(BuildAndExeApiCall buildAndExeApiCall)
        {
            _buildAndExeApiCall = buildAndExeApiCall;
        }

        public async Task<Users> GetUserById(string userId)
        {
            try
            {
                List<Users> users = await _buildAndExeApiCall.GetWithOneArgument<Users>("id", userId);

                if (users.Count == 1)
                    return users[0];

                return default;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task UpdateUserById(string userId, Users user)
        {
            try
            {
                int affectedRows = await _buildAndExeApiCall.PutWithOneArgument("users", user, "id", userId);

                if (affectedRows != 1)
                {
                    throw new Exception("Error in UpdateUserById. UserIdProvided: " + userId);
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<Coins> GetCoinByAddress(string address)
        {
            try
            {
                List<Coins> coins = await _buildAndExeApiCall.GetWithOneArgument<Coins>("address", address);
                if (coins.Count == 1)
                    return coins[0];
                return default;
            }
            catch (Exception e) { throw; }
        }

        public async Task<int> DeleteAlert(string userId, string coinAddress, string priceUsd, string alertType)
        {
            try
            {
                Dictionary<string, string> parameters = new();
                parameters.Add("userId", userId);
                parameters.Add("coinAddress", coinAddress);
                parameters.Add("priceUsd", priceUsd);
                parameters.Add("alertType", alertType);

                int result = await _buildAndExeApiCall.DeleteWithMultipleArguments("Alerts", parameters);

                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<string> GetConstantTextByName(string name)
        {
            try
            {
                var constants = await _buildAndExeApiCall.GetWithOneArgument<Constants>("name", name);
                if (constants.Count != 1)
                {
                    throw new Exception("Error in GetConstantByName. Name provided: " + name);
                }
                return constants[0].Text;
            }
            catch (Exception e) { throw; }
        }

        public async Task<ResultPancakeSwapApi> GetFromPancakeSwapApi(string baseAddress, string coin)
        {
            try
            {
                var httpResponse = await ApiCalls.ExeCall(ApiCallTypesEnum.Get, coin, baseAddress: baseAddress);
                if (!httpResponse.IsSuccessStatusCode)
                    return default;
                string httpResponseMessage = await httpResponse.Content.ReadAsStringAsync();
                ResultPancakeSwapApiPreParsed resultPancakeSwapApiPreParsed = JsonSerializer.Deserialize<ResultPancakeSwapApiPreParsed>(httpResponseMessage);
                int priceLength = int.Parse(await GetConstantTextByName(ConstantsNames.PRICE_LENGTH));
                ResultPancakeSwapApi result = new ResultPancakeSwapApi() { Updated_at = Helpers.Helpers.UnixTimeStampToDateTime(resultPancakeSwapApiPreParsed.updated_at), Name = resultPancakeSwapApiPreParsed.data.name, Symbol = resultPancakeSwapApiPreParsed.data.symbol, Price = Helpers.Helpers.StringPriceToDouble(resultPancakeSwapApiPreParsed.data.price, priceLength), Price_BNB = Helpers.Helpers.StringPriceToDouble(resultPancakeSwapApiPreParsed.data.price_BNB, priceLength + 3) };
                return result;
            }
            catch (Exception e) { throw; }
        }
    }
}
