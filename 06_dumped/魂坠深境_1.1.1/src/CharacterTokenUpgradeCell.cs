using TMPro;
using UnityEngine;

public class CharacterTokenUpgradeCell : CellUI<(PlayerCharacterState, GameConfigEffect)>
{
	public TextMeshProUGUI indexText;

	public TextMeshProUGUI descriptionText;

	public GameObject darken;

	protected override void UpdateDisplay()
	{
	}
}
