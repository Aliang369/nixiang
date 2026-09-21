namespace ContainerOfSoul.Display.Fortress.DotManagers;

public class CharacterInfoPanel_OpenTalentButton : RedDotModule
{
	public PlayerCharacterState characterState;

	protected override bool? CheckImpl()
	{
		if (!RedDotModule.applicationState.playerInfoManager.showUnlockTalent)
		{
			return false;
		}
		return RedDotModule.HasAnyUnlockableTalent(characterState);
	}
}
