using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardCharacterDisplay : CellUI<PlayerCharacterDef>
{
	[SerializeField]
	private Image avatar;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	protected override void OnSetup(PlayerCharacterDef t)
	{
		avatar.sprite = t.fullBody;
		nameText.text = t.characterName;
		descriptionText.text = t.description;
	}
}
