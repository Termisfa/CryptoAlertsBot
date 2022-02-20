using System.ComponentModel.DataAnnotations;

namespace CryptoAlertsBot.Models
{
    public class Constants
    {
        [Display(Order = 0)]
        public int? Id { get; set; }

        [Display(Order = 1)]
        public string Name { get; set; }

        [Display(Order = 2)]
        public string Text { get; set; }
    }
}
