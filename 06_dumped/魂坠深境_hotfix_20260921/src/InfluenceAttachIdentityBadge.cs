using UnityEngine;
using UnityEngine.UI;

public class InfluenceAttachIdentityBadge : MonoBehaviour
{
	[SerializeField]
	private RectTransform root;

	[SerializeField]
	private Image frameImage;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Vector2 characterIconSize;

	[SerializeField]
	private Vector2 professionIconSize;

	[SerializeField]
	private Color characterIconColor = Color.white;

	[SerializeField]
	private Color professionIconColor = new Color(41f / 51f, 67f / 85f, 0.6588235f, 1f);

	public void Setup(InfluenceDef def)
	{
		if (def == null)
		{
			Hide();
		}
		else if (def.attachCharacter != null)
		{
			SetupIcon(def.attachCharacter.obsession?.icon ?? def.attachCharacter.headIcon, characterIconSize, characterIconColor);
		}
		else if (def.attachCharacterClass != null)
		{
			SetupIcon(def.attachCharacterClass.professionIcon, professionIconSize, professionIconColor);
		}
		else
		{
			Hide();
		}
	}

	public void Hide()
	{
		if (iconImage != null)
		{
			iconImage.sprite = null;
		}
		SetVisible(visible: false);
	}

	private void SetupIcon(Sprite icon, Vector2 size, Color color)
	{
		if (icon == null || iconImage == null)
		{
			Hide();
			return;
		}
		iconImage.sprite = icon;
		iconImage.color = color;
		if (iconImage.rectTransform != null)
		{
			iconImage.rectTransform.sizeDelta = size;
		}
		SetVisible(visible: true);
	}

	private void SetVisible(bool visible)
	{
		if (root != null)
		{
			root.gameObject.SetActive(visible);
			return;
		}
		if (frameImage != null)
		{
			frameImage.gameObject.SetActive(visible);
		}
		if (iconImage != null)
		{
			iconImage.gameObject.SetActive(visible);
		}
	}
}
