namespace ContainerOfSoul.Display.Fortress.DotManagers;

public class CharacterInfoPanel_ToTokenDetailButtonModule : RedDotModule
{
	public PlayerCharacterState playerCharacterState;

	protected override bool? CheckImpl()
	{
		return RedDotModule.IsCharacterUpgradeable(playerCharacterState);
	}
}
