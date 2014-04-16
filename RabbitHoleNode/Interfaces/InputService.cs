using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitHoleNode.Interfaces
{
	public abstract class InputService<T> : IService<T, T>
	{
		public async Task<IEnumerable<T>> Process(T message)
		{
			await ProcessItem(message);
			return Enumerable.Empty<T>();
		}

		protected abstract Task ProcessItem(T message);

		public abstract string ServiceName { get; }
	}
}