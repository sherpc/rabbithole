using System;
using CommandLine.Text;
using RabbitHoleNode.Node;

namespace RabbitHoleNode
{
	static class Program
	{
		static void Main(string[] args)
		{
			var options = new NodeCreationOptions();
			if (!CommandLine.Parser.Default.ParseArguments(args, options))
			{
				Console.WriteLine(HelpText.AutoBuild(options).ToString());
				return;
			}

			var serviceTypeFullName = String.Format("{0}.{1}, MK.Import.Domain", options.ServiceNamespace, options.ServiceName);
			var serviceType = GetTypeFromString(serviceTypeFullName);
			var service = Activator.CreateInstance(serviceType, false);

			var dtoFromType = GetTypeFromString(String.Format("MK.Import.Domain.Dto.Messaging.{0}, MK.Import.Domain", options.DtoFromType));
			var dtoToType = GetTypeFromString(String.Format("MK.Import.Domain.Dto.Messaging.{0}, MK.Import.Domain", options.DtoToType));

			var template = typeof (ServiceNode<,>);
			var serviceNodeType = template.MakeGenericType(dtoFromType, dtoToType);
			
			using(var node = (IServiceNode)Activator.CreateInstance(serviceNodeType, service, options))
			//using (IServiceNode node = new ServiceNode<ImportEstateMessage, ImportEstateMessage>(new DuplicateImportService(), options))
			{
				node.Start();

				Console.ResetColor();
				Console.WriteLine("Node '{0}' listening for messages. Hit <return> to quit.", node.Name);
				Console.ReadLine();
			}
		}

		private static Type GetTypeFromString(string typeName)
		{
			var type = Type.GetType(typeName, false, true);
			if (type != null) 
				return type;
			Console.WriteLine("Can't find type '{0}'.", typeName);
			Environment.Exit(0);
			return type;
		}
	}
}
