using System;
using UnityEngine;

public class HealSpringEvent : WorldEvent
{
	public float healPercentage;

	public float addhealPercentage;

	public WorldEventChoiceText healChoiceText;

	public WorldEventChoiceText skillChoiceText;

	public WorldEventChoiceText skillChoiceWithCostText;

	[SerializeField]
	protected WorldEventChoiceText activatePotentialTalentText;

	[TextArea]
	[LocalizeText("恢复提示文本")]
	public string healStr;

	[TextArea]
	[LocalizeText("生命值已满提示文本")]
	public string reachMaxHealthHintStr;

	[NonSerialized]
	private bool encounterSmith;

	[NonSerialized]
	private int finalMaxHealth;

	[NonSerialized]
	private int initHealth;

	[NonSerialized]
	private bool skillActivated;

	[NonSerialized]
	private GameSkill activatedSkill;

	[NonSerialized]
	private TalentDef activatedTalent;

	[NonSerialized]
	private int healedAmount;

	[NonSerialized]
	private bool used;

	[NonSerialized]
	private WorldEventChoice healChoice;

	[NonSerialized]
	public WorldEventChoice skillChoice;

	[NonSerialized]
	protected WorldEventChoice activatePotentialTalentChoice;

	private bool ChoicesMutuallyExclusive
	{
		get
		{
			if (!gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.HealSpringChoicesMutuallyExclusive))
			{
				GameContractState contractState = gameState.contractState;
				if (contractState == null)
				{
					return false;
				}
				return contractState.selectedContract?.contractDef?.contractRule?.ForceHealSpringChoicesMutuallyExclusive(gameState) == true;
			}
			return true;
		}
	}

	protected virtual bool ShouldSetupHealChoice => true;

	protected virtual bool ShouldSetupDefaultUtilityChoices => true;

	protected virtual bool ShouldSetupActivatePotentialTalentChoice => false;

	public bool WasUsed => used;

	protected virtual bool ShouldSetupActivateSkillChoice => true;

	private int FinalHealCount
	{
		get
		{
			PathLocation eventCurrentLocation = base.EventCurrentLocation;
			if (eventCurrentLocation != null && eventCurrentLocation.variantType == PathLocation.LocationVariantType.FadedHealFire)
			{
				return 1;
			}
			return (int)((float)(int)gameState.playerState.bft.maxHealth * FinalHealPercent);
		}
	}

	private float FinalHealPercent
	{
		get
		{
			float num = healPercentage;
			if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableRestExtraHeal))
			{
				num += addhealPercentage;
			}
			PathLocation eventCurrentLocation = base.EventCurrentLocation;
			if (eventCurrentLocation != null && eventCurrentLocation.variantType == PathLocation.LocationVariantType.HealFireFullHeal)
			{
				num = 1f;
			}
			return num;
		}
	}

	protected override void Init()
	{
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		used = false;
		base.Init();
		if (ShouldSetupHealChoice)
		{
			PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
			if (currentLocation != null && currentLocation.variantType == PathLocation.LocationVariantType.HealFireFullHeal)
			{
				used = true;
				EnqueueAction(new HealAction(this, gameState.playerState.bft, FinalHealCount));
				goto IL_006f;
			}
		}
		if (ShouldSetupHealChoice)
		{
			SetupHealChoice();
		}
		goto IL_006f;
		IL_006f:
		SetupBeforeHealSpringUtilityChoices();
		if (ShouldSetupDefaultUtilityChoices && !gameState.IsIntroGame)
		{
			SetupActivatePotentialTalentChoice();
			if (ShouldSetupActivateSkillChoice)
			{
				SetupActivateSkillChoice();
			}
		}
		SetupAfterHealSpringUtilityChoices();
		encounterSmith = gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EncounterSmith);
		if (encounterSmith)
		{
			onTextShown = (Action)Delegate.Combine(onTextShown, new Action(EncounterSmith));
		}
		PathLocation currentLocation2 = gameState.dungeonMapManager.currentLocation;
		if (currentLocation2 != null && currentLocation2.type == PathLocation.LocationType.SectionEnd)
		{
			EnqueueAction(new EnterNewStageAction(gameState.contractState.stage + 1, toWelcome: true, showLoading: true));
		}
		if (gameState.contractState.selectedContract.contractDef.contractType == ContractDef.ContractType.PVPOffender)
		{
			PathLocation currentLocation3 = gameState.dungeonMapManager.currentLocation;
			if (currentLocation3 != null && currentLocation3.type == PathLocation.LocationType.SectionStart)
			{
				gameState.UIManager.pvpDefenderPreviewPanel.SetActiveAndDirty(active: true);
			}
		}
		Bootstrap.instance.audioManager.PlaySoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_bonfire);
		currentDisplayInfo.showLeaveButton = true;
	}

	public override void FinishedWorldEvent()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		ThinkingAnalyticsExtends.TrackHealEvent(gameState, this, activatedTalent, activatedSkill, healedAmount);
		Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_bonfire);
		if (encounterSmith)
		{
			gameState.UIManager.upgradePanel_Select.Unset();
		}
		base.FinishedWorldEvent();
	}

	private void Heal()
	{
		used = true;
		healedAmount = FinalHealCount;
		EnqueueAction(new HealAction(this, gameState.playerState.bft, FinalHealCount));
		if (ChoicesMutuallyExclusive)
		{
			RefreshAfterChoose(healChoiceText.chosenDescription);
			HideHealFireEffectObject();
		}
		else
		{
			currentDisplayInfo.RemoveChoice(healChoice);
			SetPanelDirty();
		}
	}

	private void SelectSkill()
	{
		SelectSkillInfo selectSkillInfo = new SelectSkillInfo();
		selectSkillInfo.canCancel = true;
		selectSkillInfo.confirmCallback = delegate(GameSkill newSkill)
		{
			used = true;
			skillActivated = true;
			activatedSkill = newSkill;
			if (!ChoicesMutuallyExclusive)
			{
				currentDisplayInfo.RemoveChoice(skillChoice);
				SetPanelDirty();
			}
			else
			{
				RefreshAfterChoose(skillChoiceText.chosenDescription);
				HideHealFireEffectObject();
			}
		};
		gameState.UIManager.selectSkillPanel.Setup(selectSkillInfo);
	}

	private void EncounterSmith()
	{
		onTextShown = (Action)Delegate.Remove(onTextShown, new Action(EncounterSmith));
		gameState.UIManager.upgradePanel_Select.Setup(new UpgradePanel_Select.EncounterSmithSetupInfo());
		if (gameState.UIManager.upgradePanel_Select.HasUpgradableItem())
		{
			gameState.UIManager.upgradePanel_Select.ShowPanel();
		}
	}

	private void SetupHealChoice()
	{
		initHealth = gameState.playerState.bft.health;
		bool healSpringCantHeal = gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.HealSpringCantHeal);
		healChoice = new WorldEventChoice();
		healChoice.descriptionText = string.Format(healChoiceText.choiceDescription, FinalHealCount.ToString());
		BattleFieldTransform bft = gameState.playerState.bft;
		int num = bft.health;
		int maxHealth = bft.maxHealth;
		if (num == maxHealth)
		{
			WorldEventChoice worldEventChoice = healChoice;
			worldEventChoice.descriptionText = worldEventChoice.descriptionText + Environment.NewLine + reachMaxHealthHintStr;
		}
		healChoice.buttonText = string.Format(healChoiceText.choiceName, FinalHealCount.ToString());
		healChoice.stayAfterChoose = !ChoicesMutuallyExclusive;
		healChoice.executeAbleChecker = () => !healSpringCantHeal && ((int)gameState.playerState.bft.health != maxHealth || CanRestWithAfterHealTriggerAtFullHealth());
		healChoice.executeAction = Heal;
		currentDisplayInfo.AddChoice(healChoice);
	}

	private bool CanRestWithAfterHealTriggerAtFullHealth()
	{
		BattleFieldTransform bft = gameState.playerState.bft;
		if (bft?.influences == null)
		{
			return false;
		}
		GameEvent gameEvent = new GameEvent(GameEvent.Type.AfterHeal, GameEvent.TriggerType.Real)
		{
			gameState = gameState,
			amount = FinalHealCount,
			TargetBft = bft
		};
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, this);
		foreach (Influence influence in bft.influences)
		{
			if (influence == null)
			{
				continue;
			}
			gameEvent.SetCaster(influence);
			try
			{
				if (influence.CheckTriggerGameEventAble(gameEvent))
				{
					return true;
				}
			}
			finally
			{
				gameEvent.SetCaster(null);
			}
		}
		return false;
	}

	protected virtual void SetupBeforeHealSpringUtilityChoices()
	{
	}

	protected virtual void SetupAfterHealSpringUtilityChoices()
	{
	}

	protected void SetupActivateSkillChoice()
	{
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.OnlyActivateSkillOnSectionStart))
		{
			PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
			if (currentLocation == null || currentLocation.type != PathLocation.LocationType.SectionStart)
			{
				return;
			}
		}
		if (!gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableHealSpringActiveSkill) || gameState.playerState.PreparableSkills.IsEmptyOrNull())
		{
			return;
		}
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NeedCrystalCostForUnlockSkill))
		{
			skillChoice = new WorldEventChoice();
			skillChoice.descriptionText = skillChoiceWithCostText.choiceDescription.FormatWith(ScriptableObjectSingleton<GameDataManager>.instance.highDifficultyUnlockSkillCrystalCost.ToString().ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.crystalColor));
			skillChoice.buttonText = skillChoiceWithCostText.choiceName;
			skillChoice.stayAfterChoose = true;
			skillChoice.executeAbleChecker = () => (int)gameState.playerState.crystalCount >= ScriptableObjectSingleton<GameDataManager>.instance.highDifficultyUnlockSkillCrystalCost;
			skillChoice.executeAction = SelectSkill;
			currentDisplayInfo.AddChoice(skillChoice);
		}
		else
		{
			skillChoice = new WorldEventChoice();
			skillChoice.descriptionText = skillChoiceText.choiceDescription;
			skillChoice.buttonText = skillChoiceText.choiceName;
			skillChoice.stayAfterChoose = true;
			skillChoice.executeAction = SelectSkill;
			currentDisplayInfo.AddChoice(skillChoice);
		}
	}

	protected void SetupActivatePotentialTalentChoice()
	{
		if (ShouldSetupActivatePotentialTalentChoice && !gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.DisableActivatePotentialInHealSpring) && !gameState.playerState.CurrentActivateAblePotentialTalent.IsEmptyOrNull())
		{
			activatePotentialTalentChoice = new WorldEventChoice();
			activatePotentialTalentChoice.descriptionText = activatePotentialTalentText.choiceDescription;
			activatePotentialTalentChoice.buttonText = activatePotentialTalentText.choiceName;
			activatePotentialTalentChoice.executeAction = ExecuteActivatePotentialTalentChoice;
			activatePotentialTalentChoice.stayAfterChoose = true;
			currentDisplayInfo.AddChoice(activatePotentialTalentChoice);
		}
	}

	private void ExecuteActivatePotentialTalentChoice()
	{
		ActivatePotentialTalentInfo t = new ActivatePotentialTalentInfo
		{
			onCancel = delegate
			{
				gameState.UIManager.activatePotentialTalentPanel.Unset();
			},
			onConfirm = delegate(TalentDef talent)
			{
				used = true;
				activatedTalent = talent;
				if (!ChoicesMutuallyExclusive)
				{
					currentDisplayInfo.RemoveChoice(activatePotentialTalentChoice);
					SetPanelDirty();
				}
				else
				{
					RefreshAfterChoose(activatePotentialTalentText.chosenDescription);
					HideHealFireEffectObject();
				}
			}
		};
		gameState.UIManager.activatePotentialTalentPanel.Setup(t);
		gameState.UIManager.activatePotentialTalentPanel.SetActiveAndDirtyWithFadeAnim(active: true);
	}

	private void HideHealFireEffectObject()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		RoomObjectDisplay roomObjectDisplay = gameState.currentEncounter?.majorRoomObjectDisplay;
		if (roomObjectDisplay != null && roomObjectDisplay.hideObjectAfterEffect != null)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_bonfire_out);
			Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_bonfire);
			roomObjectDisplay.hideObjectAfterEffect.gameObject.SetActive(value: false);
		}
	}
}
