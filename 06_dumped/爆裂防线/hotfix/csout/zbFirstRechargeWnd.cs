using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFirstRechargeWnd : zbBaseWnd
{
	private Button m_btnCall;

	private Button m_btnTouch;

	private Button m_btnRecharge;

	private TextElement m_lbRecharge;

	private VisualElement m_notifyRecharge;

	private zbFirstRechargeTab m_firstRechargeTab;

	private long m_leftTime = -1L;

	private Action<bool> m_callback;

	private Label m_lbBonus;

	protected override void OnEnable()
	{
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnRecharge = UQueryExtensions.Q<Button>(m_RootElement, "btn_recharge", (string)null);
		m_lbRecharge = (TextElement)(object)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRecharge, "lb_recharge", (string)null);
		m_btnCall = UQueryExtensions.Q<Button>(m_RootElement, "btn_call", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbBonus = UQueryExtensions.Q<Label>(m_RootElement, "lb_bonus", (string)null);
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel0", (string)null);
		m_firstRechargeTab = new zbFirstRechargeTab(root, this);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRecharge, (Action<ClickEvent>)OnRecharge, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCall, (Action)OnCall, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnEventUpdate);
		m_notifyRecharge = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRecharge, (Vector2?)new Vector2(0f, 0f), 1f, true);
		AddDark();
	}

	public void RefreshCustom(Action<bool> callback, bool open = true)
	{
		m_callback = callback;
		zbSelf.Instance.activityHandler.m_firstRechargeIn = true;
		string text = zbDataConfig.Instance.vipBaseJsonTable.GetRecord()["novicePack"].Value<string>((object)"bonus");
		((TextElement)m_lbBonus).text = text;
		RefreshAll();
		if (open)
		{
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		}
	}

	private void RefreshAll()
	{
		m_firstRechargeTab.Refresh();
		RefreshDetail();
	}

	private void UpdateRechargeUi()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		m_notifyRecharge.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbSelf.Instance.operateHandler.IsNovicePackCompleted())
		{
			((VisualElement)m_btnRecharge).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else if (!zbSelf.Instance.operateHandler.IsNovicePackPurchased())
		{
			int novicePackGoodsId = zbSelf.Instance.operateHandler.GetNovicePackGoodsId();
			m_lbRecharge.text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(novicePackGoodsId);
			((VisualElement)m_btnRecharge).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else if (zbSelf.Instance.operateHandler.IsNovicePackClaimableToday())
		{
			m_lbRecharge.text = zbUtil.Language("lang.10090");
			m_notifyRecharge.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnRecharge).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			m_lbRecharge.text = zbUtil.Language("lang.10148");
			m_notifyRecharge.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnRecharge).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}

	private void RefreshDetail()
	{
		m_leftTime = -1L;
		if (zbSelf.Instance.operateHandler.IsNovicePackPurchased() && zbSelf.Instance.operateHandler.GetNovicePackTodayState() == EActiveState.Done)
		{
			m_leftTime = zbSelf.Instance.timeHandler.GetRefreshLeftTime();
		}
		UpdateRechargeUi();
		UpdateTime();
	}

	private void UpdateTime()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (m_leftTime > 0)
		{
			m_leftTime--;
			m_lbRecharge.text = zbUtil.FormatTimeHHMMSS(m_leftTime);
			m_notifyRecharge.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else if (m_leftTime == 0L)
		{
			RefreshAll();
		}
	}

	private void OnRecharge(ClickEvent evt)
	{
		switch (zbSelf.Instance.operateHandler.GetNovicePackTodayState())
		{
		case EActiveState.Active:
			zbWsMsgHandler.Instance.RechargeActive_NovicePack_Req();
			break;
		case EActiveState.Done:
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10679"));
			break;
		case EActiveState.Pending:
			zbChannel_Pay.OnPay(zbSelf.Instance.operateHandler.GetNovicePackGoodsId());
			break;
		}
	}

	private void OnCall()
	{
		zbUIMgr.Instance.ShowSupportContact();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
		m_callback?.Invoke(obj: true);
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		UpdateTime();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_callback, open: false);
	}
}
