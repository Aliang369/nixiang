using GameRandom;
using UnityEngine;

public class RandomCharacterInClass : GlobalRewardSetup
{
	public CharacterClassDef characterClassDef;

	public override Sprite GlobalItemCellIcon => characterClassDef.professionIcon;

	public override string GlobalItemCellName => FGlobalItemCellName(characterClassDef.characterName);

	public override string GlobalItemTitle => FGlobalItemTitle(characterClassDef.characterName);

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return string.Format(base.GlobalItemStringConfig.GetGlobalItemDescription, characterClassDef.characterName);
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		PlayerCharacterDef def = applicationState.characterManager.GetClassAllCharacters(characterClassDef).SelectRandom().def;
		return new GlobalRewardInfo
		{
			matter = new MatterInfo(def.obsession, 1)
		};
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(characterClassDef?.characterName);
	}
}
