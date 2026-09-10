using System;
using UnityEngine;

public class AnimEventReceiver : MonoBehaviour
{
	public Action<object> onEventTrigger;

	public void _OnEventTrigger(string e)
	{
		onEventTrigger?.Invoke(e);
	}

	public void _OnEventTrigger(int i)
	{
		onEventTrigger?.Invoke(i);
	}

	public void _OnEventTrigger()
	{
		onEventTrigger?.Invoke(null);
	}
}
