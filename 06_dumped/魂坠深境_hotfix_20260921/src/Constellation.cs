using System;
using System.Collections.Generic;
using System.Linq;
using ContainerOfSoul.Display.Fortress.StarMap;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class Constellation : CTUIBaseAsset
{
	public enum ConstellationType
	{
		SeasonChallenge,
		WeeklyChallenge
	}

	[Serializable]
	public class StarLineConnection
	{
		public ChallengeStar from;

		public ChallengeStar to;

		public EffectLineRenderer line;
	}

	public ConstellationReward reward;

	[LocalizeText("挑战名称")]
	public string constellationName;

	[TextArea]
	[LocalizeText("挑战描述")]
	public string constellationDescription;

	public ConstellationType constellationType;

	public bool hideContractRanking;

	public ChallengeStar startStar;

	public Image background;

	public List<RectTransform> dailyChallengeStarPivots;

	public UnityDateTime startDate = new UnityDateTime(DateTime.UtcNow);

	public UnityDateTime endDate = new UnityDateTime(DateTime.UtcNow);

	public RectTransform connectionLinePivot;

	public List<StarLineConnection> starLineConnections;

	public override string CTAssetIDPrepend => "CONSTELLA";

	public List<ChallengeStar> ChildrenStars => GetComponentsInChildren<ChallengeStar>(includeInactive: true).ToList();

	public List<ContractDef> AllContractDefs => (from s in ChildrenStars
		select s.ContractDef into c
		where c != null
		select c).ToList();

	protected override void Init()
	{
		if (constellationType == ConstellationType.WeeklyChallenge)
		{
			foreach (RectTransform dailyChallengeStarPivot in dailyChallengeStarPivots)
			{
				dailyChallengeStarPivot.gameObject.SetActive(value: false);
			}
			for (int i = 0; i < UIBase.applicationState.contractManager.CurrentWeekContracts.Count && i < dailyChallengeStarPivots.Count; i++)
			{
				ChallengeStar challengeStar = UnityEngine.Object.Instantiate(ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.challengeStarPrefab, dailyChallengeStarPivots[i]);
				InitUI(challengeStar);
				challengeStar.Setup(UIBase.applicationState.contractManager.CurrentWeekContracts[i]);
				challengeStar.SetActiveAndDirty(active: true);
				dailyChallengeStarPivots[i].gameObject.SetActive(value: true);
			}
			return;
		}
		foreach (ChallengeStar childrenStar in ChildrenStars)
		{
			InitUI(childrenStar);
		}
	}

	protected override void UpdateDisplay()
	{
		foreach (StarLineConnection starLineConnection in starLineConnections)
		{
			starLineConnection.line.color = (starLineConnection.to.Unlocked ? ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.activePathLineColor : ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.inactivePathLineColor);
		}
	}

	public string GetNumText()
	{
		if (constellationType != ConstellationType.WeeklyChallenge)
		{
			return $"{ChildrenStars.Count((ChallengeStar c) => UIBase.applicationState.contractManager.GetContractState(UIBase.applicationState.contractManager.GetContractState(c.presetContractDef).contractDef).finishedThisCycle)}/{ChildrenStars.Count}";
		}
		return $"{UIBase.applicationState.contractManager.CurrentWeekContracts.Count((ContractState c) => c.finishedThisCycle)}/{UIBase.applicationState.contractManager.CurrentWeekContracts.Count}";
	}

	public UnityDateTime? GetExpireDateTime()
	{
		if (constellationType == ConstellationType.WeeklyChallenge)
		{
			return new UnityDateTime(DateTime.UtcNow.GetMonday4AM());
		}
		return endDate;
	}

	public bool CanOpen(DateTime utcNow)
	{
		if (constellationType == ConstellationType.WeeklyChallenge)
		{
			return true;
		}
		if (utcNow >= startDate.UtcDateTime)
		{
			return utcNow < endDate.UtcDateTime;
		}
		return false;
	}
}
