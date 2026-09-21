using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmBossInfluenceSelector : CellUI<Influence>
{
	[SerializeField]
	public Image image;

	[SerializeField]
	public TextMeshProUGUI nameText;

	[SerializeField]
	public TextMeshProUGUI descriptionText;

	[SerializeField]
	public TextMeshProUGUI willPowerCostText;

	[SerializeField]
	public TextMeshProUGUI selectButtonText;

	[SerializeField]
	public GameButton selectButton;

	[NonSerialized]
	private ConfirmBossInfluenceSelectionPanel attachPanel;

	[NonSerialized]
	protected TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	public Influence influence;

	[NonSerialized]
	private RewardUncollectedTag uncollectedTag;

	protected override void Init()
	{
		selectButton.onClick.AddListener(_SelectInfluence);
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<Influence>.applicationState, descriptionText, base.rectTransform);
		uncollectedTag = RewardUncollectedTag.AttachTo(image, RewardUncollectedTagLayout.Influence);
	}

	protected override void OnSetup(Influence setup)
	{
		attachPanel = parent as ConfirmBossInfluenceSelectionPanel;
		influence = setup;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		image.sprite = influence.def.Icon;
		uncollectedTag?.Setup(influence.def, CellUI<Influence>.applicationState);
		nameText.text = influence.def.GetNameWithQualityColor();
		willPowerCostText.text = influence.willPowerCost.ToString();
		descriptionText.text = influence.GetInfluenceDescription(CellUI<Influence>.gameState.applicationState, out var generator);
		selectButton.interactable = true;
		selectButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_confirm;
		textLinkObjectDisplayer.SetGenerator(generator);
	}

	protected override void OnUnset()
	{
		uncollectedTag?.Hide();
	}

	private void _SelectInfluence()
	{
		attachPanel.SelectInfluence(this);
	}
}
