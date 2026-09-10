using System;
using System.Collections.Generic;
using System.Linq;

public class CheckSkillState : CheckNode
{
	public enum SkillState
	{
		Activate,
		Consume,
		Prepare
	}

	public SkillState state;

	public SkillSelector skillSelector;

	public override bool? Check(GameEvent eve)
	{
		GameSkill singleSkill = skillSelector.GetSingleSkill(eve);
		if (singleSkill == null || eve.gameState == null)
		{
			return null;
		}
		return state switch
		{
			SkillState.Activate => eve.gameState.playerState.ActivatedSkills.Contains(singleSkill), 
			SkillState.Consume => eve.gameState.playerState.consumedSkills.Contains(singleSkill), 
			SkillState.Prepare => eve.gameState.playerState.PreparableSkills.Contains(singleSkill), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { skillSelector };
	}
}
