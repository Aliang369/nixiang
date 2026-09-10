public sealed class ActivateFramingThisTurnEffect : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		GameSkill casterSkill = eve.CasterSkill;
		if (casterSkill == null)
		{
			return NodeCastResult.Failure;
		}
		FramingSkillAbility.SetActive(casterSkill);
		return NodeCastResult.Success;
	}
}
