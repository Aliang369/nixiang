using System.Collections.Generic;

public class UpgradeItem : Effect
{
	public ItemTargetSelector targetSelector;

	public bool hint;

	public bool showChangedItems;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (targetSelector == null)
		{
			return NodeCastResult.Failure;
		}
		List<Item> targetItems = targetSelector.GetTargetItems(eve);
		List<Item> list = new List<Item>();
		foreach (Item item in targetItems)
		{
			if (item != null)
			{
				if (item.Upgradeable)
				{
					list.Add(item);
				}
				transformVFXEffect?.Play(eve, eve.CasterPivot, item);
				Effect.EnqueueAction(eve, new UpgradeItemAction(item, eve.CasterItem));
				hitEffect?.Play(eve, item);
				if (hint)
				{
					Effect.EnqueueAction(eve, new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_upgrade));
					Effect.EnqueueAction(eve, new HintItemAction(item));
				}
			}
		}
		if (showChangedItems && list.Count > 0)
		{
			Effect.EnqueueAction(eve, new NotifyItemModificationsAction(list));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (targetSelector != null)
		{
			list.Add(targetSelector);
		}
		return list;
	}
}
