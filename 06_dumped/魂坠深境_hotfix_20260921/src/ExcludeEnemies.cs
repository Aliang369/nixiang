using System.Collections.Generic;
using System.Linq;

public class ExcludeEnemies : DraftLocationModifier
{
	public List<EnemyAssetRef> enemyRefs = new List<EnemyAssetRef>();

	public override void ModifyLocation(PathLocation location, DraftPivot pivot, DungeonMap map, GameStateInfo gs)
	{
		if (location.attachEncounterSetupInfo == null || enemyRefs.IsNullOrEmpty())
		{
			return;
		}
		EncounterSetupInfo attachEncounterSetupInfo = location.attachEncounterSetupInfo;
		if (attachEncounterSetupInfo.excludedEnemyRefs == null)
		{
			attachEncounterSetupInfo.excludedEnemyRefs = new List<EnemyAssetRef>();
		}
		foreach (EnemyAssetRef enemyRef in enemyRefs.Where((EnemyAssetRef e) => e != null))
		{
			if (location.attachEncounterSetupInfo.excludedEnemyRefs.All((EnemyAssetRef e) => e.CTAssetId != enemyRef.CTAssetId))
			{
				location.attachEncounterSetupInfo.excludedEnemyRefs.Add(enemyRef);
			}
		}
	}
}
