using MoneyTrack.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace MoneyTrack.Infrastructure.Services
{
    // Знаю, что для тестового задания я немного переборщил,
    // времени на выполнение оочень много, решил этим воспользоваться...
    public class RabbitMQMessageBroker : IMessageBroker, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly ILogger<RabbitMQMessageBroker> _logger;

        public RabbitMQMessageBroker(IConfiguration configuration, ILogger<RabbitMQMessageBroker> logger)
        {
            _logger = logger;

            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = configuration["RabbitMQ:Host"] ?? throw new InvalidOperationException("Host name was empty"),
                    Port = int.Parse(configuration["port"] ?? "5672"),
                    UserName = configuration["RabbitMQ:UserName"] ?? throw new InvalidOperationException("User Name was empty"),
                    Password = configuration["RabbitMQ:Password"] ?? throw new InvalidOperationException("Password Name was empty"),
                    VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/",

                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                    SocketReadTimeout = TimeSpan.FromSeconds(30),
                    SocketWriteTimeout = TimeSpan.FromSeconds(30),

                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnectionAsync().Result;
                _channel = _connection.CreateChannelAsync().Result;

                _logger.LogInformation("Connection with Rabbit MQ service estableshed");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Connection with Rabbit MQ service fail");

                throw;
            }
        }

        public async Task AddMessageToQueueAsync<T>(string queueName, T message)
        {
            try
            {
                await _channel.QueueDeclareAsync
                    (
                        queue:queueName,
                        exclusive: false,
                        autoDelete: false,
                        durable: false,
                        arguments: null
                    );

                string jsonString = JsonSerializer.Serialize(message);

                var messageBytes = Encoding.UTF8.GetBytes(jsonString);

                await _channel.BasicPublishAsync
                    (
                        body: messageBytes,
                        routingKey: queueName,
                        exchange: ""
                    );

                _logger.LogInformation($"Message sent to queue: {queueName}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Sent message error, queue name: {queueName}");

                throw;
            }
        }

        public void Dispose()
        {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
            _channel?.Dispose();
            _connection?.Dispose();
        }

        public async Task HandleMessageFromQueueAsync<T, N>(string queueName, Func<T, N> handler)
        {
            try
            {
                await _channel.QueueDeclareAsync
                    (
                        queue: queueName,
                        exclusive: false,
                        durable: false,
                        arguments: null,
                        autoDelete: false
                    );

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var messageBytes = ea.Body.ToArray();
                        string json = Encoding.UTF8.GetString(messageBytes);
                        var message = JsonSerializer.Deserialize<T>(json);

                        if (message != null)
                        {
                            handler(message);
                            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        }

                        _logger.LogInformation($"Message successfully received from queue: {queueName}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Message from queue: {queueName} received with error");
                        await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple:false, requeue: true);
                    }
                };

                await _channel.BasicConsumeAsync(queue: queueName,
                                    autoAck: false,
                                    consumer: consumer);

                _logger.LogInformation($"Starts receiving messages from queue: {queueName}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Receiving messages from queue: {queueName} error");

                throw;
            }
        }
    }
}
