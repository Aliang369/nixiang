using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class FeatureDescriptionArea : CellUI<PlayerCharacterState>
{
	[SerializeField]
	private TextMeshProUGUI characterDescription;

	[SerializeField]
	private FeatureDisplay featureDisplay;

	[NonSerialized]
	private CellUIPrefabPool<FeatureDisplay, FeatureDisplay.DisplayInfo> pool;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	private PlayerCharacterState character => base.setupData;

	protected override void Init()
	{
		GetPrefabPool(ref pool, featureDisplay);
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<PlayerCharacterState>.applicationState, characterDescription);
	}

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		characterDescription.text = TextDataPreset.FormatEffectObjectDescription(character.def, character.def.description, CellUI<PlayerCharacterState>.applicationState, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
		pool.Sync(character.def.features?.ConvertAll(FeatureDisplay.GetDisplayInfo));
	}
}
