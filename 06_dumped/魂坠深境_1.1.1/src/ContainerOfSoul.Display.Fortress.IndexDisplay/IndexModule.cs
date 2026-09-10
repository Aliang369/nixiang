using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class IndexModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI itemNameText;

	[SerializeField]
	private IndexBookContentMask contentMask;

	public override void Init()
	{
		if (contentMask != null)
		{
			base.slotDisplay.InitUI(contentMask);
		}
	}

	public override void UpdateDisplay()
	{
		if (base.item == null)
		{
			itemNameText.text = "";
		}
		else if (!base.slotDisplay.IsUnlocked)
		{
			base.slotDisplay.indexHide = true;
			itemNameText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_not_unlocked;
			itemNameText.color = Color.white;
		}
		else if (!base.slotDisplay.IsDiscovered)
		{
			base.slotDisplay.indexHide = true;
			itemNameText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_not_discovered;
			itemNameText.color = Color.white;
		}
		else
		{
			base.slotDisplay.indexHide = false;
			itemNameText.text = base.item.GetItemName();
			itemNameText.color = base.item.GetItemNameColor(qualityColor: true);
		}
		contentMask?.Setup(base.item?.def);
	}
}
