using System;
using UnityEngine;

public class FinishBattleRecord : GameJournalRecord
{
	[SaveField(null, null)]
	public string enemyId;

	[SaveField(null, null)]
	public FinishBattleEncounterAction.FinishType type;

	public override string NetworkSaveArg0
	{
		get
		{
			return enemyId;
		}
		set
		{
			enemyId = value;
		}
	}

	public override string NetworkSaveArg1
	{
		get
		{
			return type.ToString();
		}
		set
		{
			type = Enum.Parse<FinishBattleEncounterAction.FinishType>(value);
		}
	}

	private FinishBattleRecord()
	{
	}

	public FinishBattleRecord(EnemyInfoData enemy, FinishBattleEncounterAction.FinishType type)
	{
		this.type = type;
		enemyId = enemy.FullId;
	}

	public override bool IsVisibleToPlayer(ApplicationStateInfo applicationState)
	{
		return true;
	}

	public override string GenerateDescription(ApplicationStateInfo applicationState)
	{
		EnemyInfoData enemyInfoData = applicationState.assetCatalog.FullIdToCTAsset<EnemyInfoData>(enemyId);
		return type switch
		{
			FinishBattleEncounterAction.FinishType.Win => ScriptableObjectSingleton<TextDataPreset>.instance.term_defeat_enemy_arg0.FormatWith(enemyInfoData.name.ColorText(Color.red)), 
			FinishBattleEncounterAction.FinishType.Lose => ScriptableObjectSingleton<TextDataPreset>.instance.term_defeated_by_enemy_arg0.FormatWith(enemyInfoData.name.ColorText(Color.red)), 
			FinishBattleEncounterAction.FinishType.Escape => ScriptableObjectSingleton<TextDataPreset>.instance.term_escape_by_Enemy_arg0.FormatWith(enemyInfoData.name.ColorText(Color.red)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
