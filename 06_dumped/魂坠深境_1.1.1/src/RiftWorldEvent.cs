using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class RiftWorldEvent : WorldEvent
{
	private const int NormalModeNoChosenBStage = 0;

	private const int FirstNormalVariantBStage = 4;

	private const int LastNormalVariantBStage = 6;

	public ClavisDef bronzeDocumentClavis;

	[TextArea]
	[LocalizeText("事件描述")]
	public string variantADescription = "你在穿过一个狭缝，路途上回荡着了无数个倒影。\n\n面前映照着无数个将要发生的过去。\n又或是已经结束的未来。";

	[TextArea]
	[LocalizeText("事件描述")]
	public string variantBDescription = "你在穿过一个狭缝，路途上回荡着了无数个倒影。\n\n这次，你与{0}再会了。\n{0}将赠予你祝福。\n也将使你坠入迷途的终点。";

	[TextArea]
	[LocalizeText("事件描述")]
	public string bossPreviewDescriptionFormat = "你感受到了{0}的存在......";

	public WorldEventChoiceText meditationChoiceText;

	[TextArea]
	[LocalizeText("事件选项描述")]
	public string noPotentialHint = "需在天赋中解锁潜能";

	public WorldEventChoiceText leapChoiceText;

	[LocalizeText("事件选项描述")]
	public string leapFaceBossText = "跳过本层，直面首领";

	[LocalizeText("事件选项描述")]
	public string leapDescriptionFormat = "接纳{0}，{1}";

	public string leapDangerColor = "#DB3B43";

	public List<RiftCharacterBlessingConfig> blessings = new List<RiftCharacterBlessingConfig>();

	[NonSerialized]
	private WorldEventChoice meditationChoice;

	[NonSerialized]
	private WorldEventChoice leapChoice;

	[NonSerialized]
	private PlayerCharacterState riftCharacterState;

	[NonSerialized]
	private RiftCharacterBlessingConfig riftBlessing;

	[NonSerialized]
	private bool isVariantB;

	[NonSerialized]
	private bool leaping;

	protected override void Init()
	{
		base.Init();
		leaping = false;
		SetupVariant();
		currentDisplayInfo.title = EventName;
		currentDisplayInfo.description = BuildDescription();
		currentDisplayInfo.showLeaveButton = true;
		SetupMeditationChoice();
		SetupLeapChoice();
	}

	private void SetupVariant()
	{
		isVariantB = ShouldUseVariantB();
		if (!isVariantB)
		{
			SetupVariantA();
			return;
		}
		List<PlayerCharacterState> unusedConfiguredUnlockedCharacterStates = GetUnusedConfiguredUnlockedCharacterStates();
		if (unusedConfiguredUnlockedCharacterStates.Count == 0)
		{
			SetupVariantA();
			return;
		}
		riftCharacterState = unusedConfiguredUnlockedCharacterStates.SelectRandom(gameState.randomManager.NextDouble);
		riftBlessing = GetBlessingConfig(riftCharacterState?.def);
	}

	private void SetupVariantA()
	{
		isVariantB = false;
		riftCharacterState = gameState.playerState.character;
		riftBlessing = GetBlessingConfig(riftCharacterState?.def);
	}

	private string BuildDescription()
	{
		string text = variantADescription;
		if (isVariantB)
		{
			string arg = riftCharacterState?.def?.characterName ?? "";
			text = string.Format(variantBDescription, arg);
		}
		string text2 = TryGetBossPreviewName();
		if (text2.IsNullOrEmpty() || bossPreviewDescriptionFormat.IsNullOrEmpty())
		{
			return text;
		}
		string arg2 = ColorText(text2, leapDangerColor).BoldText();
		return text + "\n\n" + string.Format(bossPreviewDescriptionFormat, arg2);
	}

	private string TryGetBossPreviewName()
	{
		PathLocation pathLocation = gameState.dungeonMapManager.currentDungeonMap?.VisibleEndLocation;
		if (pathLocation == null || pathLocation.type != PathLocation.LocationType.Boss)
		{
			return null;
		}
		EncounterSetupInfo attachEncounterSetupInfo = pathLocation.attachEncounterSetupInfo;
		if (attachEncounterSetupInfo == null)
		{
			return null;
		}
		EnemyInfoData enemyInfoData = attachEncounterSetupInfo.fixedEnemyInfoData ?? ((attachEncounterSetupInfo.PreselectedEnemySetup != null) ? attachEncounterSetupInfo.preselectedEnemyInfoData : null);
		if (enemyInfoData == null || !enemyInfoData.boss)
		{
			return null;
		}
		return enemyInfoData.name;
	}

	private void SetupMeditationChoice()
	{
		meditationChoice = new WorldEventChoice
		{
			buttonText = meditationChoiceText.choiceName,
			descriptionText = (HasActivatablePotential() ? meditationChoiceText.choiceDescription : noPotentialHint),
			executeAbleChecker = HasActivatablePotential,
			executeAction = ExecuteMeditationChoice,
			stayAfterChoose = true
		};
		currentDisplayInfo.AddChoice(meditationChoice);
	}

	private void SetupLeapChoice()
	{
		if (isVariantB && riftBlessing != null)
		{
			Influence influence = CreateDisplayBlessing(riftBlessing);
			leapChoice = new WorldEventChoice
			{
				buttonText = ColorText(leapChoiceText.choiceName, leapDangerColor),
				descriptionText = BuildLeapDescription(influence),
				executeAction = ExecuteLeapChoice,
				stayAfterChoose = true
			};
			if (influence != null)
			{
				leapChoice.AddDisplayInfluence(influence);
			}
			currentDisplayInfo.AddChoice(leapChoice);
		}
	}

	private string BuildLeapDescription(Influence displayBlessing)
	{
		string text = displayBlessing?.def?.InfluenceName ?? "";
		if (!string.IsNullOrEmpty(text) && displayBlessing?.def != null)
		{
			text = text.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(displayBlessing.def.quality));
		}
		string text2 = ColorText(leapFaceBossText, leapDangerColor);
		if (!string.IsNullOrEmpty(leapChoiceText.choiceDescription))
		{
			return leapChoiceText.choiceDescription.FormatWith(text, text2);
		}
		return leapDescriptionFormat.FormatWith(text, text2);
	}

	private void ExecuteMeditationChoice()
	{
		if (HasActivatablePotential())
		{
			ActivatePotentialTalentInfo t = new ActivatePotentialTalentInfo
			{
				onCancel = delegate
				{
					gameState.UIManager.activatePotentialTalentPanel.Unset();
				},
				onConfirm = delegate
				{
					currentDisplayInfo.RemoveChoice(meditationChoice);
					SetPanelDirty();
				}
			};
			gameState.UIManager.activatePotentialTalentPanel.Setup(t);
			gameState.UIManager.activatePotentialTalentPanel.SetActiveAndDirtyWithFadeAnim(active: true);
		}
	}

	private void ExecuteLeapChoice()
	{
		if (riftBlessing != null)
		{
			ResolveBlessing(riftBlessing);
		}
	}

	private void CompleteBlessingAndLeap(RiftCharacterBlessingConfig config)
	{
		if (!leaping)
		{
			GrantRiftGuidanceInfluence(config);
			MarkRiftGuidanceUsed(config);
			FinishAndLeapToBoss();
		}
	}

	private void GrantRiftGuidanceInfluence(RiftCharacterBlessingConfig config)
	{
		InfluenceDef guidance = config?.blessing;
		if (!(guidance == null) && !gameState.playerState.bft.influences.Any((Influence influence) => influence?.def == guidance))
		{
			EnqueueAction(new AddInfluenceAction(new Influence(guidance), gameState.playerState.bft));
		}
	}

	private void ResolveBlessing(RiftCharacterBlessingConfig config)
	{
		switch (config.type)
		{
		case RiftBlessingType.DoubleCoin:
			EnqueueAction(new ChangeCoinAction(gameState.playerState.coinCount));
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.DoubleCrystal:
			EnqueueAction(new ChangeCrystalAction(gameState.playerState.crystalCount));
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.SwapCoinAndCrystal:
			EnqueueAction(new SetCoinAction(gameState.playerState.crystalCount));
			EnqueueAction(new SetCrystalAction(gameState.playerState.coinCount));
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.FullHeal:
			EnqueueAction(new HealAction(this, gameState.playerState.bft, gameState.playerState.bft.maxHealth));
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.ReduceWillPowerCost:
			EnqueueAction(new ActionAction(delegate
			{
				ReduceCurrentWillPowerCost(GetBlessingValue(config));
			}));
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.CopyCard:
			SelectOneDeckCard(delegate(Item item)
			{
				int blessingValue = GetBlessingValue(config);
				for (int i = 0; i < blessingValue; i++)
				{
					EnqueueAction(new SpawnItemAction(item.Copy(), gameState.playerState.DeckContainer, withAnim: true, item));
				}
				CompleteBlessingAndLeap(config);
			}, (Item _) => true);
			break;
		case RiftBlessingType.DoubleCardRunes:
			SelectOneDeckCard(delegate(Item item)
			{
				foreach (Rune item in (from rune in item.Runes.OrEmptyIfNull()
					where rune != null && rune.def?.stacking == true
					select rune).ToList())
				{
					EnqueueAction(new ChangeRuneAction(item, item, item));
				}
				CompleteBlessingAndLeap(config);
			}, (Item item) => item.Runes.OrEmptyIfNull().Any((Rune rune) => rune != null && rune.def?.stacking == true));
			break;
		case RiftBlessingType.UpgradeCards:
			SelectDeckCards(GetBlessingValue(config), delegate(List<Item> selected)
			{
				foreach (Item item2 in selected)
				{
					EnqueueAction(new UpgradeItemAction(item2, null));
				}
				CompleteBlessingAndLeap(config);
			}, (Item item) => item.Upgradeable);
			break;
		case RiftBlessingType.RemoveCards:
			SelectDeckCards(GetBlessingValue(config), delegate(List<Item> selected)
			{
				foreach (Item item3 in selected)
				{
					EnqueueAction(new RemoveItemAction(item3, null));
				}
				CompleteBlessingAndLeap(config);
			}, (Item _) => true);
			break;
		case RiftBlessingType.BossFailureRevive:
			gameState.playerState.AddPendingRiftBossEffect(RiftBossEffectType.FailureRevive, GetBlessingValue(config));
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.BossFixedSlotDelta:
			gameState.playerState.AddPendingRiftBossEffect(RiftBossEffectType.FixedSlotDelta, GetBlessingValue(config));
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.BossTurnDrawDelta:
			gameState.playerState.AddPendingRiftBossEffect(RiftBossEffectType.TurnDrawDelta, GetBlessingValue(config));
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.BossMaxHealthPercent:
			gameState.playerState.AddPendingRiftBossEffect(RiftBossEffectType.MaxHealthPercent, GetBlessingValue(config));
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.AddSkillSlot:
			EnqueueAction(new SetCharacterSkillLimitAction(gameState.playerState.skillLimit + GetBlessingValue(config)));
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.RestoreSkillDurability:
			SelectActivatedSkill(delegate(GameSkill skill)
			{
				int num = Mathf.Max(skill.InitDurability - skill.durability, 0);
				if (num > 0)
				{
					EnqueueAction(new ChangeSkillDurabilityAction(skill, num));
				}
				CompleteBlessingAndLeap(config);
			}, (GameSkill skill) => skill.WithDurability && skill.durability < skill.InitDurability);
			break;
		case RiftBlessingType.ReduceSkillCooldown:
			SelectActivatedSkill(delegate(GameSkill skill)
			{
				int stack = GetBlessingValue(config);
				EnqueueAction(new ActionAction(delegate
				{
					AddSkillWord(skill, config.skillWord, stack);
				}));
				CompleteBlessingAndLeap(config);
			}, (GameSkill skill) => CanAddSkillWord(skill, config.skillWord));
			break;
		case RiftBlessingType.CopySkill:
			SelectActivatedSkill(delegate(GameSkill skill)
			{
				EnqueueAction(new ChangeSkillAction(skill.Copy(), ChangeSkillAction.Type.AddOutSlot));
				CompleteBlessingAndLeap(config);
			}, (GameSkill _) => true);
			break;
		case RiftBlessingType.AddHandLimit:
		{
			BattleFieldTransform bft = gameState.playerState.bft;
			bft.handLimitBonus = (int)bft.handLimitBonus + GetBlessingValue(config);
			CompleteBlessingAndLeap(config);
			break;
		}
		case RiftBlessingType.DrawAfterFirstCardMoveEachTurn:
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.BossExtraPlayerTurn:
			gameState.playerState.pendingRiftBossExtraPlayerTurn = true;
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.CopyRunesToTwoCards:
			SelectOneDeckCard(delegate(Item source)
			{
				List<Rune> runes = (from rune in source.Runes.OrEmptyIfNull()
					where rune?.def != null && rune.value > 0
					select rune).ToList();
				SelectDeckCards(2, delegate(List<Item> targets)
				{
					foreach (Item item4 in targets.Where((Item target) => target != source))
					{
						foreach (Rune item5 in runes)
						{
							EnqueueAction(new ChangeRuneAction(item4, item5, source));
						}
					}
					CompleteBlessingAndLeap(config);
				}, (Item item) => item != source);
			}, (Item item) => item.Runes.OrEmptyIfNull().Any((Rune rune) => rune?.def != null && rune.value > 0));
			break;
		case RiftBlessingType.ReplaceAllBlessingsWithSameQuality:
			ReplaceAllBlessingsWithSameQuality(config);
			break;
		case RiftBlessingType.GainThreeRandomBlessings:
			GainThreeRandomBlessings(config);
			break;
		case RiftBlessingType.GainCrystalPerBlessing:
			EnqueueAction(new ChangeCrystalAction(GetBlessingCountIncludingGuidance(config) * 4));
			CompleteBlessingAndLeap(config);
			break;
		case RiftBlessingType.GainCoinPerBlessing:
			EnqueueAction(new ChangeCoinAction(GetBlessingCountIncludingGuidance(config) * 8));
			CompleteBlessingAndLeap(config);
			break;
		default:
			FinishAndLeapToBoss();
			break;
		}
	}

	private int GetBlessingCountIncludingGuidance(RiftCharacterBlessingConfig config)
	{
		return (gameState.playerState.bft.influences?.Count ?? 0) + ((config?.blessing != null && !gameState.playerState.bft.influences.Any((Influence influence) => influence?.def == config.blessing)) ? 1 : 0);
	}

	private void GainThreeRandomBlessings(RiftCharacterBlessingConfig config)
	{
		List<InfluenceDef> list = (from influence in gameState.playerState.bft.influences.OrEmptyIfNull()
			select influence?.def into definition
			where definition != null
			select definition).ToList();
		foreach (InfluenceDef item in gameState.generationManager.GenerateRandomBlessingsByQuality(3, QualityLevel.None, list))
		{
			if (!(item == null))
			{
				list.Add(item);
				EnqueueAction(new AddInfluenceAction(new Influence(item), gameState.playerState.bft));
			}
		}
		CompleteBlessingAndLeap(config);
	}

	private void ReplaceAllBlessingsWithSameQuality(RiftCharacterBlessingConfig config)
	{
		List<Influence> list = (from influence in gameState.playerState.bft.influences.OrEmptyIfNull()
			where influence?.def != null
			select influence).ToList();
		HashSet<InfluenceDef> hashSet = new HashSet<InfluenceDef>(list.Select((Influence influence) => influence.def));
		foreach (Influence item in list)
		{
			hashSet.Remove(item.def);
			InfluenceDef influenceDef = gameState.generationManager.GenerateRandomBlessingsByQuality(1, item.def.quality, hashSet).FirstOrDefault();
			if (!(influenceDef == null) && !(influenceDef == item.def))
			{
				hashSet.Add(influenceDef);
				EnqueueAction(new RemoveInfluenceAction(item));
				EnqueueAction(new AddInfluenceAction(new Influence(influenceDef), gameState.playerState.bft));
			}
		}
		CompleteBlessingAndLeap(config);
	}

	private void SelectOneDeckCard(Action<Item> onSelected, Func<Item, bool> selectable)
	{
		List<Item> list = gameState.playerState.DeckContainer.GetAllItems().Where(selectable).ToList();
		if (list.Count == 0)
		{
			CompleteBlessingAndLeap(riftBlessing);
			return;
		}
		List<Item> selected = new List<Item>();
		SelectItemInfo selectItemInfo = SelectItemInfo.SimpleSelectSingleItem(list, delegate
		{
			Item item = selected.FirstOrDefault();
			if (item != null)
			{
				onSelected(item);
			}
		}, null, selected, selectable, () => GetBlessingPreviewText(riftBlessing));
		selectItemInfo.canCancel = () => false;
		EnqueueAction(new NormalEventSelectItemAction(selectItemInfo));
	}

	private void SelectDeckCards(int maxCount, Action<List<Item>> onSelected, Func<Item, bool> selectable)
	{
		List<Item> list = gameState.playerState.DeckContainer.GetAllItems().Where(selectable).ToList();
		int targetCount = Mathf.Min(maxCount, list.Count);
		if (targetCount <= 0)
		{
			CompleteBlessingAndLeap(riftBlessing);
			return;
		}
		List<Item> selected = new List<Item>();
		SelectItemInfo info = new SelectItemInfo
		{
			pool = list,
			output = selected,
			singleSelect = false,
			panelHintGetter = () => GetBlessingPreviewText(riftBlessing),
			shouldCancel = () => false,
			canCancel = () => false,
			canSelect = (Item item) => selected.Contains(item) || selected.Count < targetCount,
			canConfirm = () => selected.Count <= targetCount,
			onConfirm = delegate
			{
				onSelected(new List<Item>(selected));
			},
			selectorBft = gameState.playerState.bft
		};
		EnqueueAction(new NormalEventSelectItemAction(info));
	}

	private void SelectActivatedSkill(Action<GameSkill> onSelected, Func<GameSkill, bool> selectable)
	{
		List<GameSkill> list = gameState.playerState.ActivatedSkills.Where((GameSkill skill) => skill != null && selectable(skill)).ToList();
		if (list.Count == 0)
		{
			CompleteBlessingAndLeap(riftBlessing);
			return;
		}
		List<WorldEventChoice> list2 = new List<WorldEventChoice>();
		foreach (GameSkill item in list)
		{
			GameSkill captured = item;
			WorldEventChoice worldEventChoice = new WorldEventChoice
			{
				buttonText = captured.NameWithQualityAndUpgrade,
				descriptionText = GetBlessingPreviewText(riftBlessing),
				executeAction = delegate
				{
					onSelected(captured);
				}
			};
			worldEventChoice.AddDisplaySkill(captured);
			list2.Add(worldEventChoice);
		}
		RefreshAfterChoose(GetBlessingPreviewText(riftBlessing), list2, showLeaveButton: false);
	}

	private void FinishAndLeapToBoss()
	{
		if (!leaping)
		{
			leaping = true;
			PathLocation visibleEndLocation = gameState.dungeonMapManager.currentDungeonMap.VisibleEndLocation;
			FinishedWorldEvent(exitRoom: false);
			if (visibleEndLocation != null)
			{
				EnqueueAction(new SwitchToLocationFromRoomAction(visibleEndLocation));
			}
		}
	}

	private void ReduceCurrentWillPowerCost(int amount)
	{
		int num = amount;
		foreach (Influence item in gameState.playerState.bft.influences.Where((Influence influence) => influence != null && influence.willPowerCost > 0).Reverse().ToList())
		{
			if (num <= 0)
			{
				break;
			}
			int num2 = Mathf.Min(num, item.willPowerCost);
			item.willPowerCost -= num2;
			num -= num2;
		}
	}

	private bool ShouldUseVariantB()
	{
		int stage = gameState.contractState.stage;
		if (!CanUseVariantBNow())
		{
			return false;
		}
		if (gameState.difficultyState.contractDifficulty >= DifficultyLevel.Nightmare)
		{
			if (stage != 2 && stage != 4)
			{
				return stage == 6;
			}
			return true;
		}
		if (stage != 4 && stage != 6)
		{
			return false;
		}
		if (gameState.playerState.riftNormalVariantBStage == 0)
		{
			gameState.playerState.riftNormalVariantBStage = ((stage == 4 && gameState.randomManager.NextFloat() < 0.5f) ? 4 : 6);
		}
		return gameState.playerState.riftNormalVariantBStage == stage;
	}

	private bool CanUseVariantBNow()
	{
		if ((from state in gameState.applicationState.characterManager.GetAllUnlockedCharacterStates()
			where state?.def != null
			select state).ToList().Count < 2)
		{
			return false;
		}
		if (!HasBronzeDocument())
		{
			return false;
		}
		return GetUnusedConfiguredUnlockedCharacterStates().Count > 0;
	}

	private List<PlayerCharacterState> GetConfiguredUnlockedCharacterStates()
	{
		return (from state in gameState.applicationState.characterManager.GetAllUnlockedCharacterStates()
			where state?.def != null && IsValidRiftBlessingConfig(GetBlessingConfig(state.def))
			select state).ToList();
	}

	private List<PlayerCharacterState> GetUnusedConfiguredUnlockedCharacterStates()
	{
		PlayerGameStateInfo playerState = gameState.playerState;
		if (playerState.usedRiftGuidanceIds == null)
		{
			playerState.usedRiftGuidanceIds = new List<string>();
		}
		return GetConfiguredUnlockedCharacterStates().Where(delegate(PlayerCharacterState state)
		{
			RiftCharacterBlessingConfig blessingConfig = GetBlessingConfig(state.def);
			return !IsRiftGuidanceUsed(blessingConfig);
		}).ToList();
	}

	private bool IsRiftGuidanceUsed(RiftCharacterBlessingConfig config)
	{
		string riftGuidanceId = GetRiftGuidanceId(config);
		if (!string.IsNullOrEmpty(riftGuidanceId))
		{
			return gameState.playerState.usedRiftGuidanceIds.Contains(riftGuidanceId);
		}
		return false;
	}

	private void MarkRiftGuidanceUsed(RiftCharacterBlessingConfig config)
	{
		string riftGuidanceId = GetRiftGuidanceId(config);
		if (!string.IsNullOrEmpty(riftGuidanceId))
		{
			PlayerGameStateInfo playerState = gameState.playerState;
			if (playerState.usedRiftGuidanceIds == null)
			{
				playerState.usedRiftGuidanceIds = new List<string>();
			}
			if (!gameState.playerState.usedRiftGuidanceIds.Contains(riftGuidanceId))
			{
				gameState.playerState.usedRiftGuidanceIds.Add(riftGuidanceId);
			}
		}
	}

	private string GetRiftGuidanceId(RiftCharacterBlessingConfig config)
	{
		return config?.blessing?.FullID;
	}

	private bool HasBronzeDocument()
	{
		ClavisDef clavisDef = bronzeDocumentClavis;
		if (clavisDef == null)
		{
			clavisDef = gameState.applicationState.assetCatalog.clavisDefDefCatalog.ValidAssets.FirstOrDefault((ClavisDef c) => c != null && c.clavisName == "青铜文书");
		}
		if (clavisDef != null)
		{
			if (!gameState.applicationState.clavisManager.IsUnlocked(clavisDef))
			{
				return gameState.applicationState.clavisManager.IsActivated(clavisDef);
			}
			return true;
		}
		return false;
	}

	private bool HasActivatablePotential()
	{
		return !gameState.playerState.CurrentActivateAblePotentialTalent.IsEmptyOrNull();
	}

	private RiftCharacterBlessingConfig GetBlessingConfig(PlayerCharacterDef character)
	{
		if (character == null)
		{
			return null;
		}
		return blessings.OrEmptyIfNull().FirstOrDefault((RiftCharacterBlessingConfig config) => config != null && config.character == character);
	}

	private static bool IsValidRiftBlessingConfig(RiftCharacterBlessingConfig config)
	{
		if (config?.character != null)
		{
			return config.blessing != null;
		}
		return false;
	}

	private static Influence CreateDisplayBlessing(RiftCharacterBlessingConfig config)
	{
		if (!(config?.blessing == null))
		{
			return new Influence(config.blessing);
		}
		return null;
	}

	private string GetBlessingPreviewText(RiftCharacterBlessingConfig config)
	{
		TextDataGenerator generator;
		return CreateDisplayBlessing(config)?.GetInfluenceDescription(gameState.applicationState, out generator) ?? "";
	}

	private int GetBlessingValue(RiftCharacterBlessingConfig config)
	{
		Influence influence = CreateDisplayBlessing(config);
		if (influence == null)
		{
			throw new InvalidOperationException("狭缝祝福配置缺少展示祝福。");
		}
		List<ValueDisplay> descendantNodeOfType = influence.GetDescendantNodeOfType<ValueDisplay>();
		if (descendantNodeOfType.Count != 1)
		{
			throw new InvalidOperationException($"狭缝祝福【{influence.def.InfluenceName}】必须且只能配置一个 ValueDisplay，当前数量：{descendantNodeOfType.Count}。");
		}
		GameEvent eve = Effect.GenerateDisplayEvent(gameState, influence);
		int? num = descendantNodeOfType[0].GenerateValue(eve);
		if (!num.HasValue || num.Value <= 0)
		{
			throw new InvalidOperationException("狭缝祝福【" + influence.def.InfluenceName + "】的 ValueDisplay 必须生成正整数，当前值：" + (num?.ToString() ?? "NULL") + "。");
		}
		return num.Value;
	}

	private static bool CanAddSkillWord(GameSkill skill, SkillWordDef skillWord)
	{
		if (skill?.profileSkill != null && skillWord != null && !skill.OneCastPerBattle && skill.CastCooldown > 1)
		{
			return skillWord.CheckAttachAble(skill.profileSkill);
		}
		return false;
	}

	private static void AddSkillWord(GameSkill skill, SkillWordDef skillWord, int stack)
	{
		if (skill != null && !(skillWord == null) && stack > 0)
		{
			if (skill.skillWords == null)
			{
				skill.skillWords = new List<SkillWord>();
			}
			SkillWord skillWord2 = skill.skillWords.FirstOrDefault((SkillWord word) => word?.def == skillWord);
			if (skillWord2 != null)
			{
				skillWord2.stack += stack;
			}
			else
			{
				skill.skillWords.Add(new SkillWord(skillWord, stack, skill));
			}
		}
	}

	private static string ColorText(string text, string color)
	{
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(color))
		{
			return text;
		}
		return "<color=" + color + ">" + text + "</color>";
	}
}
