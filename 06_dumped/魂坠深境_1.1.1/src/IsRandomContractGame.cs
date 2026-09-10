public class IsRandomContractGame : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		return eve.gameState.contractState.selectedContract.contractDef != ScriptableObjectSingleton<FortressDataManager>.instance.defaultContractDef;
	}
}
