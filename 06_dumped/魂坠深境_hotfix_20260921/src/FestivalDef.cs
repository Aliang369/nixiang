using System.Collections.Generic;
using UnityEngine;

public class FestivalDef : CTScriptableObject
{
	public enum PanelType
	{
		Launch,
		Carnival
	}

	[CTSOAssetName]
	public string festivalName;

	[LocalizeText("签到活动名称")]
	public string displayName;

	[SerializeField]
	public PanelType panelType;

	[SerializeField]
	public List<GlobalRewardSetup> dailyCheckInRewards;

	[SerializeField]
	public UnityDateTime startDate;

	[SerializeField]
	public UnityDateTime endDate;

	[SerializeField]
	public ActivityDef limitedTimeActivity;

	public override string CTAssetIDPrepend => "FEST";

	public int RewardDayCount => dailyCheckInRewards.Count;
}
