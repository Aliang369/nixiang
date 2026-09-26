using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbWorldEventBoxWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Label m_lbName;

	private Label m_lbDesc;

	private Label m_lbAwardTitle;

	private VisualElement m_awardList;

	private Button m_btnTouch;

	private Button m_btnConfirm;

	private WorldEventDetail m_eventDetail;

	private zbItem[] m_awards = new zbItem[2];

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_awardList = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardList", (string)null);
		m_lbAwardTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_awardTitle", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>(m_awardList, $"award{i}", (string)null);
			m_awards[i].OnEnable(this);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_WorldEventAwardBox_Ack, OnWorldEventAwardBoxAck);
	}

	public void RefreshCustom(WorldEventDetail eventDetail)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		m_eventDetail = eventDetail;
		zbWorldEventBoxCsvRecord recordByKey = zbDataConfig.Instance.worldEventBoxCsvTable.GetRecordByKey(m_eventDetail.EventId);
		((TextElement)m_lbName).text = zbUtil.Language(recordByKey.name);
		((TextElement)m_lbDesc).text = zbUtil.Language(recordByKey.desc);
		for (int i = 0; i < 2; i++)
		{
			bool flag = i < recordByKey.awardList.Length;
			((VisualElement)m_awards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData((EResourceType)recordByKey.awardList[i][0], recordByKey.awardList[i][1], recordByKey.awardList[i][2]);
				m_awards[i].Refresh(data);
			}
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnConfirm()
	{
		zbWsMsgHandler.Instance.WorldEvent_AwardBox_Req(m_eventDetail.GridId);
	}

	private void OnWorldEventAwardBoxAck(object data)
	{
		OnClose();
	}
}
