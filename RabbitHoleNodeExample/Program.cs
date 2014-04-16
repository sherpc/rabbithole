using RabbitHoleNode;
using RabbitHoleNode.Node;

namespace RabbitHoleNodeExample
{
	static class Program
	{
		static void Main(string[] args)
		{
			var settings = OptionsHelper.GetOptionsFromConfig<NodeSettings>();
			NodeRunner.RunNode(settings);
		}
	}
}
