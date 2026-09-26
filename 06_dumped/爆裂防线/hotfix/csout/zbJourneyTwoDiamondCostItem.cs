using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyTwoDiamondCostItem : zbViewBaseItem
{
	private zbJourneyTwoDiamondCostWnd m_wndParent;

	private Label m_lbDesc;

	private Label m_lbNeed;

	private zbItem[] m_items;

	private Button m_btnTouch;

	private VisualElement m_spriteGetted;

	private VisualElement m_spriteProgress;

	private VisualElement m_notify;

	private JourneyTargetRecordDetail m_data;

	public zbJourneyTwoDiamondCostItem(zbJourneyTwoDiamondCostWnd wndParent, VisualTreeAsset tAsset)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		m_items = new zbItem[5];
		base._002Ector();
		m_wndParent = wndParent;
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_lbNeed = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_need", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_spriteGetted = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_getted", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(3f, 3f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbChapterTwoDiamondCostCsvRecord record)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		m_data = ((IEnumerable<JourneyTargetRecordDetail>)zbServerData.sGameData.JourneyInfo.ChapterTwo.DiamondCost.TargetList).FirstOrDefault((JourneyTargetRecordDetail v) => v.Target == record.diamondCost);
		float num = Mathf.Min((float)zbServerData.sGameData.JourneyInfo.ChapterTwo.DiamondCost.Reach / (float)m_data.Target, 1f);
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num * 200f, (LengthUnit)0));
		((TextElement)m_lbDesc).text = string.Format(string.Format("{0}x{1}", zbUtil.Language("lang.10500"), m_data.Target), zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(2).name);
		((TextElement)m_lbNeed).text = $"{zbServerData.sGameData.JourneyInfo.ChapterTwo.DiamondCost.Reach}/{m_data.Target}";
		for (int num2 = 0; num2 < 5; num2++)
		{
			((VisualElement)m_items[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num3 = 0;
		int[][] awardList = record.awardList;
		foreach (int[] array in awardList)
		{
			if (num3 < m_items.Length)
			{
				zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2]);
				((VisualElement)m_items[num3]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num3].Refresh(data);
			}
			num3++;
		}
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.State == ECommonState.End));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.State != ECommonState.Yes));
		m_spriteGetted.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.State != ECommonState.End));
		if (m_data.State == ECommonState.Yes)
		{
			((TextElement)m_btnTouch).text = zbUtil.Language("lang.10090");
			((VisualElement)m_btnTouch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_green.png"));
		}
		else if (m_data.State == ECommonState.No)
		{
			((TextElement)m_btnTouch).text = zbUtil.Language("lang.10155");
			((VisualElement)m_btnTouch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png"));
		}
	}

	private void OnTouch(ClickEvent evt)
	{
		if (m_data.State == ECommonState.No)
		{
			zbUIMgr.Instance.ShowShop();
		}
		else
		{
			zbWsMsgHandler.Instance.Journey_ChapterTwoDiamondCost_Req(m_data.Index);
		}
	}
}
