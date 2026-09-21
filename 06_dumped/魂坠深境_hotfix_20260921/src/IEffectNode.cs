using System.Collections.Generic;

public interface IEffectNode : ITextDisplay, IChildren<IEffectNode>
{
	static List<IEffectNode> ToList(params IEffectNode[] nodes)
	{
		List<IEffectNode> list = new List<IEffectNode>();
		foreach (IEffectNode effectNode in nodes)
		{
			if (effectNode != null)
			{
				list.Add(effectNode);
			}
		}
		return list;
	}
}
