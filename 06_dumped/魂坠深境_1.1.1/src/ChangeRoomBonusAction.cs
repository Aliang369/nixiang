public class ChangeRoomBonusAction : ExecuteAction
{
	public enum ChangeType
	{
		Override,
		Append
	}

	private readonly ChangeType type;

	private readonly BattleBonus bonus;

	public ChangeRoomBonusAction(BattleBonus overrideBonus, ChangeType type)
	{
		this.type = type;
		bonus = overrideBonus;
	}

	protected override void Update()
	{
		switch (type)
		{
		case ChangeType.Override:
			gameState.currentEncounter.roomBonus = bonus;
			break;
		case ChangeType.Append:
			gameState.currentEncounter.roomBonus.bonuses.AddRange(bonus.bonuses);
			break;
		}
	}
}
