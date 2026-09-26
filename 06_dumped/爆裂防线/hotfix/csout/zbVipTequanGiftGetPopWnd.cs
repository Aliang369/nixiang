using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine.UIElements;

public class zbVipTequanGiftGetPopWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnTouch;

	private Button m_btnSure;

	private VisualElement m_getPnl;

	private VisualElement m_notGetPnl;

	private Label m_lblGet;

	private zbItem[] m_getAwards = new zbItem[5];

	private zbItem[] m_notgetAwards = new zbItem[5];

	private bool bhasAwardCanget;

	private int m_curTabVip;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btnclose", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnSure = UQueryExtensions.Q<Button>(m_RootElement, "btnsure", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)Hide, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)Hide, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSure, (Action)OnGet, "", (TrickleDown)0);
		m_getPnl = UQueryExtensions.Q<VisualElement>(m_RootElement, "getpnl", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_getAwards[i] = UQueryExtensions.Q<zbItem>(m_getPnl, $"item{i}", (string)null);
			m_getAwards[i].OnEnable(this);
		}
		m_lblGet = UQueryExtensions.Q<Label>(m_RootElement, "lblget", (string)null);
		m_notGetPnl = UQueryExtensions.Q<VisualElement>(m_RootElement, "notgetpnl", (string)null);
		for (int j = 0; j < 5; j++)
		{
			m_notgetAwards[j] = UQueryExtensions.Q<zbItem>(m_notGetPnl, $"item{j}", (string)null);
			m_notgetAwards[j].OnEnable(this);
		}
		AddDark();
	}

	public void RefreshCustom(int curTabLv)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		m_curTabVip = curTabLv;
		bhasAwardCanget = false;
		zbItem[] getAwards = m_getAwards;
		for (int i = 0; i < getAwards.Length; i++)
		{
			((VisualElement)getAwards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		getAwards = m_notgetAwards;
		for (int i = 0; i < getAwards.Length; i++)
		{
			((VisualElement)getAwards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		VipInfo vipInfo = zbServerData.sGameData.VipInfo;
		if (vipInfo == null)
		{
			return;
		}
		m_getPnl.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_notGetPnl.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (vipInfo.DailyFree == 0)
		{
			m_getPnl.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lblGet).text = "今日可领取奖励";
			List<zbItemData> dailyFreeGiftAward = zbSelf.Instance.vipHandler.GetDailyFreeGiftAward(0, zbSelf.Instance.GetVipLevel());
			int num = 0;
			foreach (zbItemData item in dailyFreeGiftAward)
			{
				if (num >= m_getAwards.Length)
				{
					break;
				}
				zbItem zbItem2 = m_getAwards[num];
				if (zbItem2 != null)
				{
					((VisualElement)zbItem2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					zbItem2.Refresh(item);
				}
				num++;
			}
			bhasAwardCanget = true;
			return;
		}
		m_getPnl.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((TextElement)m_lblGet).text = "今日已领取奖励";
		List<zbItemData> dailyFreeGiftAward2 = zbSelf.Instance.vipHandler.GetDailyFreeGiftAward(0, vipInfo.DailyFree);
		int num2 = 0;
		foreach (zbItemData item2 in dailyFreeGiftAward2)
		{
			if (num2 >= m_getAwards.Length)
			{
				break;
			}
			zbItem zbItem3 = m_getAwards[num2];
			if (zbItem3 != null)
			{
				((VisualElement)zbItem3).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbItem3.Refresh(item2);
				zbItem3.SetGettedVisible(vis: true);
			}
			num2++;
		}
		if (zbSelf.Instance.GetVipLevel() <= vipInfo.DailyFree)
		{
			return;
		}
		m_notGetPnl.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		List<zbItemData> dailyFreeGiftAward3 = zbSelf.Instance.vipHandler.GetDailyFreeGiftAward(vipInfo.DailyFree + 1, zbSelf.Instance.GetVipLevel());
		num2 = 0;
		foreach (zbItemData item3 in dailyFreeGiftAward3)
		{
			if (num2 >= m_notgetAwards.Length)
			{
				break;
			}
			zbItem zbItem4 = m_notgetAwards[num2];
			if (zbItem4 != null)
			{
				((VisualElement)zbItem4).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbItem4.Refresh(item3);
			}
			num2++;
		}
		bhasAwardCanget = true;
	}

	private void OnGet()
	{
		if (bhasAwardCanget)
		{
			zbWsMsgHandler.Instance.Vip_DailyFree_Req(m_curTabVip);
		}
		Hide();
	}
}
