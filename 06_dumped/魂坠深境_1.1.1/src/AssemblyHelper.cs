using System;

public static class AssemblyHelper
{
	public static bool ImplementOrIs(this Type fieldType, Type interfaceType)
	{
		if (fieldType == interfaceType)
		{
			return true;
		}
		Type[] interfaces = fieldType.GetInterfaces();
		for (int i = 0; i < interfaces.Length; i++)
		{
			if (interfaces[i] == interfaceType)
			{
				return true;
			}
		}
		return false;
	}
}
