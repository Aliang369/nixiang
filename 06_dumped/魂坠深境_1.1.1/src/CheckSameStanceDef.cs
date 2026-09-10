using System.Collections.Generic;

public class CheckSameStanceDef : CheckNode
{
	public StanceDefSelector defASelector;

	public StanceDefSelector defBSelector;

	public override bool? Check(GameEvent eve)
	{
		return defASelector?.GenerateSingleStanceDef(eve) == defBSelector?.GenerateSingleStanceDef(eve);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { defASelector, defBSelector };
	}
}
