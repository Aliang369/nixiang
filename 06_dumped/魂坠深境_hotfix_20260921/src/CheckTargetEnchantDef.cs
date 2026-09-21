using System.Collections.Generic;
using UnityEngine;

public class CheckTargetEnchantDef : CheckNode
{
	[SerializeField]
	private EnchantDef def;

	public override bool? Check(GameEvent eve)
	{
		if (eve.TargetEnchant == null)
		{
			return null;
		}
		return eve.TargetEnchant.def == def;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = base.GetNodeDisplayInfo(eve);
		nodeDisplayInfo.refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>(new List<EnchantDef> { def });
		return nodeDisplayInfo;
	}
}
