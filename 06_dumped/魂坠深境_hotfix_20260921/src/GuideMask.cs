using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ContainerOfSoul.Display.Fortress;
using ContainerOfSoul.Display.Fortress.GameConfig;
using ContainerOfSoul.Display.Fortress.SkillOverview;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GuideMask : UIBase, IPointerClickHandler, IEventSystemHandler
{
	public enum GuideTargetType
	{
		MaskToFixedTarget = 1,
		MaskToChildIndex = 2,
		MaskToTalentCell = 3,
		MaskToCharacterConfigSkillCell = 4,
		MaskToSkinMastery = 5,
		MaskToIndexConfigSkillSlot = 6,
		MaskToSelectDifficulty = 7,
		MaskToGacahSetDrawButton = 8,
		MaskToHealSpringSkillChoiceButton = 9,
		MaskToActivateCharacterPreapreSkillCell = 10,
		MaskToFirstBattleCastSkillButton = 11,
		MaskToActivableMasteryCell = 12,
		MaskToFirstSkinUpgradeItem = 13,
		MaskToCharacterInfoOpenTalentButton = 14,
		MaskToCharacterTalentUnlockButton = 15,
		MaskToCharacterTalentExitButton = 16,
		MaskToSelectSkillActivateButton = 17,
		MaskToFortressSkillButton = 18,
		MaskToSkillCraftSkillCell = 19,
		MaskToSkillCraftButton = 20,
		MaskToSkillBreakthroughAutoSelectButton = 21,
		MaskToSkillBreakthroughConfirmButton = 22,
		MaskToGachaBackButton = 23,
		MaskToSkillGemBackButton = 24,
		MaskToFortressIdolRoomButton = 25,
		MaskToFortressIdolWarriorButton = 26,
		MaskToIdolWorshipButton = 27,
		MaskToLastUnlockedMasteryCell = 28,
		MaskToMasteryInstallButton = 29,
		MaskToFortressCrewButton = 30,
		ActiveGameObject = 101,
		WaitGameObjectInactive = 102,
		ActivateGuideHintSet = 103,
		WaitGameObjectActive = 104,
		WaitForGachaSetActive = 105,
		WaitNormalEventPanelChoiceSetup = 106,
		WaitConfigSkillPresetSuccess = 107,
		WaitGachaResultDisplayActive = 108,
		WaitGachaResultDisplayInactive = 109,
		WaitSkillBreakthroughComplete = 110,
		WaitGlobalRewardPackageDisplayInactive = 111,
		WaitIdolWorshipComplete = 112
	}

	private const string _Origin = "_Origin";

	public Image image;

	public float extraRadius;

	public CanvasGroup appendArea;

	public GuideTargetType targetType;

	public RectTransform presetTarget;

	public int childIndex;

	public TalentDef talentDef;

	public SkillDef skillDef;

	public CharacterMastery mastery;

	public DifficultyLevel difficulty;

	public GachaPoolDef gacahPool;

	public CharacterClassType characterClassType;

	private Sequence seq;

	private RectTransform target;

	private Rect? targetWorldRectOverride;

	private Rect? targetScreenRectOverride;

	private float? targetRadiusOverride;

	private MonoBehaviour coroutineHost;

	private const float TargetWaitTimeout = 5f;

	public bool finished;

	private Material guideMaskMat => image.materialForRendering;

	private Rect TargetWorldRect => targetWorldRectOverride ?? RectTransformToWorldSpace(target);

	private float TargetRadius => targetRadiusOverride ?? (TargetWorldRect.width / 2f + extraRadius);

	private bool IsMaskGuide => targetType < (GuideTargetType)100;

	private CharacterInfoPanel CurrentCharacterInfoPanel => UIBase.fortressState?.uiManager?.characterInfoBasePanel?.characterInfoPanel;

	private CharacterTalentUnlockPanel CurrentCharacterTalentPanel => UIBase.fortressState?.uiManager?.characterTalentUnlockPanel;

	private MasteryInfoPanel CurrentMasteryInfoPanel => UIBase.fortressState?.uiManager?.characterInfoBasePanel?.masteryInfoPanel;

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (targetType == GuideTargetType.MaskToLastUnlockedMasteryCell && TryForwardPointerEventToClickedMastery(eventData))
		{
			FinishGuide();
		}
		else if (TargetContainsScreenPoint(eventData.position))
		{
			if (TrySelectFirstSkinUpgradeItem())
			{
				FinishGuide();
				return;
			}
			if (ShouldWaitForConfigSkillSuccessAfterClick())
			{
				StartCoroutine(ClickTargetAndWaitForConfigSkillSuccess(eventData));
				return;
			}
			if (ShouldWaitForSkillBreakthroughAfterClick())
			{
				StartCoroutine(ClickTargetAndWaitForSkillBreakthrough(eventData));
				return;
			}
			if (ShouldWaitForIdolWorshipAfterClick())
			{
				StartCoroutine(ClickTargetAndWaitForIdolWorship(eventData));
				return;
			}
			ForwardPointerEventToTarget(eventData);
			FinishGuide();
		}
	}

	public void StartGuide(MaskGuideSequence guideSequence)
	{
		coroutineHost = guideSequence;
		if (targetType == GuideTargetType.ActiveGameObject)
		{
			if (presetTarget != null)
			{
				presetTarget.gameObject.SetActive(value: true);
			}
			else
			{
				Debug.LogWarning($"GuideMask presetTarget not assigned: {targetType}", this);
			}
			finished = true;
		}
		else if (targetType == GuideTargetType.WaitGameObjectInactive)
		{
			UIBase.applicationState.StartCoroutine(WaitForTargetInactive);
		}
		else if (targetType == GuideTargetType.WaitGameObjectActive)
		{
			UIBase.applicationState.StartCoroutine(WaitForTargetActive);
		}
		else if (targetType == GuideTargetType.ActivateGuideHintSet)
		{
			GuideHintPanelSet guideHintPanelSet = ((presetTarget != null) ? presetTarget.GetComponent<GuideHintPanelSet>() : null);
			if (guideHintPanelSet != null)
			{
				guideHintPanelSet.Show();
			}
			else
			{
				Debug.LogWarning($"GuideMask guide hint target not found: {targetType}", this);
			}
			finished = true;
		}
		else if (targetType == GuideTargetType.WaitForGachaSetActive)
		{
			GachaPanel gachaPanel = UIBase.fortressState?.uiManager?.gachaPanel;
			GachaPoolState gachaPoolState = ((gacahPool != null) ? UIBase.applicationState.gachaManager.GetGachaPoolState(gacahPool) : null);
			if (gachaPanel == null || gachaPoolState == null)
			{
				Debug.LogWarning($"GuideMask gacha target not found: {targetType}", this);
				finished = true;
				return;
			}
			gachaPanel.SetState(gachaPoolState);
			gachaPanel.Show();
			guideSequence.StartCoroutine(WaitForGacahSetActive());
		}
		else if (targetType == GuideTargetType.WaitNormalEventPanelChoiceSetup)
		{
			UIBase.applicationState.StartCoroutine(WaitNormalEventPanelChoiceSetup);
		}
		else if (targetType == GuideTargetType.WaitConfigSkillPresetSuccess)
		{
			finished = false;
			UIBase.applicationState.StartCoroutine(WaitConfigSkillPresetSuccess);
		}
		else if (targetType == GuideTargetType.WaitGachaResultDisplayActive)
		{
			finished = false;
			UIBase.applicationState.StartCoroutine(WaitGachaResultDisplayActive);
		}
		else if (targetType == GuideTargetType.WaitGachaResultDisplayInactive)
		{
			finished = false;
			UIBase.applicationState.StartCoroutine(WaitGachaResultDisplayInactive);
		}
		else if (targetType == GuideTargetType.WaitSkillBreakthroughComplete)
		{
			finished = false;
			UIBase.applicationState.StartCoroutine(WaitSkillBreakthroughComplete);
		}
		else if (targetType == GuideTargetType.WaitGlobalRewardPackageDisplayInactive)
		{
			finished = false;
			UIBase.applicationState.StartCoroutine(WaitGlobalRewardPackageDisplayInactive);
		}
		else if (targetType == GuideTargetType.WaitIdolWorshipComplete)
		{
			finished = false;
			UIBase.applicationState.StartCoroutine(WaitIdolWorshipComplete);
		}
		if (IsMaskGuide)
		{
			TweenToTarget();
		}
	}

	public void TweenToTarget()
	{
		if (seq != null)
		{
			return;
		}
		finished = false;
		if (ShouldFinishCompletedSkillConfigStepImmediately())
		{
			FinishGuide();
			return;
		}
		UpdateTarget();
		if (target == null)
		{
			if (ShouldFinishMissingMaskTargetImmediately())
			{
				FinishGuide();
			}
			else
			{
				StartWaitForMaskTargetAndTween();
			}
		}
		else
		{
			TweenToCurrentTarget();
		}
	}

	private void StartWaitForMaskTargetAndTween()
	{
		if (coroutineHost != null && coroutineHost.gameObject.activeInHierarchy)
		{
			coroutineHost.StartCoroutine(WaitForMaskTargetAndTween());
		}
		else if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(WaitForMaskTargetAndTween());
		}
		else
		{
			UIBase.applicationState.StartCoroutine(WaitForMaskTargetAndTween);
		}
	}

	private IEnumerator WaitForMaskTargetAndTween()
	{
		float timeout = Time.unscaledTime + 5f;
		while (target == null && Time.unscaledTime < timeout)
		{
			yield return null;
			UpdateTarget();
			if (target == null && ShouldFinishMissingMaskTargetImmediately())
			{
				FinishGuide();
				yield break;
			}
		}
		if (target == null)
		{
			Debug.LogWarning($"GuideMask target not found: {targetType}", this);
			SetActiveAndDirty(active: false);
			finished = true;
		}
		else
		{
			TweenToCurrentTarget();
		}
	}

	private void TweenToCurrentTarget()
	{
		SetActiveAndDirty(active: true);
		appendArea.alpha = 0f;
		SetMaskMat(TargetRadius);
		float radius = 2000f;
		seq = DOTween.Sequence();
		seq.Append(DOTween.To(() => radius, delegate(float x)
		{
			radius = x;
			SetMaskMat(x);
		}, TargetRadius, 0.75f));
		seq.Join(DOTweenModuleUI.DOFade(appendArea, 1f, 0.4f));
		seq.AppendCallback(delegate
		{
			seq = null;
		});
		seq.Play();
	}

	private void UpdateTarget()
	{
		target = null;
		targetWorldRectOverride = null;
		targetScreenRectOverride = null;
		targetRadiusOverride = null;
		GuideTargetType guideTargetType = targetType;
		switch (guideTargetType)
		{
		default:
			_ = guideTargetType - 107;
			_ = 5;
			break;
		case GuideTargetType.MaskToFixedTarget:
			target = presetTarget;
			break;
		case GuideTargetType.MaskToChildIndex:
			if (!(presetTarget == null))
			{
				List<Transform> list = presetTarget.GetChildren().ToList().FindAll((Transform t) => t.gameObject.activeSelf);
				if (childIndex >= 0 && list.Count > childIndex)
				{
					target = list[childIndex].GetComponent<RectTransform>();
				}
			}
			break;
		case GuideTargetType.MaskToTalentCell:
		{
			RectTransform rectTransform2 = ((presetTarget != null) ? presetTarget : CurrentCharacterTalentPanel?.talentTreeParent);
			TalentSlotCell talentSlotCell = ((rectTransform2 != null) ? rectTransform2.GetComponentsInChildren<TalentSlotCell>(includeInactive: true).FirstOrDefault((TalentSlotCell cell) => cell.talent == talentDef) : null);
			if (talentSlotCell != null)
			{
				target = talentSlotCell.GetComponent<RectTransform>();
			}
			break;
		}
		case GuideTargetType.MaskToLastUnlockedMasteryCell:
		{
			PlayerCharacterState masteryTalentGuideCharacter = UIBase.applicationState.storyPointManager.MasteryTalentGuideCharacter;
			CharacterMastery unlockedMastery = (UIBase.applicationState.storyPointManager.MasteryTalentGuideTalent?.node as UnlockCharacterMastery)?.mastery;
			CharacterInfoPanel currentCharacterInfoPanel = CurrentCharacterInfoPanel;
			if (currentCharacterInfoPanel?.characterState == masteryTalentGuideCharacter && !(unlockedMastery == null))
			{
				CharacterMasterySelectCell2 characterMasterySelectCell2 = currentCharacterInfoPanel.masterySelectPool.GetActiveElements().FirstOrDefault((CharacterMasterySelectCell2 cell) => cell.info?.Item1 == unlockedMastery && cell.info.Item2 == masteryTalentGuideCharacter);
				if (characterMasterySelectCell2 != null)
				{
					target = characterMasterySelectCell2.GetComponent<RectTransform>();
					targetWorldRectOverride = RectTransformCornersToWorldSpace(target);
				}
			}
			break;
		}
		case GuideTargetType.MaskToMasteryInstallButton:
			target = ActiveTargetOrNull(CurrentMasteryInfoPanel?.InstallButtonRect);
			break;
		case GuideTargetType.MaskToCharacterConfigSkillCell:
		{
			RectTransform rectTransform = ((presetTarget != null) ? presetTarget : UIBase.fortressState?.uiManager?.skillConfigPanel?.skillConfigPresetPanel?.skillSelectArea?.RectTransform());
			if (rectTransform == null)
			{
				break;
			}
			SkillDisplay[] componentsInChildren = rectTransform.GetComponentsInChildren<SkillDisplay>(includeInactive: true);
			SkillConfigPresetPanel skillConfigPresetPanelForCell = UIBase.fortressState?.uiManager?.skillConfigPanel?.skillConfigPresetPanel;
			SkillDisplay skillDisplay = componentsInChildren.FirstOrDefault((SkillDisplay cell) => cell.setupData?.skillDef == skillDef && CanTargetSkillConfigSkill(cell, skillConfigPresetPanelForCell));
			CharacterClassDef currentCharacterClass = rectTransform.GetComponentInParent<GameConfigPanel>()?.config?.currentCharacter?.attachClass ?? UIBase.fortressState?.uiManager?.skillConfigPanel?.skillConfigPresetPanel?.character?.def?.attachClass;
			if (currentCharacterClass != null && (skillDisplay == null || skillDisplay.setupData?.skillDef?.attachCharacterClass != currentCharacterClass))
			{
				skillDisplay = componentsInChildren.FirstOrDefault((SkillDisplay cell) => cell.setupData?.skillDef?.attachCharacterClass == currentCharacterClass && CanTargetSkillConfigSkill(cell, skillConfigPresetPanelForCell));
			}
			if (skillDisplay != null)
			{
				target = skillDisplay.GetComponent<RectTransform>();
			}
			break;
		}
		case GuideTargetType.MaskToSkinMastery:
			if (!(presetTarget == null))
			{
				CharacterMasterySelectCell characterMasterySelectCell3 = presetTarget.GetComponentsInChildren<CharacterMasterySelectCell>(includeInactive: true).FirstOrDefault((CharacterMasterySelectCell c) => c.info != null && c.info.Item1 == mastery);
				if (characterMasterySelectCell3 != null)
				{
					target = characterMasterySelectCell3.GetComponent<RectTransform>();
				}
			}
			break;
		case GuideTargetType.MaskToIndexConfigSkillSlot:
		{
			if (childIndex < 0)
			{
				break;
			}
			SkillConfigPresetPanel skillConfigPresetPanel2 = UIBase.fortressState?.uiManager?.skillConfigPanel?.skillConfigPresetPanel;
			ProfileSkillPresetSlotCell profileSkillPresetSlotCell2 = skillConfigPresetPanel2?.selectedCell;
			if (childIndex == 0 && IsSelectableConfigSkillSlot(profileSkillPresetSlotCell2))
			{
				target = profileSkillPresetSlotCell2.GetComponent<RectTransform>();
				break;
			}
			RectTransform rectTransform3 = ((presetTarget != null) ? presetTarget : skillConfigPresetPanel2?.skillPresetArea?.RectTransform());
			if (rectTransform3 == null)
			{
				break;
			}
			IEnumerable<ProfileSkillPresetSlotCell> source = from c in rectTransform3.GetComponentsInChildren<ProfileSkillPresetSlotCell>(includeInactive: true)
				where IsSelectableConfigSkillSlot(c)
				select c;
			if (presetTarget == null)
			{
				source = source.OrderBy((ProfileSkillPresetSlotCell c) => (c.skillSlot.skill != null) ? 1 : 0);
			}
			ProfileSkillPresetSlotCell profileSkillPresetSlotCell3 = source.Skip(childIndex).FirstOrDefault();
			if (profileSkillPresetSlotCell3 != null)
			{
				target = profileSkillPresetSlotCell3.GetComponent<RectTransform>();
			}
			break;
		}
		case GuideTargetType.MaskToSelectDifficulty:
			if (!(presetTarget == null))
			{
				DifficultyInfoCell difficultyInfoCell = presetTarget.GetComponentsInChildren<DifficultyInfoCell>(includeInactive: true).FirstOrDefault((DifficultyInfoCell c) => c.cellDifficulty?.difficultyLevel == difficulty);
				if (difficultyInfoCell != null)
				{
					target = difficultyInfoCell.GetComponent<RectTransform>();
				}
			}
			break;
		case GuideTargetType.MaskToGacahSetDrawButton:
			target = UIBase.fortressState?.uiManager?.gachaPanel?.gachaSet?.drawSingleButton?.GetComponent<RectTransform>();
			break;
		case GuideTargetType.MaskToHealSpringSkillChoiceButton:
		{
			HealSpringEvent healSpringEvent = UIBase.gameState.currentEncounter.triggerEvent as HealSpringEvent;
			if (healSpringEvent != null)
			{
				WorldEventItemChoiceDisplay choiceDisplay = UIBase.gameState.UIManager.worldEventPanel_Normal.GetChoiceDisplay(healSpringEvent.skillChoice);
				if (choiceDisplay != null)
				{
					target = choiceDisplay.GetComponent<RectTransform>();
				}
			}
			break;
		}
		case GuideTargetType.MaskToActivateCharacterPreapreSkillCell:
		{
			GameSkillPresetArea skillPresetArea = UIBase.gameState.UIManager.selectSkillPanel.skillPresetArea;
			ProfileSkillPresetSlotCell profileSkillPresetSlotCell = skillPresetArea.AllSlots().FirstOrDefault((ProfileSkillPresetSlotCell s) => s.skillSlot is GameSkillSlot gameSkillSlot && gameSkillSlot.gameSkill?.skillDef.GetAttachCharacer(UIBase.applicationState.assetCatalog) != null);
			if (profileSkillPresetSlotCell == null)
			{
				profileSkillPresetSlotCell = skillPresetArea.AllSlots().FirstOrDefault((ProfileSkillPresetSlotCell s) => s.skillSlot is GameSkillSlot gameSkillSlot && gameSkillSlot.gameSkill != null);
			}
			if (profileSkillPresetSlotCell != null)
			{
				target = profileSkillPresetSlotCell.GetComponent<RectTransform>();
			}
			break;
		}
		case GuideTargetType.MaskToSelectSkillActivateButton:
		{
			SkillConfigPresetPanel skillConfigPresetPanel = UIBase.fortressState?.uiManager?.skillConfigPanel?.skillConfigPresetPanel;
			if (IsSkillConfigPanelActive(skillConfigPresetPanel))
			{
				if (CanConfirmSkillConfigSelection(skillConfigPresetPanel))
				{
					target = UIBase.fortressState?.uiManager?.universalCommonInfoPanel?.GetActiveSkillEquipButtonRectTransform();
				}
				break;
			}
			target = UIBase.gameState?.UIManager?.universalCommonInfoPanel?.GetActiveSkillEquipButtonRectTransform();
			if (target == null)
			{
				target = UIBase.fortressState?.uiManager?.universalCommonInfoPanel?.GetActiveSkillEquipButtonRectTransform();
			}
			break;
		}
		case GuideTargetType.MaskToFirstBattleCastSkillButton:
		{
			GameSkill skill = UIBase.gameState.playerState.ActivatedSkills.FirstOrDefault();
			SkillDisplay skillDisplay2 = UIBase.gameState.UIManager.playerBftUIDisplay.skillPanel.GetSkillDisplay(skill);
			if (skillDisplay2 != null)
			{
				target = skillDisplay2.GetComponent<RectTransform>();
			}
			break;
		}
		case GuideTargetType.MaskToActivableMasteryCell:
		{
			CharacterMasterySelectCell2 characterMasterySelectCell = UIBase.fortressState.uiManager.characterInfoBasePanel.characterInfoPanel.masterySelectPool.GetActiveElements().FirstOrDefault((CharacterMasterySelectCell2 m) => m.IsUnlocked);
			if (characterMasterySelectCell != null)
			{
				target = characterMasterySelectCell.GetComponent<RectTransform>();
			}
			break;
		}
		case GuideTargetType.MaskToFirstSkinUpgradeItem:
		{
			if (presetTarget != null)
			{
				target = presetTarget;
				break;
			}
			ItemDisplayBase itemDisplayBase = UIBase.gameState.UIManager.worldEventPanel_UpgradeRoom.GetComponentsInChildren<ItemDisplayBase>().FirstOrDefault((ItemDisplayBase id) => UIBase.applicationState.itemSkinManager.GetAllUnlockedSkinsOfItem(id.item.def).Count > 0);
			if (itemDisplayBase != null)
			{
				target = itemDisplayBase.GetComponent<RectTransform>();
			}
			break;
		}
		case GuideTargetType.MaskToCharacterInfoOpenTalentButton:
			target = ActiveTargetOrNull(CurrentCharacterInfoPanel?.OpenTalentButtonRect);
			break;
		case GuideTargetType.MaskToCharacterTalentUnlockButton:
			target = ActiveTargetOrNull(CurrentCharacterTalentPanel?.unlockButtonRect);
			break;
		case GuideTargetType.MaskToCharacterTalentExitButton:
			target = ActiveTargetOrNull(CurrentCharacterTalentPanel?.exitButtonRect);
			break;
		case GuideTargetType.MaskToFortressSkillButton:
			target = ActiveTargetOrNull(UIBase.fortressState?.uiManager?.mainUIPanel?.mainPanelMainRoom?.ToSkillButtonRect);
			break;
		case GuideTargetType.MaskToFortressCrewButton:
			target = ActiveTargetOrNull(UIBase.fortressState?.uiManager?.mainUIPanel?.mainPanelMainRoom?.ToCrewButtonRect);
			break;
		case GuideTargetType.MaskToSkillCraftSkillCell:
			target = ActiveTargetOrNull(UIBase.fortressState?.uiManager?.skill_GemPanel?.skillUpgradeBasePanel?.GetSkillCellRect(skillDef));
			break;
		case GuideTargetType.MaskToSkillCraftButton:
			target = ActiveTargetOrNull(UIBase.fortressState?.uiManager?.skill_GemPanel?.skillUpgradeBasePanel?.ConfirmButtonRect);
			break;
		case GuideTargetType.MaskToSkillBreakthroughAutoSelectButton:
			target = ActiveTargetOrNull(UIBase.fortressState?.uiManager?.skill_GemPanel?.skillUpgradePanel?.breakthroughPanel?.AutoSelectButtonRect);
			break;
		case GuideTargetType.MaskToSkillBreakthroughConfirmButton:
			target = ActiveTargetOrNull(UIBase.fortressState?.uiManager?.skill_GemPanel?.skillUpgradePanel?.breakthroughPanel?.ConfirmButtonRect);
			break;
		case GuideTargetType.MaskToGachaBackButton:
			target = ActiveTargetOrNull(UIBase.fortressState?.uiManager?.gachaPanel?.BackButtonRect);
			break;
		case GuideTargetType.MaskToSkillGemBackButton:
			target = ActiveTargetOrNull(UIBase.fortressState?.uiManager?.skill_GemPanel?.BackButtonRect);
			break;
		case GuideTargetType.MaskToFortressIdolRoomButton:
		{
			FortressMainUIPanel.MainPanelLayerSetup mainPanelLayerSetup = UIBase.fortressState?.uiManager?.mainUIPanel?.mainUISwitchGroup?.mainPanelLayerSetups?.FirstOrDefault((FortressMainUIPanel.MainPanelLayerSetup setup) => setup.layer == FortressMainUIPanel.MainPanelLayer.IdolRoom);
			target = ActiveTargetOrNull((mainPanelLayerSetup?.button != null) ? (mainPanelLayerSetup.button.transform as RectTransform) : null);
			break;
		}
		case GuideTargetType.MaskToFortressIdolWarriorButton:
			target = ActiveTargetOrNull(UIBase.fortressState?.uiManager?.mainUIPanel?.MainPanelIdolRoom?.IdolWarriorButtonRect);
			UseTargetAndChildrenRect(target);
			break;
		case GuideTargetType.MaskToIdolWorshipButton:
			target = ActiveTargetOrNull(UIBase.fortressState?.uiManager?.idolBasePanel?.idolPanel?.WorshipButtonRect);
			break;
		case GuideTargetType.ActiveGameObject:
		case GuideTargetType.WaitGameObjectInactive:
		case GuideTargetType.ActivateGuideHintSet:
		case GuideTargetType.WaitGameObjectActive:
			break;
		}
	}

	private void ForwardPointerEventToTarget(PointerEventData eventData)
	{
		target.GetComponentInParent<IPointerClickHandler>()?.OnPointerClick(eventData);
		target.GetComponentInParent<IPointerDownHandler>()?.OnPointerDown(eventData);
	}

	private bool TrySelectFirstSkinUpgradeItem()
	{
		if (targetType != GuideTargetType.MaskToFirstSkinUpgradeItem)
		{
			return false;
		}
		ItemDisplayBase component = target.GetComponent<ItemDisplayBase>();
		if (component == null)
		{
			return false;
		}
		SelectableModuleBase module = component.GetModule<SelectableModuleBase>();
		ISelectablePanel componentInParent = component.GetComponentInParent<ISelectablePanel>(includeInactive: true);
		if (module == null || componentInParent == null)
		{
			return false;
		}
		componentInParent.OnClicked(module);
		if (!componentInParent.IsSelected(module) && componentInParent.CanSelect(module))
		{
			componentInParent.Select(module);
		}
		return true;
	}

	private bool TryForwardPointerEventToClickedMastery(PointerEventData eventData)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<CharacterMasterySelectCell2> readOnlyList = CurrentCharacterInfoPanel?.masterySelectPool?.GetActiveElements();
		if (readOnlyList == null)
		{
			return false;
		}
		foreach (CharacterMasterySelectCell2 item in readOnlyList)
		{
			RectTransform rectTransform = item?.GetComponent<RectTransform>();
			if (!(rectTransform == null) && rectTransform.gameObject.activeInHierarchy)
			{
				Canvas componentInParent = rectTransform.GetComponentInParent<Canvas>(includeInactive: true);
				Camera camera = (((Object)(object)componentInParent != null && (int)componentInParent.renderMode != 0) ? componentInParent.worldCamera : null);
				if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, camera))
				{
					target = rectTransform;
					ForwardPointerEventToTarget(eventData);
					return true;
				}
			}
		}
		return false;
	}

	private void FinishGuide()
	{
		SetActiveAndDirty(active: false);
		finished = true;
	}

	private bool ShouldWaitForConfigSkillSuccessAfterClick()
	{
		if (targetType != GuideTargetType.MaskToSelectSkillActivateButton)
		{
			return false;
		}
		SkillConfigPanel skillConfigPanel = UIBase.fortressState?.uiManager?.skillConfigPanel;
		if (skillConfigPanel != null && skillConfigPanel.gameObject.activeInHierarchy && skillConfigPanel.skillConfigPresetPanel != null)
		{
			return skillConfigPanel.skillConfigPresetPanel.gameObject.activeInHierarchy;
		}
		return false;
	}

	private bool ShouldWaitForSkillBreakthroughAfterClick()
	{
		return targetType == GuideTargetType.MaskToSkillBreakthroughConfirmButton;
	}

	private bool ShouldWaitForIdolWorshipAfterClick()
	{
		return targetType == GuideTargetType.MaskToIdolWorshipButton;
	}

	private bool IsSelectableConfigSkillSlot(ProfileSkillPresetSlotCell cell)
	{
		if (cell != null && !cell.isInner && cell.skillSlot != null && cell.character != null)
		{
			return cell.skillSlot.IsUnLocked(cell.character.GenerateCharacterConfigInit(UIBase.applicationState));
		}
		return false;
	}

	private bool CanTargetSkillConfigSkill(SkillDisplay skillDisplay, SkillConfigPresetPanel skillConfigPresetPanel)
	{
		if (!IsSkillConfigPanelActive(skillConfigPresetPanel))
		{
			return true;
		}
		ProfileSkill profileSkill = skillDisplay?.setupData?.profileSkill;
		if (skillConfigPresetPanel.selectedCell != null && profileSkill != null)
		{
			return skillConfigPresetPanel.skillSelectArea.CanConfig(profileSkill);
		}
		return false;
	}

	private bool CanConfirmSkillConfigSelection(SkillConfigPresetPanel skillConfigPresetPanel)
	{
		ProfileSkill profileSkill = skillConfigPresetPanel?.skillSelectArea?.selectedSkill;
		if (skillConfigPresetPanel?.selectedCell != null && profileSkill != null)
		{
			return skillConfigPresetPanel.skillSelectArea.CanConfig(profileSkill);
		}
		return false;
	}

	private bool IsSkillConfigPanelActive(SkillConfigPresetPanel skillConfigPresetPanel)
	{
		SkillConfigPanel skillConfigPanel = UIBase.fortressState?.uiManager?.skillConfigPanel;
		if (skillConfigPanel != null && skillConfigPanel.gameObject.activeInHierarchy && skillConfigPresetPanel != null)
		{
			return skillConfigPresetPanel.gameObject.activeInHierarchy;
		}
		return false;
	}

	private bool ShouldFinishMissingMaskTargetImmediately()
	{
		if (targetType == GuideTargetType.MaskToGachaBackButton)
		{
			if (!(UIBase.fortressState?.uiManager?.gachaPanel == null))
			{
				return !UIBase.fortressState.uiManager.gachaPanel.gameObject.activeInHierarchy;
			}
			return true;
		}
		if (targetType == GuideTargetType.MaskToSkillGemBackButton)
		{
			if (!(UIBase.fortressState?.uiManager?.skill_GemPanel == null))
			{
				return !UIBase.fortressState.uiManager.skill_GemPanel.gameObject.activeInHierarchy;
			}
			return true;
		}
		SkillConfigPresetPanel skillConfigPresetPanel = UIBase.fortressState?.uiManager?.skillConfigPanel?.skillConfigPresetPanel;
		if (!IsSkillConfigPanelActive(skillConfigPresetPanel))
		{
			return false;
		}
		if (targetType != GuideTargetType.MaskToIndexConfigSkillSlot && targetType != GuideTargetType.MaskToCharacterConfigSkillCell)
		{
			if (targetType == GuideTargetType.MaskToSelectSkillActivateButton)
			{
				return !CanConfirmSkillConfigSelection(skillConfigPresetPanel);
			}
			return false;
		}
		return true;
	}

	private bool ShouldFinishCompletedSkillConfigStepImmediately()
	{
		SkillConfigPresetPanel skillConfigPresetPanel = UIBase.fortressState?.uiManager?.skillConfigPanel?.skillConfigPresetPanel;
		if (!IsSkillConfigPanelActive(skillConfigPresetPanel))
		{
			return false;
		}
		ProfileSkillPreset skillPreset = skillConfigPresetPanel.skillPreset;
		if (skillPreset == null || skillPreset.configSkillSlots?.Any((PresetSkillSlot slot) => slot?.skill != null) != true)
		{
			return false;
		}
		if (targetType != GuideTargetType.MaskToIndexConfigSkillSlot && targetType != GuideTargetType.MaskToCharacterConfigSkillCell)
		{
			return targetType == GuideTargetType.MaskToSelectSkillActivateButton;
		}
		return true;
	}

	private bool IsCurrentSkillConfigSlotConfigured()
	{
		return (UIBase.fortressState?.uiManager?.skillConfigPanel?.skillConfigPresetPanel?.selectedCell)?.skillSlot?.skill != null;
	}

	private bool IsCurrentSkillConfigEvent((PresetSkillSlot slot, ProfileSkill skill) data)
	{
		PresetSkillSlot presetSkillSlot = UIBase.fortressState?.uiManager?.skillConfigPanel?.skillConfigPresetPanel?.selectedCell?.skillSlot;
		if (presetSkillSlot != null)
		{
			return data.slot == presetSkillSlot;
		}
		return true;
	}

	private IEnumerator ClickTargetAndWaitForConfigSkillSuccess(PointerEventData eventData)
	{
		bool configSuccess = false;
		UIBase.applicationState.eventManager.configSkillPresetEvent.AddListener(OnConfigSkillPreset);
		ForwardPointerEventToTarget(eventData);
		yield return new WaitUntil(() => configSuccess || !base.gameObject.activeInHierarchy);
		UIBase.applicationState.eventManager.configSkillPresetEvent.RemoveListener(OnConfigSkillPreset);
		if (configSuccess)
		{
			FinishGuide();
		}
		void OnConfigSkillPreset((PresetSkillSlot slot, ProfileSkill skill) data)
		{
			if (IsCurrentSkillConfigEvent(data))
			{
				configSuccess = true;
			}
		}
	}

	private IEnumerator ClickTargetAndWaitForSkillBreakthrough(PointerEventData eventData)
	{
		bool breakthroughSuccess = false;
		UIBase.applicationState.eventManager.breakThroughSkillEvent.AddListener(OnBreakthroughSkill);
		ForwardPointerEventToTarget(eventData);
		yield return new WaitUntil(() => breakthroughSuccess || !base.gameObject.activeInHierarchy);
		UIBase.applicationState.eventManager.breakThroughSkillEvent.RemoveListener(OnBreakthroughSkill);
		if (breakthroughSuccess)
		{
			FinishGuide();
		}
		void OnBreakthroughSkill(ProfileSkill skill)
		{
			if (skillDef == null || skill?.def == skillDef)
			{
				breakthroughSuccess = true;
			}
		}
	}

	private IEnumerator ClickTargetAndWaitForIdolWorship(PointerEventData eventData)
	{
		bool worshipSuccess = false;
		UIBase.applicationState.eventManager.idolUpgradeCompleteEvent.AddListener(OnIdolUpgradeComplete);
		ForwardPointerEventToTarget(eventData);
		yield return new WaitUntil(() => worshipSuccess || !base.gameObject.activeInHierarchy);
		UIBase.applicationState.eventManager.idolUpgradeCompleteEvent.RemoveListener(OnIdolUpgradeComplete);
		if (worshipSuccess)
		{
			FinishGuide();
		}
		void OnIdolUpgradeComplete(IdolState idolState)
		{
			if (IsTargetIdolState(idolState))
			{
				worshipSuccess = true;
			}
		}
	}

	private static RectTransform ActiveTargetOrNull(RectTransform rectTransform)
	{
		if (!(rectTransform != null) || !rectTransform.gameObject.activeInHierarchy)
		{
			return null;
		}
		return rectTransform;
	}

	private bool IsTargetIdolState(IdolState idolState)
	{
		if (idolState == null)
		{
			return false;
		}
		if (characterClassType != CharacterClassType.None)
		{
			return idolState.idolDef?.attachClass == characterClassType;
		}
		return true;
	}

	private void SetMaskMat(float radius)
	{
		Vector3 vector = base.rectTransform.WorldPositionToAnchored(TargetWorldRect.center);
		SetMaskCenterAndRadius(new Vector4(vector.x, vector.y, radius, 0f));
	}

	public void SetMaskCenterAndRadius(Vector4 centerAndRadius)
	{
		guideMaskMat.SetVector("_Origin", centerAndRadius);
	}

	private static Rect RectTransformToWorldSpace(RectTransform transform)
	{
		Vector2 vector = Vector2.Scale(transform.rect.size, transform.lossyScale);
		float x = transform.position.x - transform.pivot.x * vector.x;
		float y = transform.position.y - (1f - transform.pivot.y) * vector.y;
		return new Rect(x, y, vector.x, vector.y);
	}

	private static Rect RectTransformCornersToWorldSpace(RectTransform transform)
	{
		Vector3[] array = new Vector3[4];
		transform.GetWorldCorners(array);
		Rect result = new Rect(array[0], Vector2.zero);
		for (int i = 1; i < array.Length; i++)
		{
			result.xMin = Mathf.Min(result.xMin, array[i].x);
			result.xMax = Mathf.Max(result.xMax, array[i].x);
			result.yMin = Mathf.Min(result.yMin, array[i].y);
			result.yMax = Mathf.Max(result.yMax, array[i].y);
		}
		return result;
	}

	private void UseTargetAndChildrenRect(RectTransform root)
	{
		if (!(root == null))
		{
			List<RectTransform> list = (from rect in root.GetComponentsInChildren<RectTransform>(includeInactive: false)
				where rect != null && rect.gameObject.activeInHierarchy
				select rect).ToList();
			if (list.Count != 0)
			{
				Rect value = CombineRects(list.Select(RectTransformToWorldSpace));
				targetWorldRectOverride = value;
				targetScreenRectOverride = CombineRects(list.Select(RectTransformToScreenSpace));
				targetRadiusOverride = value.height / 2f + extraRadius;
			}
		}
	}

	private static Rect RectTransformToScreenSpace(RectTransform transform)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = new Vector3[4];
		transform.GetWorldCorners(array);
		Canvas componentInParent = transform.GetComponentInParent<Canvas>(includeInactive: true);
		Camera camera = (((Object)(object)componentInParent != null && (int)componentInParent.renderMode != 0) ? componentInParent.worldCamera : null);
		Rect result = new Rect(RectTransformUtility.WorldToScreenPoint(camera, array[0]), Vector2.zero);
		for (int i = 1; i < array.Length; i++)
		{
			Vector2 vector = RectTransformUtility.WorldToScreenPoint(camera, array[i]);
			result.xMin = Mathf.Min(result.xMin, vector.x);
			result.xMax = Mathf.Max(result.xMax, vector.x);
			result.yMin = Mathf.Min(result.yMin, vector.y);
			result.yMax = Mathf.Max(result.yMax, vector.y);
		}
		return result;
	}

	private static Rect CombineRects(IEnumerable<Rect> rects)
	{
		Rect result = default(Rect);
		bool flag = false;
		foreach (Rect rect in rects)
		{
			if (!flag)
			{
				result = rect;
				flag = true;
				continue;
			}
			result.xMin = Mathf.Min(result.xMin, rect.xMin);
			result.xMax = Mathf.Max(result.xMax, rect.xMax);
			result.yMin = Mathf.Min(result.yMin, rect.yMin);
			result.yMax = Mathf.Max(result.yMax, rect.yMax);
		}
		return result;
	}

	private bool TargetContainsScreenPoint(Vector3 position)
	{
		if (target == null)
		{
			return false;
		}
		if (targetScreenRectOverride.HasValue)
		{
			return targetScreenRectOverride.Value.Contains(position);
		}
		Camera camera = null;
		Canvas componentInParent = target.GetComponentInParent<Canvas>(includeInactive: true);
		if ((Object)(object)componentInParent != null && componentInParent.worldCamera != null)
		{
			camera = componentInParent.worldCamera;
		}
		if (RectTransformUtility.RectangleContainsScreenPoint(target, (Vector2)position, camera))
		{
			return true;
		}
		return false;
	}

	private IEnumerator WaitForTargetInactive()
	{
		GameObject waitTarget = ((presetTarget != null) ? presetTarget.gameObject : GetDynamicWaitTarget());
		if (waitTarget == null)
		{
			Debug.LogWarning($"GuideMask presetTarget not assigned: {targetType}", this);
			finished = true;
			yield break;
		}
		yield return new WaitUntil(() => !waitTarget.activeInHierarchy);
		finished = true;
	}

	private IEnumerator WaitForTargetActive()
	{
		GameObject waitTarget = ((presetTarget != null) ? presetTarget.gameObject : GetDynamicWaitTarget());
		if (waitTarget == null)
		{
			Debug.LogWarning($"GuideMask presetTarget not assigned: {targetType}", this);
			finished = true;
			yield break;
		}
		yield return new WaitUntil(() => waitTarget.activeInHierarchy);
		finished = true;
	}

	private GameObject GetDynamicWaitTarget()
	{
		if (gacahPool != null)
		{
			return UIBase.fortressState?.uiManager?.gachaResultDisplay?.gameObject;
		}
		return null;
	}

	private IEnumerator WaitGachaResultDisplayActive()
	{
		GachaResultDisplay resultDisplay = UIBase.fortressState?.uiManager?.gachaResultDisplay;
		if (resultDisplay == null)
		{
			finished = true;
			yield break;
		}
		yield return new WaitUntil(() => resultDisplay.gameObject.activeInHierarchy);
		finished = true;
	}

	private IEnumerator WaitGachaResultDisplayInactive()
	{
		GachaResultDisplay resultDisplay = UIBase.fortressState?.uiManager?.gachaResultDisplay;
		if (resultDisplay == null)
		{
			finished = true;
			yield break;
		}
		yield return new WaitUntil(() => !resultDisplay.gameObject.activeInHierarchy);
		finished = true;
	}

	private IEnumerator WaitSkillBreakthroughComplete()
	{
		bool breakthroughSuccess = false;
		UIBase.applicationState.eventManager.breakThroughSkillEvent.AddListener(OnBreakthroughSkill);
		yield return new WaitUntil(() => breakthroughSuccess || !base.gameObject.activeInHierarchy);
		UIBase.applicationState.eventManager.breakThroughSkillEvent.RemoveListener(OnBreakthroughSkill);
		if (breakthroughSuccess)
		{
			finished = true;
		}
		void OnBreakthroughSkill(ProfileSkill skill)
		{
			if (skillDef == null || skill?.def == skillDef)
			{
				breakthroughSuccess = true;
			}
		}
	}

	private IEnumerator WaitGlobalRewardPackageDisplayInactive()
	{
		GlobalRewardPackageDisplay rewardDisplay = UIBase.fortressState?.uiManager?.globalRewardPackageDisplay;
		if (rewardDisplay == null)
		{
			finished = true;
			yield break;
		}
		yield return new WaitUntil(() => !rewardDisplay.gameObject.activeInHierarchy);
		finished = true;
	}

	private IEnumerator WaitIdolWorshipComplete()
	{
		bool worshipSuccess = false;
		UIBase.applicationState.eventManager.idolUpgradeCompleteEvent.AddListener(OnIdolUpgradeComplete);
		yield return new WaitUntil(() => worshipSuccess || !base.gameObject.activeInHierarchy);
		UIBase.applicationState.eventManager.idolUpgradeCompleteEvent.RemoveListener(OnIdolUpgradeComplete);
		if (worshipSuccess)
		{
			finished = true;
		}
		void OnIdolUpgradeComplete(IdolState idolState)
		{
			if (IsTargetIdolState(idolState))
			{
				worshipSuccess = true;
			}
		}
	}

	private IEnumerator WaitForGacahSetActive()
	{
		yield return new WaitUntil(() => UIBase.fortressState?.uiManager?.gachaPanel?.gachaSet?.PoolState?.def == gacahPool);
		finished = true;
	}

	private IEnumerator WaitNormalEventPanelChoiceSetup()
	{
		yield return new WaitUntil(delegate
		{
			NormalEventPanel normalEventPanel = UIBase.gameState?.UIManager?.worldEventPanel_Normal;
			return normalEventPanel?.choicePool != null && normalEventPanel.choicePool.activeCount > 0 && !normalEventPanel.refreshing;
		});
		finished = true;
	}

	private IEnumerator WaitConfigSkillPresetSuccess()
	{
		if (IsCurrentSkillConfigSlotConfigured())
		{
			finished = true;
			yield break;
		}
		bool configSuccess = false;
		UIBase.applicationState.eventManager.configSkillPresetEvent.AddListener(OnConfigSkillPreset);
		yield return new WaitUntil(() => configSuccess || IsCurrentSkillConfigSlotConfigured() || !base.gameObject.activeInHierarchy);
		UIBase.applicationState.eventManager.configSkillPresetEvent.RemoveListener(OnConfigSkillPreset);
		finished = configSuccess || IsCurrentSkillConfigSlotConfigured();
		void OnConfigSkillPreset((PresetSkillSlot slot, ProfileSkill skill) data)
		{
			if (IsCurrentSkillConfigEvent(data))
			{
				configSuccess = true;
			}
		}
	}
}
