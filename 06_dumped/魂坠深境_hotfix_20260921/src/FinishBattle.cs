public class FinishBattle : Effect
{
	public enum Result
	{
		None,
		Win,
		Lose
	}

	public Result result;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		switch (result)
		{
		case Result.Win:
			Effect.EnqueueAction(eve, new FinishBattleEncounterAction(FinishBattleEncounterAction.FinishType.Win));
			break;
		case Result.Lose:
			Effect.EnqueueAction(eve, new FinishBattleEncounterAction(FinishBattleEncounterAction.FinishType.Lose));
			break;
		}
		return NodeCastResult.Success;
	}
}
