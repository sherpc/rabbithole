using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitHoleNode.Interfaces
{
	public interface IService<TSource, TDestination>
	{
		Task<IEnumerable<TDestination>> Process(TSource message);

		string ServiceName { get; }
	}
}