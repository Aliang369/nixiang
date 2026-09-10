using System.Collections.Generic;
using UnityEngine;

public class SeasonDef : CTScriptableObject
{
	[SerializeField]
	[CTSOAssetName]
	[LocalizeText("赛季名称")]
	public string seasonName;

	[TextArea]
	[LocalizeText("赛季描述")]
	public string description;

	public Sprite icon;

	public UnityDateTime endTime;

	public LevelLineDef levelLine;

	public ChallengeStarMap challengeStarMap;

	public List<RoutineQuestDef> seasonRoutineQuests;

	public override string CTAssetIDPrepend => "SEASON";
}
