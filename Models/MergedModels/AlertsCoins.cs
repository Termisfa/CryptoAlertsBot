using System.ComponentModel.DataAnnotations;

namespace CryptoAlertsBot.Models.MergedModels
{
    public class AlertsCoins
    {
        [Display(Order = 0)]
        public Alerts Alert { get; set; }

        [Display(Order = 1)]
        public Coins Coin { get; set; }
    }
}
