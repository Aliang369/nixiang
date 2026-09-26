using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonAirdropClashWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbTime;

	private Button[] m_btnAirdrops = (Button[])(object)new Button[3];

	private Label[] m_lbNames = (Label[])(object)new Label[3];

	private Label[] m_lbLeftCounts = (Label[])(object)new Label[3];

	private Label m_lbAward;

	private ScrollView m_scrollView;

	private Label m_lbDailyCount;

	private Button m_btnAirdropList;

	private Button m_btnClose;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_btnAirdrops[i] = UQueryExtensions.Q<Button>(m_RootElement, $"btn_airdrop{i}", (string)null);
			m_lbNames[i] = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnAirdrops[i], "lb_name", (string)null);
			m_lbLeftCounts[i] = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnAirdrops[i], "lb_leftCount", (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAirdrops[i], (Action<ClickEvent>)delegate
			{
				OnAirdrops(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_lbAward = UQueryExtensions.Q<Label>(m_RootElement, "lb_award", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		m_lbDailyCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_dailyCount", (string)null);
		m_btnAirdropList = UQueryExtensions.Q<Button>(m_RootElement, "btn_airdropList", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAirdropList, (Action<ClickEvent>)delegate
		{
			OnAirdrops(0);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_SeasonAirdrop, OnEventUpdate);
		InitRecord();
	}

	private void InitRecord()
	{
		for (int i = 0; i < 3; i++)
		{
			zbSeasonAirdropCsvRecord recordByKey = zbDataConfig.Instance.seasonAirdropCsvTable.GetRecordByKey(i + 1);
			if (recordByKey != null)
			{
				((TextElement)m_lbNames[i]).text = recordByKey.name;
				int[][] awardList = recordByKey.awardList;
				foreach (int[] awardData in awardList)
				{
					zbItem zbItem2 = CreateAward(awardData);
					((VisualElement)m_scrollView).Add((VisualElement)(object)zbItem2);
				}
			}
		}
		((VisualElement)m_scrollView).schedule.Execute((Action)delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			m_scrollView.scrollOffset = Vector2.zero;
		}).ExecuteLater(0L);
	}

	private zbItem CreateAward(int[] awardData)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		zbItem obj = new zbItem();
		zbItemData data = new zbItemData((EResourceType)awardData[0], awardData[1], awardData[2]);
		((VisualElement)obj).style.flexGrow = StyleFloat.op_Implicit(1f);
		((VisualElement)obj).style.flexShrink = StyleFloat.op_Implicit(0f);
		((VisualElement)obj).style.alignItems = StyleEnum<Align>.op_Implicit((Align)2);
		((VisualElement)obj).style.alignSelf = StyleEnum<Align>.op_Implicit((Align)2);
		((VisualElement)obj).style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)0);
		((VisualElement)obj).style.justifyContent = StyleEnum<Justify>.op_Implicit((Justify)0);
		((VisualElement)obj).style.scale = new StyleScale(new Vector2(0.8f, 0.8f));
		((VisualElement)obj).style.width = StyleLength.op_Implicit(110f);
		obj.OnEnable(this);
		obj.Refresh(data);
		return obj;
	}

	public override void Refresh()
	{
		for (int i = 0; i < 3; i++)
		{
			((TextElement)m_lbLeftCounts[i]).text = zbUtil.Language("lang.11467") + ": " + zbSelf.Instance.seasonHandler.GetAirdropLeftCount((ESeasonAirdropQuality)(i + 1));
		}
		if (zbSelf.Instance.seasonHandler.seasonAirdropListAck != null)
		{
			((TextElement)m_lbDailyCount).text = zbUtil.Language("lang.11468") + ": " + (zbSelf.Instance.seasonHandler.seasonAirdropListAck.DailyLimit - zbSelf.Instance.seasonHandler.seasonAirdropListAck.DailyWin);
		}
		RefreshTime();
	}

	private void RefreshTime()
	{
		if (zbSelf.Instance.seasonHandler.seasonAirdropListAck != null)
		{
			long num = zbSelf.Instance.seasonHandler.seasonAirdropListAck.NextRefreshTm - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(num);
			}
			else
			{
				zbWsMsgHandler.Instance.Season_Airdrop_List_Req();
			}
		}
		else
		{
			((TextElement)m_lbTime).text = "";
		}
	}

	private void OnAirdrops(int index)
	{
		zbUIMgr.Instance.ShowSeasonAirdropList((ESeasonAirdropQuality)(index + 1));
	}

	public void OnClose()
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
