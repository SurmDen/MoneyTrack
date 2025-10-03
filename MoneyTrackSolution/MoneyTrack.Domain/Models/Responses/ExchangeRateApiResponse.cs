using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MoneyTrack.Domain.Models.Responses
{
    public class ExchangeRateApiResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; } = string.Empty;

        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; } = string.Empty;

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; set; } = new();

        [JsonPropertyName("time_last_update_unix")]
        public long TimeLastUpdateUnix { get; set; }

        [JsonPropertyName("time_next_update_unix")]
        public long TimeNextUpdateUnix { get; set; }
    }
}
