using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ApplicationGlobalVariableManager : GlobalGameManager
{
	public static GlobalVariableSetup EnablePurchasePanel = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableHealSpringActiveSkill = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableSpecialBonusMechanic = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableRerollRune = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableGoldStoreReroll = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableBonusLocationGenerate = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableForceMintageRune = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableCrystalUpgradeReRoll = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableGoldStoreBuyLootItem = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableStartGameBuffSystem = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableSkillUpgrade = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableSkillBreakthrough = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableSkillDelete = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableSkillAddSlot = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableSkillMountGem = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableSkillGemGet = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableSkillGemCombine = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableForerunnerLegacy = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableUpgradeRoomSetSkin = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableRandomEnvironmentInfluenceAndLocationVariant = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableFortressForerunnerLegacyPanel = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableFortressStoreHousePanel = new GlobalVariableSetup();

	public static GlobalVariableSetup LimitedDeploymentFirekeeperConsumed = new GlobalVariableSetup();

	public static GlobalVariableSetup LimitedDeploymentFirekeeperViewed = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableGachaDrawPanel = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableRuneRoomDraftMapForkPath = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableDailyCheckIn = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableAchievementPanel = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableSeasonPanel = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableDailyQuestPanel = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableMailButton = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableLongGameStage = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableDifficultySelection = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableRootBonfireBlessRefresh = new GlobalVariableSetup();

	[SaveField(null, null)]
	public List<GlobalVariable> globalVariables;

	public static List<GlobalVariableSetup> GetAllGlobalVariableSetups()
	{
		List<GlobalVariableSetup> list = new List<GlobalVariableSetup>();
		foreach (FieldInfo item in from f in typeof(ApplicationGlobalVariableManager).GetFields(BindingFlags.Static | BindingFlags.Public)
			where f.FieldType == typeof(GlobalVariableSetup)
			select f)
		{
			if (item.GetValue(null) is GlobalVariableSetup globalVariableSetup)
			{
				globalVariableSetup.id = item.Name;
				list.Add(globalVariableSetup);
			}
		}
		return list;
	}

	public static void InitSetup()
	{
		foreach (FieldInfo item in from f in typeof(ApplicationGlobalVariableManager).GetFields(BindingFlags.Static | BindingFlags.Public)
			where f.FieldType == typeof(GlobalVariableSetup)
			select f)
		{
			(item.GetValue(null) as GlobalVariableSetup).id = item.Name;
		}
	}

	public void Reset()
	{
		globalVariables.Clear();
		foreach (GlobalVariableSetup allGlobalVariableSetup in GetAllGlobalVariableSetups())
		{
			globalVariables.Add(new GlobalVariable
			{
				id = allGlobalVariableSetup.id,
				value = allGlobalVariableSetup.initValue
			});
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		InitSetup();
		if (globalVariables == null)
		{
			globalVariables = new List<GlobalVariable>();
			Reset();
		}
	}

	public int GetVariableValue(string id)
	{
		return globalVariables.Find((GlobalVariable g) => g.id == id)?.value ?? 0;
	}

	public void SetVariableValue(string id, int value)
	{
		GlobalVariable globalVariable = globalVariables.Find((GlobalVariable g) => g.id == id);
		if (globalVariable != null)
		{
			globalVariable.value = value;
			return;
		}
		globalVariable = new GlobalVariable
		{
			id = id,
			value = value
		};
		globalVariables.Add(globalVariable);
	}

	public void SetVariableValue(GlobalVariableSetup setup, int value)
	{
		SetVariableValue(setup.id, value);
	}

	public int GetVariableValue(GlobalVariableSetup setup)
	{
		return GetVariableValue(setup.id);
	}

	public bool GetVariableFlag(GlobalVariableSetup setup)
	{
		return GetVariableValue(setup) > 0;
	}

	public bool GetVariableFlag(string id)
	{
		return GetVariableValue(id) > 0;
	}
}
