using UnityEngine;

public class ItemInfoPanel : CommonInfoPanelBase
{
	[SerializeField]
	private ItemInfoDisplayBase itemInfoDisplay;

	[SerializeField]
	private bool blockHide;

	protected override void Init()
	{
		base.Init();
		InitUI(itemInfoDisplay);
		itemInfoDisplay.Unset();
	}

	protected override void UpdateInfo()
	{
		if (currentCommonInfoData?.target is Item item)
		{
			if (blockHide && !item.GetDef().IsDiscovered(UIBase.applicationState))
			{
				itemInfoDisplay.showUncollectedStatus = false;
				itemInfoDisplay.Unset();
			}
			else
			{
				itemInfoDisplay.showUncollectedStatus = currentCommonInfoData.showUncollectedStatus;
				itemInfoDisplay.Setup(item);
			}
		}
		else
		{
			itemInfoDisplay.showUncollectedStatus = false;
			itemInfoDisplay.Unset();
		}
	}
}
