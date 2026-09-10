public class CheckEveObjType : CheckNode
{
	public enum CheckType
	{
		Item,
		Enchant,
		Rune,
		Influence
	}

	public GameEvent.EveObjType type;

	public CheckType checkType;

	public override bool? Check(GameEvent eve)
	{
		if (eve.GetEveObj(type) == null)
		{
			return null;
		}
		return checkType switch
		{
			CheckType.Item => eve.GetEveObj(type) is Item, 
			CheckType.Enchant => eve.GetEveObj(type) is Enchant, 
			CheckType.Rune => eve.GetEveObj(type) is Rune, 
			CheckType.Influence => eve.GetEveObj(type) is Influence, 
			_ => null, 
		};
	}
}
