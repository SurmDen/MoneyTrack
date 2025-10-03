using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MoneyTrack.Application.Interfaces;
using MoneyTrack.Domain.Entities;
using MoneyTrack.Domain.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrack.Infrastructure.Services
{
    public class ExchangeRateApiService : ICurrencyApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string _baseURL;
        private readonly string _cachePrefix = "exchange_rate";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(6);


        public ExchangeRateApiService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _baseURL = configuration["ExchangeRateApi:baseURL"] ?? throw new ArgumentException("Base URL is not configured");
        }

        public async Task<decimal> ConvertAsync(Currency from, Currency to)
        {
            try
            {
                if (_cache.TryGetValue($"{_cachePrefix}:{from}-{to}", out decimal cachedRate))
                {
                    return cachedRate;
                }
                else if (_cache.TryGetValue($"{_cachePrefix}:{to}-{from}", out decimal reversedCachedRate))
                {
                    return 1 / reversedCachedRate;
                }

                ExchangeRateApiResponse? response = await _httpClient.GetFromJsonAsync<ExchangeRateApiResponse>(_baseURL + from);

                if (response == null)
                {
                    throw new InvalidOperationException("Exchange Rate api returned null");
                }

                if (response.Result != "success")
                {
                    throw new InvalidOperationException($"Exchange Rate result: {response.Result}");
                }

                decimal rateValue = response.Rates[to.ToString()];

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_cacheDuration);

                _cache.Set($"{_cachePrefix}:{from}-{to}", rateValue, cacheOptions);

                return rateValue;

            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Error occured while attempling to get data from Exchange Rate service", ex);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
