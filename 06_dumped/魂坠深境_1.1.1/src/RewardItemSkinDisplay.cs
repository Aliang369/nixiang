using UnityEngine;

public class RewardItemSkinDisplay : CellUI<ItemSkinDef>
{
	[SerializeField]
	private ItemInfoDisplay_Normal itemInfoDisplay;

	protected override void Init()
	{
		InitUI(itemInfoDisplay);
	}

	protected override void OnSetup(ItemSkinDef t)
	{
		itemInfoDisplay.Setup(t.GenerateSkinedItem());
	}
}
