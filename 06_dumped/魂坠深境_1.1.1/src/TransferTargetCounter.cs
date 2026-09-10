using UnityEngine;

public class TransferTargetCounter : Effect
{
	public bool allowOverflow;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (allowOverflow || (int)eve.CasterItem.maxCounter == 0)
		{
			ProtectedInt counter = eve.TargetItem.counter;
			ChangeCounterAction action = new ChangeCounterAction(-(int)counter, eve.TargetItem, eve.CasterItem);
			ChangeCounterAction action2 = new ChangeCounterAction(counter, eve.CasterItem, eve.CasterItem);
			transformVFXEffect?.Play(eve, eve.CasterPivot, eve.TargetItem);
			Effect.EnqueueAction(eve, action);
			Effect.EnqueueAction(eve, action2);
			hitEffect?.Play(eve, eve.TargetItem);
		}
		else
		{
			int num = Mathf.Min(eve.TargetItem.counter, (int)eve.CasterItem.maxCounter - (int)eve.CasterItem.counter);
			ChangeCounterAction action3 = new ChangeCounterAction(-num, eve.TargetItem, eve.CasterItem);
			ChangeCounterAction action4 = new ChangeCounterAction(num, eve.CasterItem, eve.CasterItem);
			transformVFXEffect?.Play(eve, eve.CasterPivot, eve.TargetItem);
			Effect.EnqueueAction(eve, action3);
			Effect.EnqueueAction(eve, action4);
			hitEffect?.Play(eve, eve.TargetItem);
		}
		return NodeCastResult.Success;
	}
}
