using System;
using System.Collections.Generic;

[Serializable]
public class EncounterSetupInfo
{
	[SaveField(null, null)]
	public Level level;

	[SaveField(null, null)]
	public RoomInfoData roomInfoData;

	[SaveField(null, null)]
	public WorldEvent fixedEventTemplate;

	[SaveField(null, null)]
	public BattleBonus roomBonus;

	[SaveField(null, null)]
	public EnemyInfoData fixedEnemyInfoData;

	[SaveField(null, null)]
	public List<EnemyAssetRef> excludedEnemyRefs;

	[SaveField(null, null)]
	public EnemyEncounterType enemySpawnType = EnemyEncounterType.None;

	[SaveField(null, null)]
	public CharacterAIComponent.EnemySpawnSetup fixedEnemySetup;

	[SaveField(null, null)]
	public int fixedEnemySetupIndex = -1;

	[SaveField(null, null)]
	public EnemyInfoData preselectedEnemyInfoData;

	[SaveField(null, null)]
	public int preselectedEnemySetupIndex = -1;

	[SaveField(null, null)]
	public float enemyLevel = 1f;

	[SaveField(null, null)]
	public PVPDefender pvpDefender;

	[SaveField(null, null)]
	public bool spawnRandomWorldEvent;

	public CharacterAIComponent.EnemySpawnSetup PreselectedEnemySetup
	{
		get
		{
			if (preselectedEnemyInfoData?.setups == null || preselectedEnemySetupIndex < 0 || preselectedEnemySetupIndex >= preselectedEnemyInfoData.setups.Count)
			{
				return null;
			}
			return preselectedEnemyInfoData.setups[preselectedEnemySetupIndex];
		}
	}
}
