using System.Collections.Generic;

public class CheckItemSequenceExist : CheckNode
{
	public ItemTargetSelector sequenceItemSelector;

	public override bool? Check(GameEvent eve)
	{
		if (sequenceItemSelector == null)
		{
			return true;
		}
		return sequenceItemSelector.GetTargetItems(eve) != null;
	}

	public override List<IEffectNode> GetChildren()
	{
		return IEffectNode.ToList(sequenceItemSelector);
	}
}
