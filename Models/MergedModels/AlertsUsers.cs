using System.ComponentModel.DataAnnotations;

namespace CryptoAlertsBot.Models.MergedModels
{
    public class AlertsUsers
    {
        [Display(Order = 0)]
        public Alerts Alert { get; set; }

        [Display(Order = 1)]
        public Users User { get; set; }
    }
}
