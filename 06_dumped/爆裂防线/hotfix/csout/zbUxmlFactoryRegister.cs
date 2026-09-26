using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class zbUxmlFactoryRegister
{
	public static void RegisterHotUpdateFactories()
	{
		try
		{
			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "UnityEngine.UIElementsModule");
			if (assembly == null)
			{
				Debug.LogError((object)"[zbUxmlFactoryRegister] UnityEngine.UIElementsModule 未找到");
				return;
			}
			Type type = assembly.GetType("UnityEngine.UIElements.VisualElementFactoryRegistry");
			if (type == null)
			{
				Debug.LogError((object)"[zbUxmlFactoryRegister] VisualElementFactoryRegistry 类型未找到");
				return;
			}
			MethodInfo method = type.GetMethod("RegisterFactory", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method == null)
			{
				Debug.LogError((object)"[zbUxmlFactoryRegister] RegisterFactory 方法未找到");
				return;
			}
			Assembly assembly2 = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "gamescript");
			if (assembly2 == null)
			{
				Debug.LogError((object)"[zbUxmlFactoryRegister] gamescript 程序集未找到");
				return;
			}
			int num = 0;
			Type[] types = assembly2.GetTypes();
			foreach (Type type2 in types)
			{
				if (!type2.IsClass || type2.IsAbstract || !type2.Name.Contains("UxmlFactory") || !type2.GetInterfaces().Any((Type i) => i.Name == "IUxmlFactory"))
				{
					continue;
				}
				try
				{
					object obj = Activator.CreateInstance(type2);
					method.Invoke(null, new object[1] { obj });
					num++;
				}
				catch (TargetInvocationException ex)
				{
					if (!(ex.InnerException is ArgumentException))
					{
						Debug.LogWarning((object)("[zbUxmlFactoryRegister] 跳过 " + type2.FullName + ": " + ex.InnerException?.Message));
					}
				}
				catch (ArgumentException)
				{
				}
			}
			Debug.Log((object)$"[zbUxmlFactoryRegister] 注册完成，新增 {num} 个工厂");
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"[zbUxmlFactoryRegister] 注册失败: {arg}");
		}
	}
}
