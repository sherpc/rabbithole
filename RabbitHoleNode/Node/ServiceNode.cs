using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using RabbitHoleNode.Interfaces;

namespace RabbitHoleNode.Node
{
	class ServiceNode<TSource, TDestination> : IServiceNode
		where TDestination : class
		where TSource : class
	{
		private readonly IService<TSource, TDestination> _service;
		private readonly NodeSettings _nodeSettings;

		public string Name { get; private set; }


		private int _runningTasks;
		private readonly IBus _bus;
		private volatile bool _needShutdown;

		private readonly object _lock;

		public ServiceNode(IService<TSource, TDestination> service, NodeSettings nodeSettings)
		{
			_service = service;
			_nodeSettings = nodeSettings;

			Name = GenerateNodeName(service, nodeSettings.NodeName);

			_runningTasks = 0;
			_bus = RabbitHelper.CreateBus();
			_needShutdown = false;
			_lock = new object();
		}

		public void Start()
		{
			_bus.Receive<TSource>(_nodeSettings.SourceQueue, async message =>
			{
				if (_needShutdown)
					throw new ShutdownNeededException();
				
				while (!IncrementTasksCount())
				{
					await Task.Delay(1000);
				}
				using (new Disposable(DecrementTasksCount))
				{
					if (_needShutdown)
					{
						throw new ShutdownNeededException();
					}

					OnMessageReceived(message);
					var results = await _service.Process(message);
					if (_nodeSettings.IsOutputNode())
					{
						PublishResults(results);		
					}
				}
			});

			Action notifyEveryAction = async () =>
			{
				while (!_needShutdown)
				{
					await NotifyTasksCount();
					await Task.Delay(TimeSpan.FromSeconds(_nodeSettings.StateNotificationPeriod));
				}
			};

			Task.Run(notifyEveryAction);

			_bus.Subscribe<NodeStatus>(Name, status =>
			{
				_needShutdown = true;
				Console.WriteLine("Shutdown needed");
				// Ждем уже работающие задачи
				while (_runningTasks != 0)
				{
					Thread.Sleep(100);
				}
				// Задержка нужна, чтобы успело отправится подтверждение. Т.к. между DecrementTasksCount() и реальной отправкой ack есть задержка.
				Thread.Sleep(100);
				Console.WriteLine("Shutdown.");
				// Закрываем шину только после того, как отработали уже запущенные таски.
				_bus.Dispose();
		#warning Единственный вариант, когда система окажется не консистентной,
				// это если процесс ноды убить в тот момент, когда отработал OnCompleted (т.е. прошел Send в destination очередь), 
				// но еще не отправился Ack (подтверждение) в source очередь.
				// Тогда Source очередь будет считать, что сообщение еще не отработано, и отправит его первой живой ноде,
				// а в Destination очереди уже будет лежать результат.

				// Именно поэтому лучше убивать ноды через NodeManager и сообщение shutdown, а не убивать процесс.
				Environment.Exit(1);
			}, configuration => configuration.WithTopic("node.shutdown." + Name).WithAutoDelete());
		}

		/// <summary>
		/// Returns true if success, false if max tasks count alredy runned. Must be thread-safe.
		/// </summary>
		/// <returns></returns>
		private bool IncrementTasksCount()
		{
			if (_runningTasks >= _nodeSettings.MaxTasks)
				return false;

			lock (_lock)
			{
				if (_runningTasks >= _nodeSettings.MaxTasks)
					return false;

				Interlocked.Increment(ref _runningTasks);
				return true;
			}
		}

		private void DecrementTasksCount()
		{
			Interlocked.Decrement(ref _runningTasks);
		}

		private async Task NotifyTasksCount()
		{
			await _bus.PublishAsync(new NodeStatus(Name, NodeState.Running, _runningTasks, _nodeSettings), "node.status." + Name);
		}

		private void PublishResults(IEnumerable<TDestination> results)
		{
			Console.WriteLine("Processed message. Items:");
			foreach (var result in results)
			{
				_bus.Send(_nodeSettings.DestinationQueue, result);
				Console.WriteLine("\t--> {0}", result);
			}
		}

		private static void OnMessageReceived(TSource message)
		{
			Console.WriteLine("Got message: {0}", message);
		}

		public void Dispose()
		{
			_bus.Dispose();
		}

		private static string GenerateNodeName(IService<TSource, TDestination> service, string name)
		{
			return String.IsNullOrWhiteSpace(name)
				? String.Format("{0}#{1}", service.ServiceName.ToLowerInvariant(), Guid.NewGuid())
				: name;
		}
	}

	public class ShutdownNeededException : Exception
	{
		
	}

	public class Disposable : IDisposable
	{
		private readonly Action _dispose;

		public Disposable(Action dispose)
		{
			_dispose = dispose;
		}

		public void Dispose()
		{
			_dispose();
		}
	}
}