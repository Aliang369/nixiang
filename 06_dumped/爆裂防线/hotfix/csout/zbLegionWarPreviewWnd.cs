using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionWarPreviewWnd : zbBaseWnd
{
	private Label m_lbTime;

	private Label m_lbTemp;

	private Label m_lbTitle;

	private Button m_btnLegionWarTask;

	private Button m_btnClose;

	private Button m_btnRule;

	private Button m_btnAward;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_btnLegionWarTask = UQueryExtensions.Q<Button>(m_RootElement, "btn_legionWarTask", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegionWarTask, (Action)OnTask, "", (TrickleDown)0);
		m_btnAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_award", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAward, (Action)OnAward, "", (TrickleDown)0);
		m_lbTemp = UQueryExtensions.Q<Label>(m_RootElement, "lb_temp", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
	}

	public override void Refresh()
	{
		UpdatePerSecond();
		zbSelf.Instance.legionHandler.GetLegionZone();
	}

	protected override void UpdatePerSecond()
	{
		long num = zbSelf.Instance.legionWarHandler.getLegionWarOpenInfo().PreviewEndTm - zbSelf.Instance.timeHandler.GetServerTime();
		((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(num);
		if (num < 0)
		{
			Hide();
		}
	}

	private void OnAward()
	{
		long num = 0L;
		long num2 = 0L;
		if (zbSelf.Instance.legionWarHandler.legionWarSettlePlayerInfoAck != null)
		{
			foreach (LegionWarSettlePlayerInfo force in zbSelf.Instance.legionWarHandler.legionWarSettlePlayerInfoAck.ForceList)
			{
				foreach (LegionWarPlayerRankDetail rank in force.RankList)
				{
					if (zbSelf.Instance.GetRoleId() == rank.RoleBaseInfo.RoleId)
					{
						num = rank.RankData.Rank;
						break;
					}
				}
			}
		}
		if (zbSelf.Instance.legionWarHandler.legionWarSettleLegionInfoAck != null)
		{
			foreach (LegionWarSettleLegionInfo force2 in zbSelf.Instance.legionWarHandler.legionWarSettleLegionInfoAck.ForceList)
			{
				foreach (LegionWarLegionRankDetail rank2 in force2.RankList)
				{
					if (zbServerData.sGameData.LegionSelfInfo.LegionId == rank2.LegionFlagInfo.LegionId)
					{
						num2 = rank2.RankData.Rank;
						break;
					}
				}
			}
		}
		zbUIMgr.Instance.ShowLegionWarRankAward((int)num, (int)num2);
	}

	public void OnRule()
	{
		string[] langKeys = new string[1] { "lang.11207" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnTask()
	{
		zbUIMgr.Instance.ShowLegionWarTask();
	}

	private void OnClose()
	{
		Hide();
	}
}
