using System.Collections.Generic;
using System.Linq;

public class WithBlessRuleCondition : PVPAvatarEditRuleCondition
{
	public InfluenceDef influenceDef;

	public override bool IsMatch(DeckPreset preset)
	{
		return preset.comps.Any((DeckPresetComp c) => c.def.influence == influenceDef);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refInfluences = new NodeDisplayInfo.Element<List<Influence>>(new List<Influence>
			{
				new Influence(influenceDef)
			})
		};
	}
}
