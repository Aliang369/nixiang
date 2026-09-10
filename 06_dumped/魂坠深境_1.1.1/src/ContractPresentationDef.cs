using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ContractPresentation", menuName = "Contract/Contract Presentation")]
public sealed class ContractPresentationDef : ScriptableObject
{
	[Serializable]
	public sealed class StagePresentation
	{
		public Level level;

		public string loadingTitle;

		public Sprite loadingImage;

		public string nextStageButtonText;
	}

	public List<StagePresentation> stagePresentations = new List<StagePresentation>();

	public StagePresentation GetStagePresentation(Level level)
	{
		return stagePresentations?.Find((StagePresentation presentation) => presentation?.level == level);
	}
}
