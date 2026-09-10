using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AfterBossUpgradeInfoDisplay : CellUI<AfterBossUpgrade>
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
	private InfluenceAttachIdentityBadge attachIdentityBadge;

	[SerializeField]
	private GameButton choiceButton;

	[SerializeField]
	private TextMeshProUGUI cooldownText;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	private RewardUncollectedTag uncollectedTag;

	[NonSerialized]
	public AfterBossUpgrade upgrade;

	private AfterBossUpgradeEventPanel panel => CellUI<AfterBossUpgrade>.gameState.UIManager.worldEventPanel_AfterBossUpgrade;

	protected override void Init()
	{
		CellUI<AfterBossUpgrade>.gameState.gameEventManager.onTriggerGameEvent += GameEventHandler;
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<AfterBossUpgrade>.applicationState, briefDescriptionText);
		choiceButton.onClick.AddListener(_OnChoiceButtonClicked);
		uncollectedTag = RewardUncollectedTag.AttachTo(iconImage, RewardUncollectedTagLayout.InfluenceReward);
	}

	protected override void Dispose()
	{
		CellUI<AfterBossUpgrade>.gameState.gameEventManager.onTriggerGameEvent -= GameEventHandler;
	}

	protected override void OnSetup(AfterBossUpgrade upgrade)
	{
		this.upgrade = upgrade;
		iconImage.sprite = upgrade.Icon;
		bool showWillPowerCost = panel.afterBossUpgradePanel.ShowWillPowerCost;
		costText.gameObject.SetActive(showWillPowerCost);
		if (showWillPowerCost)
		{
			if (upgrade.CheckTriggerAble(CellUI<AfterBossUpgrade>.gameState))
			{
				costText.text = upgrade.WillPowerCost.ToString();
			}
			else
			{
				costText.text = upgrade.WillPowerCost.ToString().ColorText(new Color(1f, 0.4f, 0.4f, 1f));
			}
		}
		nameText.text = upgrade.GetUpgradeNameText(CellUI<AfterBossUpgrade>.gameState);
		briefDescriptionText.text = upgrade.GetDescription(CellUI<AfterBossUpgrade>.gameState, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
		iconBackgroundImage.gameObject.SetActive(value: false);
		cooldownText.gameObject.SetActive(value: false);
		if (upgrade is BossBlessUpgrade)
		{
			infoTitleText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_influence;
		}
		else
		{
			infoTitleText.text = string.Empty;
		}
		if (upgrade is BossBlessUpgrade bossBlessUpgrade)
		{
			uncollectedTag?.Setup(bossBlessUpgrade.inf?.def, CellUI<AfterBossUpgrade>.applicationState);
			attachIdentityBadge?.Setup(bossBlessUpgrade.inf?.def);
		}
		else
		{
			uncollectedTag?.Hide();
			attachIdentityBadge?.Hide();
		}
		UpdateDisplay();
	}

	protected override void OnUnset()
	{
		upgrade = null;
		uncollectedTag?.Hide();
		attachIdentityBadge?.Hide();
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
		AfterBossUpgrade afterBossUpgrade = upgrade;
		if (afterBossUpgrade == null)
		{
			return;
		}
		if (!panel.upgraded && afterBossUpgrade.CheckTriggerAble(CellUI<AfterBossUpgrade>.gameState))
		{
			if (!TryShowWillPowerOverLimitConfirm(afterBossUpgrade))
			{
				panel.TriggerUpgradeChoice(afterBossUpgrade);
			}
		}
		else
		{
			CellUI<AfterBossUpgrade>.gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_lack_will);
		}
	}

	private bool TryShowWillPowerOverLimitConfirm(AfterBossUpgrade selectedUpgrade)
	{
		if (!panel.afterBossUpgradePanel.UsesWillPower)
		{
			return false;
		}
		if (!CellUI<AfterBossUpgrade>.gameState.playerState.ShouldShowWillPowerOverLimitConfirm(selectedUpgrade.WillPowerCost, out var overLimitWillPower, out var penaltyInfluences))
		{
			return false;
		}
		CellUI<AfterBossUpgrade>.gameState.UIManager.commonSecondConfirmPanel.SetupWillPowerOverLimit(new WillPowerOverLimitConfirmInfo
		{
			overLimitWillPower = overLimitWillPower,
			penaltyInfluences = penaltyInfluences,
			confirmCallback = delegate
			{
				panel.TriggerUpgradeChoice(selectedUpgrade);
			}
		});
		return true;
	}
}
