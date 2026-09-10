using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelLineDef : CTScriptableObject
{
	public enum LevelLineType
	{
		None,
		Global,
		Season
	}

	public class LineLevel
	{
		public enum LevelRewardType
		{
			Small,
			Middle,
			Big
		}

		public int expCost;

		[FormerlySerializedAs("levelType")]
		public LevelRewardType baseRewardType;

		public GlobalRewardSetup reward;

		public LevelRewardType appendRewardType;

		public GlobalRewardSetup appendReward;

		public GlobalRewardInfo GenerateBaseReward(ApplicationStateInfo applicationState, LevelLine levelLine)
		{
			int lineLevelIndex = levelLine.GetLineLevelIndex(this);
			if (!levelLine.baseRewardGotLevels.Contains(lineLevelIndex))
			{
				return reward?.GenerateReward(applicationState, GlobalRewardSourceType.LevelLineBaseReward, levelLine.levelLineDef);
			}
			return null;
		}

		public GlobalRewardInfo GenerateAppendReward(ApplicationStateInfo applicationState, LevelLine levelLine)
		{
			int lineLevelIndex = levelLine.GetLineLevelIndex(this);
			if (!levelLine.appendRewardGotLevels.Contains(lineLevelIndex))
			{
				return appendReward?.GenerateReward(applicationState, GlobalRewardSourceType.LevelLineAppendReward, levelLine.levelLineDef);
			}
			return null;
		}

		public int GetSumExpToThisLevel(LevelLineDef line)
		{
			int num = 0;
			if (!line.levels.Contains(this))
			{
				return num;
			}
			for (int i = 0; i < line.levels.Count; i++)
			{
				num += line.levels[i].expCost;
				if (line.levels[i] == this)
				{
					break;
				}
			}
			return num;
		}
	}

	public class DumpExpSetup
	{
		public int expCost;

		public List<GlobalRewardSetup> rewardSetups;
	}

	[SerializeField]
	[CTSOAssetName]
	[LocalizeText("升级线名字")]
	public string levelLineName;

	public LevelLineType type;

	public List<LineLevel> levels;

	public DumpExpSetup expDump;

	public bool levelLineAutoGiveReward;

	public List<MatterInfo> manualBuyLevelCost;

	public override string CTAssetIDPrepend => "LL";

	public int SumExpCost => levels.Sum((LineLevel l) => l.expCost);

	public int GetLineLevelIndex(LineLevel level)
	{
		if (levels.Contains(level))
		{
			return levels.IndexOf(level);
		}
		return -1;
	}

	public LineLevel GetLineLevelOfIndex(int levelIndex)
	{
		if (levelIndex < 0 || levelIndex >= levels.Count)
		{
			return null;
		}
		return levels[levelIndex];
	}

	public LevelLine GetLevelLineOfDef(ApplicationStateInfo applicationState)
	{
		return GetAllLevelLine(applicationState).FirstOrDefault((LevelLine l) => l.levelLineDef == this);
	}

	public static List<LevelLine> GetAllLevelLine(ApplicationStateInfo applicationState)
	{
		return new List<LevelLine>
		{
			applicationState.globalUnlockManager.globalLevelLine,
			applicationState.seasonManager.currentSeason.seasonLeveLine
		};
	}
}
