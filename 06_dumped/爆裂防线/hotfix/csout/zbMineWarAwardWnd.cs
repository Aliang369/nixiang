using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbMineWarAwardWnd : zbBaseWnd
{
	private zbItem[] m_items = new zbItem[3];

	private Button m_btnAward;

	private Button m_btnTouchTest;

	private Button m_btnclose;

	protected override void OnEnable()
	{
		base.OnEnable();
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "award", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(val, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnclose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnclose, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		m_btnAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAward, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(AwardDetail[] awardlist)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (AwardDetail awardDetail in awardlist)
		{
			if (num < m_items.Length)
			{
				zbItemData data = new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count);
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(data);
			}
			num++;
		}
		VisualElement[] items = (VisualElement[])(object)m_items;
		zbUtil.CenterNodeItems(items);
	}

	private void OnTouchTest(ClickEvent cvt)
	{
		Hide();
	}
}
