public class CheckBattleFinishType : CheckNode
{
	public FinishBattleEncounterAction.FinishType finishType;

	public override bool? Check(GameEvent eve)
	{
		return eve.battleFinishType == finishType;
	}
}
