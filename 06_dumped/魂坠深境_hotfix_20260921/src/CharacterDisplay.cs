using UnityEngine;
using UnityEngine.UI;

public class CharacterDisplay : UIBase
{
	[SerializeField]
	private Image characterImage;

	protected override void UpdateDisplay()
	{
		characterImage.sprite = UIBase.gameState.playerState.character.Avatar;
	}
}
