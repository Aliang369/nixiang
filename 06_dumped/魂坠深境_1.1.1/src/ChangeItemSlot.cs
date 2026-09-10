using System.Collections.Generic;

public class ChangeItemSlot : Effect
{
	public ValueGenerator countGenerator;

	public GameEvent.EventBFTType target;

	public bool temp;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		ItemContainer tableContainer = eve.GetEventBFT(target).tableContainer;
		int count = 1;
		if (countGenerator != null)
		{
			count = countGenerator.GenerateValue(eve).Value;
		}
		Effect.EnqueueAction(eve, new ChangeSlotCountAction(tableContainer, count, temp));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { countGenerator };
	}
}
