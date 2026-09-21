using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GemBonus : BattleBonusObject
{
	[SaveField(null, null)]
	public Dictionary<int, int> gemConfigs;

	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.crystalBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_gem;

	public int totalCount => gemConfigs.Sum((KeyValuePair<int, int> c) => c.Value);

	public GemBonus(Dictionary<int, int> gemConfigs)
	{
		this.gemConfigs = gemConfigs;
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		foreach (KeyValuePair<int, int> gemConfig in gemConfigs)
		{
			gameState.recordManager.gemConfigs.Change(gemConfig.Key, gemConfig.Value);
		}
		gameState.gameEventManager.TriggerGemChange(gameState.recordManager.gemTotalCounts, totalCount);
		ConfirmGotBonus();
	}
}
