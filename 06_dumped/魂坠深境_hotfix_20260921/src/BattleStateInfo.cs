using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStateInfo : InGameManager
{
	public class Config
	{
		public float alphaTime = 0.3f;

		public float waitBannerTime = 0.25f;

		public float waitActionTime = 0.2f;

		public int overflowDetailedAnimationCount = 5;

		public int overflowCompressedItemsPerFrame = 1;
	}

	private const int AutoPlayPerItemLimit = 10;

	private const int AutoPlayTotalLimit = 100;

	[SaveField(null, null)]
	public string battleGUID;

	private WrapperEnumerator battleTurnStateMachine;

	private WrapperEnumerator autoPlayStateMachine;

	private readonly Dictionary<Item, int> autoPlayUseCounts = new Dictionary<Item, int>();

	private int autoPlayTotalUseCount;

	public bool endEnemyTurnFlag;

	public bool endPlayerTurnFlag;

	public CharacterAI enemy;

	public bool isConsumingOverflowItems;

	[SaveField(null, null)]
	public bool startBattleFlag;

	[SaveField(null, null)]
	public int turnCount;

	[SaveField(null, null)]
	public TurnPhase turnPhase;

	public bool isInBattle;

	public BattleFieldTransform player => base.gameState.playerState.bft;

	public ItemContainer EnemyTableContainer => enemy?.bft.tableContainer;

	public ItemContainer EnemyDrawPileContainer => enemy?.bft.drawPileItemContainer;

	public ItemContainer EnemyWastePileContainer => enemy?.bft.wastePileItemContainer;

	public ItemContainer EnemyExilePileContainer => enemy?.bft.exilePileItemContainer;

	public ItemContainer EnemyDeckPileContainer => enemy?.bft.deckContainer;

	public ItemContainer EnemyCompanionContainer => enemy?.bft.companionContainer;

	public ItemContainer PlayerTableContainer => player?.tableContainer;

	public ItemContainer PlayerDrawPileContainer => player?.drawPileItemContainer;

	public ItemContainer PlayerWastePileContainer => player?.wastePileItemContainer;

	public ItemContainer PlayerExilePileContainer => player?.exilePileItemContainer;

	public ItemContainer PlayerDeckPileContainer => player?.deckContainer;

	public ItemContainer PlayerCompanionContainer => player?.companionContainer;

	public BattleAutoPlayState autoPlayState { get; private set; }

	private bool IsAutoPlayEnabled => GlobalConfigData.autoPlayButtonMode != AutoPlayButtonMode.Disabled;

	private SavingUtil.GlobalConfigSaveData GlobalConfigData => Bootstrap.instance.globalSaveConfigData;

	public void StartBattle()
	{
		base.gameState.ResetBattleRuntimeInputState();
		ResetAutoPlayState();
		endPlayerTurnFlag = false;
		isConsumingOverflowItems = false;
		battleGUID = Guid.NewGuid().ToString();
		player?.ClearExilePile();
		enemy?.bft?.ClearExilePile();
		base.gameState.battleState.startBattleFlag = true;
		EnqueueAction(new RefreshAllSkillAction());
		battleTurnStateMachine = base.gameState.StartCoroutine(BattleTurnStateMachine);
	}

	public void StopBattleState()
	{
		StopAutoPlay();
		isInBattle = false;
		endPlayerTurnFlag = false;
		player?.ClearExilePile();
		enemy?.bft?.ClearExilePile();
		if (battleTurnStateMachine != null)
		{
			battleTurnStateMachine.Stop();
			battleTurnStateMachine = null;
		}
		battleGUID = null;
		enemy = null;
	}

	public void RefreshAutoPlayState()
	{
		if (autoPlayState != BattleAutoPlayState.Playing)
		{
			autoPlayState = ((IsAutoPlayEnabled && CanStartAutoPlay()) ? BattleAutoPlayState.Ready : BattleAutoPlayState.Disabled);
		}
	}

	public bool TryStartAutoPlay()
	{
		if (!IsAutoPlayEnabled || !CanStartAutoPlay())
		{
			autoPlayState = BattleAutoPlayState.Disabled;
			return false;
		}
		autoPlayUseCounts.Clear();
		autoPlayTotalUseCount = 0;
		base.gameState.UIManager.playerItemArea.controlInfo.CancelSpell();
		autoPlayState = BattleAutoPlayState.Playing;
		autoPlayStateMachine = base.gameState.StartCoroutine(AutoPlayCor);
		return true;
	}

	public bool TryRequestEndPlayerTurn()
	{
		if (!base.gameState.PlayerControlAble || endPlayerTurnFlag)
		{
			return false;
		}
		endPlayerTurnFlag = true;
		return true;
	}

	public IEnumerable<Item> GetAllBattleFieldItems()
	{
		if (PlayerTableContainer != null)
		{
			foreach (Item allItem in PlayerTableContainer.GetAllItems())
			{
				yield return allItem;
			}
		}
		if (PlayerCompanionContainer != null)
		{
			foreach (Item allItem2 in PlayerCompanionContainer.GetAllItems())
			{
				yield return allItem2;
			}
		}
		if (EnemyTableContainer != null)
		{
			foreach (Item allItem3 in EnemyTableContainer.GetAllItems())
			{
				yield return allItem3;
			}
		}
		if (EnemyCompanionContainer == null)
		{
			yield break;
		}
		foreach (Item allItem4 in EnemyCompanionContainer.GetAllItems())
		{
			yield return allItem4;
		}
	}

	public BattleFieldTransform GetOpponent(BattleFieldTransform bft)
	{
		if (bft == null)
		{
			return null;
		}
		if (bft.isPlayer && enemy != null)
		{
			return enemy.bft;
		}
		if (bft.isEnemy)
		{
			return player;
		}
		return null;
	}

	public void SetPlayerAfterTurnEndFlag()
	{
		foreach (Item allRealItem in player.GetAllRealItems())
		{
			SetEndTurnFlag(allRealItem);
		}
	}

	public void SetEnemyAfterTurnEndFlag()
	{
		foreach (Item allRealItem in enemy.bft.GetAllRealItems())
		{
			SetEndTurnFlag(allRealItem);
		}
	}

	private void SetEndTurnFlag(Item item)
	{
		item.afterTurnEnd = true;
		foreach (Enchant item2 in item.Enchants.OrEmptyIfNull())
		{
			item2.afterTurnEnd = true;
		}
	}

	private void SetTurnPhase(TurnPhase phase)
	{
		turnPhase = phase;
		EnqueueAction(delegate
		{
			base.gameState.gameEventManager.TriggerTurnPhaseChange();
		});
	}

	private IEnumerator BattleTurnStateMachine()
	{
		Config config = ScriptableObjectSingleton<VFXDataPreset>.instance.battleStateInfoConfig;
		enemy.BftDisplay.SetOutlineAlpha(0f, 0.3f);
		SetTurnPhase(TurnPhase.Prepare);
		yield return new WaitUntil(() => startBattleFlag);
		startBattleFlag = false;
		turnCount = 0;
		isInBattle = true;
		base.gameState.actionManager.EnqueueAction(new InitDrawPileAction(base.gameState.playerState.bft));
		EnqueueAction(delegate
		{
			base.gameState.gameEventManager.TriggerBattleStart();
		});
		yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
		EnqueueAction(delegate
		{
			base.gameState.battleState.enemy.InitAfterBattleStart();
		});
		yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
		PathLocation currentLocation = base.gameState.dungeonMapManager.currentLocation;
		if (currentLocation != null && currentLocation.variantType == PathLocation.LocationVariantType.AmbushFight)
		{
			base.gameState.playerBftDisplay.skipNextTurn = true;
		}
		while (true)
		{
			if (!base.gameState.playerBftDisplay.skipNextTurn)
			{
				yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
				SetTurnPhase(TurnPhase.PlayerTurnStartPhase);
				if (turnCount != 0)
				{
					ThinkingAnalyticsExtends.TrackRoundOver(base.gameState, battleStop: false);
				}
				turnCount++;
				EnqueueAction(new EnumeratorAction(PlayerTurnStartAnim));
				yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
				base.gameState.gameEventManager.TriggerPlayerTurnStart();
				if (turnCount == 1)
				{
					base.gameState.gameEventManager.TriggerPlayerFirstTurnStart();
				}
				yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
				if (base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.OverflowConsumeOnTurnStart))
				{
					isConsumingOverflowItems = true;
					base.gameState.UIManager.playerItemArea.playerOverloadDisplay.PlayMarkEffect();
					base.gameState.actionManager.EnqueueAction(new WaitAction(0.2f / base.gameState.actionManager.GetBattleScale()));
					base.gameState.actionManager.EnqueueAction(new ConsumeOverflowSlotAction(base.gameState.playerState.TableContainer));
					base.gameState.actionManager.EnqueueAction(new ActionAction(delegate
					{
						isConsumingOverflowItems = false;
					}));
				}
				UpdatePlayerCoolDown();
				EnqueueAction(new ActionAction(TurnStartDrawItems));
				yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
				if (base.gameState.IsCharacterTrialGame && turnCount == 1)
				{
					EnqueueAction(new PlayCharacterTrialTutorialSequenceAction());
					yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
				}
				SetTurnPhase(TurnPhase.PlayerControlPhase);
				yield return new WaitUntil(() => endPlayerTurnFlag && base.gameState.actionManager.isEmpty);
				if (base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.BattleAutomaton))
				{
					yield return BattleAutomatonEndTurnCor();
					yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
				}
				SetTurnPhase(TurnPhase.PlayerTurnEndPhase);
				endPlayerTurnFlag = false;
				enemy.BftDisplay.SetOutlineAlpha(0f, config.alphaTime / base.gameState.actionManager.GetBattleScale());
				EnqueueAction(new EnumeratorAction(PlayerTurnEndAnim));
				yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
				base.gameState.gameEventManager.TriggerBeforePlayerTurnEnd();
				yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
				if (!base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.OverflowConsumeOnTurnStart))
				{
					isConsumingOverflowItems = true;
					base.gameState.actionManager.EnqueueAction(new WaitAction(config.waitActionTime / base.gameState.actionManager.GetBattleScale()));
					base.gameState.actionManager.EnqueueAction(new ConsumeOverflowSlotAction(base.gameState.playerState.TableContainer));
					base.gameState.actionManager.EnqueueAction(new ActionAction(delegate
					{
						isConsumingOverflowItems = false;
					}));
				}
				base.gameState.actionManager.EnqueueAction(delegate
				{
					base.gameState.gameEventManager.TriggerAfterPlayerTurnEndWaste();
				});
				base.gameState.actionManager.EnqueueAction(delegate
				{
					base.gameState.gameEventManager.TriggerPlayerTurnEnd();
				});
				yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
				if (turnCount == 1 && base.gameState.playerState.pendingRiftBossExtraPlayerTurn)
				{
					PathLocation currentLocation2 = base.gameState.dungeonMapManager.currentLocation;
					if (currentLocation2 != null && currentLocation2.type == PathLocation.LocationType.Boss)
					{
						base.gameState.playerState.pendingRiftBossExtraPlayerTurn = false;
						continue;
					}
				}
				yield return base.gameState.UIManager.bannerHintDisplay.ShowBannerHint(new BannerHintDisplay.SetupInfo
				{
					hintText = ScriptableObjectSingleton<TextDataPreset>.instance.term_opponent_turn,
					backgroundType = BannerHintDisplay.BackgroundType.TurnEnd,
					soundType = BannerHintDisplay.SoundType.EnemyTurnStart,
					textType = BannerHintDisplay.TextType.Default,
					tweenTime = ScriptableObjectSingleton<VFXDataPreset>.instance.playerBftUIDisplayConfig.bannerTweenTime,
					stayTime = ScriptableObjectSingleton<VFXDataPreset>.instance.playerBftUIDisplayConfig.bannerStayTime
				}, 1f);
				EnqueueAction(new WaitAction(config.waitBannerTime / base.gameState.actionManager.GetBattleScale()));
				yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
			}
			else
			{
				base.gameState.playerBftDisplay.skipNextTurn = false;
			}
			SetTurnPhase(TurnPhase.EnemyMovePhase);
			if (!enemy.bft.skipNextTurn)
			{
				yield return enemy.EnemyTurnCor();
			}
			else
			{
				enemy.bft.skipNextTurn = false;
			}
			EnqueueAction(new ChangeTempHealthAction(-(int)player.tempHealth, base.gameState.battleState.player));
		}
	}

	private void TurnStartDrawItems()
	{
		ProtectedInt protectedInt = base.gameState.playerState.bft.turnDrawItemCount;
		if (turnCount == 1)
		{
			protectedInt = base.gameState.playerState.bft.FirstTurnDrawItemCount;
		}
		GameEvent.ResolveInfo resolveInfo = base.gameState.gameEventManager.TriggerTurnDrawResolveInfo(player, GameEvent.TriggerType.Real);
		protectedInt = (int)protectedInt + resolveInfo.deltaValue;
		if (resolveInfo.reject)
		{
			protectedInt = 0;
		}
		DrawFromPileAction.DrawMode drawMode = ((base.gameState.IsCharacterTrialGame && turnCount == 1) ? DrawFromPileAction.DrawMode.FirstAvailable : DrawFromPileAction.DrawMode.Random);
		for (int i = 0; i < (int)protectedInt; i++)
		{
			EnqueueAction(new DrawFromPileAction(base.gameState.playerState.bft, null, 0, drawMode));
		}
		EnqueueAction(new HintOverloadAction());
		EnqueueAction(new ActionAction(delegate
		{
			base.gameState.gameEventManager.TriggerAfterTurnStartDrawItems();
		}));
		if (base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.ShuffleAfterTurnStartDraw))
		{
			EnqueueAction(new ShuffleContainerAction(base.gameState.playerState.TableContainer));
		}
	}

	private void UpdatePlayerCoolDown()
	{
		foreach (Item allCoolDownItem in player.GetAllCoolDownItems())
		{
			UpdateItemTurnCoolDown(allCoolDownItem);
		}
		foreach (GameSkill activatedSkill in base.gameState.playerState.ActivatedSkills)
		{
			if (!activatedSkill.CoolDowned)
			{
				ChangeSkillCooldownAction action = new ChangeSkillCooldownAction(activatedSkill, -1);
				base.gameState.actionManager.EnqueueAction(action);
			}
		}
	}

	private void UpdateItemTurnCoolDown(Item item)
	{
		if (item.IsCoolDowning && !base.gameState.gameEventManager.TriggerTurnStartCoolDownResolve(item, GameEvent.TriggerType.Real).reject)
		{
			EnqueueAction(new ChangeCoolDownAction(item, null, -1));
		}
	}

	private void EnqueueAction(GameAction action)
	{
		base.gameState.actionManager.EnqueueAction(action);
	}

	private void EnqueueAction(Action action)
	{
		base.gameState.actionManager.EnqueueAction(action);
	}

	private IEnumerator PlayerTurnStartAnim()
	{
		List<WrapperEnumerator> list = new List<WrapperEnumerator>();
		list.Add(base.gameState.StartCoroutine(base.gameState.UIManager.gameVirtualCamera.lookPivotCameraController.PlayerTurnStartOffsetCor));
		list.Add(base.gameState.StartCoroutine(base.gameState.UIManager.playerBftUIDisplay.skillPanel.PlayerTurnStart));
		list.Add(base.gameState.StartCoroutine(base.gameState.UIManager.battlePanel.endTurnButton.PlayerTurnStart));
		if (base.applicationState.settingManager.uiTimeScaleTypeSetting == TimeScaleManager.TimeScaleType.X1)
		{
			yield return WrapperEnumerator.WaitAll(list);
		}
		enemy?.BftDisplay?.SetOutlineAlpha(1f, 0.3f);
		yield return base.gameState.UIManager.bannerHintDisplay.ShowBannerHint(new BannerHintDisplay.SetupInfo
		{
			hintText = ScriptableObjectSingleton<TextDataPreset>.instance.term_your_turn,
			subText = string.Format(ScriptableObjectSingleton<TextDataPreset>.instance.term_turn_count_arg0_colored, turnCount),
			soundType = BannerHintDisplay.SoundType.PlayerTurnStart,
			backgroundType = BannerHintDisplay.BackgroundType.TurnStart,
			textType = BannerHintDisplay.TextType.TurnStart,
			tweenTime = ScriptableObjectSingleton<VFXDataPreset>.instance.playerBftUIDisplayConfig.bannerTweenTime,
			stayTime = ScriptableObjectSingleton<VFXDataPreset>.instance.playerBftUIDisplayConfig.bannerStayTime
		}, 1f);
	}

	private IEnumerator PlayerTurnEndAnim()
	{
		List<WrapperEnumerator> list = new List<WrapperEnumerator>();
		list.Add(base.gameState.StartCoroutine(base.gameState.UIManager.gameVirtualCamera.lookPivotCameraController.PlayerTurnEndOffsetCor));
		list.Add(base.gameState.StartCoroutine(base.gameState.UIManager.battlePanel.endTurnButton.PlayerTurnEnd));
		list.Add(base.gameState.StartCoroutine(base.gameState.UIManager.playerBftUIDisplay.skillPanel.PlayerTurnEnd));
		if (!base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.OverflowConsumeOnTurnStart))
		{
			base.gameState.StartCoroutine(PlayMarkEffect);
		}
		if (base.gameState.applicationState.settingManager.uiTimeScaleTypeSetting == TimeScaleManager.TimeScaleType.X1)
		{
			yield return WrapperEnumerator.WaitAll(list);
		}
	}

	private IEnumerator PlayMarkEffect()
	{
		yield return new WaitForSeconds(0.12f);
		base.gameState.UIManager.playerItemArea.playerOverloadDisplay.PlayMarkEffect();
	}

	private IEnumerator BattleAutomatonEndTurnCor()
	{
		foreach (Item allItem in base.gameState.playerState.TableContainer.GetAllItems(orderReverse: true))
		{
			if (allItem.IsCooldownCastAble(base.gameState))
			{
				PlayerControlInfo.SpellRequest spellRequest = new PlayerControlInfo.SpellRequest
				{
					spellItem = allItem
				};
				spellRequest.interval = 0.3f;
				spellRequest.withAnim = true;
				base.gameState.UIManager.battlePanel.playerBftUIDisplay.tableItemArea.controlInfo.EnqueueSpellRequest(spellRequest);
			}
		}
		yield return new WaitUntil(() => base.gameState.actionManager.isEmpty);
	}

	private IEnumerator AutoPlayCor()
	{
		PlayerControlInfo controlInfo = base.gameState.UIManager.playerItemArea.controlInfo;
		while (IsAutoPlayContextValid() && autoPlayTotalUseCount < 100)
		{
			yield return new WaitUntil(() => base.gameState.actionManager.isEmpty && controlInfo.requests.Count == 0 && controlInfo.RealSpellCastInfo.castState == SpellCastInfo.State.Preparing);
			if (!IsAutoPlayContextValid())
			{
				break;
			}
			Item item = FindRightmostAutoPlayableItem(onlyItemsNotUsedByPreviousRun: false);
			if (item == null)
			{
				break;
			}
			autoPlayUseCounts.TryGetValue(item, out var value);
			autoPlayUseCounts[item] = value + 1;
			autoPlayTotalUseCount++;
			controlInfo.EnqueueSpellRequest(new PlayerControlInfo.SpellRequest
			{
				spellItem = item,
				interval = 0.3f,
				withAnim = true,
				waitForAnimComplete = true
			});
		}
		yield return new WaitUntil(() => base.gameState.actionManager.isEmpty && controlInfo.requests.Count == 0 && controlInfo.RealSpellCastInfo.castState == SpellCastInfo.State.Preparing);
		if (GlobalConfigData.autoPlayButtonMode == AutoPlayButtonMode.EnabledAndEndTurn && IsAutoPlayContextValid())
		{
			TryRequestEndPlayerTurn();
		}
		autoPlayStateMachine = null;
		autoPlayState = BattleAutoPlayState.Disabled;
	}

	private bool CanStartAutoPlay()
	{
		if (!IsAutoPlayContextValid() || !base.gameState.actionManager.isEmpty)
		{
			return false;
		}
		PlayerControlInfo controlInfo = base.gameState.UIManager.playerItemArea.controlInfo;
		if (controlInfo.requests.Count != 0 || controlInfo.RealSpellCastInfo.castState != SpellCastInfo.State.Preparing)
		{
			return false;
		}
		return FindRightmostAutoPlayableItem(autoPlayTotalUseCount >= 100) != null;
	}

	private bool IsAutoPlayContextValid()
	{
		if (IsAutoPlayEnabled && isInBattle && turnPhase == TurnPhase.PlayerControlPhase && !endPlayerTurnFlag && base.gameState.PlayerControlAble)
		{
			return !base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.BattleAutomaton);
		}
		return false;
	}

	private Item FindRightmostAutoPlayableItem(bool onlyItemsNotUsedByPreviousRun)
	{
		List<Item> list = base.gameState.playerState.TableContainer?.GetAllItems(orderReverse: true);
		if (list == null)
		{
			return null;
		}
		foreach (Item item in list)
		{
			autoPlayUseCounts.TryGetValue(item, out var value);
			if (value < 10 && (!onlyItemsNotUsedByPreviousRun || value <= 0) && IsAutoPlayable(item))
			{
				return item;
			}
		}
		return null;
	}

	private bool IsAutoPlayable(Item item)
	{
		if (item == null || item.owner != base.gameState.playerState.TableContainer || !item.HasSpell || !item.CoolDowned)
		{
			return false;
		}
		ItemSpell currentSpell = item.CurrentSpell;
		ItemRequirement itemRequirement = currentSpell?.requirement;
		if (currentSpell == null || itemRequirement == null || itemRequirement.NeedTarget || itemRequirement.targetRequirement != null)
		{
			return false;
		}
		if (item.IsManualSelectCost(base.gameState, GameEvent.TriggerType.InBattleDisplay))
		{
			return false;
		}
		if (currentSpell.spellNode.GetDescendantNodeOfType<PlayerSelectItemsEffect>(includeSelf: true).Count > 0 || currentSpell.spellNode.GetDescendantNodeOfType<PlayerSelectItemsCountEffect>(includeSelf: true).Count > 0)
		{
			return false;
		}
		SpellCastInfo.SpellCastFailureReason failureReason;
		return SpellCastInfo.SpellCastAble(base.gameState, item, null, inBattleDisplay: true, out failureReason);
	}

	private void StopAutoPlay()
	{
		if (autoPlayStateMachine != null)
		{
			autoPlayStateMachine.Stop();
			autoPlayStateMachine = null;
		}
		autoPlayState = BattleAutoPlayState.Disabled;
	}

	private void ResetAutoPlayState()
	{
		StopAutoPlay();
		autoPlayUseCounts.Clear();
		autoPlayTotalUseCount = 0;
	}
}
