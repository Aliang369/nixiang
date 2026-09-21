using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrystalUpgradeInfoDisplay : CellUI<CrystalUpgrade>
{
	[SerializeField]
	private TextMeshProUGUI infoTitleText;

	[SerializeField]
	private TextMeshProUGUI briefDescriptionText;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image iconBackgroundImage;

	[SerializeField]
	private GameButton choiceButton;

	[SerializeField]
	private TextMeshProUGUI cooldownText;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	private RewardUncollectedTag uncollectedTag;

	[NonSerialized]
	public CrystalUpgrade upgrade;

	private CrystalUpgradeEventPanel panel => CellUI<CrystalUpgrade>.gameState.UIManager.worldEventPanel_CrystalUpgrade;

	protected override void Init()
	{
		CellUI<CrystalUpgrade>.gameState.gameEventManager.onTriggerGameEvent += GameEventHandler;
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<CrystalUpgrade>.applicationState, briefDescriptionText, base.rectTransform);
		choiceButton.onClick.AddListener(_OnChoiceButtonClicked);
		uncollectedTag = RewardUncollectedTag.AttachTo(iconImage, RewardUncollectedTagLayout.CrystalUpgradeInfluence);
	}

	protected override void Dispose()
	{
		CellUI<CrystalUpgrade>.gameState.gameEventManager.onTriggerGameEvent -= GameEventHandler;
	}

	protected override void OnSetup(CrystalUpgrade dailyContract)
	{
		upgrade = dailyContract;
		iconImage.sprite = dailyContract.Icon;
		costText.text = ScriptableObjectSingleton<GameDataManager>.instance.crystalInTextIcon.ToTMPSpriteText() + " " + dailyContract.GetFinalPrice(CellUI<CrystalUpgrade>.gameState, real: false).ToString().ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.crystalColor);
		nameText.text = dailyContract.GetUpgradeNameText(CellUI<CrystalUpgrade>.gameState);
		briefDescriptionText.text = dailyContract.GetDescription(CellUI<CrystalUpgrade>.gameState, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
		iconBackgroundImage.gameObject.SetActive(value: false);
		cooldownText.gameObject.SetActive(value: false);
		if (dailyContract is BlessCrystalUpgrade blessCrystalUpgrade)
		{
			infoTitleText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_influence;
			uncollectedTag?.Setup(blessCrystalUpgrade.def, CellUI<CrystalUpgrade>.applicationState);
		}
		else if (dailyContract is GetCrystalUpgrade)
		{
			infoTitleText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_get_crystal;
			uncollectedTag?.Hide();
		}
	}

	protected override void OnUnset()
	{
		upgrade = null;
		uncollectedTag?.Hide();
	}

	protected override void UpdateDisplay()
	{
		if (upgrade.CheckTriggerAble(CellUI<CrystalUpgrade>.gameState))
		{
			choiceButton.interactable = true;
		}
		else
		{
			choiceButton.interactable = false;
		}
	}

	private void SetDisplayColor(Color color)
	{
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (UIColorAdjustor.CheckGOColorAdjustable(transform.gameObject))
			{
				transform.gameObject.GetOrAddComponent<UIColorAdjustor>().SetAdjustColor(color);
			}
		}
	}

	private void GameEventHandler(GameEvent eve)
	{
		if (!eve.IsResolveGameEvent())
		{
			SetDirty();
		}
	}

	private void _OnChoiceButtonClicked()
	{
		if (panel.IsSelectable && upgrade.CheckTriggerAble(CellUI<CrystalUpgrade>.gameState))
		{
			panel.TriggerUpgradeChoice(upgrade);
		}
	}
}
