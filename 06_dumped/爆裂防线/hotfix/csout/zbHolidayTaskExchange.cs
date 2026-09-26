using Proto.Types;
using UnityEngine.UIElements;

public class zbHolidayTaskExchange
{
	private zbHolidayTaskWnd m_wndParent;

	private Label m_lbScore;

	private zbHolidayTaskExchangeItem[] m_items = new zbHolidayTaskExchangeItem[5];

	public void OnEnable(zbHolidayTaskWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_lbScore = UQueryExtensions.Q<Label>(root, "lb_score", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbHolidayTaskExchangeItem>(root, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
	}

	public void Refresh()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_data == null)
		{
			return;
		}
		int num = m_wndParent.m_data.TotalScore - m_wndParent.m_data.ExchangeScore;
		((TextElement)m_lbScore).text = zbUtil.Language("lang.10900") + ": " + num;
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num2 = 0;
		foreach (HolidayTaskExchangeDetail exchange in m_wndParent.m_data.ExchangeList)
		{
			if (num2 < m_items.Length)
			{
				((VisualElement)m_items[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num2].Refresh(exchange);
			}
			num2++;
		}
	}
}
