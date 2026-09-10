using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContractDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public class DifficultyReward
	{
		public DifficultyLevel difficultyLevel;

		public List<GlobalRewardSetup> rewardSetups;
	}

	public enum ContractType
	{
		BaseGame = 0,
		Challenge = 1,
		HardChallenge = 2,
		WeekShuffle = 3,
		Story = 4,
		PVPOffender = 5,
		Intro = 10001,
		PVPEnemyTest = 10002,
		CharacterTrial = 6
	}

	[LocalizeText("挑战契约名字")]
	[CTSOAssetName]
	public string Name;

	[LocalizeText("挑战契约描述")]
	[TextArea]
	public string description;

	public Sprite icon;

	public ContractType contractType;

	public ContractPresentationDef presentation;

	public FixedContractRouteDef fixedContractRoute;

	public ContractRuleDef contractRule;

	public List<ItemDefinition> initialDeckItems = new List<ItemDefinition>();

	public List<DifficultyReward> difficultyRewardOverrides;

	public List<GlobalRewardSetup> finishRewards;

	public List<Ability> abilities;

	public override string CTAssetIDPrepend => "CONTRA";

	public DifficultyLevel? FixedDifficulty => contractRule?.FixedDifficulty;

	public bool IsDifficultyIsolated => contractRule?.IsDifficultyIsolated ?? false;

	public List<DifficultyReward> GetAllDifficultyRewards(AssetCatalog catalog)
	{
		List<DifficultyReward> list = new List<DifficultyReward>();
		foreach (DifficultyInfo item in from d in catalog.difficultyCatalog.ValidAssets
			where d.difficultyLevel >= DifficultyLevel.Normal && CheckDifficultyLevelAble(d)
			orderby d.difficultyLevel
			select d)
		{
			list.Add(new DifficultyReward
			{
				difficultyLevel = item.difficultyLevel,
				rewardSetups = GetDifficultyReward(item)
			});
		}
		return list;
	}

	public BattleRewardRule GetBattleRewardRule(GameStateInfo gameState)
	{
		return contractRule?.GetBattleRewardRule(gameState);
	}

	public IEnumerable<Item> GenerateInitialDeckItems()
	{
		return from itemDefinition in initialDeckItems.OrEmptyIfNull()
			where itemDefinition != null
			select new Item(itemDefinition);
	}

	public void ApplyEnemyGeneratedItemInitialState(GameStateInfo gameState, EnemyInfoData enemyInfoData, EnemyEncounterType encounterType, Item item)
	{
		contractRule?.ApplyEnemyGeneratedItemInitialState(gameState, enemyInfoData, encounterType, item);
	}

	public bool IsEnemyInitialItemOutsideIntentLimit(GameStateInfo gameState, EnemyInfoData enemyInfoData, EnemyEncounterType encounterType, Item item)
	{
		return contractRule?.IsEnemyInitialItemOutsideIntentLimit(gameState, enemyInfoData, encounterType, item) ?? false;
	}

	public bool CanGenerateLoot(GameStateInfo gameState, ItemDefinition itemDefinition)
	{
		return contractRule?.CanGenerateLoot(gameState, itemDefinition) ?? true;
	}

	public bool CanGenerateRune(GameStateInfo gameState, RuneDef runeDef)
	{
		return contractRule?.CanGenerateRune(gameState, runeDef) ?? true;
	}

	public void ModifyStoreItemsForBuy(GameStateInfo gameState, List<StoreEvent.PricedItem> itemsForSell)
	{
		contractRule?.ModifyStoreItemsForBuy(gameState, itemsForSell);
	}

	public void NotifyStoreItemBought(GameStateInfo gameState, Item item)
	{
		contractRule?.NotifyStoreItemBought(gameState, item);
	}

	public List<GlobalRewardSetup> GetDifficultyReward(DifficultyInfo difficultyInfo)
	{
		DifficultyReward difficultyReward = difficultyRewardOverrides?.Find((DifficultyReward r) => r.difficultyLevel == difficultyInfo.difficultyLevel);
		if (difficultyReward != null)
		{
			return difficultyReward.rewardSetups;
		}
		return difficultyInfo.GetFirstFinishReward(contractType);
	}

	public bool CheckDifficultyLevelAble(DifficultyInfo difficultyInfo)
	{
		return difficultyInfo.contractTypes.Contains(contractType);
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (abilities == null)
		{
			return list;
		}
		foreach (Ability ability in abilities)
		{
			list.Add(ability);
		}
		return list;
	}

	public Contract GenerateContract(AssetCatalog assetCatalog, DifficultyLevel difficulty)
	{
		return new Contract
		{
			contractDef = this,
			difficultyLevel = (FixedDifficulty ?? difficulty)
		};
	}

	public string GetContractDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatContractDescription(GenerateContract(applicationState.assetCatalog, DifficultyLevel.Normal), applicationState, out generator);
	}

	public List<GlobalRewardSetup> GetFirstFinishRewardInDifficulty(ApplicationStateInfo applicationState, DifficultyLevel difficultyLevel)
	{
		DifficultyInfo difficultyInfo = DifficultyInfo.GetDifficultyInfo(difficultyLevel, applicationState.assetCatalog);
		if (contractType != ContractType.BaseGame)
		{
			return difficultyInfo.GetFirstFinishReward(contractType).OrEmptyIfNull().ToList();
		}
		List<GlobalRewardSetup> list = new List<GlobalRewardSetup>();
		list.AddRange(difficultyInfo.GetBaseGameAccountFirstFinishReward().OrEmptyIfNull());
		list.AddRange(difficultyInfo.GetBaseGameClassFirstFinishReward().OrEmptyIfNull());
		return list;
	}

	public List<GlobalRewardSetup> GetAccountFirstFinishRewardInDifficulty(ApplicationStateInfo applicationState, DifficultyLevel difficultyLevel)
	{
		DifficultyInfo difficultyInfo = DifficultyInfo.GetDifficultyInfo(difficultyLevel, applicationState.assetCatalog);
		if (contractType != ContractType.BaseGame)
		{
			return new List<GlobalRewardSetup>();
		}
		return difficultyInfo.GetBaseGameAccountFirstFinishReward().OrEmptyIfNull().ToList();
	}

	public List<GlobalRewardSetup> GetClassFirstFinishRewardInDifficulty(ApplicationStateInfo applicationState, DifficultyLevel difficultyLevel)
	{
		DifficultyInfo difficultyInfo = DifficultyInfo.GetDifficultyInfo(difficultyLevel, applicationState.assetCatalog);
		if (contractType != ContractType.BaseGame)
		{
			return difficultyInfo.GetFirstFinishReward(contractType).OrEmptyIfNull().ToList();
		}
		return difficultyInfo.GetBaseGameClassFirstFinishReward().OrEmptyIfNull().ToList();
	}
}
