using System.ComponentModel.DataAnnotations;

namespace CryptoAlertsBot.Models
{
    public class Prices
    {

        [Display(Order = 0)]
        public string CoinAddress { get; set; }

        [Display(Order = 1)]
        public double PriceUsd { get; set; }

        [Display(Order = 2)]
        public DateTime? PriceDate { get; set; }
    }
}
