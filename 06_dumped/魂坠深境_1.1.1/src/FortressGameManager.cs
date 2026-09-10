public abstract class FortressGameManager : GameManager
{
	public FortressUIManager uiManager => base.fortressState.uiManager;

	public abstract void Init(FortressStateInfo fortressState);
}
