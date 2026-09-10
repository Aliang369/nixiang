using System;
using Common.UI;
using ContainerOfSoul.Display.Fortress.SkillOverview;
using UnityEngine;
using UnityEngine.UI;

public class UniversalCommonInfoCell : CellUI<CommonInfoData>
{
	[SerializeField]
	private SkillInfoDisplay skillInfoDisplay;

	[SerializeField]
	private InfluenceDisplay influenceDisplay;

	[SerializeField]
	private ItemInfoDisplayBase itemInfoDisplay;

	[SerializeField]
	private UnlockTalentInfoDisplay unlockTalentInfoDisplay;

	[SerializeField]
	private CommonReference commonReference;

	[SerializeField]
	private GlobalItemBoxInfoDisplay globalItemBoxInfoDisplay;

	[SerializeField]
	private SkillGemInfoDisplay skillGemInfoDisplay;

	[SerializeField]
	private RuneDisplay runeDisplay;

	[NonSerialized]
	private RewardUncollectedTag influenceUncollectedTag;

	public RectTransform ActiveSkillEquipButtonRectTransform
	{
		get
		{
			if (!(skillInfoDisplay != null) || !skillInfoDisplay.gameObject.activeInHierarchy)
			{
				return null;
			}
			return skillInfoDisplay.GetModule<SkillEquipModule>()?.ActiveButtonRectTransform;
		}
	}

	protected override void Init()
	{
		base.Init();
		InitUI(skillInfoDisplay);
		InitUI(influenceDisplay);
		InitUI(itemInfoDisplay);
		InitUI(unlockTalentInfoDisplay);
		InitUI(commonReference);
		InitUI(globalItemBoxInfoDisplay);
		InitUI(skillGemInfoDisplay);
		if (runeDisplay != null)
		{
			InitUI(runeDisplay);
		}
		skillInfoDisplay.gameObject.SetActive(value: false);
		influenceDisplay.gameObject.SetActive(value: false);
		itemInfoDisplay.gameObject.SetActive(value: false);
		unlockTalentInfoDisplay.gameObject.SetActive(value: false);
		globalItemBoxInfoDisplay.gameObject.SetActive(value: false);
		if (runeDisplay != null)
		{
			runeDisplay.gameObject.SetActive(value: false);
		}
	}

	protected override void OnSetup(CommonInfoData setupData)
	{
		skillInfoDisplay.Unset();
		influenceDisplay.Unset();
		itemInfoDisplay.Unset();
		unlockTalentInfoDisplay.Unset();
		commonReference.Unset();
		globalItemBoxInfoDisplay.Unset();
		skillGemInfoDisplay.Unset();
		if (runeDisplay != null)
		{
			runeDisplay.Unset();
		}
		ClearUncollectedStatus();
		if (setupData == null || setupData.target == null)
		{
			return;
		}
		RectTransform rectTransform = null;
		if (setupData.target is Influence influence)
		{
			influenceDisplay.Setup(influence, setupData.appendData);
			SetupInfluenceUncollectedStatus(influence, setupData.showUncollectedStatus);
			rectTransform = influenceDisplay.rectTransform;
		}
		else if (setupData.target is GameSkill gameSkill)
		{
			skillInfoDisplay.Setup(gameSkill, setupData.appendData);
			rectTransform = skillInfoDisplay.rectTransform;
		}
		else if (setupData.target is ProfileSkill profileSkill)
		{
			skillInfoDisplay.Setup(new GameSkill(profileSkill), setupData.appendData);
			rectTransform = skillInfoDisplay.rectTransform;
		}
		else if (setupData.target is ProfileSkillPresetSlotCell.Info info && info.slot?.skill != null)
		{
			skillInfoDisplay.Setup(new GameSkill(info.slot.skill), setupData.appendData);
			rectTransform = skillInfoDisplay.rectTransform;
		}
		else if (setupData.target is Item item)
		{
			SetupItemUncollectedStatus(itemInfoDisplay, setupData.showUncollectedStatus);
			itemInfoDisplay.Setup(item, setupData.appendData);
			rectTransform = itemInfoDisplay.rectTransform;
		}
		else if (setupData.target is ItemDefinition def)
		{
			Item item2 = new Item(def);
			SetupItemUncollectedStatus(itemInfoDisplay, setupData.showUncollectedStatus);
			itemInfoDisplay.Setup(item2, setupData.appendData);
			rectTransform = itemInfoDisplay.rectTransform;
		}
		else if (setupData.target is TalentDef talentDef)
		{
			unlockTalentInfoDisplay.Setup(talentDef, setupData.appendData);
			rectTransform = unlockTalentInfoDisplay.rectTransform;
		}
		else if (setupData.target is IGlobalItemDisplaySetup globalItemDisplaySetup)
		{
			if (globalItemDisplaySetup.GlobalItemRefSkill != null)
			{
				skillInfoDisplay.Setup(globalItemDisplaySetup.GlobalItemRefSkill.GetGameSkill(), setupData.appendData);
				rectTransform = skillInfoDisplay.rectTransform;
			}
			else if (globalItemDisplaySetup.GlobalSkillGem != null)
			{
				skillGemInfoDisplay.Setup(globalItemDisplaySetup.GlobalSkillGem, setupData.appendData);
				rectTransform = skillGemInfoDisplay.rectTransform;
			}
			else if (globalItemDisplaySetup.GlobalItemRefItem != null)
			{
				itemInfoDisplay.Setup(globalItemDisplaySetup.GlobalItemRefItem, setupData.appendData);
				rectTransform = itemInfoDisplay.rectTransform;
			}
			else if (globalItemDisplaySetup.GlobalItemRefRune != null && runeDisplay != null)
			{
				runeDisplay.Setup(globalItemDisplaySetup.GlobalItemRefRune, setupData.appendData);
				rectTransform = runeDisplay.rectTransform;
			}
			else if (globalItemDisplaySetup.globalItemRefInfluence != null && influenceDisplay != null)
			{
				influenceDisplay.Setup(globalItemDisplaySetup.globalItemRefInfluence, setupData.appendData);
				rectTransform = influenceDisplay.rectTransform;
			}
			else
			{
				globalItemBoxInfoDisplay.Setup(globalItemDisplaySetup, setupData.appendData);
				rectTransform = globalItemBoxInfoDisplay.rectTransform;
			}
		}
		else if (setupData.target is SkillGem skillGem)
		{
			skillGemInfoDisplay.Setup(skillGem, setupData.appendData);
			rectTransform = skillGemInfoDisplay.rectTransform;
		}
		else
		{
			commonReference.Setup(setupData, setupData.appendData);
			rectTransform = commonReference.rectTransform;
		}
		if (rectTransform != null)
		{
			LayoutRebuilder.MarkLayoutForRebuild(base.rectTransform);
			Canvas.ForceUpdateCanvases();
			UnityHelper.MoveTargetRectToNeighborRectInAreaRect(rectTransform, setupData.pivotRectTransform, base.rectTransform);
		}
		SetDirty();
	}

	protected override void OnUnset()
	{
		skillInfoDisplay.Unset();
		influenceDisplay.Unset();
		itemInfoDisplay.Unset();
		unlockTalentInfoDisplay.Unset();
		commonReference.Unset();
		globalItemBoxInfoDisplay.Unset();
		skillGemInfoDisplay.Unset();
		if (runeDisplay != null)
		{
			runeDisplay.Unset();
		}
		ClearUncollectedStatus();
	}

	private void SetupItemUncollectedStatus(ItemInfoDisplayBase display, bool showUncollectedStatus)
	{
		if (!(display == null))
		{
			display.showUncollectedStatus = showUncollectedStatus;
		}
	}

	private void SetupInfluenceUncollectedStatus(Influence influence, bool showUncollectedStatus)
	{
		if (influenceUncollectedTag == null && influenceDisplay != null)
		{
			Component component = ((influenceDisplay.image != null) ? ((MonoBehaviour)influenceDisplay.image) : ((MonoBehaviour)influenceDisplay));
			influenceUncollectedTag = RewardUncollectedTag.AttachTo(component, RewardUncollectedTagLayout.InfluenceReward);
		}
		if (showUncollectedStatus && DiscoverManager.IsContentInConfiguredIndexPage(influence?.def, CellUI<CommonInfoData>.applicationState.assetCatalog))
		{
			influenceUncollectedTag?.Setup(influence?.def, CellUI<CommonInfoData>.applicationState);
		}
		else
		{
			influenceUncollectedTag?.Hide();
		}
	}

	private void ClearUncollectedStatus()
	{
		SetupItemUncollectedStatus(itemInfoDisplay, showUncollectedStatus: false);
		influenceUncollectedTag?.Hide();
	}
}
