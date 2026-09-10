public class FSM<T>
{
	public FSMState<T> currentState;

	public void JumpTo(FSMState<T> state)
	{
		if (state != currentState && state != null)
		{
			currentState?.onExit?.Invoke(currentState);
			state.onEnter?.Invoke(state);
			currentState = state;
		}
	}

	public void Update()
	{
		currentState?.update?.Invoke(currentState);
	}
}
