using TMPro;
using UnityEngine.UI;

public class ItemSkinPreviewPanel : CellUI<ItemSkinDef>
{
	public ItemDisplayBase originItemDisplay;

	public ItemDisplayBase newItemDisplay;

	public ItemInfoDisplay_Normal originItemInfo;

	public ItemInfoDisplay_Normal newItemInfo;

	public Item originItem;

	public Item newItem;

	public GameButton button;

	public Image icon;

	public TextMeshProUGUI originItemText;

	public TextMeshProUGUI newItemText;

	protected override void Init()
	{
		button.onClick.AddListener(base.Unset);
	}

	protected override void OnSetup(ItemSkinDef setupData)
	{
		base.gameObject.SetActive(value: true);
		originItem = new Item(setupData.attachItem);
		newItem = new Item(setupData.attachItem);
		newItem.SetSkin(setupData);
		originItemText.text = ScriptableObjectSingleton<TextDataPreset>.instance.skin_term_default_cardskin_of_card_arg0.FormatWith(setupData.attachItem.itemName);
		newItemText.text = setupData.ItemSkinNameWithSet;
		icon.sprite = setupData.attachSkinSet.icon;
		originItemDisplay.Setup(originItem);
		originItemInfo.Setup(originItem);
		newItemDisplay.Setup(newItem);
		newItemInfo.Setup(newItem);
		SetDirty();
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
	}
}
