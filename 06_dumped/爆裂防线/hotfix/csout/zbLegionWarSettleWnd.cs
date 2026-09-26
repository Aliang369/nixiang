using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionWarSettleWnd : zbTabWnd
{
	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private VisualElement m_spriteTabSel;

	private Button m_btnAward;

	private Button m_btnBack;

	private Label m_lbWin;

	private VisualElement m_iconWin;

	private Label m_lbTitle;

	private Button m_btnLegionWarTask;

	private Button m_btnLegionWarZhanling;

	private VisualElement m_taskNotify;

	private zbLegionWarSettleTabPlayer m_modulePlayer;

	private zbLegionWarSettleTabLegion m_moduleLegion;

	private zbLegionWarZonePanel m_legionWarZone = new zbLegionWarZonePanel();

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
		}
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		m_btnAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_award", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_lbWin = UQueryExtensions.Q<Label>(m_RootElement, "lb_win", (string)null);
		m_iconWin = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_win", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_btnLegionWarTask = UQueryExtensions.Q<Button>(m_RootElement, "btn_legionWarTask", (string)null);
		m_taskNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLegionWarTask);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegionWarTask, (Action)OnTask, "", (TrickleDown)0);
		m_btnLegionWarZhanling = UQueryExtensions.Q<Button>(m_RootElement, "btn_zhanling", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegionWarZhanling, (Action)OnZhanling, "", (TrickleDown)0);
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "legionZonePullDown", (string)null);
		m_legionWarZone.OnEnable(this, root);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAward, (Action)OnAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		InitTab(m_tabButtons, m_tabLayers, null);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarSettleInfo, OnEventUpdate);
	}

	public override void Refresh()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		OnEventUpdate(null);
		RefreshNotify();
		m_legionWarZone.m_pullDown_detail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	public override void DoSelectTab()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		base.DoSelectTab();
		switch (m_currTab)
		{
		case 0:
			if (m_modulePlayer == null)
			{
				m_modulePlayer = new zbLegionWarSettleTabPlayer();
				m_modulePlayer.OnEnable(this, m_tabLayers[m_currTab]);
			}
			zbWsMsgHandler.Instance.LegionWar_SettlePlayerInfo_Req(zbSelf.Instance.legionWarHandler.currSettleZone);
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			if (m_moduleLegion == null)
			{
				m_moduleLegion = new zbLegionWarSettleTabLegion();
				m_moduleLegion.OnEnable(this, m_tabLayers[m_currTab]);
			}
			zbWsMsgHandler.Instance.LegionWar_SettleLegionInfo_Req(zbSelf.Instance.legionWarHandler.currSettleZone);
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			break;
		}
	}

	private void RefreshNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_taskNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legionWarHandler.HasTaskNotify()));
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
		zbUIMgr.Instance.ShowLegionWarRankAward((int)num, (int)num2, m_currTab);
	}

	private void OnTask()
	{
		zbUIMgr.Instance.ShowLegionWarTask();
	}

	public void OnClose()
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshNotify();
	}

	private void OnEventUpdate(object data)
	{
		if (m_currTab == 0)
		{
			m_modulePlayer.RefreshCustom();
			RefreshPlayerDetail();
		}
		else if (m_currTab == 1)
		{
			m_moduleLegion.RefreshCustom();
			RefreshLegionDetail();
		}
		m_legionWarZone.Refresh(zbSelf.Instance.legionWarHandler.currSettleZone);
		m_RootElement.schedule.Execute((Action)delegate
		{
			int redScoreCount = 0;
			int blueScoreCount = 0;
			if (zbSelf.Instance.legionWarHandler.legionWarSettlePlayerInfoAck != null)
			{
				foreach (LegionWarSettlePlayerInfo force in zbSelf.Instance.legionWarHandler.legionWarSettlePlayerInfoAck.ForceList)
				{
					if (force.ForceId == ELegionWarForce.Red)
					{
						redScoreCount = force.ScoreCount;
					}
					else
					{
						blueScoreCount = force.ScoreCount;
					}
				}
			}
			else if (zbSelf.Instance.legionWarHandler.legionWarSettleLegionInfoAck != null)
			{
				foreach (LegionWarSettleLegionInfo force2 in zbSelf.Instance.legionWarHandler.legionWarSettleLegionInfoAck.ForceList)
				{
					if (force2.ForceId == ELegionWarForce.Red)
					{
						redScoreCount = force2.ScoreCount;
					}
					else
					{
						blueScoreCount = force2.ScoreCount;
					}
				}
			}
			zbSelf.Instance.legionWarHandler.SetSharpProgress(UQueryExtensions.Q<VisualElement>(m_RootElement, "title", (string)null), redScoreCount, blueScoreCount, ELegionWarForce.Red, 296);
		});
	}

	private void RefreshPlayerDetail()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		LegionWarSettlePlayerInfoAck legionWarSettlePlayerInfoAck = zbSelf.Instance.legionWarHandler.legionWarSettlePlayerInfoAck;
		m_iconWin.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!CheckHasPlayerInfo()));
		if (legionWarSettlePlayerInfoAck.ForceList.Count != 0)
		{
			bool flag = legionWarSettlePlayerInfoAck.WinForceId == ELegionWarForce.Red;
			((TextElement)m_lbWin).text = zbUtil.Language(flag ? "lang.11166" : "lang.11167");
			m_iconWin.style.left = StyleLength.op_Implicit((StyleKeyword)((!flag) ? 2 : 0));
			m_iconWin.style.right = StyleLength.op_Implicit((StyleKeyword)(flag ? 2 : 0));
		}
	}

	private void RefreshLegionDetail()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		LegionWarSettleLegionInfoAck legionWarSettleLegionInfoAck = zbSelf.Instance.legionWarHandler.legionWarSettleLegionInfoAck;
		m_iconWin.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!CheckHasLegionInfo()));
		if (legionWarSettleLegionInfoAck.ForceList.Count != 0)
		{
			bool flag = legionWarSettleLegionInfoAck.WinForceId == ELegionWarForce.Red;
			((TextElement)m_lbWin).text = zbUtil.Language(flag ? "lang.11167" : "lang.11168");
			m_iconWin.style.left = StyleLength.op_Implicit((StyleKeyword)((!flag) ? 2 : 0));
			m_iconWin.style.right = StyleLength.op_Implicit((StyleKeyword)(flag ? 2 : 0));
		}
	}

	public bool CheckHasPlayerInfo()
	{
		LegionWarSettlePlayerInfoAck legionWarSettlePlayerInfoAck = zbSelf.Instance.legionWarHandler.legionWarSettlePlayerInfoAck;
		if (legionWarSettlePlayerInfoAck == null)
		{
			return false;
		}
		if (legionWarSettlePlayerInfoAck.ForceList == null || legionWarSettlePlayerInfoAck.ForceList.Count == 0)
		{
			return false;
		}
		if (legionWarSettlePlayerInfoAck.ForceList[0].ScoreCount == 0 && legionWarSettlePlayerInfoAck.ForceList[1].ScoreCount == 0)
		{
			return false;
		}
		return true;
	}

	public bool CheckHasLegionInfo()
	{
		LegionWarSettleLegionInfoAck legionWarSettleLegionInfoAck = zbSelf.Instance.legionWarHandler.legionWarSettleLegionInfoAck;
		if (legionWarSettleLegionInfoAck == null)
		{
			return false;
		}
		if (legionWarSettleLegionInfoAck.ForceList == null || legionWarSettleLegionInfoAck.ForceList.Count == 0)
		{
			return false;
		}
		if (legionWarSettleLegionInfoAck.ForceList[0].ScoreCount == 0 && legionWarSettleLegionInfoAck.ForceList[1].ScoreCount == 0)
		{
			return false;
		}
		return true;
	}

	private void OnZhanling()
	{
		zbUIMgr.Instance.ShowLegionWarZhanling();
	}
}
