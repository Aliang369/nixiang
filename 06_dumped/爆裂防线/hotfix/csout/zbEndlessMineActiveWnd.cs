using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEndlessMineActiveWnd : zbBaseWnd
{
	private zbItem[] m_items = new zbItem[3];

	private Label m_lbTitle;

	private Button m_btnGet;

	private Button m_btnTouch;

	private VisualElement m_notify;

	protected override void OnEnable()
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
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
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)Hide, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGet, (Vector2?)new Vector2(-3f, -6f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_EndlessMineActivateAward_Ack, OnAck);
		AddDark();
		InitRecord();
	}

	private void InitRecord()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		int[][] array = zbDataConfig.Instance.endlessMineBaseJsonTable.GetRecord()["activateAward"].ToObject<int[][]>();
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
	}

	private void OnGet()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbWsMsgHandler.Instance.EndlessMine_ActivateAward_Req();
	}

	private void OnAck(object data)
	{
		if (data is EndlessMineActivateAwardAck endlessMineActivateAwardAck)
		{
			zbSelf.Instance.dataHandler.DataChange(endlessMineActivateAwardAck.DataChange);
			zbUIMgr.Instance.ShowAward(((IEnumerable<AwardDetail>)endlessMineActivateAwardAck.AwardList).ToArray(), OnCallBack);
		}
	}

	private void OnCallBack(bool isOK)
	{
		zbUIMgr.Instance.ShowEndlessMine();
		Hide();
	}
}
