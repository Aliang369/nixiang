using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestRoomSelector : MonoBehaviour
{
	private class OptionData : Dropdown.OptionData
	{
		public RoomInfoData roomInfoData;
	}

	public Dropdown dropDown;

	[SerializeField]
	private InputField filterTextField;

	[SerializeField]
	private List<Dropdown.OptionData> options;

	[SerializeField]
	private Toggle obsoleteToggle;

	private void Awake()
	{
		dropDown = GetComponentInChildren<Dropdown>();
		filterTextField.onValueChanged.AddListener(_OnInputFieldChange);
		MenuController.OnLoadEnd = (Action)Delegate.Remove(MenuController.OnLoadEnd, new Action(Refresh));
		MenuController.OnLoadEnd = (Action)Delegate.Combine(MenuController.OnLoadEnd, new Action(Refresh));
		obsoleteToggle.onValueChanged.AddListener(_OnToggleChange);
	}

	private void OnDestroy()
	{
		MenuController.OnLoadEnd = (Action)Delegate.Remove(MenuController.OnLoadEnd, new Action(Refresh));
	}

	private void ChangeHandler(int i)
	{
		OptionData optionData = dropDown.options[i] as OptionData;
		TestMenuManager.instance.encounterRoom = new RoomRef(optionData.roomInfoData);
	}

	private void Refresh()
	{
		List<RoomInfoData> assets = TestMenuManager.instance.testAssetCatalog.roomInfoDataCatalog.assets;
		options = new List<Dropdown.OptionData>();
		foreach (RoomInfoData item in assets)
		{
			string text = item.name;
			options.Add(new OptionData
			{
				roomInfoData = item,
				text = text
			});
		}
		options.RemoveAll((Dropdown.OptionData data) => !data.text.Contains(filterTextField.text));
		options.Sort((Dropdown.OptionData a, Dropdown.OptionData b) => a.text.CompareTo(b.text));
		dropDown.options = options;
		dropDown.onValueChanged.AddListener(ChangeHandler);
		dropDown.value = options.FindIndex((Dropdown.OptionData o) => (o as OptionData).roomInfoData.FullId == TestMenuManager.instance.encounterRoom.CTAssetId);
	}

	private void _OnInputFieldChange(string s)
	{
		Refresh();
	}

	private void _OnToggleChange(bool b)
	{
		Refresh();
	}
}
