using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

public class CrystalUpgradeWorldEvent : WorldEvent
{
	public class CrystalUpgradeConfig
	{
		public ClavisDef clavis;

		public int getCrystalCountByLeave = 3;

		public int reRollPrice = 2;

		public int upgradeCount = 2;
	}

	[SerializeField]
	[LocalizeText("事件文本")]
	public string confirmText;

	[SerializeField]
	[LocalizeText("事件文本")]
	public string confirmDescriptionText;

	[SerializeField]
	[LocalizeText("事件文本")]
	public string leaveWithCrystalChoiceText;

	[SerializeField]
	[LocalizeText("事件文本")]
	public string leaveWithoutCrystalChoiceText;

	[NonSerialized]
	public List<CrystalUpgrade> allCrystalUpgrade;

	[NonSerialized]
	private bool reRolled;

	[NonSerialized]
	public List<CrystalUpgrade> upgraded;

	public CrystalUpgradeConfig crystalUpgradeConfig => ScriptableObjectSingleton<GameDataManager>.instance.crystalUpgradeConfig;

	public override WorldEventDisplayType displayType => WorldEventDisplayType.CrystalUpgrade;

	private bool FreeCrystalUpgradeBless
	{
		get
		{
			PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
			if (currentLocation != null)
			{
				return currentLocation.variantType == PathLocation.LocationVariantType.CrystalUpgradeFreeBless;
			}
			return false;
		}
	}

	public bool EndlessCrystalUpgrade
	{
		get
		{
			PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
			if (currentLocation != null)
			{
				return currentLocation.variantType == PathLocation.LocationVariantType.CrystalUpgradeEndlessRefreshAndGet;
			}
			return false;
		}
	}

	public override WorldEventPanel GetDisplayWorldEventPanel(GameStateInfo gameState)
	{
		return gameState.UIManager.worldEventPanel_CrystalUpgrade;
	}

	protected override void Init()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		base.Init();
		upgraded = new List<CrystalUpgrade>();
		reRolled = false;
		allCrystalUpgrade = GenerateCrystalUpgrades(gameState);
		Bootstrap.instance.audioManager.PlaySoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_crystal_upgrade);
	}

	public override void FinishedWorldEvent()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_crystal_upgrade);
		base.FinishedWorldEvent();
	}

	public int GetFinalLeaveCrystalCount()
	{
		int num = crystalUpgradeConfig.getCrystalCountByLeave;
		if (gameState != null)
		{
			num += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.leaveCrystalUpgradeCrystalBonusDelta);
		}
		return num;
	}

	public List<CrystalUpgrade> GenerateCrystalUpgrades(GameStateInfo gameState)
	{
		int upgradeCount = crystalUpgradeConfig.upgradeCount;
		upgradeCount += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.CrystalUpgradeBlessCountDelta);
		List<CrystalUpgrade> list = new List<CrystalUpgrade>();
		if (FreeCrystalUpgradeBless)
		{
			IEnumerable<BlessCrystalUpgrade> collection = from b in gameState.generationManager.GenerateRandomCommonBless(upgradeCount + 1)
				select new BlessCrystalUpgrade(b)
				{
					price = 0
				};
			list.AddRange(collection);
		}
		else
		{
			if (EndlessCrystalUpgrade)
			{
				upgradeCount++;
			}
			List<InfluenceDef> list2 = gameState.generationManager.GenerateRandomCommonBless(upgradeCount);
			list.AddRange(list2.ConvertAll((InfluenceDef b) => new BlessCrystalUpgrade(b)));
			if (EndlessCrystalUpgrade)
			{
				list.OfType<BlessCrystalUpgrade>().ForEach(delegate(BlessCrystalUpgrade u)
				{
					u.price += 2 * upgraded.Count;
				});
			}
			else
			{
				list.Add(new GetCrystalUpgrade(GetFinalLeaveCrystalCount()));
			}
		}
		foreach (CrystalUpgrade item in list)
		{
			if (item is BlessCrystalUpgrade blessCrystalUpgrade)
			{
				ThinkingAnalyticsExtends.TrackExposeBonusBless(gameState, blessCrystalUpgrade.def, "魂晶升级", blessCrystalUpgrade.CheckTriggerAble(gameState));
			}
		}
		return list;
	}

	public bool CheckReRollAble()
	{
		if ((int)gameState.playerState.crystalCount >= crystalUpgradeConfig.reRollPrice)
		{
			return true;
		}
		return false;
	}

	public bool ReRollEnabled()
	{
		if (!gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableCrystalUpgradeReRoll))
		{
			return false;
		}
		if (reRolled)
		{
			return EndlessCrystalUpgrade;
		}
		return true;
	}

	public void ReRollUpgrades(bool withCost = true)
	{
		allCrystalUpgrade = GenerateCrystalUpgrades(gameState);
		if (withCost)
		{
			EnqueueAction(new ChangeCrystalAction(-crystalUpgradeConfig.reRollPrice));
		}
		reRolled = true;
	}

	public void TriggerLeave()
	{
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableCrystalUpgradeLeaveGetCrystal))
		{
			ChangeCrystalAction action = new ChangeCrystalAction(GetFinalLeaveCrystalCount());
			gameState.actionManager.EnqueueAction(action);
		}
	}
}
