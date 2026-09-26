using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbWallIdleWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Label m_lbTitle;

	private Label m_lbDesc;

	private VisualElement m_progress;

	private Label m_lbProgress;

	private VisualElement[] m_awards = (VisualElement[])(object)new VisualElement[2];

	private zbItem[] m_items = new zbItem[2];

	private Label[] m_lbCounts = (Label[])(object)new Label[2];

	private VisualElement m_listNo;

	private MultiColumnListView m_listView;

	private List<zbItemData> m_listItemDatas = new List<zbItemData>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 5;

	private int rowCount;

	private Label m_lbSweepCount;

	private Label m_lbTili;

	public Button m_btnSweep;

	public Button m_btnGet;

	private int m_settleLimit;

	private int m_settleInterval;

	private int m_costTili;

	private int m_dailyCount;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_RootElement, "lb_progress", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_awards[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "award" + i, (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_awards[i], "item", (string)null);
			m_items[i].OnEnable(this);
			m_lbCounts[i] = UQueryExtensions.Q<Label>(m_awards[i], "lb_count", (string)null);
		}
		m_listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_lbSweepCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_sweepCount", (string)null);
		m_lbTili = UQueryExtensions.Q<Label>(m_RootElement, "lb_tili", (string)null);
		m_btnSweep = UQueryExtensions.Q<Button>(m_RootElement, "btn_sweep", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweep, (Action<ClickEvent>)OnSweep, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action<ClickEvent>)OnGet, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_IdleInfo, OnEventUpdate);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
	}

	public void RefreshCustom(Action<bool> callback)
	{
		m_callback = callback;
		RefreshRecords();
		RefreshTime();
		RefreshAwardList();
		if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.IdleSweep, zbSelf.Instance.GetVipLevel()) > 0)
		{
			((TextElement)m_lbSweepCount).text = "";
		}
		else
		{
			int num = Mathf.Max(0, m_dailyCount - zbServerData.sGameData.IdleInfo.FastGainCount);
			((TextElement)m_lbSweepCount).text = zbUtil.Language("lang.10159") + ": " + num;
		}
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Idle);
	}

	private void RefreshRecords()
	{
		((TextElement)m_lbTitle).text = zbResourceHelper.GetMainLevelName(zbServerData.sGameData.MainQuestInfo.MainLevel);
		zbIdleAwardCsvRecord recordByKey = zbDataConfig.Instance.idleAwardCsvTable.GetRecordByKey(zbServerData.sGameData.MainQuestInfo.MainLevel);
		if (recordByKey != null)
		{
			m_items[0].Refresh(new zbItemData(EResourceType.Money, 1L, recordByKey.gold), isNameVis: false, isCountVis: false, isTouchVis: false);
			((TextElement)m_lbCounts[0]).text = recordByKey.gold + "/小时";
			m_items[1].Refresh(new zbItemData(EResourceType.Box, 1001001L, recordByKey.randCount), isNameVis: false, isCountVis: false, isTouchVis: false);
			((TextElement)m_lbCounts[1]).text = recordByKey.randCount + "/小时";
		}
		JObject record = zbDataConfig.Instance.idleBaseJsonTable.GetRecord();
		JToken val = ((record != null) ? record["gain"] : null);
		JToken val2 = ((record != null) ? record["fastGain"] : null);
		if (val != null)
		{
			m_settleLimit = Extensions.Value<int>((IEnumerable<JToken>)val[(object)"settleLimit"]);
			m_settleInterval = Extensions.Value<int>((IEnumerable<JToken>)val[(object)"settleInterval"]);
		}
		if (val2 != null)
		{
			m_costTili = Extensions.Value<int>((IEnumerable<JToken>)val2[(object)"costTili"]);
			m_dailyCount = Extensions.Value<int>((IEnumerable<JToken>)val2[(object)"dailyCount"]);
		}
		((TextElement)m_lbTili).text = "x" + m_costTili;
	}

	private void RefreshAwardList()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		m_listView.columns.Clear();
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(120f),
				makeCell = delegate
				{
					zbItem obj = new zbItem();
					obj.OnEnable(this);
					return (VisualElement)(object)obj;
				},
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_005b: Unknown result type (might be due to invalid IL or missing references)
					int num2 = row * columnCount + columnIndex;
					if (num2 < m_listItemDatas.Count)
					{
						(element as zbItem)?.Refresh(m_listItemDatas[num2]);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		m_listItemDatas.Clear();
		if (zbServerData.sGameData.IdleInfo != null && zbServerData.sGameData.IdleInfo.AwardList != null)
		{
			foreach (AwardDetail award in zbServerData.sGameData.IdleInfo.AwardList)
			{
				m_listItemDatas.Add(new zbItemData(award.Type, award.Tid, award.Count));
			}
		}
		m_listNo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_listItemDatas.Count != 0) ? 1 : 0));
		((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_listItemDatas.Count <= 0));
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int num = 0; num < rowCount; num++)
		{
			rowIndexes.Add(num);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		((VisualElement)m_listView).style.position = StyleEnum<Position>.op_Implicit((Position)1);
	}

	private void RefreshTime()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (zbServerData.sGameData.IdleInfo != null)
		{
			long num = zbSelf.Instance.timeHandler.GetServerTime() - zbServerData.sGameData.IdleInfo.GainTm;
			float num2 = Mathf.Clamp((float)num / (float)m_settleLimit, 0f, 1f);
			m_progress.style.width = StyleLength.op_Implicit(new Length(num2 * 608f, (LengthUnit)0));
			if (num2 >= 1f)
			{
				((TextElement)m_lbProgress).text = "Max";
			}
			else
			{
				((TextElement)m_lbProgress).text = zbUtil.FormatTimeHHMMSS(num);
			}
			if (num > m_settleInterval)
			{
				((TextElement)m_btnGet).text = zbUtil.Language("lang.10090");
			}
			else
			{
				((TextElement)m_btnGet).text = zbUtil.FormatTimeHHMMSS(m_settleInterval - num);
			}
		}
	}

	private void OnSweep(ClickEvent evt)
	{
		if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.IdleSweep, zbSelf.Instance.GetVipLevel()) > 0)
		{
			zbUIMgr.Instance.ShowWallIdleBatchSweep();
		}
		else if (zbServerData.sGameData.IdleInfo?.FastGainCount >= m_dailyCount && zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.IdleSweep, zbSelf.Instance.GetVipLevel()) <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.IdleSweep)) + "【" + zbUtil.Language("lang.11493") + "】");
		}
		else if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 4, m_costTili))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, m_costTili);
		}
		else
		{
			zbWsMsgHandler.Instance.Idle_FastGain_Req();
		}
	}

	private void OnGet(ClickEvent evt)
	{
		if (zbSelf.Instance.timeHandler.GetServerTime() - zbServerData.sGameData.IdleInfo.GainTm > m_settleInterval)
		{
			zbWsMsgHandler.Instance.Idle_Gain_Req();
		}
	}

	private void OnClose()
	{
		Hide();
		m_callback?.Invoke(obj: true);
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_callback);
	}
}
