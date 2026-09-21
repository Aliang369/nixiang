using UnityEngine.UI;

public class MainUISwitchButton : GameButton
{
	public MainUISwitchGroup group;

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		if (state == SelectionState.Normal && group?.currentLayerSetups?.button == this)
		{
			base.DoStateTransition(SelectionState.Selected, instant);
		}
		else
		{
			base.DoStateTransition(state, instant);
		}
	}

	public void StateTransition(ButtonSelectionState state, bool instant)
	{
		base.DoStateTransition((SelectionState)state, instant);
	}
}
