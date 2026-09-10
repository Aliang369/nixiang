using System.Collections.Generic;
using System.Linq;

public class WithCountCardAcceptConstract : PVPAvatarEditRuleCondition
{
	public ItemConstraint itemConstraint;

	public int toCompareCount;

	public ValueCompareCheckNode.CompareType compareType;

	public override bool IsMatch(DeckPreset preset)
	{
		return ValueCompareCheckNode.Compare(preset.GetAllExpandedComps().Count((DeckPresetComp c) => c.def.item != null && itemConstraint.CheckItemAccept(c.GenerateItem()) == true), toCompareCount, compareType);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(toCompareCount)
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemConstraint };
	}
}
