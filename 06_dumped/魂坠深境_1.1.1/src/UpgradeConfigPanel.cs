using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeConfigPanel : CellUI<PlayerCharacterState>, ISelectablePanel
{
	[SerializeField]
	private Image characterHalfBodyImage;

	[SerializeField]
	private Image levelIcon;

	[SerializeField]
	private TextMeshProUGUI upgradeInfoText;

	[SerializeField]
	private RectTransform upgradeInfoTextReferencePivot;

	[SerializeField]
	private TextMeshProUGUI obsessionEffectText;

	[SerializeField]
	private TextMeshProUGUI levelCount;

	[SerializeField]
	private TextMeshProUGUI upgradeCostText;

	[SerializeField]
	private MatterCell prefab;

	[SerializeField]
	private GameButton upgradeButton;

	[SerializeField]
	private GameButton backButton;

	[NonSerialized]
	private CellUIPrefabPool<MatterCell, MatterInfo> pool;

	[NonSerialized]
	private MatterCell selectedCell;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	private PlayerCharacterState character => base.setupData;

	private int upgradeObsessionLevel => character.obsessionLevel + 1;

	private CharacterObsessionLevelConfig config => character.def.obsessionLevelDefs[upgradeObsessionLevel - 1];

	protected override void Init()
	{
		GetPrefabPool(ref pool, prefab);
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<PlayerCharacterState>.applicationState, obsessionEffectText, upgradeInfoTextReferencePivot);
		backButton.onClick.AddListener(base.Unset);
		upgradeButton.onClick.AddListener(OnUpgrade);
		void OnUpgrade()
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			if (!CanUpgrade())
			{
				CellUI<PlayerCharacterState>.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_cannot_upgrade_obsession.FormatWith(upgradeObsessionLevel, config.title));
			}
			else
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_chracter_obsession_level_add);
				character.UpgradeCharacter(config, selectedCell.setupData.GetUpgradeObsessionLevelInfo());
				Unset();
				parent.SetDirty();
			}
		}
	}

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		if (upgradeObsessionLevel > character.def.obsessionLevelDefs.Count)
		{
			Unset();
			return;
		}
		base.gameObject.SetActive(value: true);
		characterHalfBodyImage.sprite = character.def.fullBody;
		levelIcon.sprite = config.icon;
		levelCount.text = $"{upgradeObsessionLevel}";
		upgradeInfoText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock_obsession.FormatWith(upgradeObsessionLevel, config.title);
		obsessionEffectText.text = config.GetEffectDescription(character, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
		pool.Sync(GetMatterInfos());
		selectedCell = pool.GetActiveElement(0);
		UpdateDisplay();
		List<MatterInfo> GetMatterInfos()
		{
			return new List<MatterInfo> { character.obsessionDebrisInfo, character.classObsessionInfo };
		}
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		upgradeButton.gameObject.SetActive(value: false);
		upgradeCostText.gameObject.SetActive(value: false);
		if (selectedCell != null)
		{
			upgradeCostText.gameObject.SetActive(value: true);
			upgradeButton.gameObject.SetActive(value: true);
			upgradeCostText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_consume_matter_upgrade_obsession.FormatWith(selectedCell.setupData.matterDef.matterName);
		}
	}

	private bool CanUpgrade()
	{
		if (selectedCell == null)
		{
			return false;
		}
		if (character.obsessionLevel == MatterDef.obsessionMaxLevel)
		{
			return false;
		}
		MatterInfo matterInfo = selectedCell.setupData;
		return matterInfo.matterDef.matterType switch
		{
			MatterDef.MatterType.ObsessionDebris => matterInfo.count >= ScriptableObjectSingleton<FortressDataManager>.instance.obsessionDebrisUpgradeCount, 
			MatterDef.MatterType.CharacterClassObsession => matterInfo.count >= 1, 
			_ => false, 
		};
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		selectedCell = module.parent as MatterCell;
		SetDirty();
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		if (((ISelectablePanel)this).IsSelected(module))
		{
			CellUI<PlayerCharacterState>.fortressState.uiManager.universalCommonInfoPanel.AppendInfo(new CommonInfoData(selectedCell.matterInfo, CommonInfoData.ControlType.Click, selectedCell.rectTransform, false));
		}
		SetDirty();
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		return selectedCell == module.parent as MatterCell;
	}
}
