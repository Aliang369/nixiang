public class EveEnchantValueGenerator : ValueGenerator
{
	public GameEvent.EveObjType type;

	public override int? GenerateValue(GameEvent eve)
	{
		Enchant eveObj = eve.GetEveObj<Enchant>(type);
		if (eveObj == null)
		{
			return null;
		}
		return eveObj.value;
	}
}
