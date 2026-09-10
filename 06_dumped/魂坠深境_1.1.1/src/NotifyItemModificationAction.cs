using UnityEngine;

public class NotifyItemModificationAction : GameAction
{
	private readonly Item item;

	private bool started;

	private float timer;

	private readonly float wait;

	public NotifyItemModificationAction(Item item, float wait = 1f)
	{
		this.item = item;
		this.wait = wait;
		timer = 0f;
		started = false;
	}

	protected override void Update()
	{
		if (!started)
		{
			gameState.UIManager.battleItemInfoPanel.Show(item, PivotManager.PivotType.PlayerTablePivot);
			started = true;
		}
		timer += Time.deltaTime * base.actionTimeScale;
		if (timer > wait)
		{
			gameState.UIManager.battleItemInfoPanel.Hide();
			FinishAction();
		}
	}
}
