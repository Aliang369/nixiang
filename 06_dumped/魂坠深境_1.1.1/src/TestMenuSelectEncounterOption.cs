public struct TestMenuSelectEncounterOption
{
	public EnemyInfoData enemyInfoData;

	public WorldEvent worldEvent;

	public CharacterAIComponent.EnemySpawnSetup enemySetup;

	public EnemyEncounterType enemyEncounterType;

	public bool useLocationEvent;

	public PathLocation.LocationType locationType;

	public PathLocation.LocationVariantType locationVariantType;

	public string DisplayId
	{
		get
		{
			if (enemyInfoData != null)
			{
				string text = "";
				switch (enemyEncounterType)
				{
				case EnemyEncounterType.Normal:
					text = ScriptableObjectSingleton<TextDataPreset>.instance.term_enemyType_normal;
					break;
				case EnemyEncounterType.Elite:
					text = ScriptableObjectSingleton<TextDataPreset>.instance.term_enemyType_elite;
					break;
				case EnemyEncounterType.Boss:
					text = ScriptableObjectSingleton<TextDataPreset>.instance.term_enemyType_boss;
					break;
				}
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_enemy + "-" + enemyInfoData.name + "-" + text;
			}
			if (useLocationEvent)
			{
				string locationTypeName = PathLocation.GetLocationTypeName(locationType);
				if (locationVariantType == PathLocation.LocationVariantType.None)
				{
					return ScriptableObjectSingleton<TextDataPreset>.instance.term_event + "-" + locationTypeName;
				}
				string locationVaraintTypeName = PathLocation.GetLocationVaraintTypeName(locationVariantType);
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_event + "-" + locationTypeName + "-" + locationVaraintTypeName;
			}
			if (worldEvent != null)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_event + "资产-" + worldEvent.EventName;
			}
			return "XXX";
		}
	}
}
