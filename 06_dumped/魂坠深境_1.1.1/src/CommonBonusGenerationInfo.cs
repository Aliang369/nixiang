using System.Collections.Generic;
using UnityEngine;

public class CommonBonusGenerationInfo : BonusGenerationInfo
{
	public enum GenerateType
	{
		Coin,
		Crystal,
		Relic,
		RareRelic,
		Loot
	}

	public int baseValue;

	public int stageAddValue;

	public GenerateType type;

	private bool DontDisplayCounter()
	{
		if (type == GenerateType.Relic || type == GenerateType.RareRelic || type == GenerateType.Loot)
		{
			return true;
		}
		return false;
	}

	private int GetFinalValue(GameStateInfo gameState)
	{
		int num = baseValue + (gameState.contractState.stage - 1) * stageAddValue;
		if (type == GenerateType.Relic)
		{
			num = 3;
		}
		if (type == GenerateType.RareRelic)
		{
			num = 3;
		}
		if (type == GenerateType.Relic || type == GenerateType.RareRelic)
		{
			num += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.relicBonusCountDelta);
		}
		if (type == GenerateType.Crystal || type == GenerateType.Coin)
		{
			num = Mathf.Max(num, 0);
		}
		return num;
	}

	public override List<BattleBonusObject> GenerateBonusObject(GameStateInfo gameState)
	{
		BattleBonusObject item = null;
		switch (type)
		{
		case GenerateType.Coin:
			if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.RejectCoinBonusAfterBattle))
			{
				return new List<BattleBonusObject>();
			}
			item = new CoinBonus(GetFinalValue(gameState));
			break;
		case GenerateType.Crystal:
			if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.RejectCrystalBonusAfterBattle))
			{
				return new List<BattleBonusObject>();
			}
			item = new CrystalBonus(GetFinalValue(gameState));
			break;
		case GenerateType.Relic:
			item = new GenerateCardsBonus(gameState, CardRewardType.AuxiliaryNormal);
			break;
		case GenerateType.RareRelic:
			item = new GenerateCardsBonus(gameState, CardRewardType.GuaranteedRare);
			break;
		case GenerateType.Loot:
			item = new LootBonus(gameState);
			break;
		}
		return new List<BattleBonusObject> { item };
	}
}
