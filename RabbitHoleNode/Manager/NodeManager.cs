using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using EasyNetQ;
using RabbitHoleNode.Node;

namespace RabbitHoleNode.Manager
{
	public class NodeManager : IDisposable
	{
		private readonly string _nodeExePath;
		private readonly ConcurrentDictionary<string, NodeStatus> _nodes;
		private readonly IBus _bus;

		public NodeManager(string nodeExePath)
		{
			_nodeExePath = nodeExePath;
			_nodes = new ConcurrentDictionary<string, NodeStatus>();
			_bus = RabbitHelper.CreateBus();

			_bus.Receive<NodeStatus>("nodes_status", status => _nodes.AddOrUpdate(status.Name, status, (s, nodeStatus) => status));
		}

		public ICollection<NodeStatus> GetNodesStatus()
		{
			return _nodes.Values;
		}

		public void ForgetNode(NodeStatus node)
		{
			while (true)
			{
				NodeStatus removedNode;
				if(_nodes.TryRemove(node.Name, out removedNode))
					return;
			}
		}

		/// <summary>
		/// Returns false if node not present in dictionary.
		/// </summary>
		/// <param name="nodeName"></param>
		/// <returns></returns>
		public bool SendShutdown(string nodeName)
		{
			NodeStatus node;
			if (!_nodes.ContainsKey(nodeName))
				return false;
			while (!_nodes.TryGetValue(nodeName, out node))
			{
				
			}
			_bus.Publish(node, "node.shutdown." + node.Name);
			return true;
		}

		public void CreateNode(NodeSettings options, string args)
		{
			var psi = new ProcessStartInfo(_nodeExePath, args)
			{
				CreateNoWindow = false
			};
			Process.Start(psi);
		}

		public void Dispose()
		{
			_bus.Dispose();
		}
	}
}