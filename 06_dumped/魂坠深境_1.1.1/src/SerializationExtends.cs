using System;
using System.Collections.Generic;
using System.Reflection;

public static class SerializationExtends
{
	public class ObjectInstance
	{
		public object obj;

		public List<ObjectInstance> subObjects;
	}

	public static ObjectInstance GetObjectInstance(object obj)
	{
		ObjectInstance objectInstance = new ObjectInstance();
		objectInstance.obj = obj;
		objectInstance.subObjects = new List<ObjectInstance>();
		Type type = obj.GetType();
		if (type.IsPrimitive)
		{
			return new ObjectInstance
			{
				obj = obj
			};
		}
		if (type == typeof(string))
		{
			return new ObjectInstance
			{
				obj = obj
			};
		}
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			objectInstance.subObjects.Add(GetObjectInstance(fieldInfo.GetValue(obj)));
		}
		return objectInstance;
	}
}
