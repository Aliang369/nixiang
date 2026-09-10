using System.Collections.Generic;

public abstract class RuneGenerator : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refRunes = new NodeDisplayInfo.Element<List<Rune>>(GenerateRunes(eve))
		};
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract List<Rune> GenerateRunes(GameEvent eve);
}
