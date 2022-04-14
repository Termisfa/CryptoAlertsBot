using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoAlertsBot.Models.PancakeSwap
{
    public class ResultPancakeSwapApiPreParsed
    {
        public long Updated_at { get; set; }
        public DataInsideResult Data { get; set; }
    }
}
