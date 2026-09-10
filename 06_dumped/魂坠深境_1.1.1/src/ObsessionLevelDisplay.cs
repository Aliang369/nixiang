using System;
using UnityEngine;

public class ObsessionLevelDisplay : CellUI<PlayerCharacterState>
{
	[SerializeField]
	private GameObject obsessionMark;

	[SerializeField]
	private GameObject[] obsessionLevelMarks;

	[NonSerialized]
	private CharacterInfoPanel characterInfoPanel;

	protected override bool shouldUpdate => true;

	private PlayerCharacterState characterState => base.setupData;

	protected override void Init()
	{
		characterInfoPanel = GetComponentInParent<CharacterInfoPanel>(includeInactive: true);
	}

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		ForceUpdate();
	}

	protected override void UpdateDisplay()
	{
		for (int i = 0; i < obsessionLevelMarks.Length; i++)
		{
			obsessionLevelMarks[i].SetActive(characterState.obsessionLevel > i);
		}
	}
}
