﻿using System;
using CommandLine.Text;

namespace RabbitHoleNode.Node
{
	public static class NodeRunner
	{
		public static void RunNode(string[] args)
		{
			var settings = new NodeSettings();
			if (!CommandLine.Parser.Default.ParseArguments(args, settings))
			{
				Console.WriteLine(HelpText.AutoBuild(settings).ToString());
				return;
			}

			settings.ServiceName = String.Format("{0}.{1}, MK.Import.Domain", settings.ServiceNamespace, settings.ServiceName);

			settings.DtoFromType = String.Format("MK.Import.Domain.Dto.Messaging.{0}, MK.Import.Domain", settings.DtoFromType);
			settings.DtoToType = String.Format("MK.Import.Domain.Dto.Messaging.{0}, MK.Import.Domain", settings.DtoToType);

			RunNode(settings);
		}

		private static void RunNode(NodeSettings settings)
		{
			var serviceType = GetTypeFromString(settings.ServiceName);
			var service = Activator.CreateInstance(serviceType, false);

			var dtoFromType = GetTypeFromString(settings.DtoFromType);
			var dtoToType = GetTypeFromString(settings.DtoToType);

			var template = typeof(ServiceNode<,>);
			var serviceNodeType = template.MakeGenericType(dtoFromType, dtoToType);

			using (var node = (IServiceNode)Activator.CreateInstance(serviceNodeType, service, settings))
			{
				node.Start();

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
