public sealed class AwakeningHealSpringEvent : HealSpringEvent
{
	protected override bool ShouldSetupActivatePotentialTalentChoice => true;

	protected override bool ShouldSetupActivateSkillChoice => false;
}
