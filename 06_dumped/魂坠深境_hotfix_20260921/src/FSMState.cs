using System;

public class FSMState<T>
{
	public T data;

	public FSM<T> fsm;

	public Action<FSMState<T>> onEnter;

	public Action<FSMState<T>> onExit;

	public Action<FSMState<T>> update;

	public FSMState(FSM<T> fsm, T data)
	{
		this.fsm = fsm;
		this.data = data;
	}
}
