using System.Collections;
using UnityEngine;

public sealed class BlockingWorldEventAction : GameAction
{
	private readonly WorldEvent worldEventTemplate;

	private readonly bool showUpgradeView;

	private WorldEvent runtimeWorldEvent;

	private WorldEventPanel panel;

	private WorldEventPanel previousEventPanel;

	private bool started;

	private bool cleanupQueued;

	public BlockingWorldEventAction(WorldEvent worldEventTemplate, bool showUpgradeView = false)
	{
		this.worldEventTemplate = worldEventTemplate;
		this.showUpgradeView = showUpgradeView;
	}

	protected override void Update()
	{
		if (!started)
		{
			StartWorldEvent();
		}
		else if (runtimeWorldEvent == null)
		{
			FinishAction();
		}
		else if (runtimeWorldEvent.finishing)
		{
			if (!cleanupQueued)
			{
				cleanupQueued = true;
				EnqueueAction(new EnumeratorAction(CleanupWorldEvent));
			}
			FinishAction();
		}
	}

	private void StartWorldEvent()
	{
		started = true;
		if (worldEventTemplate == null)
		{
			FinishAction();
			return;
		}
		previousEventPanel = gameState.UIManager.currentEventPanel;
		runtimeWorldEvent = Object.Instantiate(worldEventTemplate);
		runtimeWorldEvent.SetupAndShow(gameState);
		panel = runtimeWorldEvent.GetDisplayWorldEventPanel(gameState);
		if (panel == null)
		{
			TraceLog.Error("阻塞事件没有可用面板: " + worldEventTemplate.name);
			Object.Destroy(runtimeWorldEvent);
			runtimeWorldEvent = null;
			FinishAction();
		}
		else
		{
			gameState.StartCoroutine(panel.ShowPanel);
			if (showUpgradeView && panel is AfterBossUpgradeEventPanel afterBossUpgradeEventPanel)
			{
				afterBossUpgradeEventPanel.ToBossUpgrade();
			}
		}
	}

	private IEnumerator CleanupWorldEvent()
	{
		yield return new WaitUntil(() => runtimeWorldEvent == null || runtimeWorldEvent.finished);
		if (panel != null)
		{
			yield return panel.HidePanel();
			panel.Unset();
			if (gameState.UIManager.currentEventPanel == panel)
			{
				gameState.UIManager.currentEventPanel = previousEventPanel;
			}
		}
		if (runtimeWorldEvent != null)
		{
			Object.Destroy(runtimeWorldEvent);
		}
	}
}
