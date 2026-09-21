using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameTest;

public class ItemPackSelectArea : MonoBehaviour
{
	public PlayerCharacterDef attachCharacterInfo;

	public GameObject TestItemDisplayPrefab;

	public TextMeshProUGUI itemDescription;

	public Toggle allItemToggle;

	public Toggle obsoleteToggle;

	public Transform currentItemPackArea;

	public Transform allItemPackArea;

	public InputField itemNameFilter;

	private readonly List<GameObject> addableItemGOs = new List<GameObject>();

	private string filterStr = "";

	private bool loaded;

	private void Start()
	{
		loaded = false;
		MenuController.OnLoadEnd = (Action)Delegate.Combine(MenuController.OnLoadEnd, new Action(OnLoadEnd));
		allItemToggle.onValueChanged.AddListener(_OnToggleValueChange);
		obsoleteToggle.onValueChanged.AddListener(_OnToggleValueChange);
		attachCharacterInfo = TestMenuManager.instance.testCharacter;
		UpdateAllItemPack();
		foreach (ItemDefinition item in TestMenuManager.instance.initItems.OrEmptyIfNull())
		{
			GameObject obj = TestItemDisplayPrefab.InstantiatePrefabIn(currentItemPackArea);
			obj.GetComponent<TestItemDisplay>().attachedItem = item;
			obj.GetComponent<TestItemDisplay>().attachedArea = this;
		}
	}

	private void Update()
	{
		if (loaded && TestMenuManager.instance.testCharacter != attachCharacterInfo)
		{
			UpdateCharacter(TestMenuManager.instance.testCharacter);
		}
	}

	private void OnDestroy()
	{
		MenuController.OnLoadEnd = (Action)Delegate.Remove(MenuController.OnLoadEnd, new Action(OnLoadEnd));
	}

	public void TransferItem(TestItemDisplay testItemDisplay)
	{
		if (testItemDisplay.attachedItem == null)
		{
			UnityEngine.Object.Destroy(testItemDisplay.gameObject);
			return;
		}
		itemDescription.text = testItemDisplay.attachedItem.itemName + " - " + testItemDisplay.attachedItem.GetTagsText() + Environment.NewLine + testItemDisplay.attachedItem.passiveFormtDescription;
		if (testItemDisplay.transform.parent == currentItemPackArea)
		{
			UnityEngine.Object.Destroy(testItemDisplay.gameObject);
			TestMenuManager.instance.initItems.Remove(testItemDisplay.attachedItem);
			return;
		}
		GameObject obj = TestItemDisplayPrefab.InstantiatePrefabIn(currentItemPackArea);
		obj.GetComponent<TestItemDisplay>().attachedItem = testItemDisplay.attachedItem;
		obj.GetComponent<TestItemDisplay>().attachedArea = this;
		TestMenuManager.instance.initItems.Add(testItemDisplay.attachedItem);
	}

	private void OnLoadEnd()
	{
		loaded = true;
		itemNameFilter.onValueChanged.AddListener(UpdateFilterStr);
	}

	public void UpdateCharacter(PlayerCharacterDef characterInfo)
	{
		attachCharacterInfo = characterInfo;
		UpdateAllItemPack();
		CleanCurrentItemPackArea();
		if (TestMenuManager.instance.initItems == null)
		{
			TestMenuManager.instance.initItems = new List<ItemDefinition>();
		}
		TestMenuManager.instance.initItems = new List<ItemDefinition>(characterInfo.initDeck);
		foreach (ItemDefinition item in TestMenuManager.instance.initItems.OrEmptyIfNull())
		{
			GameObject obj = TestItemDisplayPrefab.InstantiatePrefabIn(currentItemPackArea);
			obj.GetComponent<TestItemDisplay>().attachedItem = item;
			obj.GetComponent<TestItemDisplay>().attachedArea = this;
		}
	}

	private void CleanCurrentItemPackArea()
	{
		TestMenuManager.instance.initItems = null;
		TestItemDisplay[] componentsInChildren = currentItemPackArea.GetComponentsInChildren<TestItemDisplay>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
	}

	private void UpdateFilterStr(string str)
	{
		filterStr = str;
		UpdateAllItemPack();
	}

	private void UpdateAllItemPack()
	{
		foreach (GameObject addableItemGO in addableItemGOs)
		{
			UnityEngine.Object.Destroy(addableItemGO);
		}
		addableItemGOs.Clear();
		List<ItemDefinition> list = new List<ItemDefinition>(TestMenuManager.instance.testAssetCatalog.itemDefCatalog.assets);
		list.RemoveAll((ItemDefinition i) => i == null || i.itemName == null || (filterStr != "" && !i.itemName.Contains(filterStr)));
		list = list.FindAll(ItemCheck);
		foreach (ItemDefinition item in new ItemPack
		{
			items = list
		}.items)
		{
			GameObject gameObject = TestItemDisplayPrefab.InstantiatePrefabIn(allItemPackArea);
			gameObject.GetComponent<TestItemDisplay>().attachedItem = item;
			gameObject.GetComponent<TestItemDisplay>().attachedArea = this;
			addableItemGOs.Add(gameObject);
		}
	}

	private bool ItemCheck(ItemDefinition item)
	{
		bool num = (item.generationInfo?.attachClass != null && item.generationInfo.attachClass.Contains(attachCharacterInfo.attachClass.mClass)) || allItemToggle.isOn;
		bool flag = !item.obsolate || obsoleteToggle.isOn;
		return num & flag;
	}

	private void _OnToggleValueChange(bool on)
	{
		UpdateAllItemPack();
	}

	public void RefreshCurrentByInitItems()
	{
		TestItemDisplay[] componentsInChildren = currentItemPackArea.GetComponentsInChildren<TestItemDisplay>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
		foreach (ItemDefinition item in TestMenuManager.instance.initItems.OrEmptyIfNull())
		{
			GameObject obj = TestItemDisplayPrefab.InstantiatePrefabIn(currentItemPackArea);
			obj.GetComponent<TestItemDisplay>().attachedItem = item;
			obj.GetComponent<TestItemDisplay>().attachedArea = this;
		}
	}
}
