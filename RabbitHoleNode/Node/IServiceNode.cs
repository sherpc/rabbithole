using System;

namespace RabbitHoleNode.Node
{
	interface IServiceNode : IDisposable
	{
		void Start();
		string Name { get; }
	}
}