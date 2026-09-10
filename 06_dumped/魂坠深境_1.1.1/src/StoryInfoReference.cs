using UnityEngine;

public class StoryInfoReference : ItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private StoryReferenceDisplay storyReferenceDisplay;

	public override void Setup()
	{
		if (base.itemInfoDisplay.showStoryReference && !base.item.GetDef().itemStory.IsNullOrEmpty())
		{
			ShowStoryReference(base.item);
		}
		else
		{
			HideStoryReference();
		}
	}

	private void ShowStoryReference(Item item)
	{
		storyReferenceDisplay.Setup(item);
	}

	private void HideStoryReference()
	{
		storyReferenceDisplay.Unset();
	}
}
