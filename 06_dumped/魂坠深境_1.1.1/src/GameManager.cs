using System;
using Generated.ChillyRoomSdkClient;

public abstract class GameManager : IDisposable
{
	public virtual bool Inited => true;

	public ApplicationStateInfo applicationState => Bootstrap.instance.CurrentApplicationState;

	public FortressStateInfo fortressState => applicationState.fortressState;

	public GameStateInfo gameState => applicationState.runningGameState;

	public TextDataPreset textData => ScriptableObjectSingleton<TextDataPreset>.instance;

	protected ChillyRoomService crSDK => ChillyRoomService.GetSdk();

	public virtual void Dispose()
	{
	}
}
