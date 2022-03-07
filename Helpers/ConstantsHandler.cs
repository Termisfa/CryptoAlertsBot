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
            try
            {
                constantsList = await _buildAndExeApiCall.GetAllTable<Constants>();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public string GetConstant(string constantName)
        {
            try
            {
                string result = constantsList.FirstOrDefault(constant => constant.Name.ToLower() == constantName.ToLower().Trim())?.Text;
                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<bool> DeleteConstantAsync(string constantName)
        {
            try
            {
                int deletedRows = await _buildAndExeApiCall.DeleteWithOneArgument("constants", "name", constantName);

                if (deletedRows > 0)
                {
                    Initialize();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool UpdateConstant(string constantName, string value)
        {
            try
            {
                Constants constant = constantsList.FirstOrDefault(w => w.Name.ToLower() == constantName.ToLower().Trim());

                if (constant == null)
                    return false;

                constant.Text = value;

                _ = _buildAndExeApiCall.PutWithOneArgument("constants", constant, "name", constantName);

                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public bool AddConstant(string constantName, string value)
        {
            try
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
            catch (Exception e) { throw; }
        }

        public string ListConstants()
        {
            try
            {
                string result = string.Empty;
                constantsList.ForEach(constant => result += $"{constant.Name}: `{constant.Text}`\n");
                result = result.Substring(0, result.Length - 1);
                return result;
            }
            catch (Exception e) { throw; }
        }

    }
}
