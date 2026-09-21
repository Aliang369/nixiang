using System;
using System.Collections.Generic;

public class EventInfoNoData
{
	public List<Action> listenerMap = new List<Action>();

	public void TriggerEvent()
	{
		try
		{
			foreach (Action item in new List<Action>(listenerMap))
			{
				item();
			}
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "全局事件异常 " + ToString(), ThinkingAnalyticsExtends.ExceptionType.GlobalEvent);
		}
	}

	public void AddListener(Action listener)
	{
		listenerMap.Add(listener);
	}

	public void RemoveListener(Action listner)
	{
		listenerMap.Remove(listner);
	}
}
