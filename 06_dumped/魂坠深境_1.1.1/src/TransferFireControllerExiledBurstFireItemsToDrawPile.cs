public class TransferFireControllerExiledBurstFireItemsToDrawPile : FireControllerExileEffect
{
	public int fireSeedValue = 1;

	public int swiftValueOnUpgrade = 1;

	public int moveDelta = 1;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item targetItem = eve.TargetItem;
		if (targetItem == null)
		{
			return NodeCastResult.Success;
		}
		FireControllerExileEffect.AddEnchantToItem(eve, targetItem, "97fb47d81f913754aa2ac35e19f09a57", fireSeedValue);
		Item casterItem = eve.CasterItem;
		if (casterItem != null && casterItem.upgraded)
		{
			FireControllerExileEffect.AddEnchantToItem(eve, targetItem, "996afab9e52a8c54e8efbd04e8aab0cb", swiftValueOnUpgrade);
		}
		Effect.EnqueueAction(eve, new MoveDeltaSlotAction(moveDelta, targetItem, eve.CasterItem));
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(fireSeedValue),
			deltaValue = new NodeDisplayInfo.Element<int?>(moveDelta),
			outputBool = new NodeDisplayInfo.Element<bool?>(eve.CasterItem?.upgraded ?? false)
		};
	}
}
