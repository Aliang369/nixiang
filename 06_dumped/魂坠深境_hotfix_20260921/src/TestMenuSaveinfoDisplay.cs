using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TestMenuSaveinfoDisplay : MonoBehaviour
{
	public TextMeshProUGUI saveStateInfoText;

	public GameObject loadGameButtonPanel;

	private void Start()
	{
		StartCoroutine(UpdateDisplayCor());
	}

	private IEnumerator UpdateDisplayCor()
	{
		SavingUtil.ProfileSaveData profileSaveData;
		try
		{
			profileSaveData = SavingUtil.LoadProfileData("Default");
		}
		catch (Exception)
		{
			saveStateInfoText.text = "存档损坏或者版本差异过大";
			DisableLoadGamePanel();
			throw;
		}
		if (profileSaveData == null)
		{
			saveStateInfoText.text = "未检测到存档";
			DisableLoadGamePanel();
			yield break;
		}
		saveStateInfoText.text = "已存在存档";
		if (profileSaveData.saveVersion != GlobalConfigManager.instance.saveVersion)
		{
			TextMeshProUGUI textMeshProUGUI = saveStateInfoText;
			textMeshProUGUI.text = textMeshProUGUI.text + "  存档版本不符合!(" + profileSaveData.saveVersion + "和" + GlobalConfigManager.instance.saveVersion + ")";
			DisableLoadGamePanel();
		}
	}

	private void DisableLoadGamePanel()
	{
		loadGameButtonPanel.SetActive(value: false);
	}
}
