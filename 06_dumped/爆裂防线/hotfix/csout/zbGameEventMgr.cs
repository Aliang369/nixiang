using System;
using System.Collections.Generic;

public class zbGameEventMgr
{
	private class EventEntry
	{
		public List<Action<object>> handlers = new List<Action<object>>();

		public Action<object>[] buffer = new Action<object>[8];

		public bool dirty = true;

		public int dispatching;
	}

	private static zbGameEventMgr sInstance;

	private Dictionary<string, EventEntry> m_eventTable = new Dictionary<string, EventEntry>();

	public static zbGameEventMgr Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbGameEventMgr();
			}
			return sInstance;
		}
	}

	public void RegisterEvent(string eventName, Action<object> callback)
	{
		if (!string.IsNullOrEmpty(eventName) && callback != null)
		{
			if (!m_eventTable.TryGetValue(eventName, out var value))
			{
				value = new EventEntry();
				m_eventTable[eventName] = value;
			}
			if (!value.handlers.Contains(callback))
			{
				value.handlers.Add(callback);
				value.dirty = true;
			}
		}
	}

	public void UnregisterEvent(string eventName, Action<object> callback)
	{
		if (!string.IsNullOrEmpty(eventName) && callback != null && m_eventTable.TryGetValue(eventName, out var value))
		{
			value.handlers.Remove(callback);
			if (value.handlers.Count == 0)
			{
				m_eventTable.Remove(eventName);
			}
			else
			{
				value.dirty = true;
			}
		}
	}

	public void SendEvent(string eventName, object parameter = null)
	{
		if (!m_eventTable.TryGetValue(eventName, out var value))
		{
			return;
		}
		Action<object>[] array;
		int num;
		if (value.dispatching > 0)
		{
			array = value.handlers.ToArray();
			num = array.Length;
		}
		else
		{
			num = value.handlers.Count;
			if (num == 0)
			{
				return;
			}
			if (value.dirty || value.buffer.Length < num)
			{
				if (value.buffer.Length < num)
				{
					int num2;
					for (num2 = value.buffer.Length; num2 < num; num2 *= 2)
					{
					}
					value.buffer = new Action<object>[num2];
				}
				value.handlers.CopyTo(value.buffer);
				value.dirty = false;
			}
			array = value.buffer;
		}
		value.dispatching++;
		try
		{
			for (int i = 0; i < num; i++)
			{
				Action<object> action = array[i];
				if (action != null)
				{
					try
					{
						action(parameter);
					}
					catch (Exception)
					{
					}
				}
			}
		}
		finally
		{
			value.dispatching--;
		}
	}
}
