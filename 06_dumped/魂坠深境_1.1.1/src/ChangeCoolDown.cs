using System.Collections.Generic;

public class ChangeCoolDown : Effect
{
	public ItemTargetSelector targetSelector;

	public ValueGenerator deltaGenerator;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> targetItems = targetSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return NodeCastResult.Success;
		}
		foreach (Item item in targetItems)
		{
			if (item != null && item.HasSpell)
			{
				int num = ((deltaGenerator == null) ? 1 : deltaGenerator.GenerateValue(eve).Value);
				if (eve.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.ReverseCoolDown))
				{
					num *= -1;
				}
				transformVFXEffect?.Play(eve, eve.CasterPivot, item);
				Effect.EnqueueAction(eve, new ChangeCoolDownAction(item, eve.CasterItem, num));
				hitEffect?.Play(eve, item);
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, deltaGenerator };
	}
}
