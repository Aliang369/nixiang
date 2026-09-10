using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IdolSystemConfig
{
	[SerializeField]
	public int forgetPointsPerLevel = 1;

	[SerializeField]
	public int experienceToSoulSacrificeRatio = 1000;

	[SerializeField]
	public int seasonExperienceLimit = 12;

	[SerializeField]
	public int newCharacterReward = 1;

	[SerializeField]
	public int obsessionLevelUpReward = 1;

	public int GetDifficultyReward(int difficultyLevel)
	{
		if (!IsDifficultySoulRewardable((DifficultyLevel)difficultyLevel))
		{
			return 0;
		}
		return 1;
	}

	public bool IsDifficultySoulRewardable(DifficultyLevel level)
	{
		if (level >= DifficultyLevel.Hard)
		{
			return level <= DifficultyLevel.NightmareD9;
		}
		return false;
	}

	public List<DifficultyLevel> GetRewardableDifficultyLevels(AssetCatalog catalog)
	{
		if (catalog?.difficultyCatalog?.ValidAssets == null)
		{
			return new List<DifficultyLevel>();
		}
		return (from d in (from d in catalog.difficultyCatalog.ValidAssets
				where d != null && IsDifficultySoulRewardable(d.difficultyLevel)
				select d.difficultyLevel).Distinct()
			orderby (int)d
			select d).ToList();
	}

	public int GetDifficultySoulRewardCount(DifficultyLevel current, AssetCatalog catalog)
	{
		return GetRewardableDifficultyLevels(catalog).Count((DifficultyLevel d) => d <= current);
	}

	public DifficultyLevel? GetHighestRewardedDifficultyLevel(DifficultyLevel current, AssetCatalog catalog)
	{
		List<DifficultyLevel> rewardableDifficultyLevels = GetRewardableDifficultyLevels(catalog);
		for (int num = rewardableDifficultyLevels.Count - 1; num >= 0; num--)
		{
			if (rewardableDifficultyLevels[num] <= current)
			{
				return rewardableDifficultyLevels[num];
			}
		}
		return null;
	}
}
