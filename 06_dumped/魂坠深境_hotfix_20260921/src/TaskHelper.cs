using System;
using System.Threading.Tasks;

public static class TaskHelper
{
	public class Bool
	{
		public bool value;
	}

	public static async Task Wait(Func<bool> func)
	{
		while (!func())
		{
			await Task.Yield();
		}
	}

	public static async Task Wait(Bool checker)
	{
		while (!checker.value)
		{
			await Task.Yield();
		}
	}
}
