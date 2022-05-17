namespace Coast.RabbitMQ
{
    using System;
    using global::RabbitMQ.Client;

    public interface IRabbitMQPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
