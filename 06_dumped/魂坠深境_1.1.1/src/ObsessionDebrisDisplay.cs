using TMPro;
using UnityEngine;

public class ObsessionDebrisDisplay : CellUI<PlayerCharacterState>
{
	[SerializeField]
	private TextMeshProUGUI nameText;

	private PlayerCharacterState characterState => base.setupData;
}
