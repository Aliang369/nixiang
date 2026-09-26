using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbCrossLadderSeasonInitWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private zbLadderItem m_itemLadder;

	private Label m_lbTitle;

	private Label m_lbDesc;

	private zbItem[] m_items = new zbItem[3];

	private Button m_btnGet;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_itemLadder = UQueryExtensions.Q<zbLadderItem>(m_RootElement, "ladder", (string)null);
		m_itemLadder.OnEnable(this);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_CrossLadder_SeasonInit_Ack, OnAck);
	}

	public override void Refresh()
	{
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(zbServerData.sGameData.CrossLadderInfo.OpenInfo.StartTm);
		if (dateTimeOffset.Year == 2026 && dateTimeOffset.Month == 1)
		{
			m_itemLadder.Refresh(1);
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.10892");
			((TextElement)m_lbDesc).text = "";
		}
		else
		{
			m_itemLadder.Refresh(zbServerData.sGameData.CrossLadderInfo.SelfInfo.SeasonAward.LadderId);
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.10893");
			zbLadderCsvRecord recordByKey = zbDataConfig.Instance.ladderCsvTable.GetRecordByKey(zbServerData.sGameData.CrossLadderInfo.SelfInfo.SeasonAward.LadderId);
			((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.10888"), recordByKey.name);
		}
		for (int i = 0; i < 3; i++)
		{
			AwardDetail awardDetail = zbServerData.sGameData.CrossLadderInfo.SelfInfo.SeasonAward.AwardList[i];
			if (awardDetail != null)
			{
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[i].Refresh(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count));
			}
			else
			{
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	private void OnGet()
	{
		zbWsMsgHandler.Instance.CrossLadder_SeasonInit_Req();
	}

	private void OnTouchTest(ClickEvent cvt)
	{
		Hide();
	}

	private void OnAck(object data)
	{
		if (data is CrossLadderSeasonInitAck crossLadderSeasonInitAck)
		{
			zbSelf.Instance.dataHandler.DataChange(crossLadderSeasonInitAck.DataChange);
			zbUIMgr.Instance.ShowAward(((IEnumerable<AwardDetail>)crossLadderSeasonInitAck.AwardList).ToArray(), OnCallBack);
		}
	}

	private void OnCallBack(bool isOK)
	{
		zbUIMgr.Instance.ShowCrossLadder();
		Hide();
	}
}
