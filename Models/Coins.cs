using System.ComponentModel.DataAnnotations;

namespace CryptoAlertsBot.Models
{
    public class Coins
    {
        [Display(Order = 0)]
        public string Address { get; set; }

        [Display(Order = 1)]
        public string Name { get; set; }

        [Display(Order = 2)]
        public string Symbol { get; set; }

        [Display(Order = 3)]
        public string IdChannel { get; set; }
    }
}
