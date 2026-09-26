using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyOneRechargeSumItem : zbViewBaseItem
{
	private zbJourneyOneRechargeSumWnd m_wndParent;

	private Label m_lbDesc;

	private Label m_lbNeed;

	private zbItem[] m_items = new zbItem[5];

	private Button m_btnTouch;

	private VisualElement m_itemgetted;

	private VisualElement m_spriteProgress;

	private JourneyTargetRecordDetail m_data;

	public zbJourneyOneRechargeSumItem(zbJourneyOneRechargeSumWnd wndParent, VisualTreeAsset tAsset)
	{
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
		m_itemgetted = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "itemgetted", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(JourneyTargetRecordDetail data)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		int reach = zbServerData.sGameData.JourneyInfo.ChapterOne.RechargeSum.Reach;
		int target = m_data.Target;
		float num = Mathf.Min((float)reach / (float)target, 1f);
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num * 200f, (LengthUnit)0));
		((TextElement)m_lbDesc).text = string.Format($"累计充值{target}元", zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(3).name);
		((TextElement)m_lbNeed).text = $"{reach}/{target}";
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num2 = 0;
		int[][] awardList = zbDataConfig.Instance.chapterOneRechargeSumCsvTable.GetRecordByKey(m_data.Index).awardList;
		foreach (int[] array in awardList)
		{
			if (num2 < m_items.Length)
			{
				zbItemData data2 = new zbItemData((EResourceType)array[0], array[1], array[2]);
				((VisualElement)m_items[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num2].Refresh(data2);
			}
			num2++;
		}
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.State == ECommonState.End));
		m_itemgetted.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.State != ECommonState.End));
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
			zbUIMgr.Instance.ShowRecharge(Mathf.Max(m_data.Target - (int)zbSelf.Instance.moneyHandler.GetCountByTid(3), 0));
		}
		else
		{
			zbWsMsgHandler.Instance.Journey_ChapterOneRechargeSum_Req(m_data.Index);
		}
	}
}
