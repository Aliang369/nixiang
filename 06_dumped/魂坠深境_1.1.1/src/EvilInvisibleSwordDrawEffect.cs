using System.Collections.Generic;

public sealed class EvilInvisibleSwordDrawEffect : Effect
{
	public ItemSkinDef evilSkin;

	public DealDamage damage;

	public EnchantDef painEnchant;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (evilSkin != null && eve?.TargetItem?.skin != evilSkin)
		{
			return NodeCastResult.Success;
		}
		BattleFieldTransform eventBFT = eve.GetEventBFT(GameEvent.EventBFTType.Opponent);
		int delta = damage?.GetPreviewCastDamage(eve) ?? 4;
		foreach (Item item in eventBFT?.tableContainer?.GetAllItems() ?? new List<Item>())
		{
			if (painEnchant != null)
			{
				Effect.EnqueueAction(eve, new ChangeEnchantAction(item, eve.TargetItem ?? eve.CasterItem, painEnchant, delta));
			}
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int num = damage?.GetPreviewCastDamage(eve) ?? 4;
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(num),
			baseValue = new NodeDisplayInfo.Element<int?>(damage?.GetBaseDamage(eve) ?? 4),
			deltaValue = new NodeDisplayInfo.Element<int?>(num - (damage?.GetBaseDamage(eve) ?? 4))
		};
		if (painEnchant != null)
		{
			nodeDisplayInfo.refEnchants = new NodeDisplayInfo.Element<List<Enchant>>(new List<Enchant>
			{
				new Enchant(painEnchant, num)
			});
		}
		return nodeDisplayInfo;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { damage };
	}
}
