using UnityEngine;
using UnityEngine.UI;

public class DeckItemSelectPanel : ItemSelectPanel, ISelectablePanel
{
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	protected ItemInfoPanel itemInfoPanel;

	void ISelectablePanel.OnClicked(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		ShowItemInfo(item);
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		return info.output.Contains(item);
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		TriggerItemSelect(item);
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		TriggerItemSelect(item);
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		return CanSelect(item);
	}

	protected override void Init()
	{
		base.Init();
		InitUI(itemInfoPanel);
	}

	public override void Setup(SelectItemInfo info)
	{
		base.Setup(info);
		scrollRect.verticalNormalizedPosition = 1f;
	}

	protected override void InitPools()
	{
		base.InitPools();
		itemDisplayPool.ConfigInfoPanel(itemInfoPanel);
	}

	public override void ShowItemInfo(Item item)
	{
	}

	protected override void HideItemInfo(Item item)
	{
		itemInfoPanel.HideAllCommonInfo();
		SetDirty();
	}
}
