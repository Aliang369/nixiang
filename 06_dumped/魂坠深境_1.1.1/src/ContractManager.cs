using System.Collections.Generic;
using System.Linq;

public class ContractManager : GlobalGameManager
{
	[SaveField(null, null)]
	public Contract defaultGameContract;

	[SaveField(null, null)]
	public List<ContractState> contractStates;

	[SaveField(null, null)]
	public List<string> activeWeekContractIDs;

	public List<ContractState> CurrentWeekContracts => contractStates.FindAll((ContractState s) => activeWeekContractIDs.Contains(s.contractDef.FullID));

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (defaultGameContract == null)
		{
			defaultGameContract = ScriptableObjectSingleton<FortressDataManager>.instance.defaultContractDef.GenerateContract(applicationState.assetCatalog, DifficultyLevel.Normal);
		}
		applicationState.assetCatalog.contractDefCatalog.ValidAssets.BuildToList(ref contractStates, (ContractDef c, ContractState cs) => cs.contractDef == c, (ContractDef c) => new ContractState(c));
		if (activeWeekContractIDs == null)
		{
			activeWeekContractIDs = ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentWeekShuffleContractGroup.contracts.Select((ContractDef c) => c.FullID).ToList();
		}
	}

	public ContractState GetContractState(ContractDef contractDef)
	{
		return contractStates.Find((ContractState s) => s.contractDef == contractDef);
	}

	public bool IsContractFinished(ContractDef contractDef)
	{
		return contractStates.Find((ContractState s) => s.contractDef == contractDef && s.finishedThisCycle) != null;
	}
}
