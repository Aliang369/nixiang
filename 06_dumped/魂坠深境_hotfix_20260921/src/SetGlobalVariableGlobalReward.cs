using System.Collections.Generic;

public class SetGlobalVariableGlobalReward : GlobalRewardSetup
{
	[SaveField(null, null)]
	public string globalVariable;

	public override bool isHidden => true;

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			setGlobalVariableFlag = globalVariable
		};
	}

	private static List<string> GetAllGlobalVariableIds()
	{
		return ApplicationGlobalVariableManager.GetAllGlobalVariableSetups().ConvertAll((GlobalVariableSetup g) => g.id);
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(globalVariable);
	}
}
