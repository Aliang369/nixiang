using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ContainerOfSoul.Display.Fortress.GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class FortressGuidePanel : UIBase
{
	private readonly struct RuntimeGuideStep
	{
		public readonly GuideMask.GuideTargetType TargetType;

		public readonly string Hint;

		public readonly SkillDef SkillDef;

		public readonly float ExtraRadius;

		public readonly CharacterClassType CharacterClassType;

		public string Name => $"Runtime Guide - {TargetType}";

		public RuntimeGuideStep(GuideMask.GuideTargetType targetType, string hint, SkillDef skillDef = null, float extraRadius = 12f, CharacterClassType characterClassType = CharacterClassType.None)
		{
			TargetType = targetType;
			Hint = hint;
			SkillDef = skillDef;
			ExtraRadius = extraRadius;
			CharacterClassType = characterClassType;
		}
	}

	private const string SkillBreakthroughGuideSkillId = "#SK#000038#";

	public List<MaskGuideSeqSetup> maskGuideSeqSetups = new List<MaskGuideSeqSetup>();

	private MaskGuideSequence currentGuide;

	public GuideHintPanelSet characterInfoGuideHint;

	public GuideHintPanelSet gameConfigGuideHint;

	public GuideHintPanelSet talentGuideHint;

	public GuideHintPanelSet skillConfigGuideHint;

	public GuideHintPanelSet pvpModeGuideHint;

	public GuideHintPanelSet upgradeSkillGuideHint;

	public GuideHintPanelSet deleteSkillGuideHint;

	public GuideHintPanelSet addSkillSlotGuideHint;

	public GuideHintPanelSet mountSkillGemGuideHint;

	public GuideHintPanelSet skillBreakThroughGuideHint;

	public GuideHintPanelSet characterObsessionGuideHint;

	public GuideHintPanelSet itemSkinGuideHint;

	public GuideHintPanelSet idolGuideHint;

	public GuideHintPanelSet forgetShrineGuideHint;

	public VideoPlayer videoPlayer;

	public bool IsPlayingSkillBreakthroughGuide { get; private set; }

	public bool IsPlayingIdolWorshipGuide { get; private set; }

	public bool IsPlayingGachaGuide => maskGuideSeqSetups.Any((MaskGuideSeqSetup setup) => setup != null && setup.type == StoryPoint.PlayGuideSequence.MaskGuideType.GachaGuide && setup.guideSeq != null && setup.guideSeq.gameObject.activeInHierarchy);

	protected override void Init()
	{
		foreach (MaskGuideSeqSetup maskGuideSeqSetup in maskGuideSeqSetups)
		{
			InitUI(maskGuideSeqSetup.guideSeq);
			maskGuideSeqSetup.guideSeq.gameObject.SetActive(value: false);
		}
		GuideHintPanelSet[] componentsInChildren = GetComponentsInChildren<GuideHintPanelSet>(includeInactive: true);
		foreach (GuideHintPanelSet child in componentsInChildren)
		{
			InitUI(child);
		}
	}

	private IEnumerator GuideCor(MaskGuideSequence guideSeq)
	{
		yield return guideSeq.GuideCor();
		currentGuide = null;
	}

	public void SelectHardDifficultyAfterDifficultyGuide()
	{
		GameConfigPanel activeGameConfigPanel = GetActiveGameConfigPanel();
		if (activeGameConfigPanel == null)
		{
			Debug.LogError("Cannot select hard difficulty because no active game config panel was found.", this);
		}
		else
		{
			activeGameConfigPanel.SetCurrentDifficulty(DifficultyLevel.Hard);
		}
	}

	public IEnumerator AutoCompleteDifficultyGuideCor()
	{
		yield return null;
		SelectHardDifficultyAfterDifficultyGuide();
		yield return null;
	}

	private GameConfigPanel GetActiveGameConfigPanel()
	{
		FortressUIManager fortressUIManager = UIBase.fortressState?.uiManager;
		if ((object)fortressUIManager != null && fortressUIManager.baseGameUIPanel?.gameConfigPanel?.gameObject.activeInHierarchy == true)
		{
			return fortressUIManager.baseGameUIPanel.gameConfigPanel;
		}
		if ((object)fortressUIManager != null && fortressUIManager.starMapPanel?.gameConfigPanel?.gameObject.activeInHierarchy == true)
		{
			return fortressUIManager.starMapPanel.gameConfigPanel;
		}
		if ((object)fortressUIManager != null && fortressUIManager.pvpModeMainPanel?.gameConfigPanel?.gameObject.activeInHierarchy == true)
		{
			return fortressUIManager.pvpModeMainPanel.gameConfigPanel;
		}
		return null;
	}

	public IEnumerator PlaySkillBreakthroughCraftGuideCor()
	{
		SkillDef skillBreakthroughGuideSkill = GetSkillBreakthroughGuideSkill();
		if (!(skillBreakthroughGuideSkill == null))
		{
			yield return PlaySkillBreakthroughGuideCor(new RuntimeGuideStep[4]
			{
				new RuntimeGuideStep(GuideMask.GuideTargetType.MaskToGachaBackButton, ScriptableObjectSingleton<TextDataPreset>.instance.term_guide_return_to_lobby),
				new RuntimeGuideStep(GuideMask.GuideTargetType.MaskToFortressSkillButton, ScriptableObjectSingleton<TextDataPreset>.instance.term_guide_go_to_amulet),
				new RuntimeGuideStep(GuideMask.GuideTargetType.MaskToSkillCraftSkillCell, ScriptableObjectSingleton<TextDataPreset>.instance.term_guide_select_fearless_accessory, skillBreakthroughGuideSkill),
				new RuntimeGuideStep(GuideMask.GuideTargetType.MaskToSkillCraftButton, ScriptableObjectSingleton<TextDataPreset>.instance.term_guide_click_craft)
			});
		}
	}

	public IEnumerator PlaySkillBreakthroughCompleteGuideCor()
	{
		SkillDef skillBreakthroughGuideSkill = GetSkillBreakthroughGuideSkill();
		if (!(skillBreakthroughGuideSkill == null))
		{
			yield return PlaySkillBreakthroughGuideCor(new RuntimeGuideStep[3]
			{
				new RuntimeGuideStep(GuideMask.GuideTargetType.MaskToSkillBreakthroughAutoSelectButton, ScriptableObjectSingleton<TextDataPreset>.instance.term_guide_click_quick_select),
				new RuntimeGuideStep(GuideMask.GuideTargetType.MaskToSkillBreakthroughConfirmButton, ScriptableObjectSingleton<TextDataPreset>.instance.term_guide_click_breakthrough, skillBreakthroughGuideSkill),
				new RuntimeGuideStep(GuideMask.GuideTargetType.WaitGlobalRewardPackageDisplayInactive, string.Empty)
			});
		}
	}

	public IEnumerator PlaySkillBreakthroughReturnGuideCor()
	{
		yield return PlaySkillBreakthroughGuideCor(new RuntimeGuideStep[1]
		{
			new RuntimeGuideStep(GuideMask.GuideTargetType.MaskToSkillGemBackButton, ScriptableObjectSingleton<TextDataPreset>.instance.term_guide_return_to_lobby)
		});
	}

	public IEnumerator PlayIdolFirstWorshipGuideCor()
	{
		if (CanPlayIdolFirstWorshipGuide())
		{
			yield return PlayIdolFirstWorshipRuntimeGuideCor(new RuntimeGuideStep[3]
			{
				new RuntimeGuideStep(GuideMask.GuideTargetType.MaskToFortressIdolWarriorButton, string.Empty),
				new RuntimeGuideStep(GuideMask.GuideTargetType.MaskToIdolWorshipButton, string.Empty, null, 12f, CharacterClassType.Warrior),
				new RuntimeGuideStep(GuideMask.GuideTargetType.WaitGlobalRewardPackageDisplayInactive, string.Empty)
			});
		}
	}

	public IEnumerator PlayIdolFirstWorshipEnterRoomGuideCor()
	{
		if (CanPlayIdolFirstWorshipGuide())
		{
			yield return PlayIdolFirstWorshipRuntimeGuideCor(new RuntimeGuideStep[1]
			{
				new RuntimeGuideStep(GuideMask.GuideTargetType.MaskToFortressIdolRoomButton, string.Empty)
			});
		}
	}

	public IEnumerator PlayFirstMasteryTalentGuideCor()
	{
		StoryPointManager storyPointManager = UIBase.applicationState?.storyPointManager;
		PlayerCharacterState playerCharacterState = storyPointManager?.MasteryTalentGuideCharacter;
		CharacterMastery characterMastery = ((storyPointManager?.MasteryTalentGuideTalent)?.node as UnlockCharacterMastery)?.mastery;
		if (playerCharacterState == null || characterMastery == null)
		{
			yield break;
		}
		CharacterInfoPanel characterInfoPanel = UIBase.fortressState?.uiManager?.characterInfoBasePanel?.characterInfoPanel;
		GuideMask template = GetGuideMaskTemplate();
		if (characterInfoPanel == null || template == null || characterInfoPanel.characterState != playerCharacterState)
		{
			Debug.LogWarning("FortressGuidePanel missing mastery guide target or template.", this);
			storyPointManager.ClearMasteryTalentGuideContext();
			yield break;
		}
		characterInfoPanel.SetDirty();
		yield return null;
		Canvas.ForceUpdateCanvases();
		MaskGuideSequence sequence = CreateRuntimeGuideSequence(template, new RuntimeGuideStep[1]
		{
			new RuntimeGuideStep(GuideMask.GuideTargetType.MaskToLastUnlockedMasteryCell, string.Empty)
		});
		try
		{
			InitUI(sequence);
			currentGuide = sequence;
			yield return sequence.GuideCor();
		}
		finally
		{
			FortressGuidePanel fortressGuidePanel = this;
			fortressGuidePanel.currentGuide = null;
			storyPointManager.ClearMasteryTalentGuideContext();
			fortressGuidePanel.DisposeUI(sequence);
			Object.Destroy(sequence.gameObject);
		}
	}

	private IEnumerator PlayIdolFirstWorshipRuntimeGuideCor(IEnumerable<RuntimeGuideStep> steps)
	{
		GuideMask guideMaskTemplate = GetGuideMaskTemplate();
		if (guideMaskTemplate == null)
		{
			Debug.LogWarning("FortressGuidePanel missing guide mask template.", this);
			yield break;
		}
		IsPlayingIdolWorshipGuide = true;
		MaskGuideSequence sequence = CreateRuntimeGuideSequence(guideMaskTemplate, steps);
		try
		{
			InitUI(sequence);
			currentGuide = sequence;
			yield return sequence.GuideCor();
		}
		finally
		{
			FortressGuidePanel fortressGuidePanel = this;
			fortressGuidePanel.currentGuide = null;
			fortressGuidePanel.IsPlayingIdolWorshipGuide = false;
			fortressGuidePanel.DisposeUI(sequence);
			Object.Destroy(sequence.gameObject);
		}
	}

	private IEnumerator PlaySkillBreakthroughGuideCor(IEnumerable<RuntimeGuideStep> steps)
	{
		GuideMask guideMaskTemplate = GetGuideMaskTemplate();
		if (guideMaskTemplate == null)
		{
			Debug.LogWarning("FortressGuidePanel missing guide mask template.", this);
			yield break;
		}
		IsPlayingSkillBreakthroughGuide = true;
		MaskGuideSequence sequence = CreateRuntimeGuideSequence(guideMaskTemplate, steps);
		try
		{
			InitUI(sequence);
			currentGuide = sequence;
			yield return sequence.GuideCor();
		}
		finally
		{
			FortressGuidePanel fortressGuidePanel = this;
			fortressGuidePanel.currentGuide = null;
			fortressGuidePanel.IsPlayingSkillBreakthroughGuide = false;
			fortressGuidePanel.DisposeUI(sequence);
			Object.Destroy(sequence.gameObject);
		}
	}

	private SkillDef GetSkillBreakthroughGuideSkill()
	{
		return UIBase.applicationState?.assetCatalog?.FullIdToCTAsset<SkillDef>("#SK#000038#");
	}

	private bool CanPlayIdolFirstWorshipGuide()
	{
		if (ScriptableObjectSingleton<GameDataManager>.instance.lockIdolRoom != null && !ScriptableObjectSingleton<GameDataManager>.instance.lockIdolRoom.isUnlocked)
		{
			return false;
		}
		IdolState idolState = UIBase.applicationState?.idolManager?.GetIdolState(CharacterClassType.Warrior);
		if (idolState != null && idolState.currentLevel == 0)
		{
			return idolState.CanUpgrade(UIBase.applicationState);
		}
		return false;
	}

	private GuideMask GetGuideMaskTemplate()
	{
		return maskGuideSeqSetups.Where((MaskGuideSeqSetup setup) => setup?.guideSeq != null).SelectMany((MaskGuideSeqSetup setup) => setup.guideSeq.GuideMasks).FirstOrDefault((GuideMask mask) => mask != null);
	}

	private MaskGuideSequence CreateRuntimeGuideSequence(GuideMask template, IEnumerable<RuntimeGuideStep> steps)
	{
		GameObject gameObject = new GameObject("Runtime Skill Breakthrough Guide Sequence", typeof(RectTransform), typeof(MaskGuideSequence));
		RectTransform rectTransform = gameObject.transform as RectTransform;
		rectTransform.SetParent(base.transform, worldPositionStays: false);
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		foreach (RuntimeGuideStep step in steps)
		{
			GuideMask guideMask = Object.Instantiate(template, rectTransform, worldPositionStays: false);
			guideMask.name = step.Name;
			guideMask.targetType = step.TargetType;
			guideMask.presetTarget = null;
			guideMask.childIndex = 0;
			guideMask.talentDef = null;
			guideMask.skillDef = step.SkillDef;
			guideMask.characterClassType = step.CharacterClassType;
			guideMask.mastery = null;
			guideMask.gacahPool = null;
			guideMask.extraRadius = step.ExtraRadius;
			SetGuideHintText(guideMask, step.Hint);
		}
		return gameObject.GetComponent<MaskGuideSequence>();
	}

	private static void SetGuideHintText(GuideMask guide, string hint)
	{
		TextMeshProUGUI textMeshProUGUI = guide.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true).FirstOrDefault();
		if (textMeshProUGUI != null)
		{
			textMeshProUGUI.text = hint;
		}
	}
}
