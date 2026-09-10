using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AddBossInfluenceBonus : BattleBonusObject, PickBonusSelectionInfoPanel.IShowTextHint
{
	[SaveField(null, null)]
	public List<Influence> selectableInfluence;

	public Influence selectedInfluence;

	public override object infoObject => selectableInfluence?.FirstOrDefault();

	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.defaultBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_boss_influence_bonus;

	private AddBossInfluenceBonus()
	{
	}

	public AddBossInfluenceBonus(List<Influence> selectableInfluence, GameStateInfo gs)
	{
		this.selectableInfluence = selectableInfluence;
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			influences = new List<Influence> { selectedInfluence }
		}));
		ThinkingAnalyticsExtends.TrackBonusBless(gameState, selectedInfluence.def, "精英战斗");
		ConfirmGotBonus();
	}

	public string GetHint()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_boss_influence_bonus;
	}
}
