using UnityEngine;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class ReferenceButtonModule : MainItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private GameButton referenceButton;

	public override void Init()
	{
		referenceButton.onClick.AddListener(_OnReferenceButtonClicked);
	}

	public override void UpdateDisplay()
	{
		if (base.item == null || base.item.GetDef().itemStory.Length == 0)
		{
			referenceButton.gameObject.SetActive(value: false);
		}
		else
		{
			referenceButton.gameObject.SetActive(value: false);
		}
	}

	private void _OnReferenceButtonClicked()
	{
		base.mainItemInfoDisplay.itemInfoDisplay.GetModule<ItemInfoReferenceModule>().SetupItemStoryReference(base.mainItemInfoDisplay, base.item);
	}
}
