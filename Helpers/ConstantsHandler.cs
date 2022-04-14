using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using GenericApiHandler.Models;

namespace CryptoAlertsBot
{
    public class ConstantsHandler
    {
        private List<Constants> constantsList;
        private readonly BuildAndExeApiCall _buildAndExeApiCall;

        public ConstantsHandler(BuildAndExeApiCall buildAndExeApiCall)
        {
            _buildAndExeApiCall = buildAndExeApiCall;
        }

        public async Task InitializeAsync()
        {
            try
            {
                constantsList = await _buildAndExeApiCall.GetAllTable<Constants>();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in ConstantHandler.InitializeAsync: " + e.Message);
            }
        }

        public async Task<string> GetConstantAsync(string constantName)
        {
            try
            {
                await InitializeIfEmpty();

                string result = constantsList.FirstOrDefault(constant => constant.Name.ToLower() == constantName.ToLower().Trim())?.Text;
                return result;
            }
            catch (Exception e)
            {               
                return default;
            }
        }

        public async Task<bool> DeleteConstantAsync(string constantName)
        {
            try
            {
                await InitializeIfEmpty();

                int deletedRows = await _buildAndExeApiCall.DeleteWithOneParameter("constants", HttpParameter.DefaultParameter("name", constantName));

                if (deletedRows > 0)
                {
                    await InitializeAsync();
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return default;
            }
        }

        public async Task<bool> UpdateConstantAsync(string constantName, string value)
        {
            try
            {
                await InitializeIfEmpty();

                Constants constant = constantsList.FirstOrDefault(w => w.Name.ToLower() == constantName.ToLower().Trim());

                if (constant == null)
                    return false;

                constant.Text = value;

                _ = _buildAndExeApiCall.PutWithOneParameter("constants", constant, HttpParameter.DefaultParameter("name", constantName));

                return true;
            }
            catch (Exception e)
            {
                return default;
            }
        }

        public async Task<bool> AddConstantAsync(string constantName, string value)
        {
            try
            {
                await InitializeIfEmpty();

                Constants constant = constantsList.FirstOrDefault(w => w.Name.ToLower() == constantName.ToLower().Trim());
                if (constant != null)
                    return false;
                constant = new();
                constant.Name = constantName;
                constant.Text = value;
                _ = _buildAndExeApiCall.Post("constants", constant);
                return true;
            }
            catch (Exception e)
            {
                return default;
            }
        }

        public async Task<string> ListConstantsAsync()
        {
            try
            {
                await InitializeIfEmpty();
                string result = string.Empty;
                constantsList.ForEach(constant => result += $"{constant.Name}: `{constant.Text}`\n");
                result = result.Substring(0, result.Length - 1);
                return result;
            }
            catch (Exception e)
            {
                return default;
            }
        }

        private async Task InitializeIfEmpty()
        {
            try
            {
                if (constantsList == default)
                    await this.InitializeAsync();
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
