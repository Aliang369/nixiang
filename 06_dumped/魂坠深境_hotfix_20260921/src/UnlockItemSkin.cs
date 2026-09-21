using UnityEngine;

public class UnlockItemSkin : GlobalRewardSetup
{
	[SaveField(null, null)]
	public ItemSkinDef skin;

	public override Sprite GlobalItemCellIcon => skin.globalItemIcon;

	public override string GlobalItemTitle => FGlobalItemTitle(skin.attachItem.itemName, skin.attachSkinSet.skinSetName);

	public override ItemSkinDef GlobalItemRefItemSkin => skin;

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return base.GlobalItemStringConfig.GetGlobalItemDescription.FormatWith(skin.attachItem.itemName, skin.attachSkinSet.skinSetName, skin.attachSkinSet.description);
	}

	private UnlockItemSkin()
	{
	}

	public UnlockItemSkin(ItemSkinDef skin)
	{
		this.skin = skin;
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			unlockItemSkin = skin
		};
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(skin.ItemSkinNameWithSet);
	}
}
