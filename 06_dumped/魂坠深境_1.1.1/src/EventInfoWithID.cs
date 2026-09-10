using System;
using System.Collections.Generic;

public class EventInfoWithID<DATATYPE, IDTYPE>
{
	public List<Action<IDTYPE, DATATYPE>> globallistenerMap = new List<Action<IDTYPE, DATATYPE>>();

	public Dictionary<IDTYPE, List<Action<IDTYPE, DATATYPE>>> listenerMap = new Dictionary<IDTYPE, List<Action<IDTYPE, DATATYPE>>>();

	public void TriggerEvent(IDTYPE id, DATATYPE data)
	{
		try
		{
			if (listenerMap.ContainsKey(id))
			{
				foreach (Action<IDTYPE, DATATYPE> item in new List<Action<IDTYPE, DATATYPE>>(listenerMap[id].OrEmptyIfNull()))
				{
					item(id, data);
				}
			}
			foreach (Action<IDTYPE, DATATYPE> item2 in new List<Action<IDTYPE, DATATYPE>>(globallistenerMap))
			{
				item2(id, data);
			}
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "全局事件异常 " + ToString(), ThinkingAnalyticsExtends.ExceptionType.GlobalEvent);
		}
	}

	public void AddListener(IDTYPE id, Action<IDTYPE, DATATYPE> listener)
	{
		if (!listenerMap.ContainsKey(id))
		{
			listenerMap.Add(id, new List<Action<IDTYPE, DATATYPE>>());
		}
		listenerMap[id].Add(listener);
	}

	public void RemoveListener(IDTYPE id, Action<IDTYPE, DATATYPE> listener)
	{
		if (listenerMap.ContainsKey(id))
		{
			listenerMap[id].Remove(listener);
		}
	}

	public void AddGlobalListener(Action<IDTYPE, DATATYPE> listener)
	{
		globallistenerMap.Add(listener);
	}

	public void RemoveGlobalListener(Action<IDTYPE, DATATYPE> listener)
	{
		globallistenerMap.Remove(listener);
	}
}
