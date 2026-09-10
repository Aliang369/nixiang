public class FinishWorldEventAfterChoice : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		eve.TargetWorldEvent.RefreshAfterChoose(eve.worldEventChoiceSetup.choiceText.chosenDescription);
		return NodeCastResult.Success;
	}
}
