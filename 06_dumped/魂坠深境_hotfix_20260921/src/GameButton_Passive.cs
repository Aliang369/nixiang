using UnityEngine.UI;

public class GameButton_Passive : GameButton
{
	protected override void DoStateTransition(SelectionState state, bool instant)
	{
	}

	public void StateTransition(ButtonSelectionState state, bool instant)
	{
		base.DoStateTransition((SelectionState)state, instant);
	}
}
