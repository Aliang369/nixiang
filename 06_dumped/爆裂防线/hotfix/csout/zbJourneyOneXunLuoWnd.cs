using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyOneXunLuoWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbTime;

	private Label m_lbDay;

	private VisualElement m_container;

	private List<zbJourneyOneXunLuoItem> m_dayItems = new List<zbJourneyOneXunLuoItem>();

	private StyleBackground m_notGetAwardBg;

	private StyleBackground m_getAwardBg;

	private const int kTotalDays = 7;

	protected override void OnEnable()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_notGetAwardBg = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/journey/journey1/xunluo/journeyXunLuo_0.png"));
		m_getAwardBg = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/journey/journey1/xunluo/journeyXunLuo_1.png"));
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.10810");
		m_container = UQueryExtensions.Q<VisualElement>(m_RootElement, "itemContainer", (string)null);
		if (m_container != null)
		{
			m_container.Clear();
			m_dayItems.Clear();
			for (int i = 0; i < 7; i++)
			{
				zbJourneyOneXunLuoItem zbJourneyOneXunLuoItem2 = new zbJourneyOneXunLuoItem();
				zbJourneyOneXunLuoItem2.OnEnable(this);
				zbJourneyOneXunLuoItem2.SetBackgrounds(m_notGetAwardBg, m_getAwardBg);
				zbJourneyOneXunLuoItem2.Init(i);
				m_dayItems.Add(zbJourneyOneXunLuoItem2);
				m_container.Add((VisualElement)(object)zbJourneyOneXunLuoItem2);
			}
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, delegate
		{
			Refresh();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, delegate
		{
			Refresh();
		});
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
	}

	public override void Refresh()
	{
		Dictionary<int, zbChapterOneSignCsvRecord> records = zbDataConfig.Instance.chapterOneSignCsvTable.GetRecords().records;
		if (records == null || m_dayItems.Count == 0)
		{
			return;
		}
		int valueOrDefault = (zbServerData.sGameData?.JourneyInfo?.ChapterOne?.Sign?.Reach).GetValueOrDefault();
		for (int i = 0; i < m_dayItems.Count; i++)
		{
			int num = i + 1;
			bool flag = records.TryGetValue(num, out var value);
			m_dayItems[i].SetVisible(flag);
			if (flag)
			{
				m_dayItems[i].RefreshAll(num, valueOrDefault, value.awardList);
			}
		}
		UpdateNotify();
		UpdateTime();
	}

	private void UpdateTime()
	{
		long valueOrDefault = (zbServerData.sGameData?.JourneyInfo?.ChapterOne?.BaseInfo?.EndTm).GetValueOrDefault();
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		((TextElement)m_lbTime).text = ((valueOrDefault > serverTime) ? zbUtil.FormatTimeHHMMSS(valueOrDefault - serverTime) : zbUtil.Language("lang.10345"));
	}

	private void UpdateNotify()
	{
		Dictionary<int, zbChapterOneSignCsvRecord> records = zbDataConfig.Instance.chapterOneSignCsvTable.GetRecords().records;
		int valueOrDefault = (zbServerData.sGameData?.JourneyInfo?.ChapterOne?.Sign?.Reach).GetValueOrDefault();
		for (int i = 0; i < m_dayItems.Count; i++)
		{
			int num = i + 1;
			bool flag = records?.ContainsKey(num) ?? false;
			m_dayItems[i].SetVisible(flag);
			if (flag)
			{
				m_dayItems[i].UpdateStateTick(num, valueOrDefault);
			}
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		UpdateTime();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
