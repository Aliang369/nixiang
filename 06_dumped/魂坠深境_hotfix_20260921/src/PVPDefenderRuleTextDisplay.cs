using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class PVPDefenderRuleTextDisplay : UIBase
{
	public RectTransform pvpRuleTextParent;

	public TextMeshProUGUI pvpRuleTextPrefab;

	[NonSerialized]
	private PrefabPool<TextMeshProUGUI> pvpRuleTextPool;

	public bool enableRuleAcceptColor;

	public Color ruleAcceptTextColor;

	public PVPEnemyAvatarConfig avatarConfig;

	public DeckPreset deckPreset;

	protected override void Init()
	{
		pvpRuleTextPool = new PrefabPool<TextMeshProUGUI>(pvpRuleTextPrefab, pvpRuleTextParent);
	}

	public void Setup(PVPEnemyAvatarConfig avatarConfig, DeckPreset deckPreset = null)
	{
		this.avatarConfig = avatarConfig;
		this.deckPreset = deckPreset;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		pvpRuleTextPool.DespawnAll();
		if (avatarConfig == null)
		{
			return;
		}
		foreach (PVPEnemyDeckEditRule deckEditRule in avatarConfig.deckEditRules)
		{
			string text = deckEditRule.GetRuleDescription(UIBase.applicationState, out var generator);
			if (deckPreset != null && enableRuleAcceptColor && deckEditRule.condition.IsMatch(deckPreset))
			{
				text = text.ColorText(ruleAcceptTextColor);
			}
			TextMeshProUGUI textMeshProUGUI = pvpRuleTextPool.Spawn();
			textMeshProUGUI.text = text;
			new CommonTextLinkObjectDisplayer(UIBase.applicationState, textMeshProUGUI, textMeshProUGUI.rectTransform).SetGenerator(generator);
		}
	}
}
