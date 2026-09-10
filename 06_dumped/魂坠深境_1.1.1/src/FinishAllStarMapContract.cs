using System.Linq;

public class FinishAllStarMapContract : GlobalTriggerObserverCondition
{
	public Constellation constellation;

	public override bool Check(ApplicationStateInfo applicationState)
	{
		foreach (ContractDef item in constellation.ChildrenStars.Select((ChallengeStar s) => s.presetContractDef))
		{
			if (!applicationState.contractManager.IsContractFinished(item))
			{
				return false;
			}
		}
		return true;
	}
}
