using System;
using UnityEngine;

public class ContractRankingModule : ConstellationDetailPanel.DisplayModule
{
	[SerializeField]
	private ContractRankingPanel contractRankingPanel;

	[NonSerialized]
	private ChallengeStar current;

	public override void Init()
	{
		parent.InitUI(contractRankingPanel);
		contractRankingPanel.gameObject.SetActive(value: false);
	}

	public override void Setup()
	{
		contractRankingPanel.gameObject.SetActive(value: false);
		current = null;
		UpdateDisplay();
	}

	public override void UpdateDisplay()
	{
		if (base.constellation.hideContractRanking)
		{
			current = null;
			base.manager.HideUI(contractRankingPanel);
		}
		else if (!(base.constellationDetailPanel.focusStar == current))
		{
			current = base.constellationDetailPanel.focusStar;
			if (current != null)
			{
				contractRankingPanel.Setup(current.ContractDef);
				base.manager.ShowUI(contractRankingPanel);
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIDisplayModule.applicationState, "观星室-排行榜");
			}
			else
			{
				base.manager.HideUI(contractRankingPanel);
			}
		}
	}

	private void OnDisable()
	{
		contractRankingPanel.ClearCachedRankingData();
	}
}
