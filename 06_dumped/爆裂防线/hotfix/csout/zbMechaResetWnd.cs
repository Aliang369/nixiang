using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaResetWnd : zbBaseWnd
{
	private zbItem[] m_items = new zbItem[5];

	private Button m_btnReset;

	private Label m_lbDiamond;

	private Button m_btnCancel;

	private Button m_btnClose;

	private Button m_btnTouch;

	private int m_mechaId;

	private int m_resetDiamond;

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_btnReset = UQueryExtensions.Q<Button>(m_RootElement, "btn_reset", (string)null);
		m_lbDiamond = UQueryExtensions.Q<Label>(m_RootElement, "lb_diamond", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		((TextElement)UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null)).text = zbUtil.Language("lang.10957");
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReset, (Action)OnReset, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MechaInfo, delegate
		{
			OnClose();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, delegate
		{
			RefreshDiamondState();
		});
	}

	public void RefreshCustom(int mechaId, AwardDetail[] awardList)
	{
		m_mechaId = mechaId;
		m_resetDiamond = zbSelf.Instance.mechaHandler.resetDiamond;
		((TextElement)m_lbDiamond).text = "x" + m_resetDiamond;
		RefreshDiamondState();
		RefreshAwards(awardList);
	}

	private void RefreshAwards(AwardDetail[] awardlist)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
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

	private void RefreshDiamondState()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.moneyHandler.HasEnoughMoney(2, m_resetDiamond);
		((VisualElement)m_lbDiamond).style.color = StyleColor.op_Implicit(flag ? Color.white : Color.red);
	}

	private void OnReset()
	{
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(2, m_resetDiamond))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 2L, m_resetDiamond);
		}
		else
		{
			zbWsMsgHandler.Instance.Mecha_Reset_Req(m_mechaId);
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
