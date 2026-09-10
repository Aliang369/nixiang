using System.Collections.Generic;
using System.Linq;

public sealed class EvilPresentTrickCastRequirementAbility : Ability
{
	public ValueGenerator requiredCount;

	public ItemSkinDef evilSkin;

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (eve.type != GameEvent.Type.SpellCastResolve || eve.CasterItem != eve.SourceItem || eve.resolveInfo == null)
		{
			return;
		}
		Item caster = eve.CasterItem;
		if (!(caster?.skin != evilSkin))
		{
			ItemContainer itemContainer = caster?.OwnerBft?.tableContainer;
			int num = requiredCount?.GenerateValue(eve) ?? 3;
			if (itemContainer == null || itemContainer.GetAllItems().Count((Item item) => item != caster) < num)
			{
				eve.resolveInfo.reject = true;
			}
		}
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.SpellCastResolve;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { requiredCount };
	}
}
