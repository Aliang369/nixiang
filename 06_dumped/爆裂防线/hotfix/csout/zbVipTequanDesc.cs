using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbVipTequanDesc
{
	private zbVipTequanWnd m_parent;

	private VisualElement m_root;

	private Label m_tq_title;

	private ScrollView m_tqSv;

	private Button m_btnLeft;

	private Button m_btnRight;

	private VisualElement m_btnLeftTag;

	private VisualElement m_btnRightTag;

	private int curVipLv;

	private List<zbVipTequanDescItem> m_items = new List<zbVipTequanDescItem>();

	public zbVipTequanDesc(zbVipTequanWnd parent, VisualElement root)
	{
		m_parent = parent;
		m_root = root;
	}

	public void OnEnable()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		m_tq_title = UQueryExtensions.Q<Label>(m_root, "tq_title", (string)null);
		m_tqSv = UQueryExtensions.Q<ScrollView>(m_root, "tq_sc", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_root, "btn_leftar", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_root, "btn_rightar", (string)null);
		zbUIEffect.CreateBreathingLightEffect((VisualElement)(object)m_btnLeft, 0.5f, 1f, 2f);
		zbUIEffect.CreateBreathingLightEffect((VisualElement)(object)m_btnRight, 0.5f, 1f, 2f);
		m_btnLeftTag = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLeft, (Vector2?)new Vector2(0f, 20f), 1f, true);
		m_btnRightTag = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRight, (Vector2?)new Vector2(0f, 20f), 1f, true);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action<ClickEvent>)OnBtnLeft, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action<ClickEvent>)OnBtnRight, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		curVipLv = ((zbServerData.sGameData.RoleBaseInfo.VipLevel <= 0) ? 1 : zbSelf.Instance.GetVipLevel());
		RefreshList();
	}

	private void RefreshList()
	{
		((TextElement)m_tq_title).text = $"VIP{curVipLv}特权";
		((VisualElement)m_tqSv).Clear();
		m_items.Clear();
		List<zbViptdescCsvRecord> vipTqDescByLv = zbSelf.Instance.vipHandler.GetVipTqDescByLv(curVipLv);
		if (vipTqDescByLv == null)
		{
			return;
		}
		int num = 1;
		foreach (zbViptdescCsvRecord item in vipTqDescByLv)
		{
			zbVipTequanDescItem zbVipTequanDescItem2 = new zbVipTequanDescItem();
			zbVipTequanDescItem2.Init(item, num);
			num++;
			m_items.Add(zbVipTequanDescItem2);
			((VisualElement)m_tqSv).Add((VisualElement)(object)zbVipTequanDescItem2);
		}
		RefreshArrowsNotify();
		m_parent.OnTabVipChange(curVipLv);
	}

	private void RefreshArrowsNotify()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.vipHandler.hasDailyFreeGift(curVipLv))
		{
			m_btnLeftTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_btnRightTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		bool flag = false;
		for (int i = 1; i < curVipLv; i++)
		{
			if (zbSelf.Instance.vipHandler.hasDailyFreeGift(i))
			{
				flag = true;
				break;
			}
		}
		m_btnLeftTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		bool flag2 = false;
		int maxVipLv = zbSelf.Instance.vipHandler.maxVipLv;
		for (int j = curVipLv + 1; j <= maxVipLv; j++)
		{
			if (zbSelf.Instance.vipHandler.hasDailyFreeGift(j))
			{
				flag2 = true;
				break;
			}
		}
		m_btnRightTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
	}

	private void OnBtnLeft(ClickEvent evt)
	{
		if (curVipLv <= 1)
		{
			curVipLv = zbSelf.Instance.vipHandler.maxVipLv;
		}
		else
		{
			curVipLv--;
		}
		RefreshList();
	}

	private void OnBtnRight(ClickEvent evt)
	{
		if (curVipLv >= zbSelf.Instance.vipHandler.maxVipLv)
		{
			curVipLv = 1;
		}
		else
		{
			curVipLv++;
		}
		RefreshList();
	}
}
