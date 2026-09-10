using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestCharacterInfoSelector : MonoBehaviour
{
	public Dropdown dropDown => GetComponentInChildren<Dropdown>();

	private void Awake()
	{
		MenuController.OnLoadEnd = (Action)Delegate.Remove(MenuController.OnLoadEnd, new Action(OnLoadEnd));
		MenuController.OnLoadEnd = (Action)Delegate.Combine(MenuController.OnLoadEnd, new Action(OnLoadEnd));
	}

	private void OnDestroy()
	{
		MenuController.OnLoadEnd = (Action)Delegate.Remove(MenuController.OnLoadEnd, new Action(OnLoadEnd));
	}

	private void ChangeHandler(int i)
	{
		string name = dropDown.options[i].text;
		PlayerCharacterDef testCharacter = TestMenuManager.instance.testAssetCatalog.characterDefCatalog.assets.Find((PlayerCharacterDef r) => r.name == name);
		TestMenuManager.instance.testCharacter = testCharacter;
	}

	private void OnLoadEnd()
	{
		List<Dropdown.OptionData> list = TestMenuManager.instance.testAssetCatalog.characterDefCatalog.assets.ConvertAll((PlayerCharacterDef n) => new Dropdown.OptionData(n.name));
		dropDown.options = list;
		dropDown.onValueChanged.AddListener(ChangeHandler);
		dropDown.value = list.FindIndex((Dropdown.OptionData o) => o.text == TestMenuManager.instance.testCharacter.name);
	}
}
