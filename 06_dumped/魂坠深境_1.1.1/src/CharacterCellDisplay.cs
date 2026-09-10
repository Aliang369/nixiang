using TMPro;
using UnityEngine.UI;

public class CharacterCellDisplay : CellUI<PlayerCharacterDef>
{
	public Image characterImage;

	public TextMeshProUGUI characterNameText;

	public TextMeshProUGUI characterDescriptionText;

	public GameButton inspectButton;

	protected override void OnSetup(PlayerCharacterDef t)
	{
		characterImage.sprite = t.fullBody;
		if (characterNameText != null)
		{
			characterNameText.text = t.characterName;
		}
		if (characterDescriptionText != null)
		{
			characterDescriptionText.text = t.description;
		}
	}
}
