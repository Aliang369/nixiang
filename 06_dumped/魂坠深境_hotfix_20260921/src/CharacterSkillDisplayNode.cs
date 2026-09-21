using System.Collections.Generic;
using System.Linq;

public class CharacterSkillDisplayNode : Effect
{
	public QualityLevel qualityLevel;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		nodeDisplayInfo.refSkills = new NodeDisplayInfo.Element<List<GameSkill>>(new List<GameSkill>());
		SkillDef skillDef = (eve.nodeTempValues.TryGetValue("character") as PlayerCharacterState)?.def?.innateSkill.FirstOrDefault();
		if (skillDef != null)
		{
			ProfileSkill profileSkill = new ProfileSkill(skillDef, 0);
			profileSkill.SetArchetypeIndex(qualityLevel);
			nodeDisplayInfo.refSkills.value.Add(new GameSkill(profileSkill));
		}
		return nodeDisplayInfo;
	}
}
