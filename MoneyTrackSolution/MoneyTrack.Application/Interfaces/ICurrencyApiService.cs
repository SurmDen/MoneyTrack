using MoneyTrack.Domain.Entities;
using MoneyTrack.Domain.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrack.Application.Interfaces
{
    public interface ICurrencyApiService
    {
        public Task<decimal> ConvertAsync(Currency from, Currency to);
    }
}
