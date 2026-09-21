using System;
using UnityEngine;

public abstract class CrystalUpgrade
{
	protected abstract int BasePrice { get; }

	public abstract Sprite Icon { get; }

	public abstract string Title { get; }

	public abstract string GetDescription(GameStateInfo gameState, out TextDataGenerator generator);

	public abstract void Trigger(GameStateInfo gameState, Action successFallback);

	public abstract QualityLevel GetQuality();

	public virtual bool CheckTriggerAble(GameStateInfo gameState)
	{
		return (int)gameState.playerState.crystalCount >= GetFinalPrice(gameState, real: false);
	}

	public virtual int GetFinalPrice(GameStateInfo gameState, bool real)
	{
		int basePrice = BasePrice;
		basePrice += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.CrystalUpgradePriceDelta);
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerCrystalUpdatePriceResolveInfo((!real) ? GameEvent.TriggerType.OutBattleDisplay : GameEvent.TriggerType.Real);
		if (resolveInfo.reject)
		{
			return 0;
		}
		basePrice = (int)((float)basePrice * ((100f + (float)resolveInfo.targetDeltaPercent) / 100f) + (float)resolveInfo.targetDelta);
		basePrice = ((basePrice > 0 && basePrice <= 1) ? 1 : Mathf.FloorToInt(basePrice));
		basePrice = Mathf.Min(basePrice, resolveInfo.upperBoundary);
		basePrice = Mathf.Max(basePrice, resolveInfo.lowerBoundary);
		return Mathf.Clamp(basePrice, 0, int.MaxValue);
	}

	public virtual string GetUpgradeNameText(GameStateInfo gameState)
	{
		return Title.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(GetQuality()));
	}
}
