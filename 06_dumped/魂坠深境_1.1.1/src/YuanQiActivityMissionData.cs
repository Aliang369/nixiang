using System.Collections.Generic;

public class YuanQiActivityMissionData
{
	public string missionId;

	public int order;

	public string titleKey;

	public string descriptionKey;

	public YuanQiActivityMissionType type;

	public int targetProgress;

	public string characterId;

	public DifficultyLevel? minDifficulty;

	public int? stage;

	public string bossId;

	public YuanQiActivityMissionJumpType jumpType;

	public string jumpParam;

	public List<MatterReward> rewards;

	public IReadOnlyList<MatterReward> Rewards => rewards;
}
