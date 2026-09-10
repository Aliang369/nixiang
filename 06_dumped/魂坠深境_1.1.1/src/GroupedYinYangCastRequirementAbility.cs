using System.Collections.Generic;

public sealed class GroupedYinYangCastRequirementAbility : Ability
{
	public ValueGenerator yinPerGroup;

	public ValueGenerator yangPerGroup;

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.SpellCastResolve && eve.CasterItem == eve.SourceItem && eve.resolveInfo != null)
		{
			int valueOrDefault = (yinPerGroup?.GenerateValue(eve)).GetValueOrDefault();
			int valueOrDefault2 = (yangPerGroup?.GenerateValue(eve)).GetValueOrDefault();
			eve.resolveInfo.reject |= valueOrDefault <= 0 || valueOrDefault2 <= 0 || (int)eve.gameState.trigramsManager.yinPower < valueOrDefault || (int)eve.gameState.trigramsManager.yangPower < valueOrDefault2;
		}
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.SpellCastResolve;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { yinPerGroup, yangPerGroup };
	}
}
