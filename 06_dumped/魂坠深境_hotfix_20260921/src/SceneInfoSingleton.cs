using UnityEngine;

public class SceneInfoSingleton : MonoBehaviour
{
	private GameStateInfo _gameState;

	public static SceneInfoSingleton instance;

	public ActionManager actionManager => gameState?.actionManager;

	public GameStateInfo gameState
	{
		get
		{
			if (_gameState == null)
			{
				_gameState = BootSetter<GameStateInfo>.GetCurrentBootTarget();
			}
			return _gameState;
		}
	}

	public GameUIManager UIManager => gameState.UIManager;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Object.Destroy(this);
		}
	}
}
