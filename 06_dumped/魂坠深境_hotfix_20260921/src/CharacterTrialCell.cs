using UnityEngine;
using UnityEngine.UI;

public class CharacterTrialCell : CellUI<PlayerCharacterState>
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private ObsessionLevelDisplay_Image obsessionLevelDisplayImage;

	private PlayerCharacterState characterState => base.setupData;

	protected override void Init()
	{
		InitUI(obsessionLevelDisplayImage);
	}

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		icon.sprite = setupData.def.halfBody;
		obsessionLevelDisplayImage.Setup(characterState);
	}
}
