using System;
using System.Collections.Generic;
using GameTest;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TestMenuManager : SerializedMonoBehaviour
{
	[Serializable]
	private class DeckItemJson
	{
		public string id;

		public int upgrade_level;

		public int counter;

		public List<string> runes;
	}

	[Serializable]
	private class DeckItemJsonListWrapper
	{
		public List<DeckItemJson> items;
	}

	private const string NewProfileButtonName = "Button_开新档";

	private const string SkipIntroNewProfileButtonName = "Button_开新档（跳过序章）";

	public AssetCatalog testAssetCatalog;

	public Canvas canvas;

	public static TestMenuManager instance;

	private string fullRecordJson = "";

	private List<GameRecord.ItemRecord> importedDeckRecords = new List<GameRecord.ItemRecord>();

	private List<InfluenceDef> importedBlesses = new List<InfluenceDef>();

	private List<GameSkill> importedSkills = new List<GameSkill>();

	private List<TalentDef> importedActivatedTalents = new List<TalentDef>();

	private string deckJsonEditor = "";

	public TestMenuConfigData TestMenuConfigData => ScriptableObjectSingleton<GameLoadDataManager>.instance.testMenuConfigData;

	public PlayerCharacterDef testCharacter
	{
		get
		{
			return TestMenuConfigData.selectedCharacter;
		}
		set
		{
			TestMenuConfigData.selectedCharacter = value;
		}
	}

	public RoomRef encounterRoom
	{
		get
		{
			return TestMenuConfigData.selectedRoom;
		}
		set
		{
			TestMenuConfigData.selectedRoom = value;
		}
	}

	public TestMenuSelectEncounterOption TestMenuSelectEncounterOption
	{
		get
		{
			return TestMenuConfigData.selectEncounterOption;
		}
		set
		{
			TestMenuConfigData.selectEncounterOption = value;
		}
	}

	public int EnemyLevel
	{
		get
		{
			return TestMenuConfigData.initEnemyLevel;
		}
		set
		{
			TestMenuConfigData.initEnemyLevel = value;
		}
	}

	public List<ItemDefinition> initItems
	{
		get
		{
			return TestMenuConfigData.initDeck;
		}
		set
		{
			TestMenuConfigData.initDeck = value;
		}
	}

	public DifficultyLevel difficulty
	{
		get
		{
			return TestMenuConfigData.selectedDifficulty;
		}
		set
		{
			TestMenuConfigData.selectedDifficulty = value;
		}
	}

	public ContractDef selectedContract
	{
		get
		{
			return TestMenuConfigData.selectedContract;
		}
		set
		{
			TestMenuConfigData.selectedContract = value;
		}
	}

	public FixedContractRouteDef fixedContractRoute
	{
		get
		{
			return TestMenuConfigData.fixedContractRoute;
		}
		set
		{
			TestMenuConfigData.fixedContractRoute = value;
		}
	}

	private void ImportFullTestRecord()
	{
		try
		{
			ImportedTestSetup importedTestSetup = TestMenuImportUtility.ImportFromJson(fullRecordJson, testAssetCatalog);
			TestMenuConfigData.initDeck = importedTestSetup.DeckRecords.ConvertAll((GameRecord.ItemRecord r) => r.itemDef);
			importedDeckRecords = importedTestSetup.DeckRecords;
			importedBlesses = importedTestSetup.Blesses;
			importedSkills = importedTestSetup.Skills;
			importedActivatedTalents = importedTestSetup.ActivatedTalents;
			TestMenuConfigData.initImportedTestSetup = importedTestSetup;
			ItemPackSelectArea itemPackSelectArea = UnityEngine.Object.FindObjectOfType<ItemPackSelectArea>();
			if (itemPackSelectArea != null)
			{
				itemPackSelectArea.RefreshCurrentByInitItems();
			}
			Debug.Log($"导入完成：牌 {importedTestSetup.DeckRecords.Count}，祝福 {importedTestSetup.Blesses.Count}，技能 {importedTestSetup.Skills.Count}，天赋 {importedTestSetup.ActivatedTalents.Count}");
		}
		catch (Exception arg)
		{
			Debug.LogError($"导入测试记录失败：{arg}");
		}
	}

	private void CleanImport()
	{
		TestMenuConfigData.initImportedTestSetup = null;
		ItemPackSelectArea itemPackSelectArea = UnityEngine.Object.FindObjectOfType<ItemPackSelectArea>();
		if (itemPackSelectArea != null)
		{
			itemPackSelectArea.RefreshCurrentByInitItems();
		}
		Debug.Log("导入清空");
	}

	private void ImportDeckJsonEditorButton()
	{
		ImportDeckFromJsonString(deckJsonEditor);
	}

	public void SetTestMenuConfigData_initImportedTestSetup(ImportedTestSetup setup)
	{
		TestMenuConfigData.initImportedTestSetup = setup;
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		testAssetCatalog = Bootstrap.instance.assetCatalog;
	}

	private void ImportDeckFromJsonString(string raw)
	{
		if (string.IsNullOrWhiteSpace(raw))
		{
			Debug.LogWarning("导入失败：JSON 内容为空。");
			return;
		}
		raw = raw.Trim();
		if (raw.StartsWith("["))
		{
			raw = "{\"items\":" + raw + "}";
		}
		DeckItemJsonListWrapper deckItemJsonListWrapper;
		try
		{
			deckItemJsonListWrapper = JsonUtility.FromJson<DeckItemJsonListWrapper>(raw);
		}
		catch (Exception ex)
		{
			Debug.LogError("JSON 解析失败：" + ex.Message + "\n内容：" + raw);
			return;
		}
		if (deckItemJsonListWrapper == null || deckItemJsonListWrapper.items == null)
		{
			Debug.LogError("JSON 格式错误：未能解析 items。");
			return;
		}
		List<ItemDefinition> list = new List<ItemDefinition>();
		foreach (DeckItemJson item in deckItemJsonListWrapper.items)
		{
			ItemDefinition itemDefinition = FindItemDefinitionByFullId(item.id);
			if (itemDefinition != null)
			{
				list.Add(itemDefinition);
			}
			else
			{
				Debug.LogWarning("找不到 ItemDefinition，id = " + item.id);
			}
		}
		initItems = list;
		Debug.Log($"成功导入 {list.Count} 个 ItemDefinition。");
		ItemPackSelectArea itemPackSelectArea = UnityEngine.Object.FindObjectOfType<ItemPackSelectArea>();
		if (itemPackSelectArea != null)
		{
			itemPackSelectArea.RefreshCurrentByInitItems();
		}
	}

	private ItemDefinition FindItemDefinitionByFullId(string fullId)
	{
		if (string.IsNullOrEmpty(fullId) || testAssetCatalog == null || testAssetCatalog.itemDefCatalog == null || testAssetCatalog.itemDefCatalog.assets == null)
		{
			return null;
		}
		string importIdAppend = ICTAsset.GetImportIdAppend(fullId);
		int importIdIndex = ICTAsset.GetImportIdIndex(fullId);
		foreach (ItemDefinition asset in testAssetCatalog.itemDefCatalog.assets)
		{
			if (!(asset == null))
			{
				ICTAsset iCTAsset = asset;
				if (iCTAsset != null && iCTAsset.CTAssetIDPrepend == importIdAppend && iCTAsset.AssetIndex == importIdIndex)
				{
					return asset;
				}
			}
		}
		return null;
	}

	private void Start()
	{
		CreateSkipIntroNewProfileButton();
		if (GlobalConfigManager.instance.enableNetwork)
		{
			TestBattleLevelSelector component = GameObject.Find("Button_登陆界面").GetComponent<TestBattleLevelSelector>();
			Bootstrap.instance.Load(BootStrapLoadData.FromBattleParams(component.randomInitItem, component.newGame, component.newProfile, component.target, component.withIntro));
		}
	}

	private void CreateSkipIntroNewProfileButton()
	{
		if (GameObject.Find("Button_开新档（跳过序章）") != null)
		{
			return;
		}
		GameObject gameObject = GameObject.Find("Button_开新档");
		if (gameObject == null)
		{
			Debug.LogWarning("未找到测试界面按钮模板: Button_开新档");
			return;
		}
		GameObject obj = UnityEngine.Object.Instantiate(gameObject, gameObject.transform.parent);
		obj.name = "Button_开新档（跳过序章）";
		obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(450f, -150f);
		Text componentInChildren = obj.GetComponentInChildren<Text>();
		if (componentInChildren != null)
		{
			componentInChildren.text = "开新档（跳过序章）";
			componentInChildren.resizeTextForBestFit = true;
			componentInChildren.resizeTextMinSize = 18;
			componentInChildren.resizeTextMaxSize = 30;
		}
		TestBattleLevelSelector component = obj.GetComponent<TestBattleLevelSelector>();
		if (component != null)
		{
			component.randomInitItem = false;
			component.newGame = true;
			component.newProfile = true;
			component.target = BootStrapLoadData.TargetContext.GameMap;
			component.withIntro = false;
			component.completeIntroBeforeFirstGame = true;
		}
	}
}
