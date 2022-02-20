using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoAlertsBot.Models
{
    public class ResultPancakeSwapApi
    {
        public DateTime Updated_at { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public double Price { get; set; }
        public double Price_BNB { get; set; }
    }
}
