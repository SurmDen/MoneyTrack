using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrack.Application.Interfaces
{
    public interface IMessageBroker
    {
        public Task AddMessageToQueueAsync<T>(string queueName, T message);

        public Task HandleMessageFromQueueAsync<T, N>(string queueName, Func<T, N> handler);
    }
}
