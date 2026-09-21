using System;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class EnemyInfoDisplay : UIBase, ISelectablePanel
{
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private ItemDisplayBase enemyItemPrefab;

	[SerializeField]
	private Image enemyAvatar;

	[SerializeField]
	private Transform enemyItemParent;

	[SerializeField]
	private GameButton backButton;

	[NonSerialized]
	public EnemyInfoData enemy;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> enemyItemPool;

	[NonSerialized]
	private EnemyIndexPanel enemyPanel;

	[NonSerialized]
	private Item selectedItem;

	private void UnSelect()
	{
		if (selectedItem != null)
		{
			selectedItem = null;
			SetDirty();
		}
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		if (item != selectedItem)
		{
			selectedItem = item;
			SetDirty();
		}
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		UnSelect();
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		return selectedItem == item;
	}

	protected override void Init()
	{
		enemyPanel = (EnemyIndexPanel)parent;
		backButton.onClick.AddListener(_OnBackButtonClicked);
		enemyItemPool = GetPrefabPool<ItemDisplayBase, Item>(enemyItemPrefab, enemyItemParent);
		enemyItemPool.ConfigInfoPanel((parent.parent as FortressIndexUIPanel).itemInfoPanel);
		UIBase.fortressState.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
	}

	protected override void Dispose()
	{
		UIBase.fortressState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	public void Setup(EnemyInfoData setupData)
	{
		enemy = setupData;
		enemyItemPool.DespawnAll();
		CharacterAIComponent.EnemySpawnSetup enemySpawnSetup = setupData.setups.First();
		EnemyEncounterType encounterType = enemySpawnSetup.difficultyInfo.allowEncounterTypes.First();
		EnemyInitInfo initInfo = new EnemyInitInfo(enemySpawnSetup, encounterType, 12f, DifficultyLevel.Normal, exIntent: true);
		foreach (ItemGeneration itemGeneration in enemySpawnSetup.itemGenerations)
		{
			Item item = itemGeneration.GenerateItem(initInfo);
			item.indexOverrideOwnerName = enemy.name;
			enemyItemPool.Spawn(item);
		}
		nameText.text = enemy.name;
		descriptionText.text = enemy.description;
		enemyAvatar.sprite = ((enemy.avatar != null) ? enemy.avatar : ScriptableObjectSingleton<GameDataManager>.instance.enemyAvatarPlaceHolderIcon);
		SetDirty();
	}

	public void Unset()
	{
		enemy = null;
		selectedItem = null;
		enemyItemPool.DespawnAll();
		UIBase.fortressState.uiManager.indexUIPanel.itemInfoPanel.HideAllCommonInfo();
	}

	protected override void UpdateDisplay()
	{
		if (selectedItem != null)
		{
			UIBase.fortressState.uiManager.indexUIPanel.itemInfoPanel.ShowCommonInfo(new CommonInfoData(selectedItem, CommonInfoData.ControlType.Click, null, false));
		}
		else
		{
			UIBase.fortressState.uiManager.indexUIPanel.itemInfoPanel.HideAllCommonInfo();
		}
	}

	private void _OnBackButtonClicked()
	{
		enemyPanel.UnSelect();
	}

	private void _OnEmptyClicked()
	{
		UnSelect();
	}
}
