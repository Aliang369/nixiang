using System.Collections.Generic;
using System.Linq;

public sealed class DancerCharmSkirtClaspRecordArmorEffect : Effect
{
	public const string TargetKey = "DancerCharmSkirtClasp.TargetArmor";

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		GameSkill gameSkill = eve?.CasterSkill;
		if (gameSkill == null || eve.SelectedItems == null)
		{
			return NodeCastResult.Failure;
		}
		List<Item> list = eve.SelectedItems.Where(delegate(Item item)
		{
			if (item != null)
			{
				BattleFieldTransform ownerBft = item.OwnerBft;
				if (ownerBft != null && ownerBft.isPlayer)
				{
					ItemContainer owner = item.owner;
					if (owner != null && owner.type == ItemContainer.ContainerType.Table)
					{
						return item.DefenceLike;
					}
				}
			}
			return false;
		}).Distinct().ToList();
		if (list.Count != 1)
		{
			return NodeCastResult.Failure;
		}
		GameSkill gameSkill2 = gameSkill;
		if (gameSkill2.tempValues == null)
		{
			gameSkill2.tempValues = new Dictionary<string, object>();
		}
		gameSkill.tempValues["DancerCharmSkirtClasp.TargetArmor"] = list[0];
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
