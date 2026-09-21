using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MasteryInfoDisplay : CellUI<MasteryInfoDisplay.DisplayInfo>, ISelectablePanel
{
	public class DisplayInfo
	{
		public PlayerCharacterState character;

		public CharacterMastery mastery;
	}

	[SerializeField]
	private Image masteryIcon;

	[SerializeField]
	private TextMeshProUGUI masteryNameText;

	[SerializeField]
	private TextMeshProUGUI masteryDescription;

	[SerializeField]
	private ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	private FeatureDisplay featureDisplay;

	[SerializeField]
	public ItemInfoPanel itemInfoPanel;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> pool;

	[NonSerialized]
	private CellUIPrefabPool<FeatureDisplay, FeatureDisplay.DisplayInfo> featureDisplayPool;

	[NonSerialized]
	private Item selectedItem;

	private DisplayInfo displayInfo => base.setupData;

	private PlayerCharacterState character => displayInfo.character;

	private CharacterMastery mastery => displayInfo.mastery;

	protected override void Init()
	{
		InitUI(featureDisplay);
		InitUI(itemInfoPanel);
		GetPrefabPool(ref pool, itemDisplayPrefab, null, itemInfoPanel);
		GetPrefabPool(ref featureDisplayPool, featureDisplay);
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<DisplayInfo>.applicationState, masteryDescription);
		if (CellUI<DisplayInfo>.fortressState != null)
		{
			featureDisplay.SetInfoPanel(UIHelper.GetDefaultCommonInfoPanel());
		}
	}

	protected override void OnSetup(DisplayInfo setupData)
	{
		selectedItem = null;
		itemInfoPanel.HideAllCommonInfo();
		CharacterGameInitConfig characterGameInitConfig = character.GenerateCharacterConfigInit(CellUI<DisplayInfo>.applicationState, mastery);
		pool.Sync(characterGameInitConfig.initDeck);
		masteryIcon.sprite = mastery.icon;
		masteryNameText.text = mastery.masteryName;
		masteryDescription.text = mastery.GetDescription(CellUI<DisplayInfo>.applicationState, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
		featureDisplayPool.Sync(mastery.features?.ConvertAll(FeatureDisplay.GetDisplayInfo));
	}

	protected override void OnUnset()
	{
		selectedItem = null;
		itemInfoPanel.HideAllCommonInfo();
		pool.DespawnAll();
		featureDisplayPool.DespawnAll();
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase)?.item;
		if (item != null)
		{
			selectedItem = item;
			SetDirty();
		}
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		selectedItem = null;
		SetDirty();
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase)?.item;
		if (item != null)
		{
			return selectedItem == item;
		}
		return false;
	}
}
