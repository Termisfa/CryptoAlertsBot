using System.ComponentModel.DataAnnotations;

namespace CryptoAlertsBot.Models
{
    public class Alerts
    {
        [Display(Order = 0)]
        public int? Id { get; set; }

        [Display(Order = 1)]
        public string UserId { get; set; }

        [Display(Order = 2)]
        public string CoinAddress { get; set; }

        [Display(Order = 3)]
        public double PriceUsd { get; set; }

        [Display(Order = 4)]
        public string AlertType { get; set; }

        [Display(Order = 5)]
        public DateTime? LastAlert { get; set; }
    }
}
