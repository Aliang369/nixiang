using UnityEngine;

public class GameStateBoot : BootSetter<GameStateInfo>
{
	[SerializeField]
	public BattleFieldTransformDisplay playerBft;

	[SerializeField]
	public GameUIManager uiManager;

	public GameStateInfo GameState => bootTarget;

	public override void Setup(GameStateInfo t)
	{
		base.Setup(t);
		t.playerBftDisplay = playerBft;
		t.UIManager = uiManager;
		t.sceneGo = base.gameObject;
	}
}
