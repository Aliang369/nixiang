using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbMainQuestPassAwardWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnAllClose;

	private zbItem[] m_items = new zbItem[5];

	private Label m_lbStar;

	private Label m_lbContent;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnAllClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_allClose", (string)null);
		m_lbStar = UQueryExtensions.Q<Label>(m_RootElement, "lb_star", (string)null);
		m_lbContent = UQueryExtensions.Q<Label>(m_RootElement, "lb_content", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAllClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshAwards(int boxIndex, zbMainLevelCsvRecord mainLevelRecord)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		int[][] array = null;
		switch (boxIndex)
		{
		case 1:
			array = mainLevelRecord.awardStar1;
			((TextElement)m_lbContent).text = zbUtil.Language("lang.10098");
			break;
		case 2:
			array = mainLevelRecord.awardStar2;
			((TextElement)m_lbContent).text = zbUtil.Language("lang.10099");
			break;
		case 3:
			array = mainLevelRecord.awardStar3;
			((TextElement)m_lbContent).text = zbUtil.Language("lang.10100");
			break;
		}
		if (mainLevelRecord == null || array == null)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		int[][] array2 = array;
		foreach (int[] array3 in array2)
		{
			if (num < m_items.Length)
			{
				zbItemData data = new zbItemData((EResourceType)array3[0], array3[1], array3[2]);
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(data);
			}
			num++;
		}
		VisualElement[] items = (VisualElement[])(object)m_items;
		zbUtil.CenterNodeItems(items);
	}

	public void RefreshEliteAwards(int boxIndex, zbEliteLevelCsvRecord eliteLevelRecord)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		int[][] array = null;
		switch (boxIndex)
		{
		case 1:
			array = eliteLevelRecord.awardStar1;
			((TextElement)m_lbContent).text = zbUtil.Language("lang.10098");
			break;
		case 2:
			array = eliteLevelRecord.awardStar2;
			((TextElement)m_lbContent).text = zbUtil.Language("lang.10099");
			break;
		case 3:
			array = eliteLevelRecord.awardStar3;
			((TextElement)m_lbContent).text = zbUtil.Language("lang.10100");
			break;
		}
		if (eliteLevelRecord == null || array == null)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		int[][] array2 = array;
		foreach (int[] array3 in array2)
		{
			if (num < m_items.Length)
			{
				zbItemData data = new zbItemData((EResourceType)array3[0], array3[1], array3[2]);
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(data);
			}
			num++;
		}
		VisualElement[] items = (VisualElement[])(object)m_items;
		zbUtil.CenterNodeItems(items);
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
