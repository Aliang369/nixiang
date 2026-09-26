using System;

public class zbLanguageConfig
{
	private static zbLanguageConfig sInstance;

	public static zbLanguageConfig Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbLanguageConfig();
			}
			return sInstance;
		}
	}

	public void Init()
	{
		TransferBarProbabilityCsvTable();
		TransferBoxCsvTable();
		TransferDefenseAttrCsvTable();
		TransferDefenseTypeCsvTable();
		TransferDirectShopCsvTable();
		TransferDungeonCsvTable();
		TransferDungeoneventCsvTable();
		TransferElementTrialCsvTable();
		TransferEquipCsvTable();
		TransferFunctionUnlockCsvTable();
		TransferGemCsvTable();
		TransferGeneCsvTable();
		TransferGetwayCsvTable();
		TransferGuideTalkCsvTable();
		TransferHandbookCsvTable();
		TransferHeroCsvTable();
		TransferHomeRoomTypeCsvTable();
		TransferItemCsvTable();
		TransferLadderCsvTable();
		TransferLegionBossChallengeCsvTable();
		TransferLegionUnlockCsvTable();
		TransferLegendaryEffectCsvTable();
		TransferFootholdEffectCsvTable();
		TransferMechaCsvTable();
		TransferMechaSkillCsvTable();
		TransferModCsvTable();
		TransferModHandbookCsvTable();
		TransferMoneyCsvTable();
		TransferMonsterAttrCsvTable();
		TransferMonsterCsvTable();
		TransferNpcDescCsvTable();
		TransferOperateCsvTable();
		TransferPeerageCsvTable();
		TransferPlayerHeadCsvTable();
		TransferPlayerHeadFrameCsvTable();
		TransferPlayerGunCsvTable();
		TransferPlayerSuitCsvTable();
		TransferPropCsvTable();
		TransferQualityCsvTable();
		TransferRestaurantTargetCsvTable();
		TransferSkillAttrCsvTable();
		TransferSkillCsvTable();
		TransferSkillTypeCsvTable();
		TransferTaskTypeCsvTable();
		TransferTeamAttrCsvTable();
		TransferSubscribeShopCsvTable();
		TransferVipCrossLadderCsvTable();
		TransferVipTruckCsvTable();
		TransferVipShopCsvTable();
		TransferWorldBuildingCsvTable();
		TransferWorldEventBoxCsvTable();
		TransferWorldEventFightCsvTable();
		TransferWorldBossCsvTable();
		TransferWallCsvTable();
		TransferWallSkillCsvTable();
		TransferFootholdCsvTable();
		TransferTreasureCsvTable();
		TransferTreasurePrivilegeCsvTable();
		TransferMineNodeCsvTable();
		TransferFacilityCsvTable();
		TransferBossSubjectCsvTable();
		TransferWallSubjectCsvTable();
		TransferWallSubjectRelicCsvTable();
		TransferMechaSubjectCsvTable();
		TransferMechaSubjectRelicCsvTable();
		TransferPushSetCsvTable();
		TransferStrongerCsvTable();
		TransferSkillBuffCsvTable();
		TransferSeasonAirdropCsvTable();
		TransferVipDescCsvTable();
	}

	private void TransferField(Func<string> get, Action<string> set, Func<string> getKey, Action<string> setKey)
	{
		string text = get();
		string text2 = getKey();
		if (string.IsNullOrEmpty(text2))
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			text2 = text;
			setKey(text2);
		}
		string obj = zbUtil.Language(text2);
		set(obj);
	}

	private void TransferBarProbabilityCsvTable()
	{
		zbBarProbabilityCsvRecord[] records = zbDataConfig.Instance.barProbabilityCsvTable.GetRecords().records;
		foreach (zbBarProbabilityCsvRecord record in records)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferBoxCsvTable()
	{
		foreach (zbBoxCsvRecord record in zbDataConfig.Instance.boxCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferDefenseAttrCsvTable()
	{
		foreach (zbDefenseAttrCsvRecord record in zbDataConfig.Instance.defenseAttrCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferDefenseTypeCsvTable()
	{
		foreach (zbDefenseTypeCsvRecord record in zbDataConfig.Instance.defenseTypeCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferDirectShopCsvTable()
	{
		foreach (zbDirectShopCsvRecord record in zbDataConfig.Instance.directShopCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferDungeonCsvTable()
	{
		foreach (zbDungeonCsvRecord record in zbDataConfig.Instance.dungeonCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferDungeoneventCsvTable()
	{
		foreach (zbDungeonEventCsvRecord record in zbDataConfig.Instance.dungeonEventCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferElementTrialCsvTable()
	{
		foreach (zbElementTrialCsvRecord record in zbDataConfig.Instance.elementTrialCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferEquipCsvTable()
	{
		foreach (zbEquipCsvRecord record in zbDataConfig.Instance.equipCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferFunctionUnlockCsvTable()
	{
		foreach (zbFunctionUnlockCsvRecord record in zbDataConfig.Instance.functionUnlockCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferGemCsvTable()
	{
		foreach (zbGemCsvRecord record in zbDataConfig.Instance.gemCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferGeneCsvTable()
	{
		foreach (zbGeneCsvRecord record in zbDataConfig.Instance.geneCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferGetwayCsvTable()
	{
		foreach (zbGetWayCsvRecord record in zbDataConfig.Instance.getWayCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferGuideTalkCsvTable()
	{
		zbGuideTalkCsvRecord[] records = zbDataConfig.Instance.guideTalkCsvTable.GetRecords().records;
		foreach (zbGuideTalkCsvRecord record in records)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferHandbookCsvTable()
	{
		foreach (zbHandbookCsvRecord record in zbDataConfig.Instance.handbookCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferHeroCsvTable()
	{
		foreach (zbHeroCsvRecord record in zbDataConfig.Instance.heroCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferHomeRoomTypeCsvTable()
	{
		foreach (zbHomeRoomTypeCsvRecord record in zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferItemCsvTable()
	{
		foreach (zbItemCsvRecord record in zbDataConfig.Instance.itemCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferLadderCsvTable()
	{
		foreach (zbLadderCsvRecord record in zbDataConfig.Instance.ladderCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferLegionBossChallengeCsvTable()
	{
		foreach (zbLegionBossChallengeCsvRecord record in zbDataConfig.Instance.legionBossChallengeCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferLegionUnlockCsvTable()
	{
		foreach (zbLegionUnlockCsvRecord record in zbDataConfig.Instance.legionUnlockCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferLegendaryEffectCsvTable()
	{
		foreach (zbLegendaryEffectCsvRecord record in zbDataConfig.Instance.legendaryEffectCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferFootholdEffectCsvTable()
	{
		foreach (zbFootholdEffectCsvRecord record in zbDataConfig.Instance.footholdEffectCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferMechaCsvTable()
	{
		foreach (zbMechaCsvRecord record in zbDataConfig.Instance.mechaCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferMechaSkillCsvTable()
	{
		foreach (zbMechaSkillCsvRecord record in zbDataConfig.Instance.mechaSkillCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
			TransferField(() => record.upgradeDesc, delegate(string val)
			{
				record.upgradeDesc = val;
			}, () => record.tempParam3, delegate(string val)
			{
				record.tempParam3 = val;
			});
		}
	}

	private void TransferModCsvTable()
	{
		foreach (zbModCsvRecord record in zbDataConfig.Instance.modCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferModHandbookCsvTable()
	{
		foreach (zbModHandbookCsvRecord record in zbDataConfig.Instance.modHandbookCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferMoneyCsvTable()
	{
		foreach (zbMoneyCsvRecord record in zbDataConfig.Instance.moneyCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferMonsterAttrCsvTable()
	{
		foreach (zbMonsterAttrCsvRecord record in zbDataConfig.Instance.monsterAttrCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferMonsterCsvTable()
	{
		foreach (zbMonsterCsvRecord record in zbDataConfig.Instance.monsterCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferNpcDescCsvTable()
	{
		foreach (zbNpcDescCsvRecord record in zbDataConfig.Instance.npcDescCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferOperateCsvTable()
	{
		foreach (zbOperateCsvRecord record in zbDataConfig.Instance.operateCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.shortName, delegate(string val)
			{
				record.shortName = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferPeerageCsvTable()
	{
		foreach (zbPeerageCsvRecord record in zbDataConfig.Instance.peerageCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferPlayerHeadCsvTable()
	{
		foreach (zbPlayerHeadCsvRecord record in zbDataConfig.Instance.playerHeadCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferPlayerHeadFrameCsvTable()
	{
		foreach (zbPlayerHeadFrameCsvRecord record in zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferPlayerGunCsvTable()
	{
		foreach (zbPlayerGunCsvRecord record in zbDataConfig.Instance.playerGunCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferPlayerSuitCsvTable()
	{
		foreach (zbPlayerSuitCsvRecord record in zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.skillName, delegate(string val)
			{
				record.skillName = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
			TransferField(() => record.skillDesc1, delegate(string val)
			{
				record.skillDesc1 = val;
			}, () => record.tempParam3, delegate(string val)
			{
				record.tempParam3 = val;
			});
			TransferField(() => record.skillDesc2, delegate(string val)
			{
				record.skillDesc2 = val;
			}, () => record.tempParam4, delegate(string val)
			{
				record.tempParam4 = val;
			});
			TransferField(() => record.skillDesc3, delegate(string val)
			{
				record.skillDesc3 = val;
			}, () => record.tempParam5, delegate(string val)
			{
				record.tempParam5 = val;
			});
			TransferField(() => record.skillDesc4, delegate(string val)
			{
				record.skillDesc4 = val;
			}, () => record.tempParam6, delegate(string val)
			{
				record.tempParam6 = val;
			});
			TransferField(() => record.skillDesc5, delegate(string val)
			{
				record.skillDesc5 = val;
			}, () => record.tempParam7, delegate(string val)
			{
				record.tempParam7 = val;
			});
		}
	}

	private void TransferPropCsvTable()
	{
		foreach (zbPropCsvRecord record in zbDataConfig.Instance.propCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferQualityCsvTable()
	{
		foreach (zbQualityCsvRecord record in zbDataConfig.Instance.qualityCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferRestaurantTargetCsvTable()
	{
		foreach (zbRestaurantTargetCsvRecord record in zbDataConfig.Instance.restaurantTargetCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferSkillAttrCsvTable()
	{
		foreach (zbSkillAttrCsvRecord record in zbDataConfig.Instance.skillAttrCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferSkillCsvTable()
	{
		foreach (zbSkillCsvRecord record in zbDataConfig.Instance.skillCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferSkillTypeCsvTable()
	{
		foreach (zbSkillTypeCsvRecord record in zbDataConfig.Instance.skillTypeCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferTaskTypeCsvTable()
	{
		foreach (zbTaskTypeCsvRecord record in zbDataConfig.Instance.taskTypeCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferTeamAttrCsvTable()
	{
		foreach (zbTeamAttrCsvRecord record in zbDataConfig.Instance.teamAttrCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferSubscribeShopCsvTable()
	{
		foreach (zbSubscribeShopCsvRecord record in zbDataConfig.Instance.subscribeShopCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferVipCrossLadderCsvTable()
	{
		foreach (zbVipCrossLadderCsvRecord record in zbDataConfig.Instance.vipCrossLadderCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferVipTruckCsvTable()
	{
		foreach (zbVipTruckCsvRecord record in zbDataConfig.Instance.vipTruckCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferVipShopCsvTable()
	{
		foreach (zbVipShopCsvRecord record in zbDataConfig.Instance.vipShopCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.goodsName, delegate(string val)
			{
				record.goodsName = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.goodsDesc, delegate(string val)
			{
				record.goodsDesc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferWorldBuildingCsvTable()
	{
		foreach (zbWorldBuildingCsvRecord record in zbDataConfig.Instance.worldBuildingCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferWorldEventBoxCsvTable()
	{
		foreach (zbWorldEventBoxCsvRecord record in zbDataConfig.Instance.worldEventBoxCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferWorldEventFightCsvTable()
	{
		foreach (zbWorldEventFightCsvRecord record in zbDataConfig.Instance.worldEventFightCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferWorldBossCsvTable()
	{
		foreach (zbWorldBossCsvRecord record in zbDataConfig.Instance.worldBossCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.levelName, delegate(string val)
			{
				record.levelName = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferWallCsvTable()
	{
		foreach (zbWallCsvRecord record in zbDataConfig.Instance.wallCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferWallSkillCsvTable()
	{
		foreach (zbWallSkillCsvRecord record in zbDataConfig.Instance.wallSkillCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferFootholdCsvTable()
	{
		foreach (zbFootholdCsvRecord record in zbDataConfig.Instance.footholdCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferTreasureCsvTable()
	{
		foreach (zbTreasureCsvRecord record in zbDataConfig.Instance.treasureCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferTreasurePrivilegeCsvTable()
	{
		foreach (zbTreasurePrivilegeCsvRecord record in zbDataConfig.Instance.treasurePrivilegeCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferMineNodeCsvTable()
	{
		foreach (zbMineNodeCsvRecord record in zbDataConfig.Instance.mineNodeCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferFacilityCsvTable()
	{
		foreach (zbFacilityCsvRecord record in zbDataConfig.Instance.facilityCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferBossSubjectCsvTable()
	{
		foreach (zbBossSubjectCsvRecord record in zbDataConfig.Instance.bossSubjectCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferWallSubjectCsvTable()
	{
		foreach (zbWallSubjectCsvRecord record in zbDataConfig.Instance.wallSubjectCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferWallSubjectRelicCsvTable()
	{
		zbWallSubjectRelicCsvRecord[] records = zbDataConfig.Instance.wallSubjectRelicCsvTable.GetRecords().records;
		foreach (zbWallSubjectRelicCsvRecord record in records)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferMechaSubjectCsvTable()
	{
		foreach (zbMechaSubjectCsvRecord record in zbDataConfig.Instance.mechaSubjectCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferMechaSubjectRelicCsvTable()
	{
		zbMechaSubjectRelicCsvRecord[] records = zbDataConfig.Instance.mechaSubjectRelicCsvTable.GetRecords().records;
		foreach (zbMechaSubjectRelicCsvRecord record in records)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferPushSetCsvTable()
	{
		zbPushSetCsvRecord[] records = zbDataConfig.Instance.pushSetCsvTable.GetRecords().records;
		foreach (zbPushSetCsvRecord record in records)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferStrongerCsvTable()
	{
		foreach (zbStrongerCsvRecord record in zbDataConfig.Instance.strongerCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam2, delegate(string val)
			{
				record.tempParam2 = val;
			});
		}
	}

	private void TransferSkillBuffCsvTable()
	{
		foreach (zbSkillBuffCsvRecord record in zbDataConfig.Instance.skillBuffCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferSeasonAirdropCsvTable()
	{
		foreach (zbSeasonAirdropCsvRecord record in zbDataConfig.Instance.seasonAirdropCsvTable.GetRecords().records.Values)
		{
			TransferField(() => record.name, delegate(string val)
			{
				record.name = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}

	private void TransferVipDescCsvTable()
	{
		zbViptdescCsvRecord[] records = zbDataConfig.Instance.viptdescCsvTable.GetRecords().records;
		foreach (zbViptdescCsvRecord record in records)
		{
			TransferField(() => record.desc, delegate(string val)
			{
				record.desc = val;
			}, () => record.tempParam1, delegate(string val)
			{
				record.tempParam1 = val;
			});
		}
	}
}
