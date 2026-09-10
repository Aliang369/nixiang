using UnityEngine;

public class ObsessionLevelDisplay_Image : CellUI<PlayerCharacterState>
{
	[SerializeField]
	private GameObject[] obsessionLevelMarks;

	protected override bool shouldUpdate => true;

	private PlayerCharacterState characterState => base.setupData;

	protected override void Init()
	{
	}

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		ForceUpdate();
	}

	protected override void UpdateDisplay()
	{
		for (int i = 0; i < obsessionLevelMarks.Length; i++)
		{
			obsessionLevelMarks[i].SetActive(characterState.obsessionLevel == i);
		}
	}
}
