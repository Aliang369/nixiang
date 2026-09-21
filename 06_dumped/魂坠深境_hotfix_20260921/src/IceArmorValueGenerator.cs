public class IceArmorValueGenerator : MItemBlackBoardValueGenerator
{
	private const string IceArmorItemGuid = "7c944459b959a6d45bff374199049f13";

	public override int? GenerateValue(GameEvent eve)
	{
		Item item = eve?.CasterItem;
		if (item?.def?.id != "7c944459b959a6d45bff374199049f13")
		{
			return base.GenerateValue(eve);
		}
		if (IceArmorTurnTriggerAbility.IsBearMageIceArmor(item))
		{
			return 4;
		}
		return item.upgraded ? 5 : 3;
	}
}
