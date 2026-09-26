using System;
using System.Collections.Generic;

public class zbGameEvent
{
	private Dictionary<string, List<Action<object>>> m_eventTable;

	public void RegisterEvent(string eventName, Action<object> callback)
	{
		if (!string.IsNullOrEmpty(eventName) && callback != null)
		{
			if (m_eventTable == null)
			{
				m_eventTable = new Dictionary<string, List<Action<object>>>();
			}
			if (!m_eventTable.TryGetValue(eventName, out var value))
			{
				value = new List<Action<object>>();
				m_eventTable[eventName] = value;
			}
			if (!value.Contains(callback))
			{
				value.Add(callback);
				zbGameEventMgr.Instance.RegisterEvent(eventName, callback);
			}
		}
	}

	public void UnRegisterCallback()
	{
		if (m_eventTable == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<Action<object>>> item in m_eventTable)
		{
			foreach (Action<object> item2 in item.Value)
			{
				zbGameEventMgr.Instance.UnregisterEvent(item.Key, item2);
			}
		}
		m_eventTable.Clear();
	}
}
