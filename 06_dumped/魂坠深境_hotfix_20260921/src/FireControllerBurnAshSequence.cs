public class FireControllerBurnAshSequence : SequenceSpells
{
	private const string SwiftEnchantGuid = "996afab9e52a8c54e8efbd04e8aab0cb";

	public int fireSeedValue = 1;

	public int swiftValueOnUpgrade = 1;

	public int moveDelta = 1;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		NodeCastResult result = base.OnCast(eve);
		Item casterItem = eve.CasterItem;
		if (casterItem != null && casterItem.upgraded && eve.TargetItem != null)
		{
			EnchantDef enchantDef = eve.gameState?.applicationState?.assetCatalog?.enchantDefCatalog?.GetAsset("996afab9e52a8c54e8efbd04e8aab0cb");
			if (enchantDef != null)
			{
				Effect.EnqueueAction(eve, new ChangeEnchantAction(eve.TargetItem, eve.CasterItem, enchantDef, swiftValueOnUpgrade));
			}
		}
		return result;
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
