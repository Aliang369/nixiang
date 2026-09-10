using System;
using Common.UI;
using UnityEngine;

public class CharacterInfoArea : CellUI<PlayerCharacterState>
{
	[SerializeField]
	private CharacterStrengthenArea characterStrengthenArea;

	[SerializeField]
	private FeatureDescriptionArea featureDescriptionArea;

	[SerializeField]
	private ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	private PotentialDisplayCell potentialDisplayCell;

	[NonSerialized]
	private CharacterInfoPanel characterInfoPanel;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> initDeckPool;

	[NonSerialized]
	private CellUIPrefabPool<PotentialDisplayCell, TalentDef> potentialPool;

	[NonSerialized]
	private GameObject potentialArea;

	public bool isPreview => characterInfoPanel?.isPreview ?? false;

	public PlayerCharacterState characterState => characterInfoPanel?.characterState;

	protected override void Init()
	{
		characterInfoPanel = GetComponentInParent<CharacterInfoPanel>(includeInactive: true);
		InitUI(characterStrengthenArea);
		InitUI(featureDescriptionArea);
		GetPrefabPool(ref initDeckPool, itemDisplayPrefab, null, characterInfoPanel.itemInfoPanel);
		GetPrefabPool(ref potentialPool, potentialDisplayCell);
		potentialArea = potentialDisplayCell.transform.parent.gameObject;
	}

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		characterStrengthenArea.Setup(setupData);
		if (characterState.def.description.IsNullOrEmpty())
		{
			featureDescriptionArea.gameObject.SetActive(value: false);
			return;
		}
		featureDescriptionArea.gameObject.SetActive(value: true);
		featureDescriptionArea.Setup(characterState);
	}

	protected override void UpdateDisplay()
	{
		initDeckPool.Sync(characterState.GenerateCharacterConfigInit(CellUI<PlayerCharacterState>.applicationState).initDeck);
		potentialArea.SetActive(!isPreview);
		if (isPreview)
		{
			potentialPool.DespawnAll();
		}
		else
		{
			potentialPool.Sync(characterState.GetPotentialTalents());
		}
	}
}
