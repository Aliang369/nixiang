using System;
using UnityEngine;

public abstract class AfterBossUpgrade
{
	public abstract int WillPowerCost { get; }

	public abstract Sprite Icon { get; }

	public abstract string Title { get; }

	public abstract string GetDescription(GameStateInfo gameState, out TextDataGenerator generator);

	public abstract void Trigger(GameStateInfo gameState, Action successFallback = null);

	public abstract QualityLevel GetQuality();

	public virtual bool CheckTriggerAble(GameStateInfo gameState)
	{
		return true;
	}

	public virtual string GetUpgradeNameText(GameStateInfo gameState)
	{
		return Title.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(GetQuality()));
	}
}
