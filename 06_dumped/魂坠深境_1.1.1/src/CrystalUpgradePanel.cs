using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class CrystalUpgradePanel : UIBase
{
	[SerializeField]
	private CrystalUpgradeInfoDisplay infoDisplayPrefab;

	[SerializeField]
	private RectTransform infoDisplayParent;

	[SerializeField]
	private GameButton reRollButton;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private TextMeshProUGUI reRollPriceText;

	[NonSerialized]
	private CellUIPrefabPool<CrystalUpgradeInfoDisplay, CrystalUpgrade> crystalUpgradeDisplayPool;

	[NonSerialized]
	private CrystalUpgradeWorldEvent worldEvent;

	private CrystalUpgradeEventPanel panel => UIBase.gameState.UIManager.worldEventPanel_CrystalUpgrade;

	protected override bool shouldUpdate => parent.gameObject.activeInHierarchy;

	protected override void Init()
	{
		infoDisplayPrefab.gameObject.SetActive(value: false);
		crystalUpgradeDisplayPool = GetPrefabPool<CrystalUpgradeInfoDisplay, CrystalUpgrade>(infoDisplayPrefab, infoDisplayParent);
		reRollButton.onClick.AddListener(_OnReRollButtonClicked);
		backButton.onClick.AddListener(_OnBackButtonClicked);
	}

	public void Setup(CrystalUpgradeWorldEvent worldEvent)
	{
		this.worldEvent = worldEvent;
		SetDirty();
	}

	public void Unset()
	{
		crystalUpgradeDisplayPool.DespawnAll();
	}

	protected override void UpdateDisplay()
	{
		if (worldEvent.ReRollEnabled())
		{
			reRollButton.transform.parent.gameObject.SetActive(value: true);
			reRollPriceText.text = worldEvent.crystalUpgradeConfig.reRollPrice.ToString();
			reRollButton.interactable = worldEvent.CheckReRollAble();
		}
		else
		{
			reRollButton.transform.parent.gameObject.SetActive(value: false);
		}
		crystalUpgradeDisplayPool.DespawnAll();
		foreach (CrystalUpgrade item in worldEvent.allCrystalUpgrade)
		{
			crystalUpgradeDisplayPool.Spawn(item);
		}
	}

	private void _OnReRollButtonClicked()
	{
		worldEvent.ReRollUpgrades();
		SetDirty();
	}

	private void _OnBackButtonClicked()
	{
		panel._OnBackButtonClicked();
	}
}
