public class CheckWinBattle : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		return eve.battleFinishType == FinishBattleEncounterAction.FinishType.Win;
	}
}
