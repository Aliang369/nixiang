public class CanRunIdolFirstWorshipGuide : GlobalTriggerObserverCondition
{
	public CharacterClassType targetCharacterClass = CharacterClassType.Warrior;

	public ClavisDef requiredClavis;

	public override bool Check(ApplicationStateInfo applicationState)
	{
		if (applicationState?.fortressState == null)
		{
			return false;
		}
		if (requiredClavis != null && !applicationState.clavisManager.IsUnlocked(requiredClavis))
		{
			return false;
		}
		IdolState idolState = applicationState.idolManager.GetIdolState(targetCharacterClass);
		if (idolState != null && idolState.currentLevel == 0)
		{
			return idolState.CanUpgrade(applicationState);
		}
		return false;
	}
}
