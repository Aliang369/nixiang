using System;
using UnityEngine;

public class Safe
{
	public static void Do(Action action)
	{
		try
		{
			action?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
