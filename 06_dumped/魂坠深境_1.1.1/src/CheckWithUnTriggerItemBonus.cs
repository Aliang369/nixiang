public class CheckWithUnTriggerItemBonus : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.battleBonus?.bonuses == null)
		{
			return null;
		}
		return eve.battleBonus.bonuses.Find((BattleBonusObject b) => !b.triggered && b.GetType() == typeof(GenerateCardsBonus)) != null;
	}
}
