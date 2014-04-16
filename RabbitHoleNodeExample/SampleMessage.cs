using System.Threading.Tasks;
using RabbitHoleNode.Interfaces;

namespace RabbitHoleNodeExample
{
	class SampleMessage
	{
		public int Value { get; set; }
	}

	class IdentityService : SameInputOutputService<SampleMessage>
	{
		protected override Task<SampleMessage> ProcessItem(SampleMessage message)
		{
			return Task.Run(() => message);
		}

		public override string ServiceName
		{
			get { return "IdentityService"; }
		}
	}
}
