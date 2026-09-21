using System;
using System.Collections.Generic;

public class EventInfo<DATATYPE>
{
	public List<Action<DATATYPE>> listenerMap = new List<Action<DATATYPE>>();

	public void TriggerEvent(DATATYPE data)
	{
		try
		{
			foreach (Action<DATATYPE> item in new List<Action<DATATYPE>>(listenerMap))
			{
				item(data);
			}
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "全局事件异常 " + ToString(), ThinkingAnalyticsExtends.ExceptionType.GlobalEvent);
		}
	}

	public void AddListener(Action<DATATYPE> listener)
	{
		listenerMap.Add(listener);
	}

	public void RemoveListener(Action<DATATYPE> listner)
	{
		listenerMap.Remove(listner);
	}
}
