using System.Collections.Generic;

public class ChangeSkillCoolDown : Effect
{
	public SkillSelector skillSelector;

	public ValueGenerator valueGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<GameSkill> list = skillSelector?.GetTargetSkills(eve);
		int? num = valueGenerator?.GenerateValue(eve);
		if (list == null || !num.HasValue)
		{
			return NodeCastResult.Failure;
		}
		foreach (GameSkill item in list.OrEmptyIfNull())
		{
			Effect.EnqueueAction(eve, new ChangeSkillCooldownAction(item, num.Value));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { skillSelector, valueGenerator };
	}
}
