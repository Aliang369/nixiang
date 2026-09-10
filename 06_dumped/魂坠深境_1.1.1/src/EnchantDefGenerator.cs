using System.Collections.Generic;

public abstract class EnchantDefGenerator : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>(GenerateEnchants(eve))
		};
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract List<EnchantDef> GenerateEnchants(GameEvent eve);
}
