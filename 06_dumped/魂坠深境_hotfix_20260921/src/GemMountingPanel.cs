using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GemMountingPanel : UIBase, IShowAsync, IHideAsync
{
	[Header("Template Area")]
	[SerializeField]
	private SkillGemInfoPanel infoPanel;

	[SerializeField]
	private SkillGemDisplay skillGemDisplay;

	[SerializeField]
	private GemMountedDisplay mountedDisplay;

	[Header("Skill Area")]
	[SerializeField]
	public SkillInfoDisplay skillDisplay;

	[Header("Description Area")]
	[SerializeField]
	private CellUIPrefabPoolConfig<GemWordCell, GemWordCell.CellData> descriptionPool;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[SerializeField]
	private Image skillGemSlotDisplayBg;

	[SerializeField]
	private SkillGemSlotDisplay skillGemSlotDisplay;

	[Header("Button Area")]
	[SerializeField]
	private GameButton helpButton;

	[SerializeField]
	private GameButton confirmButton;

	[Header("Skill Display Collapse")]
	[SerializeField]
	private GameButton skillDisplayCollapseButton;

	[SerializeField]
	private RectTransform skillDisplayCollapseArrow;

	[SerializeField]
	private GameObject[] skillDisplayCollapsibleObjects = Array.Empty<GameObject>();

	[SerializeField]
	private GameObject skillDisplayCollapsedLayoutSpacer;

	[SerializeField]
	private TMP_Dropdown sortDropDown;

	[SerializeField]
	private GameObject unSlotMark;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	private CellUIPrefabPool<SkillGemDisplay, SkillGem> skillGemDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<GemMountedDisplay, SkillGem> mountedSkillGemPool;

	[NonSerialized]
	private GemMountingDraggerModule selectedInventoryGem;

	[NonSerialized]
	private GemMountedDisplay selectedMountedGem;

	[NonSerialized]
	private GemMountedDisplay draggingMountedGem;

	[NonSerialized]
	public bool isDragging;

	[NonSerialized]
	private List<SkillGem> allGemsDeepCopy;

	[NonSerialized]
	private bool preventDragging;

	[NonSerialized]
	private bool isDragValid;

	[NonSerialized]
	private RectTransform draggingGemRect;

	[NonSerialized]
	private Vector2Int dragStartOffset;

	[NonSerialized]
	private int dragStartRotateIndex;

	[NonSerialized]
	private Vector3 dragStartLocalPosition;

	[NonSerialized]
	private GemMountingBoardInput boardInput;

	[NonSerialized]
	private bool isSkillDisplayCollapsed;

	[NonSerialized]
	private Vector2 skillDisplayExpandedAnchoredPosition;

	[NonSerialized]
	private bool[] skillDisplayExpandedObjectStates;

	private List<GemMountedDisplay> tempMountedSkillGems => mountedSkillGemPool.GetActiveElements().ToList();

	private SkillUpgradePanel skillUpgradePanel => parent as SkillUpgradePanel;

	private ProfileSkill currentSkill => skillUpgradePanel.selectingSkill;

	private bool enableMountGem => UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillMountGem);

	protected override void Init()
	{
		InitUI(infoPanel);
		InitUI(skillGemSlotDisplay);
		InitUI(skillDisplay);
		if (skillDisplay != null && skillDisplay.transform is RectTransform rectTransform)
		{
			skillDisplayExpandedAnchoredPosition = rectTransform.anchoredPosition;
		}
		Init(descriptionPool);
		GetPrefabPool(ref skillGemDisplayPool, skillGemDisplay, null, infoPanel);
		GetPrefabPool(ref mountedSkillGemPool, mountedDisplay, null, infoPanel);
		mountedDisplay.gameObject.SetActive(value: false);
		InitBoardInput();
		UIHelper.RegisterEmptyAreaClickedEvent(OnEmptyAreaClicked);
		sortDropDown.options = UIBase.applicationState.skillGemManager.GetSortOptions();
		sortDropDown.onValueChanged.AddListener(OnSubGroupOptionChange);
		helpButton.onClick.AddListener(delegate
		{
			UIBase.fortressState.uiManager.fortressGuidePanel.mountSkillGemGuideHint.Show();
		});
		confirmButton.onClick.AddListener(TryConfirm);
		if (skillDisplayCollapseButton != null)
		{
			skillDisplayCollapseButton.onClick.AddListener(ToggleSkillDisplay);
		}
	}

	private void InitBoardInput()
	{
		if (!(skillGemSlotDisplayBg == null))
		{
			boardInput = skillGemSlotDisplayBg.GetComponent<GemMountingBoardInput>();
			if (boardInput == null)
			{
				boardInput = skillGemSlotDisplayBg.gameObject.AddComponent<GemMountingBoardInput>();
			}
			boardInput.Setup(this);
		}
	}

	private void OnSubGroupOptionChange(int value)
	{
		ClearInventorySelection();
		SetDirty();
	}

	private void OnEmptyAreaClicked()
	{
		ClearSelection(hideInfo: true);
	}

	void IShowAsync.OnShowStart()
	{
		ResetUI();
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
		skillDisplay.gameObject.SetActive(value: false);
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: false);
			CleanupCloseState();
			yield break;
		}
		if ((UnityEngine.Object)(object)animator == null)
		{
			base.gameObject.SetActive(value: false);
			CleanupCloseState();
			yield break;
		}
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.hide);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		base.gameObject.SetActive(value: false);
		CleanupCloseState();
	}

	private void ResetUI()
	{
		ClearSelection(hideInfo: true);
		if (!enableMountGem)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillMountGem));
			base.gameObject.SetActive(value: false);
			return;
		}
		if (currentSkill == null)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill);
			base.gameObject.SetActive(value: false);
			return;
		}
		isDragging = false;
		isDragValid = false;
		draggingMountedGem = null;
		draggingGemRect = null;
		SetSkillDisplayCollapsed(collapsed: false);
		skillDisplay.Setup(currentSkill.GetGameSkill());
		SetSkillDisplayCollapsed(collapsed: false);
		allGemsDeepCopy = UIBase.applicationState.skillGemManager.DeepCopyAllSkillGems();
		if (currentSkill.hexShape != null)
		{
			preventDragging = false;
			skillGemSlotDisplay.Setup(currentSkill.hexShape, HexagonHelper.HexCellSize);
			skillGemSlotDisplay.gameObject.SetActive(value: true);
		}
		else
		{
			preventDragging = true;
			skillGemSlotDisplay.gameObject.SetActive(value: false);
		}
		mountedSkillGemPool.DespawnAll();
		if (currentSkill.skillGemsMounted != null)
		{
			foreach (SkillGem item in currentSkill.skillGemsMounted)
			{
				SyncGemEditState(item);
				GemMountedDisplay gemMountedDisplay = mountedSkillGemPool.Spawn(item, skillGemSlotDisplay.transform);
				Vector2 hexPointPosition = HexagonHelper.GetHexPointPosition(item.offset, HexagonHelper.HexCellSize);
				gemMountedDisplay.transform.localPosition = hexPointPosition;
				gemMountedDisplay.GetModule<GemMountingDraggerModule>().IsMountedGem = true;
				gemMountedDisplay.SetSelected(b: false);
			}
		}
		UIBase.fortressState.uiManager.fortressGuidePanel.mountSkillGemGuideHint.TryFirstOpenAutoPopup();
		SetActiveAndDirty(active: true);
	}

	private void ToggleSkillDisplay()
	{
		SetSkillDisplayCollapsed(!isSkillDisplayCollapsed);
	}

	private void SetSkillDisplayCollapsed(bool collapsed)
	{
		bool flag = isSkillDisplayCollapsed;
		isSkillDisplayCollapsed = collapsed;
		if (skillDisplayCollapseArrow != null)
		{
			skillDisplayCollapseArrow.localEulerAngles = new Vector3(0f, 0f, collapsed ? (-90f) : 90f);
		}
		if (skillDisplay == null || !(skillDisplay.transform is RectTransform rectTransform))
		{
			return;
		}
		skillDisplay.gameObject.SetActive(value: true);
		if (collapsed & flag)
		{
			return;
		}
		rectTransform.anchoredPosition = skillDisplayExpandedAnchoredPosition;
		GameObject[] array = skillDisplayCollapsibleObjects ?? Array.Empty<GameObject>();
		if (skillDisplayCollapsedLayoutSpacer != null)
		{
			skillDisplayCollapsedLayoutSpacer.SetActive(value: false);
		}
		if (!collapsed)
		{
			if (flag && skillDisplayExpandedObjectStates != null)
			{
				for (int i = 0; i < array.Length && i < skillDisplayExpandedObjectStates.Length; i++)
				{
					if (array[i] != null)
					{
						array[i].SetActive(skillDisplayExpandedObjectStates[i]);
					}
				}
			}
			RebuildSkillDisplayLayout(rectTransform);
			return;
		}
		RebuildSkillDisplayLayout(rectTransform);
		float height = rectTransform.rect.height;
		skillDisplayExpandedObjectStates = new bool[array.Length];
		for (int j = 0; j < array.Length; j++)
		{
			GameObject gameObject = array[j];
			if (!(gameObject == null))
			{
				skillDisplayExpandedObjectStates[j] = gameObject.activeSelf;
				gameObject.SetActive(value: false);
			}
		}
		if (skillDisplayCollapsedLayoutSpacer != null)
		{
			skillDisplayCollapsedLayoutSpacer.SetActive(value: true);
		}
		RebuildSkillDisplayLayout(rectTransform);
		float height2 = rectTransform.rect.height;
		Vector2 anchoredPosition = skillDisplayExpandedAnchoredPosition;
		anchoredPosition.y += (height - height2) * (1f - rectTransform.pivot.y);
		rectTransform.anchoredPosition = anchoredPosition;
	}

	private static void RebuildSkillDisplayLayout(RectTransform skillDisplayRect)
	{
		if (!(skillDisplayRect == null))
		{
			Canvas.ForceUpdateCanvases();
			LayoutRebuilder.ForceRebuildLayoutImmediate(skillDisplayRect);
			Canvas.ForceUpdateCanvases();
		}
	}

	protected override void Dispose()
	{
		UIHelper.UnregisterEmptyAreaClickedEvent(OnEmptyAreaClicked);
	}

	private void DecreaseSkillGemAmount(SkillGem skillGem)
	{
		SkillGem skillGem2 = allGemsDeepCopy.Find((SkillGem g) => g.def.id == skillGem.def.id);
		if (skillGem2 != null)
		{
			skillGem2.amount--;
			if (skillGem2.amount <= 0)
			{
				allGemsDeepCopy.Remove(skillGem2);
			}
			MarkInventoryDisplaysDirty();
		}
	}

	private void IncreaseSkillGemAmount(SkillGem skillGem)
	{
		SkillGem skillGem2 = allGemsDeepCopy.Find((SkillGem g) => g.def.id == skillGem.def.id);
		if (skillGem2 != null)
		{
			skillGem2.amount++;
		}
		else
		{
			allGemsDeepCopy.Add(new SkillGem(skillGem.def, 1));
		}
		MarkInventoryDisplaysDirty();
	}

	private void MarkInventoryDisplaysDirty()
	{
		if (skillGemDisplayPool == null)
		{
			return;
		}
		foreach (SkillGemDisplay activeElement in skillGemDisplayPool.GetActiveElements())
		{
			activeElement.SetDirty();
		}
	}

	protected override void UpdateDisplay()
	{
		if (currentSkill != null && allGemsDeepCopy != null)
		{
			unSlotMark.SetActive(currentSkill.HexShapeSlotCount == 0);
			confirmButton.interactable = enableMountGem && currentSkill.HexShapeSlotCount > 0;
			ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(UIBase.applicationState.skillGemManager.sortMethods[sortDropDown.value].Item2?.Invoke(allGemsDeepCopy), skillGemDisplayPool, layoutGroup);
			UpdateDescriptionArea();
		}
	}

	private void UpdateDescriptionArea()
	{
		descriptionPool.DespawnAll();
		foreach (SkillGem item in tempMountedSkillGems.Select((GemMountedDisplay gd) => gd.SkillGem))
		{
			foreach (SkillWord gemSkillWord in item.def.GetGemSkillWords())
			{
				string formatDescription = gemSkillWord.GetFormatDescription(UIBase.applicationState, currentSkill, out var generator);
				descriptionPool.Spawn(new GemWordCell.CellData
				{
					skillGem = item,
					description = formatDescription,
					generator = generator
				});
			}
		}
	}

	public void OnGemClicked(GemMountingDraggerModule gemMountingDraggerModule)
	{
		if (!enableMountGem || gemMountingDraggerModule == null)
		{
			return;
		}
		if (gemMountingDraggerModule.IsMountedGem)
		{
			GemMountedDisplay component = gemMountingDraggerModule.GetComponent<GemMountedDisplay>();
			if (!(component == null))
			{
				_ = selectedMountedGem == component;
				SelectMountedGem(component);
			}
		}
		else if (selectedInventoryGem == gemMountingDraggerModule)
		{
			TryMountInventoryGemAtFirstValidPosition(gemMountingDraggerModule);
		}
		else
		{
			SelectInventoryGem(gemMountingDraggerModule);
		}
	}

	public void OnMountBoardClicked(Vector2 screenPos)
	{
		if (enableMountGem && !preventDragging)
		{
			if (selectedInventoryGem == null)
			{
				ClearSelection(hideInfo: true);
				return;
			}
			Vector2Int boardOffset = GetBoardOffset(screenPos);
			TryMountInventoryGemAtOffset(selectedInventoryGem, boardOffset);
		}
	}

	public void RotateMountedGem(GemMountedDisplay display)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (enableMountGem && !(display == null))
		{
			SelectMountedGem(display, showInfo: false, playSound: false);
			int num = (display.SkillGem.tempRotateIndex - 1 + 6) % 6;
			display.SkillGem.tempRotateIndex = num;
			display.UpdateDisplay(HexagonHelper.RotateHexShape(display.SkillGem.def.hexShape, num));
			display.SetSelected(b: true);
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gem_mount_move);
			SetDirty();
		}
	}

	public void UnmountMountedGem(GemMountedDisplay display)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (!(display == null))
		{
			if (selectedMountedGem == display)
			{
				selectedMountedGem = null;
			}
			display.SetSelected(b: false);
			GemMountingDraggerModule module = display.GetModule<GemMountingDraggerModule>();
			if (module != null)
			{
				module.IsMountedGem = false;
				module.SetSelected(selected: false, playSound: false);
			}
			IncreaseSkillGemAmount(display.SkillGem);
			mountedSkillGemPool.Despawn(display);
			infoPanel.HideAllCommonInfo();
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gem_mount_move);
			SetDirty();
		}
	}

	private void TryConfirm()
	{
		if (!enableMountGem)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillMountGem));
		}
		else if (CheckMountPositionValid() && GlobalConfigManager.instance.enableNetwork)
		{
			UIBase.applicationState.skillGemManager.ConfirmMountSkillGem(currentSkill.profileSkillGUID, tempMountedSkillGems.Select((GemMountedDisplay g) => new ChillyRoom.Services.ContainerOfSoul.SkillGem
			{
				DefId = g.SkillGem.def.FullID,
				RotateIndex = g.SkillGem.tempRotateIndex,
				Offset = new Vector2IntSvr
				{
					X = g.SkillGem.tempOffset.x,
					Y = g.SkillGem.tempOffset.y
				},
				Amount = 1
			}).ToList(), AfterConfirmMounted);
		}
	}

	private void AfterConfirmMounted()
	{
		currentSkill.skillGemsMounted = tempMountedSkillGems.Select(delegate(GemMountedDisplay sg)
		{
			sg.SkillGem.rotateIndex = sg.SkillGem.tempRotateIndex;
			sg.SkillGem.offset = sg.SkillGem.tempOffset;
			return sg.SkillGem;
		}).ToList();
		UIBase.applicationState.skillGemManager.allSkillGems.Clear();
		UIBase.applicationState.skillGemManager.allSkillGems.AddRange(allGemsDeepCopy);
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			UIBase.applicationState.savingManager.SetProfileDirtyToSave();
		}
		manager.SetDirty();
		UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skillgem_mounting_state_saved);
		ThinkingAnalyticsExtends.TrackFortressMountSkillGem(UIBase.applicationState, currentSkill, tempMountedSkillGems.Select((GemMountedDisplay g) => g.SkillGem).ToList());
		ResetUI();
	}

	private void CleanupCloseState()
	{
		ClearSelection(hideInfo: true);
		mountedSkillGemPool.DespawnAll();
		skillGemDisplayPool.DespawnAll();
		allGemsDeepCopy?.Clear();
		isDragValid = false;
		isDragging = false;
		draggingMountedGem = null;
		draggingGemRect = null;
	}

	private void SelectInventoryGem(GemMountingDraggerModule gemMountingDraggerModule)
	{
		ClearMountedSelection();
		if (selectedInventoryGem != null && selectedInventoryGem != gemMountingDraggerModule)
		{
			selectedInventoryGem.SetSelected(selected: false, playSound: false);
		}
		selectedInventoryGem = gemMountingDraggerModule;
		selectedInventoryGem.SetSelected(selected: true);
		selectedInventoryGem.transform.SetAsLastSibling();
	}

	private void SelectMountedGem(GemMountedDisplay display, bool showInfo = true, bool playSound = true)
	{
		ClearInventorySelection();
		if (selectedMountedGem != null && selectedMountedGem != display)
		{
			selectedMountedGem.SetSelected(b: false);
		}
		selectedMountedGem = display;
		selectedMountedGem.SetSelected(b: true);
		selectedMountedGem.GetModule<GemMountingDraggerModule>()?.SetSelected(selected: true, playSound);
		selectedMountedGem.transform.SetAsLastSibling();
		if (showInfo)
		{
			infoPanel.HideAllCommonInfo();
			infoPanel.ShowCommonInfo(new CommonInfoData(display.SkillGem, CommonInfoData.ControlType.Click, display.rectTransform, false));
		}
	}

	private void ClearSelection(bool hideInfo)
	{
		ClearInventorySelection();
		ClearMountedSelection();
		if (hideInfo)
		{
			infoPanel.HideAllCommonInfo();
		}
	}

	private void ClearInventorySelection()
	{
		if (selectedInventoryGem != null)
		{
			selectedInventoryGem.SetSelected(selected: false, playSound: false);
		}
		selectedInventoryGem = null;
	}

	private void ClearMountedSelection()
	{
		if (selectedMountedGem != null)
		{
			selectedMountedGem.SetSelected(b: false);
			selectedMountedGem.GetModule<GemMountingDraggerModule>()?.SetSelected(selected: false, playSound: false);
		}
		selectedMountedGem = null;
	}

	public bool IsGemSelected(GemMountingDraggerModule gemMountingDraggerModule)
	{
		if (gemMountingDraggerModule == null)
		{
			return false;
		}
		if (!gemMountingDraggerModule.IsMountedGem)
		{
			return selectedInventoryGem == gemMountingDraggerModule;
		}
		if (selectedMountedGem != null)
		{
			return selectedMountedGem.GetModule<GemMountingDraggerModule>() == gemMountingDraggerModule;
		}
		return false;
	}

	private void TryMountInventoryGemAtFirstValidPosition(GemMountingDraggerModule inventoryGem)
	{
		if (inventoryGem?.SkillGem != null)
		{
			if (!TryFindFirstValidMountPosition(inventoryGem.SkillGem.def, out var offset, out var rotateIndex))
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skillgem_mounting_out_of_range);
			}
			else
			{
				MountInventoryGem(inventoryGem, offset, rotateIndex);
			}
		}
	}

	private void TryMountInventoryGemAtOffset(GemMountingDraggerModule inventoryGem, Vector2Int offset)
	{
		if (inventoryGem?.SkillGem == null)
		{
			return;
		}
		for (int i = 0; i < 6; i++)
		{
			HexShape shape = BuildMountedHexShape(inventoryGem.SkillGem.def, offset, i);
			if (IsHexShapeValid(shape, null, showHint: false))
			{
				MountInventoryGem(inventoryGem, offset, i);
				return;
			}
		}
		HexShape shape2 = BuildMountedHexShape(inventoryGem.SkillGem.def, offset, 0);
		IsHexShapeValid(shape2, null, showHint: true);
	}

	private void MountInventoryGem(GemMountingDraggerModule inventoryGem, Vector2Int offset, int rotateIndex)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		SkillGem data = new SkillGem(inventoryGem.SkillGem.def, rotateIndex, offset, 1);
		GemMountedDisplay gemMountedDisplay = mountedSkillGemPool.Spawn(data, skillGemSlotDisplay.transform);
		gemMountedDisplay.transform.localPosition = HexagonHelper.GetHexPointPosition(offset, HexagonHelper.HexCellSize);
		gemMountedDisplay.GetModule<GemMountingDraggerModule>().IsMountedGem = true;
		gemMountedDisplay.UpdateDisplay(HexagonHelper.RotateHexShape(gemMountedDisplay.SkillGem.def.hexShape, rotateIndex));
		if (!IsMountedDisplayValid(gemMountedDisplay, showHint: true))
		{
			mountedSkillGemPool.Despawn(gemMountedDisplay);
			return;
		}
		DecreaseSkillGemAmount(inventoryGem.SkillGem);
		SelectMountedGem(gemMountedDisplay);
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gem_mount_move);
		SetDirty();
	}

	public GemMountedDisplay BeginDragInventoryGem(SkillGem inventoryGem, Vector2 screenPos)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		if (!enableMountGem || preventDragging || inventoryGem?.def == null)
		{
			return null;
		}
		ClearSelection(hideInfo: true);
		SkillGem data = new SkillGem(inventoryGem.def, 0, Vector2Int.zero, 1);
		GemMountedDisplay gemMountedDisplay = mountedSkillGemPool.Spawn(data, skillGemSlotDisplay.transform);
		if (gemMountedDisplay == null)
		{
			return null;
		}
		GemMountingDraggerModule module = gemMountedDisplay.GetModule<GemMountingDraggerModule>();
		if (module != null)
		{
			module.IsMountedGem = true;
			module.SetSelected(selected: false, playSound: false);
		}
		gemMountedDisplay.SetSelected(b: false);
		gemMountedDisplay.transform.SetAsLastSibling();
		draggingMountedGem = gemMountedDisplay;
		draggingGemRect = gemMountedDisplay.GetComponent<RectTransform>();
		isDragValid = true;
		isDragging = true;
		DragInventoryGem(gemMountedDisplay, screenPos);
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gem_mount_move);
		return gemMountedDisplay;
	}

	public void DragInventoryGem(GemMountedDisplay display, Vector2 screenPos)
	{
		if (enableMountGem && !preventDragging && isDragValid && !(display == null))
		{
			if (draggingGemRect == null)
			{
				draggingGemRect = display.GetComponent<RectTransform>();
			}
			if (!(draggingGemRect == null))
			{
				isDragging = true;
				draggingGemRect.localPosition = skillGemSlotDisplay.rectTransform.InverseTransformPoint(screenPos);
			}
		}
	}

	public void EndDragInventoryGem(SkillGem inventoryGem, GemMountedDisplay display, Vector2 screenPos)
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		if (!(display == null))
		{
			isDragging = false;
			if (!enableMountGem || preventDragging || !isDragValid || inventoryGem?.def == null || !IsScreenPosInMountArea(screenPos))
			{
				CancelDragInventoryGem(display);
				return;
			}
			Vector2Int boardOffset = GetBoardOffset(screenPos);
			display.SkillGem.tempOffset = boardOffset;
			display.SkillGem.offset = boardOffset;
			display.SkillGem.tempRotateIndex = 0;
			display.SkillGem.rotateIndex = 0;
			display.transform.localPosition = HexagonHelper.GetHexPointPosition(boardOffset, HexagonHelper.HexCellSize);
			display.UpdateDisplay(HexagonHelper.RotateHexShape(display.SkillGem.def.hexShape, 0));
			display.GetModule<GemMountingDraggerModule>().IsMountedGem = true;
			DecreaseSkillGemAmount(inventoryGem);
			SelectMountedGem(display);
			draggingMountedGem = null;
			draggingGemRect = null;
			isDragValid = false;
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gem_mount_move);
			SetDirty();
		}
	}

	private void CancelDragInventoryGem(GemMountedDisplay display)
	{
		if (display != null)
		{
			mountedSkillGemPool.Despawn(display);
		}
		draggingMountedGem = null;
		draggingGemRect = null;
		isDragValid = false;
	}

	private bool IsScreenPosInMountArea(Vector2 screenPos)
	{
		if (skillGemSlotDisplay == null || skillGemSlotDisplay.rectTransform == null)
		{
			return false;
		}
		return RectTransformUtility.RectangleContainsScreenPoint(skillGemSlotDisplay.rectTransform, screenPos, Camera.main);
	}

	private bool TryFindFirstValidMountPosition(SkillGemDef def, out Vector2Int offset, out int rotateIndex)
	{
		foreach (Vector2Int candidateOffset in GetCandidateOffsets())
		{
			for (int i = 0; i < 6; i++)
			{
				HexShape shape = BuildMountedHexShape(def, candidateOffset, i);
				if (IsHexShapeValid(shape, null, showHint: false))
				{
					offset = candidateOffset;
					rotateIndex = i;
					return true;
				}
			}
		}
		offset = Vector2Int.zero;
		rotateIndex = 0;
		return false;
	}

	private IEnumerable<Vector2Int> GetCandidateOffsets()
	{
		return Enumerable.Range(-HexagonHelper.HexShapeMaxRadius, HexagonHelper.HexShapeMaxRadius * 2 + 1).SelectMany((int q) => from r in Enumerable.Range(-HexagonHelper.HexShapeMaxRadius, HexagonHelper.HexShapeMaxRadius * 2 + 1)
			select new Vector2Int(q, r)).Where(IsWithinBoardRadius)
			.OrderBy(GetBoardDistance)
			.ThenBy((Vector2Int p) => p.y)
			.ThenBy((Vector2Int p) => p.x);
	}

	public void OnMountedGemBeginDrag(Vector2 pos, GemMountingDraggerModule gemMountingDraggerModule)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (enableMountGem && !preventDragging && (object)gemMountingDraggerModule != null && gemMountingDraggerModule.IsMountedGem)
		{
			GemMountedDisplay component = gemMountingDraggerModule.GetComponent<GemMountedDisplay>();
			if (!(component == null))
			{
				SelectMountedGem(component, showInfo: false, playSound: false);
				infoPanel.HideAllCommonInfo();
				draggingMountedGem = component;
				draggingGemRect = component.GetComponent<RectTransform>();
				dragStartOffset = component.SkillGem.tempOffset;
				dragStartRotateIndex = component.SkillGem.tempRotateIndex;
				dragStartLocalPosition = draggingGemRect.localPosition;
				isDragValid = true;
				isDragging = true;
				draggingGemRect.position = pos;
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gem_mount_move);
			}
		}
	}

	public void OnMountedGemDrag(Vector2 pos)
	{
		if (enableMountGem && !preventDragging && isDragValid && !(draggingGemRect == null))
		{
			isDragging = true;
			draggingGemRect.localPosition = skillGemSlotDisplay.rectTransform.InverseTransformPoint(pos);
		}
	}

	public void OnMountedGemEndDrag(Vector2 pos)
	{
		if (enableMountGem && !preventDragging && isDragValid && !(draggingMountedGem == null))
		{
			isDragging = false;
			if (!IsScreenPosInMountArea(pos))
			{
				GemMountedDisplay display = draggingMountedGem;
				draggingMountedGem = null;
				draggingGemRect = null;
				isDragValid = false;
				UnmountMountedGem(display);
			}
			else
			{
				Vector2Int boardOffset = GetBoardOffset(pos);
				Vector2 hexPointPosition = HexagonHelper.GetHexPointPosition(boardOffset, HexagonHelper.HexCellSize);
				draggingMountedGem.SkillGem.tempOffset = boardOffset;
				draggingGemRect.localPosition = hexPointPosition;
				draggingMountedGem.SetSelected(b: true);
				draggingMountedGem = null;
				draggingGemRect = null;
				isDragValid = false;
				SetDirty();
			}
		}
	}

	private Vector2Int GetBoardOffset(Vector2 screenPos)
	{
		Vector2Int result = HexagonHelper.LocalRectToHexOffset(HexagonHelper.ScreenPosToLocalRectPos(screenPos, skillGemSlotDisplay.rectTransform, Camera.main), HexagonHelper.HexCellSize, -HexagonHelper.HexShapeMaxRadius, HexagonHelper.HexShapeMaxRadius, -HexagonHelper.HexShapeMaxRadius, HexagonHelper.HexShapeMaxRadius);
		if (result.x + result.y < -HexagonHelper.HexShapeMaxRadius)
		{
			if (result.x < result.y)
			{
				result.y = -HexagonHelper.HexShapeMaxRadius - result.x;
			}
			else
			{
				result.x = -HexagonHelper.HexShapeMaxRadius - result.y;
			}
		}
		else if (result.x + result.y > HexagonHelper.HexShapeMaxRadius)
		{
			if (result.x > result.y)
			{
				result.y = HexagonHelper.HexShapeMaxRadius - result.x;
			}
			else
			{
				result.x = HexagonHelper.HexShapeMaxRadius - result.y;
			}
		}
		return result;
	}

	private bool CheckMountPositionValid()
	{
		List<HexShape> hexShapeList = GetHexShapeList();
		List<Vector2Int> hexShapeIntersectPoints = HexagonHelper.GetHexShapeIntersectPoints(hexShapeList);
		bool flag = hexShapeIntersectPoints.Count == 0;
		if (flag)
		{
			foreach (HexShape item in hexShapeList)
			{
				flag = IsWithinSkillShape(item);
				if (!flag)
				{
					UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skillgem_mounting_out_of_range);
					break;
				}
			}
		}
		else
		{
			LogMountOverlap(hexShapeIntersectPoints);
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skillgem_mounting_overlap);
		}
		return flag;
	}

	private bool IsMountedDisplayValid(GemMountedDisplay display, bool showHint)
	{
		if (display == null)
		{
			return false;
		}
		return IsHexShapeValid(GetMountedHexShape(display), display, showHint);
	}

	private bool IsHexShapeValid(HexShape shape, GemMountedDisplay ignoreDisplay, bool showHint)
	{
		if (!IsWithinSkillShape(shape))
		{
			if (showHint)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skillgem_mounting_out_of_range);
			}
			return false;
		}
		List<HexShape> hexShapeList = GetHexShapeList(ignoreDisplay);
		hexShapeList.Add(shape);
		List<Vector2Int> hexShapeIntersectPoints = HexagonHelper.GetHexShapeIntersectPoints(hexShapeList);
		if (hexShapeIntersectPoints.Count <= 0)
		{
			return true;
		}
		if (showHint)
		{
			LogMountOverlap(hexShapeIntersectPoints);
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skillgem_mounting_overlap);
		}
		return false;
	}

	private bool IsWithinSkillShape(HexShape shape)
	{
		if (currentSkill?.hexShape == null || shape == null)
		{
			return false;
		}
		HashSet<Vector2Int> skillPoints = GetOccupiedPoints(currentSkill.hexShape).ToHashSet();
		return GetOccupiedPoints(shape).All((Vector2Int point) => skillPoints.Contains(point));
	}

	private static void SyncGemEditState(SkillGem skillGem)
	{
		if (skillGem != null)
		{
			skillGem.tempRotateIndex = skillGem.rotateIndex;
			skillGem.tempOffset = skillGem.offset;
		}
	}

	private List<HexShape> GetHexShapeList(GemMountedDisplay ignoreDisplay = null)
	{
		return tempMountedSkillGems.Where((GemMountedDisplay display) => display != null && display != ignoreDisplay).Select(GetMountedHexShape).ToList();
	}

	private void LogMountOverlap(List<Vector2Int> intersectPoints)
	{
		List<GemMountedDisplay> list = tempMountedSkillGems;
		List<string> list2 = new List<string> { $"[GemMountingPanel] CheckMountPositionValid overlap. skill={currentSkill?.profileSkillGUID}, mountedCount={list.Count}, intersectPoints={FormatPoints(intersectPoints)}" };
		for (int i = 0; i < list.Count; i++)
		{
			GemMountedDisplay gemMountedDisplay = list[i];
			SkillGem skillGem = gemMountedDisplay.SkillGem;
			List<Vector2Int> occupiedPoints = GetOccupiedPoints(GetMountedHexShape(gemMountedDisplay));
			List<Vector2Int> visibleGemPoints = gemMountedDisplay.GetVisibleGemPoints();
			List<Vector2Int> visibleOutlinePoints = gemMountedDisplay.GetVisibleOutlinePoints();
			list2.Add($"  gem[{i}] name={skillGem?.def?.skillGemName}, def={skillGem?.def?.FullID}, isMounted={gemMountedDisplay.GetComponent<GemMountingDraggerModule>()?.IsMountedGem}, activeSelf={gemMountedDisplay.gameObject.activeSelf}, activeInHierarchy={gemMountedDisplay.gameObject.activeInHierarchy}, finalOffset={FormatPoint(skillGem?.offset ?? Vector2Int.zero)}, tempOffset={FormatPoint(skillGem?.tempOffset ?? Vector2Int.zero)}, finalRotate={skillGem?.rotateIndex}, tempRotate={skillGem?.tempRotateIndex}, localPosition={FormatVector(gemMountedDisplay.transform.localPosition)}, occupied={FormatPoints(occupiedPoints)}, visibleGemLocal={FormatPoints(visibleGemPoints)}, visibleOutlineLocal={FormatPoints(visibleOutlinePoints)}");
		}
		string message = string.Join("\n", list2);
		WriteMountDiagnosticLog(message);
		TraceLog.Warn(message);
	}

	private static void WriteMountDiagnosticLog(string message)
	{
		try
		{
			string text = Path.Combine(Application.dataPath, "..", "Logs");
			Directory.CreateDirectory(text);
			File.AppendAllText(Path.Combine(text, "GemMountingPanel.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\n{message}\n\n");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[GemMountingPanel] Failed to write diagnostic log: " + ex.Message);
		}
	}

	private HexShape GetMountedHexShape(GemMountedDisplay display)
	{
		return BuildMountedHexShape(display.SkillGem.def, display.SkillGem.tempOffset, display.SkillGem.tempRotateIndex);
	}

	private static HexShape BuildMountedHexShape(SkillGemDef def, Vector2Int offset, int rotateIndex)
	{
		Vector2Int offset2 = offset + def.hexShape.offset;
		return HexagonHelper.RotateHexShape(new HexShape
		{
			points = def.hexShape.points,
			offset = offset2
		}, rotateIndex);
	}

	private static List<Vector2Int> GetOccupiedPoints(HexShape shape)
	{
		if (shape?.points == null)
		{
			return new List<Vector2Int>();
		}
		return shape.points.Select((Vector2Int point) => point + shape.offset).Distinct().ToList();
	}

	private static bool IsWithinBoardRadius(Vector2Int point)
	{
		return GetBoardDistance(point) <= HexagonHelper.HexShapeMaxRadius;
	}

	private static int GetBoardDistance(Vector2Int point)
	{
		return Mathf.Max(Mathf.Abs(point.x), Mathf.Abs(point.y), Mathf.Abs(point.x + point.y));
	}

	private static string FormatPoints(IEnumerable<Vector2Int> points)
	{
		if (points != null)
		{
			return "[" + string.Join(", ", points.Select(FormatPoint)) + "]";
		}
		return "[]";
	}

	private static string FormatPoint(Vector2Int point)
	{
		return $"({point.x},{point.y})";
	}

	private static string FormatVector(Vector3 vector)
	{
		return $"({vector.x:F1},{vector.y:F1},{vector.z:F1})";
	}

	public bool IsPositionValid(GemMountedDisplay display, Vector2Int pos)
	{
		if (HexagonHelper.GetIsHexShapeIntersect(GetHexShapeList(display), pos))
		{
			return false;
		}
		return HexagonHelper.GetIsHexShapeIntersect(new List<HexShape> { currentSkill.hexShape }, pos);
	}
}
