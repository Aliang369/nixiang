using System;
using System.Collections.Generic;
using UnityEngine;

public class BossBlessUpgrade : AfterBossUpgrade
{
	public Influence inf;

	public override Sprite Icon => inf.def.Icon;

	public override string Title => inf.def.InfluenceName;

	public override int WillPowerCost => inf.willPowerCost;

	public BossBlessUpgrade(Influence inf)
	{
		this.inf = inf;
	}

	public override string GetDescription(GameStateInfo gameState, out TextDataGenerator generator)
	{
		return inf.GetInfluenceDescription(gameState.applicationState, out generator);
	}

	public override QualityLevel GetQuality()
	{
		return inf.def.quality;
	}

	public override void Trigger(GameStateInfo gameState, Action successFallback)
	{
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			influences = new List<Influence> { inf }
		}));
		gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			successFallback?.Invoke();
		}));
	}
}
