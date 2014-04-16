using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace RabbitHoleNode.Node
{
	public class NodeStatus
	{
		public string Name { get; private set; }
		public NodeState State { get; private set; }
		public int TasksCount { get; private set; }
		public DateTime Timestamp { get; private set; }
		public string MachineName { get; private set; }
		public int ProcessId { get; private set; }
		public long Memory { get; private set; }

		public NodeSettings NodeSettings { get; private set; }

		[JsonConstructor]
		public NodeStatus(string name, NodeState state, int tasksCount, DateTime timestamp, string machineName, int processId, long memory, NodeSettings nodeSettings)
		{
			Name = name;
			State = state;
			TasksCount = tasksCount;
			Timestamp = timestamp;
			MachineName = machineName;
			ProcessId = processId;
			Memory = memory;
			NodeSettings = nodeSettings;
		}

		public NodeStatus(string name, NodeState state, int tasksCount, NodeSettings nodeSettings)
			: this(name, state, tasksCount, DateTime.Now, Environment.MachineName, Process.GetCurrentProcess().Id, Process.GetCurrentProcess().PrivateMemorySize64, nodeSettings)
		{
			
		}
	}

	public enum NodeState
	{
		Stopped = 0,
		Stops = 1,
		Running = 10,
	}
}