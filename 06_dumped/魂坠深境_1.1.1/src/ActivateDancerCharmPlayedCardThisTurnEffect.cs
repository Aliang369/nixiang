public sealed class ActivateDancerCharmPlayedCardThisTurnEffect : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		GameSkill casterSkill = eve.CasterSkill;
		if (casterSkill == null)
		{
			return NodeCastResult.Failure;
		}
		DancerCharmPlayedCardAbility.SetActive(casterSkill);
		return NodeCastResult.Success;
	}
}
