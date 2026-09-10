using System;
using System.Collections.Generic;
using UnityEngine;

public class BlessCrystalUpgrade : CrystalUpgrade
{
	public InfluenceDef def;

	public int price;

	public override Sprite Icon => def.Icon;

	public override string Title => def.InfluenceName ?? "";

	protected override int BasePrice => price;

	public BlessCrystalUpgrade(InfluenceDef def)
	{
		this.def = def;
		price = def.InGameCrystalCost;
	}

	public override string GetDescription(GameStateInfo gameState, out TextDataGenerator generator)
	{
		return new Influence(def).GetInfluenceDescription(gameState.applicationState, out generator);
	}

	public override QualityLevel GetQuality()
	{
		return def.quality;
	}

	public override void Trigger(GameStateInfo gameState, Action successFallback)
	{
		gameState.actionManager.EnqueueAction(new ChangeCrystalAction(-GetFinalPrice(gameState, real: true)));
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			influences = new List<Influence>
			{
				new Influence(def)
			}
		}));
		gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			successFallback?.Invoke();
		}));
		ThinkingAnalyticsExtends.TrackBonusBless(gameState, def, "魂晶升级");
	}
}
