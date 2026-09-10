namespace ContainerOfSoul.Display.Fortress.DotManagers;

public class CharacterInfoPanel_TalentCellModule : RedDotModule
{
	public PlayerCharacterState playerCharacterState;

	public TalentSlotCell cell;

	protected override bool? CheckImpl()
	{
		return RedDotModule.TalentUnlockable(playerCharacterState, cell);
	}
}
