using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRechargeDailyItem : zbViewBaseItem
{
	private zbRechargeDailyWnd m_wndParent;

	private Label m_lbDesc;

	private Label m_lbNeed;

	private zbItem[] m_items = new zbItem[5];

	private Button m_btnTouch;

	private VisualElement m_itemgetted;

	private VisualElement m_spriteProgress;

	private RechargeTargetDetail m_data;

	private string m_uuid;

	public zbRechargeDailyItem(zbRechargeDailyWnd wndParent, VisualTreeAsset tAsset)
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

	public void Refresh(RechargeTargetDetail data, string uuid)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_uuid = uuid;
		int rechargeDailyDaysByUUID = zbSelf.Instance.operateHandler.GetRechargeDailyDaysByUUID(m_uuid);
		int index = m_data.Index;
		float num = Mathf.Min((float)rechargeDailyDaysByUUID / (float)index, 1f);
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num * 200f, (LengthUnit)0));
		((TextElement)m_lbDesc).text = "完成任意1笔充值";
		((TextElement)m_lbNeed).text = $"{index} Day";
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num2 = 0;
		foreach (AwardDetail award in m_data.AwardList)
		{
			if (num2 < m_items.Length)
			{
				zbItemData data2 = new zbItemData(award.Type, award.Tid, award.Count);
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
			zbUIMgr.Instance.ShowVip(2);
		}
		else
		{
			zbWsMsgHandler.Instance.Operate_RechargeDaily_Req(m_uuid, m_data.Index);
		}
	}
}
