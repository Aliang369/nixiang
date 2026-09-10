using System.Collections.Generic;
using System.Linq;

public class MoonCocoonAbility : Ability
{
	private const string BrokenStateKey = "MoonCocoonAbility.Broken";

	public EnchantDef enrageEnchant;

	public int enrageDuration = 99;

	public int healPercent = 50;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.BftDeathCheckResolve;
		yield return GameEvent.Type.PlayerTurnStart;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		Item casterItem = eve.CasterItem;
		if (casterItem == null || casterItem.owner?.type != ItemContainer.ContainerType.Table)
		{
			return false;
		}
		if (eve.type == GameEvent.Type.BftDeathCheckResolve)
		{
			if (eve.TargetBft == casterItem.OwnerBft)
			{
				return (int?)eve.TargetBft?.health <= 0;
			}
			return false;
		}
		if (eve.type == GameEvent.Type.PlayerTurnStart)
		{
			return IsBroken(casterItem);
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		Item cocoon = eve.CasterItem;
		if (eve.type == GameEvent.Type.BftDeathCheckResolve)
		{
			eve.resolveInfo.reject = true;
			eve.resolveInfo.rejectSource = cocoon;
			eve.gameState.actionManager.EnqueueAction(new SetBftHealthAction(1, cocoon, cocoon.OwnerBft));
			if (!IsBroken(cocoon))
			{
				cocoon.tempValues["MoonCocoonAbility.Broken"] = true;
				eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
				{
					eve.gameState.gameEventManager.TriggerMoonCocoonBreak(cocoon.OwnerBft, cocoon);
				}));
			}
		}
		else
		{
			ResolveBrokenCocoon(eve, cocoon);
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		List<Enchant> list = new List<Enchant>();
		if (enrageEnchant != null)
		{
			list.Add(new Enchant(enrageEnchant, enrageDuration));
		}
		return new NodeDisplayInfo
		{
			refEnchants = new NodeDisplayInfo.Element<List<Enchant>>(list),
			value = new NodeDisplayInfo.Element<int?>(healPercent)
		};
	}

	private static bool IsBroken(Item cocoon)
	{
		if (cocoon?.tempValues != null && cocoon.tempValues.TryGetValue("MoonCocoonAbility.Broken", out var value))
		{
			if (value is bool)
			{
				return (bool)value;
			}
			return false;
		}
		return false;
	}

	private void ResolveBrokenCocoon(GameEvent eve, Item cocoon)
	{
		BattleFieldTransform ownerBft = cocoon.OwnerBft;
		eve.gameState.actionManager.EnqueueAction(new ExileItemAction(cocoon, cocoon));
		if (ownerBft != null)
		{
			int healAmount = (int)ownerBft.maxHealth * healPercent / 100;
			eve.gameState.actionManager.EnqueueAction(new HealAction(cocoon, ownerBft, healAmount));
		}
		ItemTag attackTag = ScriptableObjectSingleton<GameDataManager>.instance?.attackTag;
		if (ownerBft == null || !(attackTag != null) || !(enrageEnchant != null))
		{
			return;
		}
		foreach (Item item in from item in ownerBft.GetAllRealItems()
			where item != cocoon && item.HasTag(attackTag)
			select item)
		{
			eve.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(item, cocoon, enrageEnchant, enrageDuration));
		}
	}
}
