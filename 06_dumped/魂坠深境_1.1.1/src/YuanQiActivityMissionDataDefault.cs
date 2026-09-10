using System.Collections.Generic;

public class YuanQiActivityMissionDataDefault
{
	private const string GiftCodeMissionId = "activity_mission_01_gift_code";

	private const string ShareMissionId = "activity_mission_02_share";

	private const string YuanQiKnightCharacterId = "#CHAD#000027#";

	private const string TalentMissionId = "activity_mission_03_talent";

	private const string BossKillMissionId = "activity_mission_04_boss_kill";

	public static List<YuanQiActivityMissionData> BuildLocalPreviewConfigs()
	{
		return new List<YuanQiActivityMissionData>
		{
			new YuanQiActivityMissionData
			{
				missionId = "activity_mission_01_gift_code",
				order = 1,
				titleKey = "term_activity_mission_yuanqi_01_title",
				descriptionKey = "term_activity_mission_yuanqi_01_description",
				type = YuanQiActivityMissionType.GiftCode,
				targetProgress = 1,
				jumpType = YuanQiActivityMissionJumpType.GiftCode
			},
			new YuanQiActivityMissionData
			{
				missionId = "activity_mission_02_share",
				order = 2,
				titleKey = "term_activity_mission_yuanqi_02_title",
				descriptionKey = "term_activity_mission_yuanqi_02_description",
				type = YuanQiActivityMissionType.Share,
				targetProgress = 1,
				characterId = "#CHAD#000027#",
				jumpType = YuanQiActivityMissionJumpType.StartGame
			},
			new YuanQiActivityMissionData
			{
				missionId = "activity_mission_03_talent",
				order = 3,
				titleKey = "term_activity_mission_yuanqi_03_title",
				descriptionKey = "term_activity_mission_yuanqi_03_description",
				type = YuanQiActivityMissionType.TalentCount,
				targetProgress = 10,
				jumpType = YuanQiActivityMissionJumpType.AscenderTalent
			},
			new YuanQiActivityMissionData
			{
				missionId = "activity_mission_04_boss_kill",
				order = 4,
				titleKey = "term_activity_mission_yuanqi_04_title",
				descriptionKey = "term_activity_mission_yuanqi_04_description",
				type = YuanQiActivityMissionType.BossKill,
				targetProgress = 1,
				minDifficulty = DifficultyLevel.Nightmare,
				stage = 7,
				bossId = "#ENEMYINFO#000064#",
				jumpType = YuanQiActivityMissionJumpType.StartGame
			}
		};
	}
}
