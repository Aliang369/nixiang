using UnityEngine.UIElements;

public class zbSetNotifyPnl
{
	private zbSetWnd m_parent;

	private ScrollView m_sc;

	public zbSetNotifyPnl(VisualElement root, zbSetWnd parent)
	{
		m_sc = UQueryExtensions.Q<ScrollView>(root, "sc", (string)null);
		m_parent = parent;
	}

	public void Refresh()
	{
		zbWsMsgHandler.Instance.PushSet_Data_Req();
	}

	public void OnPushSetAck()
	{
		zbPushSetCsvRecord[] records = zbDataConfig.Instance.pushSetCsvTable.GetRecords().records;
		if (records.Length != 0)
		{
			((VisualElement)m_sc).Clear();
			foreach (zbPushSetCsvRecord record in records)
			{
				zbSetNotifyItem zbSetNotifyItem2 = new zbSetNotifyItem(m_parent);
				zbSetNotifyItem2.Init(record);
				((VisualElement)m_sc).Add((VisualElement)(object)zbSetNotifyItem2);
			}
		}
	}
}
