using System.Collections.Generic;

public class CheckSkillDef : CheckNode
{
	public SkillSelector skillSelector;

	public SkillDef skillDef;

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = base.GetNodeDisplayInfo(eve);
		nodeDisplayInfo.refSkills = new NodeDisplayInfo.Element<List<GameSkill>>((skillDef == null) ? null : new List<GameSkill>
		{
			new GameSkill(new ProfileSkill(skillDef, 0))
		});
		return nodeDisplayInfo;
	}

	public override bool? Check(GameEvent eve)
	{
		GameSkill gameSkill = skillSelector?.GetSingleSkill(eve);
		if (gameSkill == null || skillDef == null)
		{
			return null;
		}
		return gameSkill.skillDef == skillDef;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { skillSelector };
	}
}
