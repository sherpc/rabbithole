using System;
using System.Configuration;
using System.Linq;
using CommandLine;

namespace RabbitHoleNode.Node
{
	public static class OptionsHelper
	{
		public static T GetOptionsFromConfig<T>()
		{
			var type = typeof(T);
			var optionType = typeof(OptionAttribute);
			var properties = type
				.GetProperties()
				.Select(p => new { Property = p, Option = p.GetCustomAttributes(optionType, false).OfType<OptionAttribute>().FirstOrDefault() })
				.Where(p => p.Option != null)
				.ToList();
			var result = Activator.CreateInstance<T>();
			foreach (var property in properties)
			{
				object value = ConfigurationManager.AppSettings[property.Option.LongName];
				if (value == null || String.IsNullOrWhiteSpace(value.ToString()))
				{
					if(property.Option.Required)
						throw new ArgumentException("Can't found option with key '" + property.Option.LongName + "' in *.config.");
					value = property.Option.DefaultValue;
				}
				
				property.Property.SetValue(result, value);
			}
			return result;
		}
	}
}
