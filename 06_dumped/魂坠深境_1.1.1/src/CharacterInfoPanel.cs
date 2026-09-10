using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using Common.UI;
using ContainerOfSoul.Display.Fortress.DotManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoPanel : CellUI<PlayerCharacterState>, ISelectablePanel, IShowAsync, IHideAsync, IDisplayModulesController
{
	private const float SetCharacterSuccessVfxScale = 0.6f;

	private static readonly Color32 SetCharacterAvailableTextColor = new Color32(247, 237, 217, byte.MaxValue);

	[SerializeField]
	private CharacterSpineDisplay characterSpineDisplay;

	[SerializeField]
	private GameButton setCharacterButton;

	[SerializeField]
	private TextMeshProUGUI setCharacterButtonText;

	[SerializeField]
	private Image setCharacterButtonImage;

	[SerializeField]
	private Sprite selectedCharacterIcon;

	[SerializeField]
	private Sprite unselectedCharacterIcon;

	[SerializeField]
	private GameObject setCharacterObsessionArea;

	[SerializeField]
	private ObsessionProgressDisplay setCharacterObsessionProgressDisplay;

	[SerializeField]
	private GameButton unlockButton;

	[SerializeField]
	private GameButton trialButton;

	[SerializeField]
	private TextMeshProUGUI trialTitleText;

	[SerializeField]
	private GameObject trialRewardArea;

	[SerializeField]
	private Image trialRewardIcon;

	[SerializeField]
	private TextMeshProUGUI trialRewardAmountText;

	[SerializeField]
	private Color32 normalColor;

	[SerializeField]
	private Color32 lockedColor;

	[SerializeField]
	private CharacterInfoArea characterInfoArea;

	[SerializeField]
	private GameButton recommendPanelButton;

	[SerializeField]
	private Image classIcon;

	[SerializeField]
	private TextMeshProUGUI characterNameText;

	[SerializeField]
	private TextMeshProUGUI characterClassText;

	[SerializeField]
	private RectTransform masterySelectCellParent;

	[SerializeField]
	private CharacterMasterySelectCell2 masterySelectCellPrefab;

	[SerializeField]
	private RectTransform itemDisplayParent;

	[SerializeField]
	private HintRedDot hintRedDot_ToTokenDetailButton;

	[SerializeField]
	private GameButton openCharacterTokenUpgradeDetailButton;

	[SerializeField]
	private GameButton openTalentButton;

	[SerializeField]
	private HintRedDot openTalentButtonRedDot;

	[SerializeField]
	private GameButton skillInfoButton;

	[SerializeField]
	private ObsessionLevelDisplay_Image obsessionLevelDisplay;

	[SerializeField]
	private ObsessionProgressDisplay obsessionProgressDisplay;

	[SerializeField]
	public SkillInfoPanel skillInfoPanel;

	[SerializeField]
	public ItemInfoPanel itemInfoPanel;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	public PlayerCharacterState characterState;

	[NonSerialized]
	private Item selectedItem;

	[NonSerialized]
	public CellUIPrefabPool<CharacterMasterySelectCell2, Tuple<CharacterMastery, PlayerCharacterState>> masterySelectPool;

	[NonSerialized]
	private bool postFirstGameTalentFocusMode;

	[NonSerialized]
	private bool limitedDeploymentDisplay;

	[NonSerialized]
	private Color setCharacterSelectedTextColor;

	public bool isPreview => base.setupData.locked;

	public bool isLimitedDeployment
	{
		get
		{
			if (base.setupData != null)
			{
				return LimitedCharacterDeploymentService.IsAvailable(CellUI<PlayerCharacterState>.applicationState, base.setupData);
			}
			return false;
		}
	}

	protected override bool shouldUpdate
	{
		get
		{
			if (base.shouldUpdate && base.setupData != null)
			{
				return characterState != null;
			}
			return false;
		}
	}

	public RectTransform OpenTalentButtonRect
	{
		get
		{
			if (!(openTalentButton != null))
			{
				return null;
			}
			return openTalentButton.GetComponent<RectTransform>();
		}
	}

	public PlayerCharacterState currentConfigCharacter
	{
		get
		{
			return CellUI<PlayerCharacterState>.applicationState.newGameConfigManager.currentNewGameConfig.currentCharacter.state;
		}
		set
		{
			CellUI<PlayerCharacterState>.applicationState.newGameConfigManager.currentNewGameConfig.SetCharacter(value.def);
		}
	}

	protected override void Init()
	{
		InitUI(obsessionLevelDisplay);
		InitUI(itemInfoPanel);
		InitUI(characterSpineDisplay);
		InitUI(skillInfoPanel);
		InitUI(hintRedDot_ToTokenDetailButton);
		InitUI(characterInfoArea);
		InitUI(obsessionProgressDisplay);
		InitUI(openTalentButtonRedDot);
		InitUI(setCharacterObsessionProgressDisplay);
		setCharacterSelectedTextColor = setCharacterButtonText.color;
		masterySelectPool = GetPrefabPool<CharacterMasterySelectCell2, Tuple<CharacterMastery, PlayerCharacterState>>(masterySelectCellPrefab, masterySelectCellParent);
		openTalentButton.onClick.AddListener(_OnOpenTalentButtonClicked);
		CellUI<PlayerCharacterState>.applicationState.eventManager.characterClassExpChangeEvent.AddListener(_OnExpChange);
		skillInfoButton.onClick.AddListener(delegate
		{
			CellUI<PlayerCharacterState>.fortressState.uiManager.skillConfigPanel.Setup(characterState);
			CellUI<PlayerCharacterState>.fortressState.uiManager.skillConfigPanel.ShowUI();
			SetDirty();
		});
		setCharacterButton.onClick.AddListener(_OnSetCharacterButtonClicked);
		if (trialButton != null)
		{
			trialButton.onClick.AddListener(_OnTrialButtonClicked);
		}
		recommendPanelButton.onClick.AddListener(delegate
		{
			CellUI<PlayerCharacterState>.fortressState.uiManager.recommendItemPanel.ShowItem(characterState.def.attachClass);
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(CellUI<PlayerCharacterState>.applicationState, "其他-卡组推荐界面", "角色详情");
		});
		openCharacterTokenUpgradeDetailButton.onClick.AddListener(delegate
		{
			CellUI<PlayerCharacterState>.applicationState.fortressState.uiManager.characterObsessionPanel.ShowUI(characterState);
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(CellUI<PlayerCharacterState>.applicationState, "主仪式间-角色详情-执念等级");
		});
		unlockButton.onClick.AddListener(delegate
		{
			CellUI<PlayerCharacterState>.applicationState.characterManager.UnlockCharacterByObsessionDebris(base.setupData, delegate
			{
				Setup(base.setupData);
				manager.SetDirty();
			});
		});
		UIHelper.RegisterEmptyAreaClickedEvent(_OnEmptyAreaClicked);
	}

	protected override void OnSetup(PlayerCharacterState _)
	{
		limitedDeploymentDisplay = isLimitedDeployment;
		if (limitedDeploymentDisplay)
		{
			LimitedCharacterDeploymentService.MarkViewed(CellUI<PlayerCharacterState>.applicationState, base.setupData);
		}
		characterState = (limitedDeploymentDisplay ? LimitedCharacterDeploymentService.CreateTemporaryCharacter(CellUI<PlayerCharacterState>.applicationState, base.setupData.def) : (isPreview ? base.setupData.GetPreview() : base.setupData));
		obsessionLevelDisplay.Setup(characterState);
		obsessionProgressDisplay.Setup(characterState);
		characterInfoArea.Setup(characterState);
		if (isPreview)
		{
			openTalentButtonRedDot.Unset();
			openTalentButtonRedDot.gameObject.SetActive(value: false);
		}
		else
		{
			openTalentButtonRedDot.Setup(new CharacterInfoPanel_OpenTalentButton
			{
				characterState = characterState
			});
		}
		classIcon.sprite = characterState.characterClass.classDef.professionIcon;
		characterClassText.text = characterState.characterClass.classDef.characterName;
		CellUI<PlayerCharacterState>.fortressState.uiManager.fortressMainBaseUI.SetDirty();
		CellUI<PlayerCharacterState>.fortressState.uiManager.fortressGuidePanel.characterInfoGuideHint.TryFirstOpenAutoPopup();
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(CellUI<PlayerCharacterState>.applicationState, "主仪式间-角色详情界面", characterState.Name);
		manager.ShowUI(this);
		hintRedDot_ToTokenDetailButton.Setup(CellUI<PlayerCharacterState>.fortressState.redDotManager.characterInfoPanel_ToTokenDetailButtonModules[characterState.def]);
		SetDirty();
	}

	public void SetPostFirstGameTalentFocusMode(bool active)
	{
		postFirstGameTalentFocusMode = active;
		SetDirty();
	}

	protected override void OnUnset()
	{
		UnSelect();
		itemInfoPanel.HideAllCommonInfo();
	}

	protected override void Dispose()
	{
		base.Dispose();
		CellUI<PlayerCharacterState>.applicationState.eventManager.characterClassExpChangeEvent.RemoveListener(_OnExpChange);
		UIHelper.UnregisterEmptyAreaClickedEvent(_OnEmptyAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		if (limitedDeploymentDisplay != isLimitedDeployment)
		{
			CellUI<PlayerCharacterState>.applicationState.newGameConfigManager.SanitizeLimitedDeploymentSelection();
			Setup(base.setupData);
			return;
		}
		UpdateCharacterInfo();
		UpdateMastery();
		UpdateSetCharacterArea();
		bool active = !isPreview && !postFirstGameTalentFocusMode;
		openTalentButton.gameObject.SetActive(!isPreview);
		openCharacterTokenUpgradeDetailButton.gameObject.SetActive(active);
		hintRedDot_ToTokenDetailButton.gameObject.SetActive(active);
		skillInfoButton.gameObject.SetActive(active);
		obsessionProgressDisplay.gameObject.SetActive(active);
		recommendPanelButton.gameObject.SetActive(!isPreview);
		UpdateTrialButton();
		void UpdateCharacterInfo()
		{
			characterSpineDisplay.skinImage.color = ((isPreview && !isLimitedDeployment) ? lockedColor : normalColor);
			characterSpineDisplay.Setup(characterState);
			characterNameText.text = characterState.def.characterName;
		}
		void UpdateMastery()
		{
			masterySelectPool.DespawnAll();
			List<CharacterMastery> list = new List<CharacterMastery>(characterState.def.characterMasteries);
			list.Sort(delegate(CharacterMastery s1, CharacterMastery s2)
			{
				int num = characterState.IsMasteryUnlocked(s1).CompareTo(characterState.IsMasteryUnlocked(s2));
				return (num != 0) ? (-num) : characterState.def.characterMasteries.IndexOf(s1).CompareTo(characterState.def.characterMasteries.IndexOf(s2));
			});
			foreach (CharacterMastery item in list)
			{
				masterySelectPool.Spawn(new Tuple<CharacterMastery, PlayerCharacterState>(item, characterState));
			}
		}
		void UpdateSetCharacterArea()
		{
			bool num = !base.setupData.locked || isLimitedDeployment;
			NewGameConfigData currentNewGameConfig = CellUI<PlayerCharacterState>.applicationState.newGameConfigManager.currentNewGameConfig;
			CharacterUsageMode characterUsageMode = (isLimitedDeployment ? CharacterUsageMode.LimitedDeployment : CharacterUsageMode.Owned);
			bool flag = currentNewGameConfig.currentCharacter == base.setupData.def && currentNewGameConfig.characterUsageMode == characterUsageMode;
			setCharacterButton.gameObject.SetActive(value: false);
			setCharacterObsessionArea.gameObject.SetActive(value: false);
			unlockButton.gameObject.SetActive(value: false);
			if (num)
			{
				setCharacterButton.gameObject.SetActive(value: true);
				if (flag)
				{
					setCharacterButtonText.text = (isLimitedDeployment ? CellUI<PlayerCharacterState>.textData.term_limited_character_selected : CellUI<PlayerCharacterState>.textData.term_character_selected);
					setCharacterButtonText.color = setCharacterSelectedTextColor;
					setCharacterButtonImage.sprite = selectedCharacterIcon;
				}
				else
				{
					setCharacterButtonText.text = (isLimitedDeployment ? CellUI<PlayerCharacterState>.textData.term_set_limited_character_deployment : CellUI<PlayerCharacterState>.textData.term_set_character_selected);
					setCharacterButtonText.color = SetCharacterAvailableTextColor;
					setCharacterButtonImage.sprite = unselectedCharacterIcon;
				}
			}
			else
			{
				setCharacterObsessionArea.gameObject.SetActive(value: true);
				setCharacterObsessionProgressDisplay.Setup(base.setupData);
				if (base.setupData.CheckCanUnlockCharacterByObsessionDebris())
				{
					unlockButton.gameObject.SetActive(value: true);
				}
			}
		}
		void UpdateTrialButton()
		{
			if (!(trialButton == null))
			{
				CharacterTrialConfig.TrialEntry trialEntry = ScriptableObjectSingleton<CharacterTrialConfig>.instance.TryGetEntry(characterState.def);
				bool flag = characterState.def != ScriptableObjectSingleton<FortressDataManager>.instance.defaultCharacter && trialEntry != null && !isLimitedDeployment && !CellUI<PlayerCharacterState>.applicationState.characterTrialManager.ShouldLockTrialEntry(characterState.def);
				trialButton.gameObject.SetActive(flag);
				if (flag)
				{
					trialTitleText.text = CellUI<PlayerCharacterState>.textData.term_character_experience;
					bool flag2 = trialEntry.completionRewards != null && trialEntry.completionRewards.Count > 0;
					trialRewardArea.SetActive(flag2);
					if (flag2)
					{
						GlobalRewardSetup globalRewardSetup = trialEntry.completionRewards[0];
						trialRewardIcon.sprite = globalRewardSetup.GlobalItemCellIcon;
						trialRewardAmountText.text = "x".SizeTextPercent(80) + globalRewardSetup.GlobalItemCount;
					}
				}
			}
		}
	}

	void IShowAsync.OnShowStart()
	{
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.show);
	}

	void IHideAsync.OnHideStart()
	{
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.hide);
	}

	private void UnSelect()
	{
		if (selectedItem != null)
		{
			selectedItem = null;
			SetDirty();
		}
	}

	public void SelectMastery(CharacterMastery mastery)
	{
		if (!(mastery == null))
		{
			CellUI<PlayerCharacterState>.fortressState.uiManager.characterInfoBasePanel.masteryInfoPanel.Setup(new MasteryInfoDisplay.DisplayInfo
			{
				character = characterState,
				mastery = mastery
			});
			SetDirty();
		}
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		if (item != selectedItem)
		{
			selectedItem = item;
			SetDirty();
		}
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		UnSelect();
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		return selectedItem == item;
	}

	private void _OnOpenTalentButtonClicked()
	{
		CellUI<PlayerCharacterState>.applicationState.fortressState.uiManager.characterTalentUnlockPanel.ShowUI(characterState);
	}

	private void _OnSetCharacterButtonClicked()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		NewGameConfigData currentNewGameConfig = CellUI<PlayerCharacterState>.applicationState.newGameConfigManager.currentNewGameConfig;
		CharacterUsageMode characterUsageMode = (isLimitedDeployment ? CharacterUsageMode.LimitedDeployment : CharacterUsageMode.Owned);
		if (currentNewGameConfig.currentCharacter == base.setupData.def && currentNewGameConfig.characterUsageMode == characterUsageMode)
		{
			return;
		}
		if (isLimitedDeployment)
		{
			if (!currentNewGameConfig.SetLimitedDeploymentCharacter(base.setupData.def))
			{
				return;
			}
		}
		else
		{
			if (base.setupData.locked)
			{
				return;
			}
			currentNewGameConfig.SetCharacter(base.setupData.def);
		}
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
		PlaySetCharacterSuccessVfx();
		manager.SetDirty();
	}

	private void PlaySetCharacterSuccessVfx()
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if (setCharacterButton == null || ScriptableObjectSingleton<VFXDataPreset>.instance == null)
		{
			return;
		}
		NFXSetup effect_multiInfoDisplayPanel_ShowItem = ScriptableObjectSingleton<VFXDataPreset>.instance.effect_multiInfoDisplayPanel_ShowItem;
		RectTransform component = setCharacterButton.GetComponent<RectTransform>();
		if (effect_multiInfoDisplayPanel_ShowItem == null || component == null || component.parent == null)
		{
			return;
		}
		Vector3 worldPosition = component.TransformPoint(component.rect.center);
		NFXPlayState nFXPlayState = CellUI<PlayerCharacterState>.applicationState.nfxManager.SpawnAndPlay(effect_multiInfoDisplayPanel_ShowItem, component.parent, worldPosition);
		if (nFXPlayState?.nfxInstance == null)
		{
			return;
		}
		nFXPlayState.nfxInstance.transform.SetSiblingIndex(component.GetSiblingIndex());
		UIParticle component2 = nFXPlayState.nfxInstance.GetComponent<UIParticle>();
		if ((UnityEngine.Object)(object)component2 != null)
		{
			component2.scale3D *= 0.6f;
			((Graphic)(object)component2).raycastTarget = false;
		}
		ParticleSystem[] componentsInChildren = nFXPlayState.nfxInstance.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		foreach (ParticleSystem val in componentsInChildren)
		{
			MainModule main = val.main;
			if (((MainModule)(ref main)).loop || UsesOutlineMaterial(val))
			{
				val.Stop(true, (ParticleSystemStopBehavior)0);
				((Component)(object)val).gameObject.SetActive(value: false);
			}
		}
	}

	private static bool UsesOutlineMaterial(ParticleSystem particleSystem)
	{
		ParticleSystemRenderer component = ((Component)(object)particleSystem).GetComponent<ParticleSystemRenderer>();
		if ((UnityEngine.Object)(object)component == null)
		{
			return false;
		}
		Material[] sharedMaterials = ((Renderer)(object)component).sharedMaterials;
		foreach (Material material in sharedMaterials)
		{
			if (material != null && material.name.IndexOf("Outline", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
		}
		return false;
	}

	private void _OnTrialButtonClicked()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (characterState != null && !(characterState.def == ScriptableObjectSingleton<FortressDataManager>.instance.defaultCharacter) && ScriptableObjectSingleton<CharacterTrialConfig>.instance.TryGetEntry(characterState.def) != null && !CellUI<PlayerCharacterState>.applicationState.characterTrialManager.ShouldLockTrialEntry(characterState.def))
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
			BootStrapLoadData bootStrapLoadData = BootStrapLoadData.ToCharacterTrialBattle(CellUI<PlayerCharacterState>.applicationState, characterState.def, characterState.currentMastery);
			Bootstrap.instance.Load(bootStrapLoadData);
		}
	}

	private void _OnExpChange(CharacterClassState chracter)
	{
		SetDirty();
	}

	private void _OnEmptyAreaClicked()
	{
		UnSelect();
	}

	public void _OnAnimatorShowEnd()
	{
		((Behaviour)(object)animator).enabled = false;
	}

	public void _OnAnimatorHideEnd()
	{
		((Behaviour)(object)animator).enabled = false;
		base.gameObject.SetActive(value: false);
	}
}
