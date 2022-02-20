using System.ComponentModel.DataAnnotations;

namespace CryptoAlertsBot.Models
{
    public class Users
    {
        [Display(Order = 0)]
        public string Id { get; set; }

        [Display(Order = 1)]
        public string Name { get; set; }

        [Display(Order = 2)]
        public bool Active { get; set; }

        [Display(Order = 3)]
        public string IdCategoryChannel { get; set; }
    }
}
