using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSnowVeilActiveWnd : zbBaseWnd
{
	private zbItem[] m_items = new zbItem[3];

	private Label m_lbTitle;

	private Button m_btnGet;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private SnowVeilDetail m_data;

	protected override void OnEnable()
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGet, (Vector2?)new Vector2(-3f, -6f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SnowVeilActivate_Ack, OnAck);
		AddDark();
		InitRecord();
	}

	private void InitRecord()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		int[][] array = zbDataConfig.Instance.snowVeilBaseJsonTable.GetRecord()["activateAward"].ToObject<int[][]>();
		for (int i = 0; i < 3; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		int[][] array2 = array;
		foreach (int[] array3 in array2)
		{
			if (num < m_items.Length)
			{
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(new zbItemData((EResourceType)array3[0], array3[1], array3[2]));
			}
			num++;
		}
	}

	public override void Refresh()
	{
		m_data = zbSelf.Instance.snowVeilHandler.GetSnowVeilDetail();
	}

	private void OnGet()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_data != null)
		{
			zbWsMsgHandler.Instance.Operate_SnowVeilActivate_Req(m_data.BaseInfo.UUID);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnAck(object data)
	{
		if (data is OperateSnowVeilActivateAck operateSnowVeilActivateAck)
		{
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilActivateAck.DataChange);
			zbUIMgr.Instance.ShowAward(((IEnumerable<AwardDetail>)operateSnowVeilActivateAck.AwardList).ToArray(), OnCallBack);
		}
	}

	private void OnCallBack(bool isOK)
	{
		Refresh();
		zbUIMgr.Instance.ShowSnowVeil(m_data);
		Hide();
	}
}
