public class CheckSpellCooldownCastDependsOnlyOnCaster : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		Item item = eve.spellInfo?.spellItem;
		IGameEventSubscriber gameEventSubscriber = eve.GetEveObj(GameEvent.EveObjType.Caster) as IGameEventSubscriber;
		if (item == null || gameEventSubscriber == null || eve.gameState?.gameEventManager == null)
		{
			return false;
		}
		GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.TriggerItemCastAbleInCoolDownResolve(item, eve.triggerType);
		return resolveInfo.promoteSources.Count == 1 && resolveInfo.promoteSources.Contains(gameEventSubscriber);
	}
}
