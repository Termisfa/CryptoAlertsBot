using CryptoAlertsBot.ApiHandler;
using CryptoAlertsBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoAlertsBot
{
    public class ConstantsHandler
    {
        private List<Constants> constantsList;
        private readonly BuildAndExeApiCall _buildAndExeApiCall;

        public ConstantsHandler(BuildAndExeApiCall buildAndExeApiCall)
        {
            _buildAndExeApiCall = buildAndExeApiCall;
            Initialize();
        }

        private async void Initialize()
        {
            constantsList = await _buildAndExeApiCall.GetAllTable<Constants>();
        }

        public string GetConstant(string constantName)
        {
            string result = constantsList.FirstOrDefault(constant => constant.Name.ToLower() == constantName.ToLower().Trim())?.Text;
            return result;
        }

        public async Task<bool> DeleteConstantAsync(string constantName)
        {
            int deletedRows = await _buildAndExeApiCall.DeleteWithOneArgument("constants", "name", constantName);

            if (deletedRows > 0)
            {
                Initialize();
                return true;
            }

            return false;
        }

        public bool UpdateConstant(string constantName, string value)
        {
            Constants constant = constantsList.FirstOrDefault(w => w.Name.ToLower() == constantName.ToLower().Trim());

            if (constant == null)
                return false;

            constant.Text = value;

            _ = _buildAndExeApiCall.PutWithOneArgument("constants", constant, "name", constantName);

            return true;
        }

        public bool AddConstant(string constantName, string value)
        {
            Constants constant = constantsList.FirstOrDefault(w => w.Name.ToLower() == constantName.ToLower().Trim());

            if (constant != null)
                return false;

            constant = new();
            constant.Name = constantName;
            constant.Text = value;

            _ = _buildAndExeApiCall.Post("constants", constant);

            return true;
        }

        public string ListConstants()
        {
            string result = string.Empty;

            constantsList.ForEach(constant => result += $"{constant.Name}: `{constant.Text}`\n");

            result = result.Substring(0, result.Length - 1); //To remove last \n

            return result;
        }

    }
}
