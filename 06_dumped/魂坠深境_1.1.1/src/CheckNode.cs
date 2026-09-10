using System.Collections.Generic;

public abstract class CheckNode : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			outputBool = new NodeDisplayInfo.Element<bool?>(Check(eve))
		};
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract bool? Check(GameEvent eve);
}
