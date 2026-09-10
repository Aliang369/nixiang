public class EveStanceMaxValueGenerator : ValueGenerator
{
	public GameEvent.EveObjType type;

	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.GetEveObj<Stance>(type) == null)
		{
			return null;
		}
		return eve.gameState?.stanceManager?.stanceMaxValue;
	}
}
