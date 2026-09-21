public class CheckCurrentContractType : CheckNode
{
	public ContractDef.ContractType type;

	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState == null)
		{
			return null;
		}
		return eve.gameState.contractState.selectedContract.contractDef.contractType == type;
	}
}
