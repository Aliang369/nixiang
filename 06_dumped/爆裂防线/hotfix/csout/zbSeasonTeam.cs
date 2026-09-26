using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonTeam
{
	private zbSeasonWnd m_wndParent;

	private VisualElement m_root;

	private VisualElement m_team;

	private Button m_btnPosition;

	private VisualElement m_progressBg;

	private VisualElement m_progress;

	private Label m_lbLeftTime;

	private VisualElement m_iconStatus;

	private Label m_lbStatus;

	private Button m_btnSpeed;

	private SeasonTeamPosInfo m_teamInfo;

	private Sprite m_iconIdle;

	private Sprite m_iconMarch;

	private Sprite m_iconRepair;

	private Sprite m_iconBattle;

	public void OnEnable(zbSeasonWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_root = root;
		m_team = UQueryExtensions.Q<VisualElement>(m_root, "team", (string)null);
		m_btnPosition = UQueryExtensions.Q<Button>(m_team, "btn_position", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPosition, (Action)OnPosition, "", (TrickleDown)0);
		m_progressBg = UQueryExtensions.Q<VisualElement>(m_team, "progress_bg", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_progressBg, "progress", (string)null);
		m_lbLeftTime = UQueryExtensions.Q<Label>(m_team, "lb_leftTime", (string)null);
		m_iconStatus = UQueryExtensions.Q<VisualElement>(m_team, "icon_status", (string)null);
		m_lbStatus = UQueryExtensions.Q<Label>(m_team, "lb_status", (string)null);
		m_btnSpeed = UQueryExtensions.Q<Button>(m_team, "btn_speed", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSpeed, (Action)OnSpeed, "", (TrickleDown)0);
		m_iconIdle = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/icon_idle.png");
		m_iconMarch = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/icon_march.png");
		m_iconRepair = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/icon_repair.png");
		m_iconBattle = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/icon_battle.png");
	}

	public void Refresh()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		m_teamInfo = zbSelf.Instance.seasonHandler.teamPosList[0];
		if (m_teamInfo == null)
		{
			return;
		}
		switch (m_teamInfo.State)
		{
		case 0:
			m_iconStatus.style.backgroundImage = new StyleBackground(m_iconIdle);
			((TextElement)m_lbStatus).text = zbUtil.Language("lang.11481");
			break;
		case 1:
			m_iconStatus.style.backgroundImage = new StyleBackground(m_iconMarch);
			((TextElement)m_lbStatus).text = zbUtil.Language("lang.11482");
			break;
		case 3:
			m_iconStatus.style.backgroundImage = new StyleBackground(m_iconBattle);
			((TextElement)m_lbStatus).text = zbUtil.Language("lang.11483");
			break;
		case 4:
			m_iconStatus.style.backgroundImage = new StyleBackground(m_iconRepair);
			((TextElement)m_lbStatus).text = zbUtil.Language("lang.11484");
			break;
		}
		bool flag = m_teamInfo.State == 1 || m_teamInfo.State == 4;
		bool flag2 = m_teamInfo.State == 1;
		m_progressBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_btnSpeed).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		if (flag && m_teamInfo.State == 1 && m_teamInfo.March != null)
		{
			SeasonTeamMarchInfo march = m_teamInfo.March;
			long num = march.ArrivalTm - march.StartTm;
			long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
			long num2 = serverTime - march.StartTm;
			long seconds = march.ArrivalTm - serverTime;
			((TextElement)m_lbLeftTime).text = zbUtil.FormatTimeHHMMSS(seconds);
			float num3 = 0f;
			if (num > 0)
			{
				num3 = Mathf.Clamp01((float)num2 / (float)num);
			}
			m_progress.style.width = StyleLength.op_Implicit(Length.Percent(num3 * 100f));
		}
	}

	private void OnPosition()
	{
		zbSeasonMgr.Instance.SeasonMap.TrackCityByAreaId(m_teamInfo.CityId);
	}

	private void OnSpeed()
	{
	}

	public void Update()
	{
	}
}
