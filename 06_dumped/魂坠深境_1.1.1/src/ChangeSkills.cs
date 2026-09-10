using System.Collections.Generic;

public class ChangeSkills : Effect
{
	public SkillSelector skillSelector;

	public ChangeSkillAction.Type type;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<GameSkill> list = skillSelector?.GetTargetSkills(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (GameSkill item in list.OrEmptyIfNull())
		{
			Effect.EnqueueAction(eve, new ChangeSkillAction(item, type));
		}
		return NodeCastResult.Success;
	}
}
