using System;
using System.Collections.Generic;
using System.Linq;

public class ChangeSkill : Effect
{
	public SkillSelector skillSelector;

	public ChangeSkillAction.Type type;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<GameSkill> list = (from skill in skillSelector.GetTargetSkills(eve).OrEmptyIfNull()
			where skill != null
			select skill).Distinct().ToList();
		switch (type)
		{
		case ChangeSkillAction.Type.AddOutSlot:
			foreach (GameSkill item in list)
			{
				Effect.EnqueueAction(eve, new ChangeSkillAction(item, type));
			}
			break;
		case ChangeSkillAction.Type.Remove:
			foreach (GameSkill item2 in list)
			{
				Effect.EnqueueAction(eve, new ChangeSkillAction(item2, type));
			}
			break;
		case ChangeSkillAction.Type.AddInSlot:
			foreach (GameSkill item3 in list)
			{
				Effect.EnqueueAction(eve, new ChangeSkillAction(item3, type));
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { skillSelector };
	}
}
