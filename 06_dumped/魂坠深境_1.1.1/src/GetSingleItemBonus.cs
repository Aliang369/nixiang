public abstract class GetSingleItemBonus : BattleBonusObject
{
	[SaveField(null, null)]
	public Item item;

	public override object infoObject => item;
}
