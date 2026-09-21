using TMPro;
using UnityEngine.UI;

public class ItemTagCell : CellUI<ItemTag>
{
	public Image tagImage;

	public TextMeshProUGUI nameText;

	private new ItemTag tag;

	protected override void OnSetup(ItemTag t)
	{
		tag = t;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (!(tag == null))
		{
			tagImage.gameObject.SetActive(tag.textSpriteIcon != null);
			tagImage.sprite = tag.textSpriteIcon;
			nameText.text = tag.tagName;
			nameText.color = tag.textColor;
		}
	}
}
