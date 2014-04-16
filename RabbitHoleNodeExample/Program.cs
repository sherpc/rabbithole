using System;
using System.Collections.Generic;
using System.Linq;
using RabbitHoleNode.Manager;
using RabbitHoleNode.Node;

namespace RabbitHoleNodeExample
{
	static class Program
	{
		static void Main(string[] args)
		{
			var manager = new NodeManager(@"");

			Console.WriteLine("Hello!");
			Console.WriteLine("Enter 's' for status, 'c <node-args>' for node spawn, 'k <nodename>' for kill node, 'q' for quit.");

			while (true)
			{
				Console.Write("Command: ");
				var input = Console.ReadLine();
				var command = input.Split(' ');
				switch (command[0])
				{
					case "":
					case "s": // Status
						var statuses = manager
							.GetNodesStatus()
							.ToList()
							// Если 5 минут ничего не слышно от ноды, считаем её неактивной
							.GroupBy(s => (DateTime.Now - s.Timestamp).TotalSeconds > 10)
							.ToList();
						var aliveNodes = statuses.Where(s => !s.Key).SelectMany(s => s).ToList();
						var deadNodes = statuses.Where(s => s.Key).SelectMany(s => s).ToList();

						foreach (var status in deadNodes)
						{
							manager.ForgetNode(status);
						}
						PrintTable(aliveNodes);
						break;
					case "q": // Quit
						manager.Dispose();
						Console.WriteLine("Bye!");
						return;
					//case "c": // Create
					//	var options = new NodeSettings();
					//	if (!CommandLine.Parser.Default.ParseArguments(command, options))
					//	{
					//		Console.WriteLine("Invalid node creation args. Help:");
					//		Console.WriteLine(HelpText.AutoBuild(options).ToString());
					//		break;
					//	}
					//	manager.CreateNode(options, String.Join(" ", command.Skip(1)));
					//	break;
					case "k": // Kill node
						if (command.Length != 2)
						{
							Console.WriteLine("Invalid args count.");
							break;
						}
						var nodeName = command[1];
						if (!manager.SendShutdown(nodeName))
							Console.WriteLine("Node '{0}' doesn't exists or inactive for long time.", nodeName);
						break;
					default:
						Console.WriteLine("Invalid command!");
						break;
				}
			}
		}

		static void PrintTable(List<NodeStatus> aliveNodes)
		{
			if (aliveNodes.Count == 0)
			{
				Console.WriteLine("No active nodes.");
				return;
			}

			Console.WriteLine(Separator);
			Console.WriteLine(RowFormatter, "Name", "Status", "TasksCount", "Timestamp");
			Console.WriteLine(Separator);
			foreach (var status in aliveNodes)
			{
				Console.WriteLine(RowFormatter, status.Name, status.State, status.TasksCount + "/" + status.NodeSettings.MaxTasks, status.Timestamp);
			}
			Console.WriteLine(Separator);
		}


		private readonly static string Separator = new string(Enumerable.Repeat('-', 74).ToArray());
		private const string RowFormatter = "|{0,-30}|{1,-10}|{2,-10}|{3,-20}|";
	}
}
