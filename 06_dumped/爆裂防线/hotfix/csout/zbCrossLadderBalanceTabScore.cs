using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossLadderBalanceTabScore
{
	private zbCrossLadderBalanceWnd m_wndParent;

	private VisualElement m_rootElement;

	private zbLadderItem m_itemLadder;

	private VisualElement m_nodeMatch;

	private Label m_lbScore;

	private VisualElement[] m_spriteVictoryCounts = (VisualElement[])(object)new VisualElement[3];

	private Button m_btnVictoryCount;

	private VisualElement m_nodeTip;

	private Label m_lbTip;

	private IVisualElementScheduledItem m_hideSchedule;

	private VisualElement m_nodeCard;

	private Label m_lbCardDesc;

	private VisualElement m_nodeUpgrade;

	private VisualElement[] m_spriteResults = (VisualElement[])(object)new VisualElement[5];

	private int m_ladderId;

	public void OnEnable(zbCrossLadderBalanceWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_rootElement = root;
		m_itemLadder = UQueryExtensions.Q<zbLadderItem>(m_rootElement, "ladder", (string)null);
		m_itemLadder.OnEnable(wndParent);
		m_nodeMatch = UQueryExtensions.Q<VisualElement>(m_rootElement, "node_match", (string)null);
		m_lbScore = UQueryExtensions.Q<Label>(m_nodeMatch, "lb_score", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_spriteVictoryCounts[i] = UQueryExtensions.Q<VisualElement>(m_nodeMatch, "bg_victoryCount" + i, (string)null);
		}
		m_btnVictoryCount = UQueryExtensions.Q<Button>(m_nodeMatch, "btn_victoryCount", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnVictoryCount, (Action)OnVictoryCount, "", (TrickleDown)0);
		m_nodeTip = UQueryExtensions.Q<VisualElement>(m_rootElement, "tip", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_nodeTip, "lb_tip", (string)null);
		m_nodeCard = UQueryExtensions.Q<VisualElement>(m_nodeMatch, "node_card", (string)null);
		m_lbCardDesc = UQueryExtensions.Q<Label>(m_nodeMatch, "lb_cardDesc", (string)null);
		m_nodeUpgrade = UQueryExtensions.Q<VisualElement>(m_rootElement, "node_upgrade", (string)null);
		for (int j = 0; j < 5; j++)
		{
			m_spriteResults[j] = UQueryExtensions.Q<VisualElement>(m_nodeUpgrade, "bg_result" + j, (string)null);
		}
	}

	public void Refresh()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		m_nodeTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_ladderId != zbServerData.sGameData.CrossLadderInfo.SelfInfo.SeasonAward.LadderId)
		{
			m_itemLadder.Refresh(zbServerData.sGameData.CrossLadderInfo.SelfInfo.SeasonAward.LadderId);
			m_ladderId = zbServerData.sGameData.CrossLadderInfo.SelfInfo.SeasonAward.LadderId;
		}
		if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.CrossLadderMatch)
		{
			m_nodeMatch.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			RefreshMatch();
		}
		else if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.CrossLadderUpgrade)
		{
			m_nodeMatch.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			RefreshUpgrade();
		}
	}

	private void RefreshMatch()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbScore).text = zbSelf.Instance.crossLadderHandler.GetScoreChangeDesc();
		for (int i = 0; i < 3; i++)
		{
			string resName = "ui/image/crossLadder/crossLadder_point.jpg";
			if (i < zbServerData.sGameData.CrossLadderInfo.CombatInfo.VictoryCount)
			{
				resName = "ui/image/crossLadder/crossLadder_point_light.jpg";
			}
			m_spriteVictoryCounts[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
		}
		if (zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard())
		{
			m_nodeCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (m_wndParent.m_isFail)
			{
				((TextElement)m_lbCardDesc).text = zbUtil.Language("lang.10919");
			}
			else
			{
				((TextElement)m_lbCardDesc).text = zbUtil.Language("lang.10920");
			}
		}
		else
		{
			m_nodeCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void RefreshUpgrade()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			string resName = "ui/image/crossLadder/crossLadder_upgrade_bg.png";
			if (i < zbServerData.sGameData.CrossLadderInfo.CombatInfo.UpgradeState.Count)
			{
				switch ((ELadderUpgradeState)zbServerData.sGameData.CrossLadderInfo.CombatInfo.UpgradeState[i])
				{
				case ELadderUpgradeState.Win:
					resName = "ui/image/crossLadder/crossLadder_upgrade_win.png";
					break;
				case ELadderUpgradeState.Fail:
					resName = "ui/image/crossLadder/crossLadder_upgrade_fail.png";
					break;
				}
			}
			m_spriteResults[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
		}
	}

	private void OnVictoryCount()
	{
		string strTip = string.Format(zbUtil.Language("lang.10898"), zbSelf.Instance.crossLadderHandler.victory3);
		ShowTip(strTip);
	}

	private void ShowTip(string strTip)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbTip).text = strTip;
		m_nodeTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (m_hideSchedule != null)
		{
			m_hideSchedule.Pause();
			m_hideSchedule = null;
		}
		m_hideSchedule = m_rootElement.schedule.Execute((Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_nodeTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}).StartingIn(5000L);
	}
}
