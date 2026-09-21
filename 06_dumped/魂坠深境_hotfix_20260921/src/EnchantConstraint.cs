using System.Collections.Generic;

public class EnchantConstraint : ItemConstraint
{
	public EnchantDef enchant;

	public override bool? CheckItemAccept(Item item)
	{
		if (enchant == null)
		{
			TraceLog.Error("[EnchantConstraint] enchant == null");
			return null;
		}
		if (enchant.addConstraint == null)
		{
			return true;
		}
		return enchant.addConstraint.CheckItemAccept(item);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		if (enchant == null)
		{
			nodeDisplayInfo.refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>(null);
		}
		else
		{
			nodeDisplayInfo.refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>(new List<EnchantDef> { enchant });
		}
		return nodeDisplayInfo;
	}
}
