using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TestEnCounterSelector : MonoBehaviour
{
	[SerializeField]
	private Dropdown dropDown;

	[SerializeField]
	private InputField filterTextField;

	[SerializeField]
	private InputField progressSetter;

	private List<TestMenuSelectEncounterOption> allEncounterInfos;

	private void Awake()
	{
		allEncounterInfos = new List<TestMenuSelectEncounterOption>();
		dropDown.onValueChanged.AddListener(_OnSelectorValueChange);
		filterTextField.onValueChanged.AddListener(_OnInputFieldChange);
		progressSetter.onValueChanged.AddListener(_OnProgressValueChange);
		progressSetter.characterValidation = InputField.CharacterValidation.Integer;
		MenuController.OnLoadEnd = (Action)Delegate.Remove(MenuController.OnLoadEnd, new Action(_OnLoadEnd));
		MenuController.OnLoadEnd = (Action)Delegate.Combine(MenuController.OnLoadEnd, new Action(_OnLoadEnd));
	}

	private void OnDestroy()
	{
		MenuController.OnLoadEnd = (Action)Delegate.Remove(MenuController.OnLoadEnd, new Action(_OnLoadEnd));
	}

	private void RefreshList()
	{
		string text = filterTextField.text;
		List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
		foreach (TestMenuSelectEncounterOption allEncounterInfo in allEncounterInfos)
		{
			if (allEncounterInfo.DisplayId.Contains(text))
			{
				list.Add(new Dropdown.OptionData(allEncounterInfo.DisplayId));
			}
		}
		dropDown.options = list;
		string currentId = allEncounterInfos.Find((TestMenuSelectEncounterOption en) => en.DisplayId == TestMenuManager.instance.TestMenuSelectEncounterOption.DisplayId).DisplayId;
		dropDown.value = dropDown.options.FindIndex((Dropdown.OptionData i) => i.text == currentId);
	}

	private void _OnLoadEnd()
	{
		foreach (EnemyInfoData asset in TestMenuManager.instance.testAssetCatalog.enemyInfoDataCatalog.assets)
		{
			foreach (CharacterAIComponent.EnemySpawnSetup setup in asset.setups)
			{
				foreach (EnemyEncounterType allowEncounterType in setup.difficultyInfo.allowEncounterTypes)
				{
					allEncounterInfos.Add(new TestMenuSelectEncounterOption
					{
						enemyInfoData = asset,
						enemySetup = setup,
						enemyEncounterType = allowEncounterType
					});
				}
			}
		}
		AddLocationEventOptions();
		foreach (WorldEvent asset2 in TestMenuManager.instance.testAssetCatalog.worldEventCatalog.assets)
		{
			TestMenuSelectEncounterOption item = new TestMenuSelectEncounterOption
			{
				worldEvent = asset2
			};
			allEncounterInfos.Add(item);
		}
		RefreshList();
		progressSetter.text = TestMenuManager.instance.EnemyLevel.ToString();
	}

	private void AddLocationEventOptions()
	{
		foreach (PathLocation.LocationType item in from t in (from g in DungeonGenerationManager.GetAllLocationGeneratorInCurrentAssembly()
				select g.LocationType).Where(ShouldShowLocationEvent).Distinct()
			orderby (int)t
			select t)
		{
			allEncounterInfos.Add(new TestMenuSelectEncounterOption
			{
				useLocationEvent = true,
				locationType = item,
				locationVariantType = PathLocation.LocationVariantType.None
			});
			foreach (PathLocation.LocationVariantType enumValue in CollectionExtend.GetEnumValues<PathLocation.LocationVariantType>())
			{
				if (enumValue != PathLocation.LocationVariantType.None && PathLocation.GetLocationVariantSourceLocation(enumValue) == item)
				{
					allEncounterInfos.Add(new TestMenuSelectEncounterOption
					{
						useLocationEvent = true,
						locationType = item,
						locationVariantType = enumValue
					});
				}
			}
		}
	}

	private static bool ShouldShowLocationEvent(PathLocation.LocationType locationType)
	{
		if (locationType != PathLocation.LocationType.HealFire && locationType != PathLocation.LocationType.GoldStore && locationType != PathLocation.LocationType.UpgradeRoom && locationType != PathLocation.LocationType.AttachRune && locationType != PathLocation.LocationType.CrystalUpgradeRoom)
		{
			return locationType == PathLocation.LocationType.BonusGame;
		}
		return true;
	}

	private void _OnSelectorValueChange(int i)
	{
		string id = dropDown.options[i].text;
		TestMenuSelectEncounterOption testMenuSelectEncounterOption = allEncounterInfos.Find((TestMenuSelectEncounterOption t) => t.DisplayId == id);
		TestMenuManager.instance.TestMenuSelectEncounterOption = testMenuSelectEncounterOption;
	}

	private void _OnInputFieldChange(string s)
	{
		RefreshList();
	}

	private void _OnProgressValueChange(string s)
	{
		int enemyLevel = Convert.ToInt32(s);
		TestMenuManager.instance.EnemyLevel = enemyLevel;
	}
}
