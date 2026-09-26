using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRestaurantActiveWnd : zbBaseWnd
{
	private zbItem m_item;

	private Label m_lbTitle;

	private Button m_btnGet;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private RestaurantDetail m_data;

	protected override void OnEnable()
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGet, (Vector2?)new Vector2(-3f, -6f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_RestaurantActivateAward_Ack, OnAck);
		AddDark();
		InitRecord();
	}

	private void InitRecord()
	{
		int[] array = zbDataConfig.Instance.restaurantBaseJsonTable.GetRecord()["activateAward"].ToObject<int[][]>()[0];
		m_item.Refresh(new zbItemData((EResourceType)array[0], array[1], array[2]));
	}

	public override void Refresh()
	{
		m_data = zbSelf.Instance.restaurantHandler.GetRestaurantDetail();
	}

	private void OnGet()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_data != null)
		{
			zbWsMsgHandler.Instance.Operate_RestaurantActivateAward_Req(m_data.BaseInfo.UUID);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnAck(object data)
	{
		if (data is OperateRestaurantActivateAwardAck operateRestaurantActivateAwardAck)
		{
			zbSelf.Instance.dataHandler.DataChange(operateRestaurantActivateAwardAck.DataChange);
			zbUIMgr.Instance.ShowAward(((IEnumerable<AwardDetail>)operateRestaurantActivateAwardAck.AwardList).ToArray(), OnCallBack);
		}
	}

	private void OnCallBack(bool isOK)
	{
		Refresh();
		zbUIMgr.Instance.ShowRestaurant(m_data);
		Hide();
	}
}
