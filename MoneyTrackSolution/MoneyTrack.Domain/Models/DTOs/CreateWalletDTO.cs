using MoneyTrack.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrack.Domain.Models.DTOs
{
    public class CreateWalletDTO
    {
        [Required]
        public string WalletName { get; set; } = string.Empty;

        [Required]
        public decimal InitialBalance { get; set; }

        [Required]
        public Currency Currency { get; set; }
    }
}
