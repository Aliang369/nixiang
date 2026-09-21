using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NotifyItemModificationsAction : GameAction
{
	private readonly List<Item> items;

	private readonly float wait;

	private int currentIndex = -1;

	private float timer;

	private bool showing;

	public NotifyItemModificationsAction(IEnumerable<Item> items, float wait = 1f)
	{
		this.items = items?.Where((Item item) => item != null).Distinct().ToList() ?? new List<Item>();
		this.wait = wait;
	}

	protected override void Update()
	{
		if (!showing)
		{
			currentIndex++;
			if (currentIndex >= items.Count)
			{
				FinishAction();
				return;
			}
			gameState.UIManager.battleItemInfoPanel.Show(items[currentIndex], PivotManager.PivotType.PlayerTablePivot);
			timer = 0f;
			showing = true;
		}
		timer += Time.deltaTime * base.actionTimeScale;
		if (!(timer <= wait))
		{
			gameState.UIManager.battleItemInfoPanel.Hide();
			showing = false;
		}
	}
}
