using System;
using CommandLine;

namespace RabbitHoleNode.Node
{
	public class NodeSettings
	{
		[Option('f', "From", Required = true, HelpText = "Source queue for node.")]
		public string SourceQueue { get; set; }

		[Option('t', "To", Required = false, DefaultValue = null, HelpText = "Destination queue for node.")]
		public string DestinationQueue { get; set; }

		[Option('l', "TasksLimit", DefaultValue = 2, HelpText = "Maximum count of async message processing tasks.")]
		public int MaxTasks { get; set; }

		[Option('n', "Name", DefaultValue = null, HelpText = "Node name.")]
		public string NodeName { get; set; }

		[Option('p', "State-notification-period", DefaultValue = 5, HelpText = "Node state notification period in seconds.")]
		public int StateNotificationPeriod { get; set; }

		[Option('s', "Service", Required = true, HelpText = "Service class name. Must be in namespace, setted by -namespace.")]
		public string ServiceName { get; set; }

		[Option("DtoFrom", Required = true, HelpText = "Type for source DTO.")]
		public string DtoFromType { get; set; }

		[Option("DtoTo", Required = false, DefaultValue = null, HelpText = "Type for destination DTO.")]
		public string DtoToType { get; set; }

		public bool IsOutputNode()
		{
			// Если не задан хотя бы один из параметров, нужных для
			// выходной очереди, считаем, что нода ничего не производит
			return !(String.IsNullOrWhiteSpace(DestinationQueue)
			       || String.IsNullOrWhiteSpace(DtoToType));
		}
	}
}
