public class DowngradeItem : Effect
{
	public ItemTargetSelector targetSelector;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (targetSelector == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item targetItem in targetSelector.GetTargetItems(eve))
		{
			if (targetItem != null)
			{
				transformVFXEffect?.Play(eve, eve.CasterPivot, targetItem);
				Effect.EnqueueAction(eve, new DowngradeItemAction(targetItem, eve.CasterItem));
				hitEffect?.Play(eve, targetItem);
			}
		}
		return NodeCastResult.Success;
	}
}
