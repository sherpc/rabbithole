using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Producer;
using EasyNetQ.Topology;
using RabbitHoleNode.Logger;

namespace RabbitHoleNode
{
	public static class RabbitHelper
	{
		public static IBus CreateBus()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["rabbit"];
			if (connectionString == null || String.IsNullOrWhiteSpace(connectionString.ConnectionString))
			{
				throw new ArgumentException("Connection string for key 'rabbit' is null of empty.");
			}
			return RabbitHutch.CreateBus(connectionString.ConnectionString, register =>
			{
				register.Register<IEasyNetQLogger>(_ => new QueueLogger());
				register.Register<IAdvancedBus, RabbitAdvancedBusExtended>();
			});
		}

		public static void PublishWithDelay<T>(this IBus bus, T message, string resultQueueName, TimeSpan delay) 
			where T : class
		{
			var exchange = bus.Advanced.ExchangeDeclare(resultQueueName + "_exchange", EasyNetQ.Topology.ExchangeType.Direct);
			var delayQueue = ((RabbitAdvancedBusExtended)bus.Advanced).QueueWithDelayDeclare(resultQueueName + "_delay", exchange.Name, perQueueTtl: (int)delay.TotalMilliseconds);
			var resultQueue = bus.Advanced.QueueDeclare(resultQueueName);
			bus.Advanced.Bind(exchange, resultQueue, "");
			bus.Advanced.Publish(Exchange.GetDefault(), delayQueue.Name, false, false, new Message<T>(message));
		}
	}

	public class RabbitAdvancedBusExtended : RabbitAdvancedBus
	{
		public RabbitAdvancedBusExtended(IConnectionFactory connectionFactory, ISerializer serializer, IConsumerFactory consumerFactory, IEasyNetQLogger logger, Func<string> getCorrelationId, IClientCommandDispatcherFactory clientCommandDispatcherFactory, IPublisherConfirms publisherConfirms, IEventBus eventBus, ITypeNameSerializer typeNameSerializer, IHandlerCollectionFactory handlerCollectionFactory, IContainer container) 
			: base(connectionFactory, serializer, consumerFactory, logger, getCorrelationId, clientCommandDispatcherFactory, publisherConfirms, eventBus, typeNameSerializer, handlerCollectionFactory, container)
		{
			
		}

		public IQueue QueueWithDelayDeclare(string name, string deadLetterExchange, string deadLetterRoutingKey = "", int perQueueTtl = 2147483647, int expires = 2147483647)
		{
			IDictionary<string, object> arguments = new Dictionary<string, object>();
			
			if (perQueueTtl != int.MaxValue)
			{
				arguments.Add("x-message-ttl", perQueueTtl);
			}

			if (expires != int.MaxValue)
			{
				arguments.Add("x-expires", expires);
			}

			arguments.Add("x-dead-letter-exchange", deadLetterExchange);
			arguments.Add("x-dead-letter-routing-key", deadLetterRoutingKey);

			var d = typeof (RabbitAdvancedBus).GetField("clientCommandDispatcher", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
			var dispatcher = d.GetValue(this) as IClientCommandDispatcher;
			if (dispatcher == null)
			{
				throw new Exception("Error getting dispatcher.");
			}
			dispatcher.Invoke(
				x => x.QueueDeclare(name, true, false, false, arguments)
			).Wait();

			return new Queue(name, false);
		}
	}
}