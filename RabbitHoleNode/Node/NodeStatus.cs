using System;
using System.Diagnostics;

namespace RabbitHoleNode.Node
{
	public class NodeStatus
	{
		public string Name { get; private set; }
		public NodeState State { get; private set; }
		public int TasksCount { get; private set; }
		public int MaxTasks { get; private set; }
		public DateTime Timestamp { get; private set; }
		public string MachineName { get; private set; }
		public int ProcessId { get; private set; }

		public NodeStatus(string name, NodeState state, int tasksCount, int maxTasks, DateTime timestamp, string machineName, int processId)
		{
			Name = name;
			State = state;
			TasksCount = tasksCount;
			MaxTasks = maxTasks;
			Timestamp = timestamp;
			MachineName = machineName;
			ProcessId = processId;
		}

		public NodeStatus(string name, NodeState state, int tasksCount, int maxTasks)
			: this(name, state, tasksCount, maxTasks, DateTime.Now, Environment.MachineName, Process.GetCurrentProcess().Id)
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