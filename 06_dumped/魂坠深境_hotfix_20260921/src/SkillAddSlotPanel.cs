using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using ContainerOfSoul.Display.Fortress;
using TMPro;
using UnityEngine;

public class SkillAddSlotPanel : UIBase, IShowAsync, IHideAsync
{
	[SerializeField]
	private SkillGemSlotDisplay singleSkillGemSlotDisplay;

	[SerializeField]
	private GameButton helpButton;

	[Header("Skill Area")]
	[SerializeField]
	private SkillDisplay skillDisplay;

	[Header("Preview Area")]
	[SerializeField]
	private GameObject addSlotArea;

	[SerializeField]
	private GameObject changeSlotAreaGo;

	[SerializeField]
	private SkillGemSlotDisplay preSkillGemSlotDisplay;

	[SerializeField]
	private SkillGemSlotDisplay afterSkillGemSlotDisplay;

	[Header("Slot Operation Area")]
	[SerializeField]
	private GameObject bottomArea;

	[SerializeField]
	private GameButton addSlotButton;

	[SerializeField]
	private GameButton cancelSlotButton;

	[SerializeField]
	private GameButton confirmSlotButton;

	[SerializeField]
	private TextMeshProUGUI cancelSlotCountText;

	[SerializeField]
	private TextMeshProUGUI confirmSlotCountText;

	[SerializeField]
	private TextMeshProUGUI warnTextRange;

	[SerializeField]
	private TextMeshProUGUI confirmButtonText;

	[Header("Cost Matter")]
	[SerializeField]
	private MatterCellGroup matterCellGroup1;

	[SerializeField]
	private GameObject unSlotMark;

	[SerializeField]
	private RectTransform singleHexShapeBg;

	[SerializeField]
	private NFXSetup effect_UI_GemSlotChang_NotifyEffect;

	[SerializeField]
	private Animator animator;

	private const int SlotCap = 15;

	[NonSerialized]
	public Action<ProfileSkill> onSlotChanged;

	private HexShape currentShape;

	private FortressDataManager.SkillAddSlotMatterCost matterCost;

	private bool isAddSlotRunning;

	private bool isComparingSlotShape;

	private SkillUpgradePanel skillUpgradePanel => parent as SkillUpgradePanel;

	private ProfileSkill currentSkill => skillUpgradePanel.selectingSkill;

	private bool enableAddSlot => UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillAddSlot);

	protected override void Init()
	{
		InitUI(singleSkillGemSlotDisplay);
		InitUI(skillDisplay);
		InitUI(matterCellGroup1);
		InitUI(preSkillGemSlotDisplay);
		InitUI(afterSkillGemSlotDisplay);
		DisableSlotOperation();
		HideComparingArea();
		cancelSlotButton.onClick.AddListener(OnCancelSlotClicked);
		addSlotButton.onClick.AddListener(StartAddSlot);
		confirmSlotButton.onClick.AddListener(ConfirmAddSlot);
		helpButton.onClick.AddListener(delegate
		{
			UIBase.fortressState.uiManager.fortressGuidePanel.addSkillSlotGuideHint.Show();
		});
	}

	void IShowAsync.OnShowStart()
	{
		if (!enableAddSlot)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillAddSlot));
			base.gameObject.SetActive(value: false);
			return;
		}
		if (currentSkill == null)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill);
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		isAddSlotRunning = false;
		isComparingSlotShape = false;
		skillDisplay.Setup(currentSkill.GetGameSkill());
		matterCost = ScriptableObjectSingleton<FortressDataManager>.instance.skillAddSlotMatterCosts.FirstOrDefault((FortressDataManager.SkillAddSlotMatterCost m) => m.qualityLevel == currentSkill.QualityLevel);
		matterCellGroup1.Setup(matterCost.matterInfos);
		if (currentSkill.QualityLevel == QualityLevel.None)
		{
			singleSkillGemSlotDisplay.gameObject.SetActive(value: false);
			bottomArea.SetActive(value: false);
			TraceLog.Warn("SkillAddSlotPanel: current skill quality level is not suitable!");
			return;
		}
		bottomArea.SetActive(value: true);
		addSlotArea.SetActive(value: true);
		if (currentSkill.hexShape != null)
		{
			singleSkillGemSlotDisplay.gameObject.SetActive(value: true);
			UpdateHexShapeImage();
		}
		else
		{
			singleSkillGemSlotDisplay.gameObject.SetActive(value: false);
		}
		changeSlotAreaGo.SetActive(value: false);
		DisableSlotOperation();
		UIBase.fortressState.uiManager.fortressGuidePanel.addSkillSlotGuideHint.TryFirstOpenAutoPopup();
		UpdateWarnText(currentSkill.HexShapeSlotCount);
	}

	public IEnumerator ShowAnim()
	{
		if (!((UnityEngine.Object)(object)animator == null))
		{
			((Behaviour)(object)animator).enabled = true;
			animator.SetTrigger(AnimKeywords.show);
			yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		}
	}

	public IEnumerator HideAnim()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: false);
			yield break;
		}
		if ((UnityEngine.Object)(object)animator == null)
		{
			base.gameObject.SetActive(value: false);
			yield break;
		}
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.hide);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		unSlotMark.gameObject.SetActive(value: false);
		if (currentSkill.hexShape == null)
		{
			unSlotMark.gameObject.SetActive(value: true);
		}
	}

	private void OnCancelSlotClicked()
	{
		int count = currentSkill.hexShape.points.Count;
		int count2 = currentShape.points.Count;
		ThinkingAnalyticsExtends.TrackFortressSkillAddGemSlot(UIBase.applicationState, currentSkill, count2, count, isApplyAddSlot: false);
		UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skillgem_slot_state_saved);
		HideComparingArea();
		DisableSlotOperation();
	}

	private void UpdateHexShapeImage()
	{
		singleSkillGemSlotDisplay.Setup(currentSkill.hexShape, HexagonHelper.HexCellSize);
	}

	private void AfterStartAddSlotToSkill()
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		if (currentSkill.hexShape == null)
		{
			int oldGemSlotCount = 0;
			int count = currentShape.points.Count;
			currentSkill.hexShape = currentShape;
			if (!GlobalConfigManager.instance.enableNetwork)
			{
				UIBase.applicationState.savingManager.SetProfileDirtyToSave();
			}
			singleSkillGemSlotDisplay.gameObject.SetActive(value: true);
			UpdateHexShapeImage();
			DisableSlotOperation();
			ThinkingAnalyticsExtends.TrackFortressSkillAddGemSlot(UIBase.applicationState, currentSkill, count, oldGemSlotCount, isApplyAddSlot: true);
			UIBase.applicationState.nfxManager.SpawnAndPlay(effect_UI_GemSlotChang_NotifyEffect, singleHexShapeBg, singleHexShapeBg.position);
			UIBase.fortressState.uiManager.quickHintPanel.HintText(base.textData.term_got_slot);
		}
		else
		{
			isComparingSlotShape = true;
			ShowComparingArea();
			UpdateComparingHexShapeImage();
			EnableSlotOperation();
		}
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_amulet_gem_slot);
		UIBase.applicationState.eventManager.addSkillGemSlotEvent.TriggerEvent(currentSkill);
		onSlotChanged?.Invoke(currentSkill);
		SetDirty();
	}

	private void StartAddSlot()
	{
		if (!enableAddSlot)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillAddSlot));
		}
		else
		{
			if (isAddSlotRunning || !GlobalConfigManager.instance.enableNetwork)
			{
				return;
			}
			if (!CanConsumeMatter())
			{
				TraceLog.Warn("SkillAddSlotPanel: matter is not enough!");
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skillgem_insufficient_matter_cost);
				return;
			}
			isAddSlotRunning = true;
			addSlotButton.interactable = false;
			AddSlotToSkillRequest request = new AddSlotToSkillRequest
			{
				SkillGuid = currentSkill.profileSkillGUID
			};
			GeneralGameAsyncOperation<AddSlotToSkillRequest, AddSlotToSkillResult> op = new GeneralGameAsyncOperation<AddSlotToSkillRequest, AddSlotToSkillResult>(request, async (AddSlotToSkillRequest c) => await UIBase.applicationState.ChillyRoomSDK.Skill.StartAddSlotToSkillAsync(c));
			op.OnComplete(delegate
			{
				HexShapeSvr hexShape = op.OpResult.HexShape;
				currentShape = hexShape.ToClient();
				UIBase.applicationState.matterManager.ConsumeMatter(matterCost.matterInfos, "护符打孔");
				AfterStartAddSlotToSkill();
				isAddSlotRunning = false;
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_updating_skill_gem_slots);
		}
	}

	private void ConfirmAddSlot()
	{
		if (!enableAddSlot)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillAddSlot));
		}
		else if (currentSkill?.hexShape?.points == null || currentShape?.points == null)
		{
			TraceLog.Warn("SkillAddSlotPanel: confirm add slot failed, slot shape is invalid!");
		}
		else if (currentShape.points.Count < currentSkill.hexShape.points.Count)
		{
			SecondConfirmInfo secondConfirmInfo = new SecondConfirmInfo();
			secondConfirmInfo.description = ScriptableObjectSingleton<TextDataPreset>.instance.term_new_skill_slot_with_less_slot_count_ask_confirm;
			secondConfirmInfo.confirmChoiceText = ScriptableObjectSingleton<TextDataPreset>.instance.term_confirm;
			secondConfirmInfo.cancelChoiceText = ScriptableObjectSingleton<TextDataPreset>.instance.term_cancel;
			secondConfirmInfo.confirmCallback = DoConfirm;
			UIBase.fortressState.uiManager.commonSecondConfirmPanel.Setup(secondConfirmInfo);
		}
		else
		{
			DoConfirm();
		}
		void DoConfirm()
		{
			if (GlobalConfigManager.instance.enableNetwork)
			{
				UIBase.applicationState.skillGemManager.ConfirmAddSlotToSkill(delegate(ChillyRoom.Services.ContainerOfSoul.ProfileSkill skill)
				{
					AfterConfirmAddSlot(UIBase.applicationState.skillGemManager.ConvertHexShapeFromServer(skill.HexShape));
				});
			}
			else
			{
				AfterConfirmAddSlot(currentShape);
			}
		}
	}

	private void AfterConfirmAddSlot(HexShape newShape)
	{
		if (currentSkill?.hexShape?.points == null || newShape?.points == null)
		{
			TraceLog.Warn("SkillAddSlotPanel: after confirm add slot failed, slot shape is invalid!");
			return;
		}
		int count = currentSkill.hexShape.points.Count;
		int count2 = newShape.points.Count;
		currentSkill.hexShape = newShape;
		if (currentSkill.skillGemsMounted != null)
		{
			foreach (SkillGem item in currentSkill.skillGemsMounted)
			{
				UIBase.applicationState.skillGemManager.AddGem(item);
			}
			currentSkill.skillGemsMounted.Clear();
		}
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			UIBase.applicationState.savingManager.SetProfileDirtyToSave();
		}
		UIBase.fortressState.uiManager.quickHintPanel.HintText(base.textData.term_skillgem_slot_state_keeped);
		ThinkingAnalyticsExtends.TrackFortressSkillAddGemSlot(UIBase.applicationState, currentSkill, count2, count, isApplyAddSlot: true);
		UpdateHexShapeImage();
		HideComparingArea();
		DisableSlotOperation();
		onSlotChanged?.Invoke(currentSkill);
		SetDirty();
	}

	private void UpdateHexShapeImage(SkillGemSlotDisplay skillGemSlotDisplay, HexShape hexShape, float hexCellSize)
	{
		skillGemSlotDisplay.Setup(hexShape, hexCellSize);
	}

	private void UpdateWarnText(int oldSlotCount)
	{
		if (!warnTextRange)
		{
			return;
		}
		warnTextRange.gameObject.SetActive(value: false);
		if (oldSlotCount >= 15)
		{
			warnTextRange.gameObject.SetActive(value: true);
			return;
		}
		int maxSlotCount = UIBase.applicationState.skillGemManager.GetMaxSlotCount(oldSlotCount);
		maxSlotCount = Mathf.Clamp(maxSlotCount, oldSlotCount, 15);
		int num = Mathf.Min(oldSlotCount + 1, maxSlotCount);
		warnTextRange.text = base.textData._term_MaxSlot.FormatWith(num, maxSlotCount);
		if (num >= maxSlotCount)
		{
			warnTextRange.gameObject.SetActive(value: true);
			warnTextRange.text = base.textData._term_AddSlotTo.FormatWith(num, maxSlotCount);
		}
		else
		{
			warnTextRange.gameObject.SetActive(value: true);
			warnTextRange.text = base.textData._term_AddSlotRange.FormatWith(num, maxSlotCount);
		}
	}

	private void UpdateComparingHexShapeImage()
	{
		UpdateHexShapeImage(preSkillGemSlotDisplay, currentSkill.hexShape, HexagonHelper.HexCellSize);
		UpdateHexShapeImage(afterSkillGemSlotDisplay, currentShape, HexagonHelper.HexCellSize);
		PlayHexShapeDiffEffects();
		cancelSlotCountText.text = base.textData.term_slot_count.FormatWith(currentSkill.HexShapeSlotCount);
		confirmSlotCountText.text = base.textData.term_slot_count.FormatWith(currentShape.points.Count);
		if (currentShape.points.Count > currentSkill.hexShape.points.Count)
		{
			confirmSlotCountText.text += ScriptableObjectSingleton<GameDataManager>.instance.increaseTextIcon.ToTMPSpriteText();
		}
		else if (currentShape.points.Count < currentSkill.hexShape.points.Count)
		{
			confirmSlotCountText.text += ScriptableObjectSingleton<GameDataManager>.instance.decreaseTextIcon.ToTMPSpriteText();
		}
		UpdateWarnText(currentSkill.HexShapeSlotCount);
	}

	private void PlayHexShapeDiffEffects()
	{
		HashSet<Vector2Int> oldPoints = GetActualHexPoints(currentSkill.hexShape);
		HashSet<Vector2Int> newPoints = GetActualHexPoints(currentShape);
		List<Vector2Int> points = oldPoints.Where((Vector2Int p) => !newPoints.Contains(p)).ToList();
		List<Vector2Int> points2 = newPoints.Where((Vector2Int p) => !oldPoints.Contains(p)).ToList();
		afterSkillGemSlotDisplay.PlayRemovedSlotEffects(points, HexagonHelper.HexCellSize);
		afterSkillGemSlotDisplay.PlayAddedSlotEffects(points2, HexagonHelper.HexCellSize);
	}

	private HashSet<Vector2Int> GetActualHexPoints(HexShape shape)
	{
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
		if (shape?.points == null)
		{
			return hashSet;
		}
		foreach (Vector2Int point in shape.points)
		{
			hashSet.Add(point + shape.offset);
		}
		return hashSet;
	}

	private bool CanConsumeMatter()
	{
		foreach (MatterInfo matterInfo in matterCost.matterInfos)
		{
			if (UIBase.applicationState.matterManager.GetMatterCount(matterInfo.matterDef) < matterInfo.count)
			{
				return false;
			}
		}
		return true;
	}

	private void GenerateHexShape()
	{
		HexShape hexShape = currentSkill.hexShape;
		int valueOrDefault = (hexShape?.points?.Count).GetValueOrDefault();
		int randomSlotCount = UIBase.applicationState.skillGemManager.GetRandomSlotCount(valueOrDefault);
		if (randomSlotCount == 0)
		{
			TraceLog.Error("SkillAddSlotPanel: get random slot count failed!");
			return;
		}
		currentShape = ((hexShape == null || valueOrDefault == 0) ? HexagonHelper.GenerateRandomSpreadHexShape(randomSlotCount, HexagonHelper.HexShapeMaxRadius) : HexagonHelper.GenerateMutatedHexShape(hexShape, randomSlotCount, HexagonHelper.HexShapeMaxRadius));
		if (currentShape.points.Count == 0)
		{
			TraceLog.Error("SkillAddSlotPanel: generate hex shape failed!");
		}
	}

	private void EnableSlotOperation()
	{
		matterCellGroup1.SetDirty();
		addSlotButton.interactable = true;
		confirmButtonText.text = base.textData._term_ChangeSlot;
		cancelSlotButton.gameObject.SetActive(value: true);
		confirmSlotButton.gameObject.SetActive(value: true);
	}

	private void DisableSlotOperation()
	{
		matterCellGroup1.SetDirty();
		isComparingSlotShape = false;
		addSlotButton.interactable = true;
		confirmButtonText.text = base.textData._term_AddSlot;
		cancelSlotButton.gameObject.SetActive(value: false);
		confirmSlotButton.gameObject.SetActive(value: false);
	}

	private void ShowComparingArea()
	{
		changeSlotAreaGo.SetActive(value: true);
		addSlotArea.SetActive(value: false);
	}

	private void HideComparingArea()
	{
		changeSlotAreaGo.SetActive(value: false);
		addSlotArea.SetActive(value: true);
		preSkillGemSlotDisplay.ClearSlotEffects();
		afterSkillGemSlotDisplay.ClearSlotEffects();
	}
}
