using System;
using EasyNetQ;

namespace RabbitHoleNode.Logger
{
	class QueueLogger : IEasyNetQLogger
	{
		public void DebugWrite(string format, params object[] args)
		{
			
		}

		public void InfoWrite(string format, params object[] args)
		{
			
		}

		public void ErrorWrite(string format, params object[] args)
		{
			//Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR: " + format);
			//Console.ResetColor();
		}

		public void ErrorWrite(Exception exception)
		{
			//Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR: " + exception);
			//Console.ResetColor();
		}
	}
}