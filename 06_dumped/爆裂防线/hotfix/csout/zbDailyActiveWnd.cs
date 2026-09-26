using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDailyActiveWnd : zbBaseWnd
{
	private SpineVisualElement m_spine;

	private Button m_btnTouch;

	private Label m_lbTime;

	private Label m_lbName;

	private Label m_lbCurActive;

	private Button m_btnOnGet;

	private readonly zbItem[] m_items = new zbItem[3];

	private VisualElement m_notify;

	private DailyActiveDetail m_data;

	private zbOperateCsvRecord m_record;

	private Sprite m_cachedBtnSprite;

	private Sprite m_cachedGrayBtnSprite;

	protected override void OnEnable()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbCurActive = UQueryExtensions.Q<Label>(m_RootElement, "lb_curActive", (string)null);
		m_btnOnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_onGet", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOnGet, (Vector2?)new Vector2(5f, 5f), 1f, true);
		for (int i = 0; i < m_items.Length; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, $"item{i}", (string)null);
			m_items[i]?.OnEnable(this);
		}
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
		m_cachedBtnSprite = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_300_120_yellow.png");
		m_cachedGrayBtnSprite = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_300x120.png");
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOnGet, (Action<ClickEvent>)OnGet, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DailyTask_Data, OnEventUpdateOperateData);
		AddDark();
	}

	public void RefreshCustom(string uuid, zbOperateCsvRecord record)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!(uuid == ""))
		{
			m_data = zbSelf.Instance.operateHandler.GetActiveInfoByUUID(uuid);
			m_record = record;
			if (m_data != null)
			{
				m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.State != ECommonState.Yes));
				RefreshView();
				RefreshTime();
				m_spine.AnimationState.ClearTrack(0);
				m_spine.AnimationState.SetAnimation(0, "in", false);
				m_spine.AnimationState.AddAnimation(0, "loop", true, 0f);
			}
		}
	}

	private void RefreshView()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = m_record.name;
		int valueOrDefault = (zbServerData.sGameData.DailyTaskInfo?.TaskTarget?.Active).GetValueOrDefault();
		((TextElement)m_lbCurActive).text = string.Format("{0}: {1}", zbUtil.Language("lang.10154"), valueOrDefault);
		if (m_data.State == ECommonState.No)
		{
			((TextElement)m_btnOnGet).text = zbUtil.Language("lang.10155");
			((VisualElement)m_btnOnGet).style.backgroundImage = new StyleBackground(m_cachedBtnSprite);
		}
		else if (m_data.State == ECommonState.End)
		{
			((TextElement)m_btnOnGet).text = zbUtil.Language("lang.10148");
			((VisualElement)m_btnOnGet).style.backgroundImage = new StyleBackground(m_cachedGrayBtnSprite);
		}
		else
		{
			((TextElement)m_btnOnGet).text = zbUtil.Language("lang.10090");
			((VisualElement)m_btnOnGet).style.backgroundImage = new StyleBackground(m_cachedBtnSprite);
		}
		for (int i = 0; i < m_items.Length; i++)
		{
			((VisualElement)m_items[i]).visible = false;
		}
		if (m_data.AwardList != null && m_data.AwardList.Count != 0)
		{
			for (int j = 0; j < m_items.Length && j < m_data.AwardList.Count; j++)
			{
				AwardDetail awardDetail = m_data.AwardList[j];
				zbItemData data = new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count);
				m_items[j].Refresh(data);
				((VisualElement)m_items[j]).visible = true;
			}
		}
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_data.BaseInfo.UUID, m_record);
	}

	private void OnEventUpdateOperateData(object data)
	{
		zbWsMsgHandler.Instance.Operate_Data_Req();
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnGet(ClickEvent evt)
	{
		if (!zbSelf.Instance.operateHandler.IsShowTime(m_data.BaseInfo))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10156"));
			return;
		}
		switch (m_data.State)
		{
		case ECommonState.No:
			zbLinkHelper.OpenTask();
			break;
		case ECommonState.Yes:
			zbWsMsgHandler.Instance.Operate_DailyActive_Req(m_data.BaseInfo.UUID);
			break;
		case ECommonState.End:
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10157"));
			break;
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
