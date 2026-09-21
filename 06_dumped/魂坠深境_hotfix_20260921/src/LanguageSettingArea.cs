using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSettingArea : UIBase
{
	private const float DropdownBottomSpacing = 12f;

	[SerializeField]
	private Toggle togglePrefab;

	[SerializeField]
	private TMP_Dropdown languageDropdown;

	[NonSerialized]
	private PrefabPool<Toggle> pool;

	[NonSerialized]
	private Dictionary<LocalizationManager.LanguageCode, Toggle> languageToggleDic;

	[NonSerialized]
	private List<LocalizationManager.LanguageCode> dropdownLanguages;

	[NonSerialized]
	private Action<LocalizationManager.LanguageCode> languageChangeRequestHandler;

	[SerializeField]
	private GameObject setLanguageWaitPanel;

	private bool settingLanguage;

	private LocalizationManager.LanguageCode currentLanguage => ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage;

	protected override void Init()
	{
		dropdownLanguages = new List<LocalizationManager.LanguageCode>(ScriptableObjectSingleton<LocalizationManager>.instance.localLanguages);
		InitDropdown();
		InitToggleList();
		ForceUpdate();
		if (setLanguageWaitPanel != null)
		{
			setLanguageWaitPanel.gameObject.SetActive(value: false);
		}
	}

	public void SetLanguageChangeRequestHandler(Action<LocalizationManager.LanguageCode> handler)
	{
		languageChangeRequestHandler = handler;
	}

	public void ApplyLanguage(LocalizationManager.LanguageCode code, Action onComplete = null)
	{
		StartCoroutine(SetLanguageCor(code, onComplete));
	}

	public void RestoreCurrentLanguageSelection()
	{
		ForceUpdate();
	}

	private void InitDropdown()
	{
		if (languageDropdown == null)
		{
			return;
		}
		languageDropdown.ClearOptions();
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		foreach (LocalizationManager.LanguageCode dropdownLanguage in dropdownLanguages)
		{
			list.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLanguageNameText(dropdownLanguage)));
		}
		languageDropdown.AddOptions(list);
		ResizeDropdownTemplate(list.Count);
		languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
		EnsureDropdownSortsOnTop(languageDropdown);
	}

	private void ResizeDropdownTemplate(int optionCount)
	{
		RectTransform template = languageDropdown.template;
		if (!(template == null))
		{
			RectTransform rectTransform = template.GetComponent<ScrollRect>()?.content;
			RectTransform rectTransform2 = ((rectTransform != null && rectTransform.childCount > 0) ? (rectTransform.GetChild(0) as RectTransform) : null);
			VerticalLayoutGroup verticalLayoutGroup = ((rectTransform != null) ? rectTransform.GetComponent<VerticalLayoutGroup>() : null);
			if (!(rectTransform2 == null))
			{
				float num = ((rectTransform2.rect.height > 0f) ? rectTransform2.rect.height : rectTransform2.sizeDelta.y);
				int num2 = ((verticalLayoutGroup != null) ? verticalLayoutGroup.padding.vertical : 0);
				float num3 = ((verticalLayoutGroup != null) ? verticalLayoutGroup.spacing : 0f);
				float size = (float)num2 + 12f + num * (float)optionCount + num3 * (float)Mathf.Max(0, optionCount - 1);
				template.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
			}
		}
	}

	private void InitToggleList()
	{
		if (languageDropdown != null || togglePrefab == null)
		{
			return;
		}
		pool = new PrefabPool<Toggle>(togglePrefab);
		languageToggleDic = new Dictionary<LocalizationManager.LanguageCode, Toggle>();
		foreach (LocalizationManager.LanguageCode localLanguage in ScriptableObjectSingleton<LocalizationManager>.instance.localLanguages)
		{
			Spawn(localLanguage);
		}
		void Spawn(LocalizationManager.LanguageCode code)
		{
			Toggle toggle = pool.Spawn();
			languageToggleDic.Add(code, toggle);
			toggle.onValueChanged.AddListener(delegate(bool b)
			{
				if (b)
				{
					RequestLanguageChange(code);
				}
			});
			toggle.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.GetLanguageNameText(code);
		}
	}

	protected override void UpdateDisplay()
	{
		if (languageDropdown != null)
		{
			int num = dropdownLanguages.IndexOf(ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage);
			if (num >= 0)
			{
				languageDropdown.SetValueWithoutNotify(num);
			}
			languageDropdown.RefreshShownValue();
		}
		if (languageToggleDic == null)
		{
			return;
		}
		foreach (KeyValuePair<LocalizationManager.LanguageCode, Toggle> item in languageToggleDic)
		{
			item.Value.SetIsOnWithoutNotify(ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage == item.Key);
		}
	}

	private void OnDropdownValueChanged(int index)
	{
		if (dropdownLanguages != null && index >= 0 && index < dropdownLanguages.Count)
		{
			RequestLanguageChange(dropdownLanguages[index]);
		}
	}

	private void RequestLanguageChange(LocalizationManager.LanguageCode code)
	{
		if (settingLanguage)
		{
			ForceUpdate();
		}
		else if (currentLanguage != code)
		{
			if (languageChangeRequestHandler != null)
			{
				languageChangeRequestHandler(code);
			}
			else
			{
				StartCoroutine(SetLanguageCor(code));
			}
		}
	}

	private IEnumerator SetLanguageCor(LocalizationManager.LanguageCode code, Action onComplete = null)
	{
		if (!settingLanguage)
		{
			settingLanguage = true;
			if (setLanguageWaitPanel != null)
			{
				setLanguageWaitPanel.gameObject.SetActive(value: true);
			}
			yield return new WaitForSeconds(0.2f);
			if (currentLanguage != code)
			{
				ScriptableObjectSingleton<LocalizationManager>.instance.SetLanguage(code);
			}
			yield return new WaitForEndOfFrame();
			Bootstrap.instance.globalSaveConfigData.languageCode = code;
			Bootstrap.instance.globalSaveConfigData.Save();
			yield return new WaitForEndOfFrame();
			ForceUpdate();
			if (setLanguageWaitPanel != null)
			{
				setLanguageWaitPanel.gameObject.SetActive(value: false);
			}
			settingLanguage = false;
			onComplete?.Invoke();
		}
	}

	private static void EnsureDropdownSortsOnTop(TMP_Dropdown dropdown)
	{
		Canvas val = dropdown.GetComponent<Canvas>();
		if ((UnityEngine.Object)(object)val == null)
		{
			val = dropdown.gameObject.AddComponent<Canvas>();
		}
		val.overrideSorting = true;
		val.sortingOrder = 200;
		if (dropdown.GetComponent<GraphicRaycaster>() == null)
		{
			dropdown.gameObject.AddComponent<GraphicRaycaster>();
		}
	}
}
