using UnityEngine;

public class HintItemAction : GameAction
{
	private const float hintTime = 1f;

	private readonly Item item;

	private bool started;

	private float timer;

	public HintItemAction(Item item)
	{
		this.item = item;
	}

	protected override void Update()
	{
		if (!started)
		{
			gameState.UIManager.battleItemInfoPanel.Show(item, PivotManager.PivotType.PlayerTablePivot);
			started = true;
		}
		timer += Time.deltaTime;
		if (timer * base.actionTimeScale > 1f)
		{
			gameState.UIManager.battleItemInfoPanel.Hide();
			FinishAction();
		}
	}
}
