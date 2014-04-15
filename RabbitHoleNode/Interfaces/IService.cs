using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitHoleNode.Interfaces
{
	public interface IService<TSource, TDestination>
	{
		Task<IEnumerable<TDestination>> Process(TSource message);

		string ServiceName { get; }
	}

	public abstract class SingleItemImportService<T> : IService<T, T>
	{
		public async Task<IEnumerable<T>> Process(T message)
		{
			var result = await ProcessItem(message);
			return Enumerable.Repeat(result, 1);
		}

		protected abstract Task<T> ProcessItem(T message);

		public abstract string ServiceName { get; }
	}
}