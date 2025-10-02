using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrack.Domain.Models.DTOs
{
    public class CreateTransactionDTO
    {
        [Required]
        public Guid SenderWalletId { get; set; }

        [Required]
        public Guid ReceiverWalletId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
