public class EveStanceValueGenerator : ValueGenerator
{
	public GameEvent.EveObjType type;

	public override int? GenerateValue(GameEvent eve)
	{
		return eve.GetEveObj<Stance>(type)?.value;
	}
}
