using System;
using UnityEngine;

public class ChallengeStarInfoModule : ConstellationDetailPanel.DisplayModule
{
	[SerializeField]
	private ChallengeStarInfoDisplay starInfoDisplay;

	[NonSerialized]
	private ChallengeStar current;

	public override void Init()
	{
		parent.InitUI(starInfoDisplay);
		starInfoDisplay.gameObject.SetActive(value: false);
	}

	public override void Setup()
	{
		starInfoDisplay.gameObject.SetActive(value: false);
		current = null;
		UpdateDisplay();
	}

	public override void UpdateDisplay()
	{
		if (!(base.constellationDetailPanel.focusStar == current))
		{
			current = base.constellationDetailPanel.focusStar;
			if (current != null)
			{
				starInfoDisplay.Setup(current);
				base.manager.ShowUI(starInfoDisplay);
			}
			else
			{
				base.manager.HideUI(starInfoDisplay);
			}
		}
	}
}
