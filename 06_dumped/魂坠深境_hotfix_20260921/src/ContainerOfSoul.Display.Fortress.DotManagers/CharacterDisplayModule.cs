namespace ContainerOfSoul.Display.Fortress.DotManagers;

public class CharacterDisplayModule : RedDotModule
{
	public PlayerCharacterState playerCharacterState;

	protected override bool? CheckImpl()
	{
		if (RedDotModule.IsCharacterUnlockable(playerCharacterState))
		{
			return true;
		}
		if (!playerCharacterState.locked && RedDotModule.HasAnyUnlockableTalent(playerCharacterState) && RedDotModule.applicationState.playerInfoManager.showUnlockTalent)
		{
			return true;
		}
		return false;
	}
}
