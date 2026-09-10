using System.Collections.Generic;

internal class ChangeBouns : Effect
{
	public NewItemGenerator itemGenerator;

	public BattleBonusObject bbo;

	public ChangeRoomBonusAction.ChangeType type;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleBonus battleBonus = new BattleBonus();
		List<Item> list = itemGenerator?.GenerateNewItems(eve);
		if (list != null)
		{
			List<SingleItemBonus> collection = list.ConvertAll((Item item) => new SingleItemBonus(item));
			battleBonus.bonuses.AddRange(collection);
		}
		if (bbo != null)
		{
			battleBonus.bonuses.Add(bbo);
		}
		Effect.EnqueueAction(eve, new ChangeRoomBonusAction(battleBonus, type));
		return NodeCastResult.Success;
	}
}
