using System;
using System.Reflection;
using Generated.ChillyRoomSdkClient;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.SettingPanelModule;

public class MainMenuModule : SettingPanel.DisplayModule
{
	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private LanguageSettingArea languageSettingArea;

	[SerializeField]
	private RectTransform accountArea;

	[SerializeField]
	private GameButton logOffButton;

	[SerializeField]
	private GameButton copyHotUpdateCodeButton;

	public override void Init()
	{
		base.display.InitUI(languageSettingArea);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		if (GlobalConfigManager.instance.enableLocalization)
		{
			languageSettingArea.gameObject.SetActive(value: true);
		}
		else
		{
			languageSettingArea.gameObject.SetActive(value: false);
		}
		logOffButton.onClick.AddListener(_OnLogOffButtonClicked);
		copyHotUpdateCodeButton.onClick.AddListener(_OnClickCopyHotUpdateCode);
		copyHotUpdateCodeButton.GetComponentInChildren<TextMeshProUGUI>().alpha = 0f;
	}

	private void _OnClickCopyHotUpdateCode()
	{
		GUIUtility.systemCopyBuffer = _GetHotUpdateCode();
	}

	private static string _GetHotUpdateCode()
	{
		Type typeFromHandle = typeof(HotUpdateCode);
		return ((typeFromHandle.GetMethod("GetRealDeviceCode", BindingFlags.Static | BindingFlags.Public) ?? typeFromHandle.GetMethod("GetHotfixDeviceCode", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))?.Invoke(null, null) as string) ?? string.Empty;
	}

	private void Update()
	{
		accountArea.gameObject.SetActive(ChillyRoomService.GetSdk().Accounts.StateManager.HasLoggedIn);
	}

	private void _OnBackButtonClicked()
	{
		if (UIDisplayModule.applicationState != null)
		{
			UIDisplayModule.applicationState.savingManager.SetProfileDirtyToSave();
		}
		base.display.SetActiveAndDirtyWithFadeAnim(active: false);
	}

	private void _OnLogOffButtonClicked()
	{
		ChillyRoomService.GetSdk().Accounts.LogOff();
	}
}
