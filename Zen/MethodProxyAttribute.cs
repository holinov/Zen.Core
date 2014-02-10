using System;
using System.Reflection;

namespace Zen
{
	public class MethodProxyAttribute:Attribute
	{
		private readonly Type targetType;

		public MethodProxyAttribute(Type targetType)
		{
			this.targetType = targetType;
		}

		public MethodInfo GetMethod(string name)
		{
			return targetType.GetMethod(name);
		}
	}
}