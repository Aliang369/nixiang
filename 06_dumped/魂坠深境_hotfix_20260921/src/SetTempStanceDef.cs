using System.Collections.Generic;

public class SetTempStanceDef : SetTempKey
{
	public StanceDefSelector stanceDefSelector;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<StanceDef> list = stanceDefSelector.GenerateStanceDefs(eve);
		if (list.IsNullOrEmpty())
		{
			return NodeCastResult.Failure;
		}
		if (!SetNormalTempStanceDefs(eve, list))
		{
			return NodeCastResult.Failure;
		}
		return NodeCastResult.Success;
	}

	private bool SetNormalTempStanceDefs(GameEvent eve, List<StanceDef> stanceDefs)
	{
		TempValue.GetDictionary(eve, type)[key] = stanceDefs;
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { stanceDefSelector };
	}
}
