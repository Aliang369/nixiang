using System.Linq;
using GameRandom;
using UnityEngine;

public class RandomObsessionInUnlockedCharacter : GlobalRewardSetup
{
	public override Sprite GlobalItemCellIcon => ScriptableObjectSingleton<VFXDataPreset>.instance.icon_global_item_random_loot_matter;

	public override string GlobalItemCellName => FGlobalItemCellName();

	public override string GlobalItemTitle => FGlobalItemTitle();

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return base.GlobalItemStringConfig.GetGlobalItemDescription;
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		PlayerCharacterDef def = applicationState.characterManager.characterStates.Where((PlayerCharacterState c) => !c.locked).ToList().SelectRandom()
			.def;
		return new GlobalRewardInfo
		{
			matter = new MatterInfo(def.obsession, 1)
		};
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief();
	}
}
