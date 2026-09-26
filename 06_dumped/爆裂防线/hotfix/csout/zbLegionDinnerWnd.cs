using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionDinnerWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbTime;

	private zbItem m_item;

	private Button m_btnFree;

	private Button m_btnDouble;

	private Label m_lbDiamond;

	private int m_diamond;

	private LegionSelfDinner m_data;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_btnFree = UQueryExtensions.Q<Button>(m_RootElement, "btn_free", (string)null);
		m_btnDouble = UQueryExtensions.Q<Button>(m_RootElement, "btn_double", (string)null);
		m_lbDiamond = UQueryExtensions.Q<Label>(m_RootElement, "lb_diamond", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFree, (Action)OnFree, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDouble, (Action)OnDouble, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, delegate
		{
			Refresh();
		});
		AddDark();
		InitRecord();
	}

	private void InitRecord()
	{
		m_item.Refresh(new zbItemData(EResourceType.Money, 4L, 20L));
		m_diamond = Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.legionHandler.GetLegionBase()[(object)"dinner"][(object)"diamondCost"]);
		((TextElement)m_lbDiamond).text = m_diamond.ToString();
	}

	public override void Refresh()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		foreach (LegionSelfDinner dinner in zbServerData.sGameData.LegionSelfInfo.DinnerList)
		{
			if (zbSelf.Instance.timeHandler.GetServerTime() > dinner.StartTm && zbSelf.Instance.timeHandler.GetServerTime() < dinner.EndTm)
			{
				m_data = dinner;
				break;
			}
		}
		if (m_data != null)
		{
			((VisualElement)m_btnFree).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data.State != EActiveState.Pending) ? 1 : 0));
			((VisualElement)m_btnDouble).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data.State != EActiveState.Pending) ? 1 : 0));
		}
	}

	private void OnFree()
	{
		if (m_data != null)
		{
			zbWsMsgHandler.Instance.LegionAux_Dinner_Req(m_data.Index - 1, EMoneyType.EmptyValue);
		}
	}

	private void OnDouble()
	{
		if (m_data != null)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 2, m_diamond))
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 31L, m_diamond);
			}
			else
			{
				zbWsMsgHandler.Instance.LegionAux_Dinner_Req(m_data.Index - 1, EMoneyType.Diamond);
			}
		}
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdateAtShow();
		RefreshTime();
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			long num = m_data.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(num);
			}
			else
			{
				Refresh();
			}
		}
		else
		{
			((TextElement)m_lbTime).text = zbSelf.Instance.timeHandler.GetRefreshTimeStr();
		}
	}
}
