using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ContainerOfSoul.Display.Game.BattleDisplay;
using ContainerOfSoul.Display.SettingPanelModule;
using FMODUnity;
using UnityEngine;

public class GuideModule_Intro : GuideModule
{
	private GameStateInfo gameState;

	public int mentorEnemyHealthBattl1 = 24;

	public int mentorEnemyHealthBattl2 = 24;

	public int initPlayerHealth;

	public int initPlayerMaxHealth;

	public DraftMapRef guideDraftMapRef;

	public CutsceneRef introCutsceneRef;

	[LocalizeText("新手引导－开幕")]
	public string startDarkScreen1;

	[LocalizeText("新手引导－开幕")]
	public string startDarkScreen2;

	[LocalizeText("新手引导－阶段1")]
	public string teach1_dialogue1;

	[LocalizeText("新手引导－阶段1")]
	public string teach1_dialogue2;

	[LocalizeText("新手引导－阶段1")]
	public string teach1_dialogue3;

	[LocalizeText("新手引导－阶段1")]
	public string teach1_dialogue4;

	[LocalizeText("新手引导－阶段1")]
	public string teach1_dialogue5;

	[LocalizeText("新手引导－阶段1")]
	public string teach1_subtitle1;

	[LocalizeText("新手引导－阶段1")]
	public string teach1_dialogue6;

	[LocalizeText("新手引导－阶段1")]
	public string teach1_subtitle2;

	[LocalizeText("新手引导－阶段1")]
	public string teach1_subtitle3;

	[LocalizeText("新手引导－阶段1")]
	public string teach1_dialogue7;

	[LocalizeText("新手引导－阶段1")]
	public string teach1_subtitle4;

	[LocalizeText("新手引导－阶段2")]
	public string teach2_dialogue1;

	[LocalizeText("新手引导－阶段2")]
	public string teach2_dialogue2;

	[LocalizeText("新手引导－阶段2")]
	public string teach2_subtitle1;

	[LocalizeText("新手引导－阶段2")]
	public string teach2_subtitle2;

	[LocalizeText("新手引导－阶段2")]
	public string teach2_dialogue3;

	[LocalizeText("新手引导－阶段2")]
	public string teach2_dialogue4;

	[LocalizeText("新手引导－阶段2")]
	public string teach2_subtitle3;

	[LocalizeText("新手引导－阶段3")]
	public string teach3_dialogue1;

	[LocalizeText("新手引导－阶段3")]
	public string teach3_dialogue2;

	[LocalizeText("新手引导－阶段3")]
	public string teach3_Subtitle1;

	[LocalizeText("新手引导－阶段3")]
	public string teach3_dialogue4;

	[LocalizeText("新手引导－阶段3")]
	public string teach3_dialogue5;

	[LocalizeText("新手引导－阶段3")]
	public string teach3_subtitle2;

	[LocalizeText("新手引导－阶段3")]
	public string teach3_dialogue6;

	[LocalizeText("新手引导－阶段3")]
	public string teach3_dialogue7;

	[LocalizeText("新手引导－阶段3")]
	public string teach3_dialogue8;

	[LocalizeText("新手引导－阶段3")]
	public string teach3_dialogue9;

	[LocalizeText("新手引导－阶段3")]
	public string teach3_subtitle3;

	[LocalizeText("新手引导－阶段4")]
	public string teach4_subtitle1;

	public CutsceneRef teach4_cutscene;

	public ItemDefinition enemyStrongAttackDef;

	public DialogueSequence successEnemyDialogue;

	[LocalizeText("新手引导－闭幕")]
	public string endDarkScreen1;

	[LocalizeText("新手引导－闭幕")]
	public string endDarkScreen2;

	[LocalizeText("新手引导－闭幕")]
	public string endDarkScreen3;

	[LocalizeText("新手引导－其他")]
	public string loc1_name;

	[LocalizeText("新手引导－其他")]
	public string loc1_description;

	[LocalizeText("新手引导－其他")]
	public string loc2_name;

	[LocalizeText("新手引导－其他")]
	public string loc2_description;

	[LocalizeText("新手引导－其他")]
	public string loc3_name;

	[LocalizeText("新手引导－其他")]
	public string loc3_description;

	[LocalizeText("新手引导－其他")]
	public string loc4_name;

	[LocalizeText("新手引导－其他")]
	public string loc4_description;

	[SerializeField]
	public EventReference peaceMusicRef;

	[SerializeField]
	public EventReference battleMusicRef;

	private GuideManager guideManager => applicationState.guideManager;

	private GuideUIPanel guideUIPanel => gameState.UIManager.guideUIPanel;

	public override void RunModule()
	{
		WrapperEnumerator.StartCoroutine(GuideCor, Bootstrap.instance);
	}

	private void SetDeckButtonVisible(bool v)
	{
		gameState.UIManager.battlePanel.deckButton.gameObject.SetActive(v);
		gameState.UIManager.battlePanel.speedButton.gameObject.SetActive(v);
		gameState.UIManager.dungeonMapPanel.deckButton.gameObject.SetActive(v);
	}

	private void SetMapButtonVisible(bool v)
	{
		gameState.UIManager.battlePanel.showMapButton.gameObject.SetActive(v);
	}

	private void SetMapPauseButtonVisible(bool v)
	{
		PauseButton componentInChildren = gameState.UIManager.dungeonMapPanel.GetComponentInChildren<PauseButton>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(v);
		}
	}

	private void SetCoinVisible(bool v)
	{
		gameState.UIManager.battlePanel.coinDisplay.gameObject.SetActive(v);
		gameState.UIManager.dungeonMapPanel.GetComponentInChildren<PlayerCoinDisplay>().gameObject.SetActive(v);
	}

	private void SetCrystalVisible(bool v)
	{
		gameState.UIManager.battlePanel.crystalDisplay.gameObject.SetActive(v);
		gameState.UIManager.dungeonMapPanel.GetComponentInChildren<PlayerCrystalDisplay>().gameObject.SetActive(v);
	}

	private void SetPauseLeaveButtonInteractable(bool v)
	{
		gameState.UIManager.settingPanel.GetModule<GameModule>().giveUpButton.interactable = v;
	}

	private void SetEndTurnButtonVisible(bool v)
	{
		applicationState.guideManager.showEndTurnButton = v;
		gameState.UIManager.battlePanel.SetDirty();
	}

	private void SetDrawPileVisible(bool v)
	{
		gameState.UIManager.battlePanel.drawPileButton.gameObject.SetActive(v);
	}

	private void SetWastePileVisible(bool v)
	{
		gameState.UIManager.battlePanel.wastePileButton.gameObject.SetActive(v);
	}

	private void SetEnemyUIVisible(bool v)
	{
		gameState.UIManager.battlePanel.enemyBftHealthDisplay.gameObject.SetActive(v);
		gameState.UIManager.battlePanel.enemyBftUIDisplay.GetComponent<EnemyBftUITextDisplay>().nameText.gameObject.SetActive(v);
	}

	private void SetBonusLeaveButtonInteractable(bool v)
	{
		if (v)
		{
			gameState.UIManager.pickSelectPanel.leaveButton.Show();
			return;
		}
		gameState.UIManager.pickSelectPanel.leaveButton.HideNextLeaveButton();
		gameState.UIManager.pickSelectPanel.leaveButton.Hide();
	}

	private IEnumerator<CustomYieldInstruction> WaitUntilCloseHint(GuideHintPanelSet presetPanel, float afterWaitTime = 0f)
	{
		bool closeHint = false;
		EnqueueAction(new ShowGuideHintSetAction(presetPanel, delegate
		{
			closeHint = true;
		}));
		EnqueueAction(new WaitUntilAction(() => closeHint));
		if (afterWaitTime != 0f)
		{
			EnqueueAction(new WaitAction(afterWaitTime));
		}
		yield return new WaitUntil(() => closeHint);
	}

	private IEnumerator<CustomYieldInstruction> WaitShotDarkScreenText(string text)
	{
		gameState.UIManager.darkScreenPanel.ShotText(text);
		yield return new WaitUntil(() => !gameState.UIManager.darkScreenPanel.Tweening);
	}

	private IEnumerator<CustomYieldInstruction> WaitForFinishDialogue(DialoguePlayer player, string str)
	{
		bool finished = false;
		DialogueSequence sequence = new DialogueSequence
		{
			dialogueSequence = new List<DialogueText>
			{
				new DialogueText
				{
					text = str
				}
			}
		};
		gameState.UIManager.dialoguePanel.TriggerDialogue(sequence, player, delegate
		{
			finished = true;
		});
		yield return new WaitUntil(() => finished);
	}

	private IEnumerator<CustomYieldInstruction> WaitForFinishDialogue(DialoguePlayer player, List<DialogueText> seqText)
	{
		bool finished = false;
		DialogueSequence sequence = new DialogueSequence
		{
			dialogueSequence = seqText
		};
		gameState.UIManager.dialoguePanel.TriggerDialogue(sequence, player, delegate
		{
			finished = true;
		});
		yield return new WaitUntil(() => finished);
	}

	public IEnumerator<CustomYieldInstruction> WaitForGuideMask(GuideMask guideMask)
	{
		guideMask.TweenToTarget();
		yield return new WaitUntil(() => !guideMask.gameObject.activeInHierarchy);
	}

	private IEnumerator<CustomYieldInstruction> WaitUntilTriggerEvent(GameEvent.Type type, Func<GameEvent.Type, GameEvent, bool> callBack = null)
	{
		bool triggered = false;
		applicationState.eventManager.gameEvent.AddListener(type, CallbackInter);
		yield return new WaitUntil(() => triggered);
		void CallbackInter(GameEvent.Type geType, GameEvent ge)
		{
			if (callBack == null || callBack(type, ge))
			{
				applicationState.eventManager.gameEvent.RemoveListener(geType, CallbackInter);
				triggered = true;
			}
		}
	}

	private void PlayerDeathAutoReviveInIntro(GameEvent.Type type, GameEvent eve)
	{
		if (type == GameEvent.Type.BftDeathCheckResolve && eve.triggerType == GameEvent.TriggerType.Real && eve.TargetBft.isPlayer)
		{
			eve.resolveInfo.reject = true;
			EnqueueAction(new HealAction(null, gameState.playerState.bft, gameState.playerState.bft.maxHealth));
		}
	}

	private IEnumerator GuideCor()
	{
		gameState = applicationState.runningGameState;
		ThinkingAnalyticsExtends.TrackFortressProgressNode(gameState.applicationState, "开始新手引导");
		yield return SetupCor();
		yield return IntroEnterCor();
		yield return TeachCor_1();
		yield return TeachCor_2();
		yield return TeachCor_3();
		yield return TeachCor_4();
	}

	private IEnumerator SetupCor()
	{
		EnqueueAction(new LoadLevelAction(ScriptableObjectSingleton<GuideData>.instance.introLevel));
		yield return new WaitUntil(() => gameState.actionManager.isEmpty);
		AssetCatalog catalog = gameState.applicationState.assetCatalog;
		ScriptableObjectSingleton<GuideData>.instance.introEnemy.StartLoad(catalog);
		ScriptableObjectSingleton<GuideData>.instance.finalEnemy.StartLoad(catalog);
		List<RoomRef> guideRooms = ScriptableObjectSingleton<GuideData>.instance.introLevel.GetAllValidLevelRoomRef(catalog);
		guideRooms.ForEach(delegate(RoomRef r)
		{
			r.StartLoad(catalog);
		});
		yield return new WaitUntil(() => ScriptableObjectSingleton<GuideData>.instance.introEnemy.IsLoaded(catalog) && guideRooms.TrueForAll((RoomRef r) => r.IsLoaded(catalog)) && ScriptableObjectSingleton<GuideData>.instance.finalEnemy.IsLoaded(catalog));
		DungeonMap dungeonMap = GenerateIntroDungeonMap();
		gameState.dungeonMapManager.SetDungeonMap(dungeonMap);
		ItemContainer deckContainer = gameState.playerState.DeckContainer;
		foreach (Item allItem in deckContainer.GetAllItems())
		{
			EnqueueAction(new RemoveItemAction(allItem, null));
		}
		Item item = new Item(ScriptableObjectSingleton<GuideData>.instance.fistItemDef);
		Item item2 = new Item(ScriptableObjectSingleton<GuideData>.instance.fistItemDef);
		Item item3 = new Item(ScriptableObjectSingleton<GuideData>.instance.fistItemDef);
		Item item4 = new Item(ScriptableObjectSingleton<GuideData>.instance.fistItemDef);
		Item item5 = new Item(ScriptableObjectSingleton<GuideData>.instance.fistItemDef);
		Item item6 = new Item(ScriptableObjectSingleton<GuideData>.instance.swordItemDef);
		Item item7 = new Item(ScriptableObjectSingleton<GuideData>.instance.swordItemDef);
		foreach (Item item8 in new List<Item> { item, item2, item3, item4, item5, item6, item7 })
		{
			EnqueueAction(new SpawnItemAction(item8, deckContainer));
		}
		yield return new WaitUntil(() => gameState.actionManager.isEmpty);
		SetDeckButtonVisible(v: false);
		SetMapButtonVisible(v: false);
		SetMapPauseButtonVisible(v: false);
		SetCoinVisible(v: false);
		SetCrystalVisible(v: false);
		SetPauseLeaveButtonInteractable(v: false);
		SetEndTurnButtonVisible(v: false);
		SetDrawPileVisible(v: false);
		SetWastePileVisible(v: false);
		SetEnemyUIVisible(v: false);
		guideManager.blockEnemyDrawItems = true;
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BftDeathCheckResolve, PlayerDeathAutoReviveInIntro);
		EnqueueAction(new SetBftMaxHealthAction(initPlayerMaxHealth, null, gameState.playerState.bft));
		EnqueueAction(new SetBftHealthAction(initPlayerHealth, null, gameState.playerState.bft));
	}

	private IEnumerator IntroEnterCor()
	{
		gameState.UIManager.darkScreenPanel.Show();
		yield return new WaitUntil(() => !gameState.UIManager.darkScreenPanel.Tweening);
		BootstrapExtra.instance.loadingPanel.NotifyProgress(1f);
		yield return new WaitUntil(() => !BootstrapExtra.instance.loadingPanel.showing);
		EnqueueAction(new EnterLocationImplAction(gameState.dungeonMapManager.currentDungeonMap.locations[0]));
		WaitUntilTriggerEvent(GameEvent.Type.TurnDrawResolve, BlockTurnDrawResolve).MoveNext();
		EnqueueAction(new ActionAction(delegate
		{
			gameState.playerState.TableContainer.SetSlotCount(3);
		}));
		EnqueueAction(new ActionAction(delegate
		{
			gameState.playerState.bft.turnDrawItemCount = 4;
		}));
		PlayCutsceneAction acion = new PlayCutsceneAction(introCutsceneRef, gameState.applicationState);
		EnqueueAction(acion);
		yield return new WaitUntil(() => gameState.actionManager.isEmpty);
		Bootstrap.instance.audioManager.StopAllSustained();
		gameState.actionManager.AcquireLock(ActionLock.Type.Default);
		gameState.UIManager.inputUserNamePanel.Setup();
		yield return new WaitUntil(() => !gameState.UIManager.inputUserNamePanel.gameObject.activeInHierarchy);
		gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
		ThinkingAnalyticsExtends.TrackFortressProgressNode(gameState.applicationState, "新手引导完成名字输入");
		Bootstrap.instance.audioManager.PlaySustained(peaceMusicRef, "BgmTag");
		EnqueueAction(new EnterRoomTransitAnimAction());
		yield return new WaitUntil(() => gameState.battleState.turnPhase == TurnPhase.PlayerControlPhase);
		yield return new WaitUntil(() => gameState.actionManager.isEmpty);
		gameState.UIManager.darkScreenPanel.Hide();
		yield return new WaitUntil(() => !gameState.UIManager.darkScreenPanel.Tweening);
	}

	private Item GetDrawPileItemOfDef(ItemDefinition def, int index)
	{
		return gameState.playerState.DrawPileContainer.GetAllItems().FindAll((Item it) => it.GetDef() == def)[index];
	}

	private bool BlockTurnDrawResolve(GameEvent.Type type, GameEvent eve)
	{
		if (eve.triggerType == GameEvent.TriggerType.Real && eve.TargetOwner.isPlayer)
		{
			eve.resolveInfo.reject = true;
			return true;
		}
		return false;
	}

	private IEnumerator TeachCor_1()
	{
		EnqueueAction(new SetBftMaxHealthAction(mentorEnemyHealthBattl1, null, gameState.currentEncounter.enemyAI.bft));
		EnqueueAction(new SetBftHealthAction(mentorEnemyHealthBattl1, null, gameState.currentEncounter.enemyAI.bft));
		gameState.currentEncounter.autoEndAfterFinished = false;
		DialoguePlayer mentorPlayer = gameState.currentEncounter.room.roomDialoguePlayer;
		yield return WaitForFinishDialogue(mentorPlayer, teach1_dialogue1);
		yield return WaitForFinishDialogue(mentorPlayer, teach1_dialogue2);
		yield return WaitForFinishDialogue(mentorPlayer, teach1_dialogue3);
		yield return WaitForFinishDialogue(mentorPlayer, teach1_dialogue4);
		SetDrawPileVisible(v: true);
		SetWastePileVisible(v: true);
		SetEnemyUIVisible(v: true);
		guideManager.blockPlayerInput = true;
		Item drawPileItemOfDef = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.fistItemDef, 0);
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef, null));
		yield return new WaitUntil(() => gameState.actionManager.isEmpty);
		yield return WaitUntilCloseHint(guideUIPanel.introHint_teach1_1);
		WaitForFinishDialogue(mentorPlayer, teach1_dialogue5).MoveNext();
		guideManager.blockPlayerInput = false;
		gameState.UIManager.subtitlePanel.ShowSubtitle(teach1_subtitle1);
		PlayerItemDisplay playerItemDisplay = gameState.UIManager.playerItemArea.itemDisplays.Find((PlayerItemDisplay it) => it.item.GetDef() == ScriptableObjectSingleton<GuideData>.instance.fistItemDef);
		guideUIPanel.guideHoverHint_castItem.ShowOnTarget(playerItemDisplay.rectTransform, follow: false);
		yield return WaitUntilTriggerEvent(GameEvent.Type.AfterSpellCast);
		guideUIPanel.guideHoverHint_castItem.Hide();
		gameState.UIManager.subtitlePanel.HideSubtitle();
		yield return new WaitForSeconds(0.8f);
		yield return WaitUntilCloseHint(guideUIPanel.introHint_teach1_2);
		yield return WaitForFinishDialogue(mentorPlayer, teach1_dialogue6);
		SetEndTurnButtonVisible(v: true);
		gameState.UIManager.subtitlePanel.ShowSubtitle(teach1_subtitle2);
		yield return WaitForGuideMask(guideUIPanel.guideMask_endTurnButton);
		SetEndTurnButtonVisible(v: false);
		gameState.UIManager.subtitlePanel.HideSubtitle();
		yield return WaitUntilTriggerEvent(GameEvent.Type.TurnDrawResolve, BlockTurnDrawResolve);
		yield return WaitUntilCloseHint(guideUIPanel.introHint_teach1_3, 0.3f);
		Item drawPileItemOfDef2 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.fistItemDef, 0);
		Item drawPileItemOfDef3 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.fistItemDef, 1);
		Item drawPileItemOfDef4 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.fistItemDef, 2);
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef2, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef3, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef4, null));
		yield return new WaitUntil(() => gameState.actionManager.isEmpty);
		gameState.UIManager.subtitlePanel.ShowSubtitle(teach1_subtitle3);
		SetEndTurnButtonVisible(v: true);
		yield return WaitUntilTriggerEvent(GameEvent.Type.BeforePlayerTurnEnd);
		SetEndTurnButtonVisible(v: false);
		yield return WaitUntilCloseHint(guideUIPanel.introHint_teach1_4, 0.3f);
		ThinkingAnalyticsExtends.TrackFortressProgressNode(gameState.applicationState, "完成引导教学1");
	}

	private IEnumerator TeachCor_2()
	{
		DialoguePlayer mentorPlayer = gameState.currentEncounter.room.roomDialoguePlayer;
		yield return WaitUntilTriggerEvent(GameEvent.Type.TurnDrawResolve, BlockTurnDrawResolve);
		guideManager.blockPlayerInput = true;
		Item drawPileItemOfDef = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.swordItemDef, 0);
		Item drawPileItemOfDef2 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.fistItemDef, 0);
		Item drawPileItemOfDef3 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.swordItemDef, 1);
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef2, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef3, null));
		yield return new WaitUntil(() => gameState.actionManager.isEmpty);
		yield return WaitUntilCloseHint(guideUIPanel.introHint_teach2_1);
		WaitForFinishDialogue(mentorPlayer, teach2_dialogue1).MoveNext();
		gameState.UIManager.subtitlePanel.ShowSubtitle(teach2_subtitle1);
		guideManager.blockPlayerInput = false;
		yield return WaitUntilTriggerEvent(GameEvent.Type.AfterSpellCast, _OnCastSword);
		gameState.UIManager.subtitlePanel.HideSubtitle();
		guideManager.blockPlayerInput = true;
		WaitForFinishDialogue(mentorPlayer, teach2_dialogue2).MoveNext();
		SetEndTurnButtonVisible(v: true);
		guideManager.blockPlayerInput = false;
		gameState.UIManager.subtitlePanel.ShowSubtitle(teach2_subtitle2);
		bool finishBattleEndDialogue = false;
		yield return WaitUntilTriggerEvent(GameEvent.Type.BeforeBattlEncounterEnd, _OnBattleEncounterEnd);
		SetEndTurnButtonVisible(v: false);
		gameState.UIManager.subtitlePanel.HideSubtitle();
		yield return WaitForFinishDialogue(mentorPlayer, teach2_dialogue3);
		yield return WaitForFinishDialogue(mentorPlayer, teach2_dialogue4);
		finishBattleEndDialogue = true;
		PathLocation nextGuideLocation = gameState.dungeonMapManager.currentLocation.connections.First();
		SetBonusLeaveButtonInteractable(v: false);
		gameState.UIManager.subtitlePanel.ShowSubtitle(teach2_subtitle3);
		yield return WaitUntilTriggerEvent(GameEvent.Type.AddItem);
		gameState.UIManager.subtitlePanel.HideSubtitle();
		yield return new WaitUntil(() => !gameState.UIManager.multiInfoDisplayPanel.gameObject.activeInHierarchy);
		EnqueueAction(new SpawnItemAction(new Item(ScriptableObjectSingleton<GuideData>.instance.armorItemDef), gameState.playerState.DeckContainer));
		SetBonusLeaveButtonInteractable(v: true);
		yield return WaitForGuideMask(guideUIPanel.guideMask_leaveButton);
		EnqueueAction(new SwitchToLocationFromRoomAction(nextGuideLocation));
		ThinkingAnalyticsExtends.TrackFortressProgressNode(gameState.applicationState, "完成引导教学2");
		yield return WaitUntilTriggerEvent(GameEvent.Type.EnterLocation);
		gameState.UIManager.subtitlePanel.HideSubtitle();
		bool _OnBattleEncounterEnd(GameEvent.Type type, GameEvent eve)
		{
			EnqueueAction(new WaitUntilAction(() => finishBattleEndDialogue));
			return true;
		}
		static bool _OnCastSword(GameEvent.Type type, GameEvent eve)
		{
			if (eve.type == GameEvent.Type.AfterSpellCast && eve.SourceItem.def == ScriptableObjectSingleton<GuideData>.instance.swordItemDef)
			{
				return true;
			}
			return false;
		}
	}

	private IEnumerator TeachCor_3()
	{
		EnqueueAction(new SetBftMaxHealthAction(mentorEnemyHealthBattl2, null, gameState.currentEncounter.enemyAI.bft));
		EnqueueAction(new SetBftHealthAction(mentorEnemyHealthBattl2, null, gameState.currentEncounter.enemyAI.bft));
		DialoguePlayer mentorPlayer = gameState.currentEncounter.room.roomDialoguePlayer;
		guideManager.blockEnemyDrawItems = false;
		yield return WaitUntilTriggerEvent(GameEvent.Type.TurnDrawResolve, _OnTurnDrawResolve);
		guideManager.blockPlayerInput = true;
		yield return WaitUntilCloseHint(guideUIPanel.introHint_teach3_1);
		yield return WaitForFinishDialogue(mentorPlayer, teach3_dialogue1);
		Item drawPileItemOfDef = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.fistItemDef, 0);
		Item drawPileItemOfDef2 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.fistItemDef, 1);
		Item drawPileItemOfDef3 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.armorItemDef, 0);
		Item drawPileItemOfDef4 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.swordItemDef, 0);
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef2, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef4, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef3, null));
		yield return new WaitUntil(() => gameState.actionManager.isEmpty);
		SetEndTurnButtonVisible(v: true);
		yield return WaitUntilCloseHint(guideUIPanel.introHint_teach3_2);
		yield return WaitForFinishDialogue(mentorPlayer, teach3_dialogue2);
		gameState.UIManager.subtitlePanel.ShowSubtitle(teach3_Subtitle1);
		guideManager.blockPlayerInput = false;
		yield return WaitUntilTriggerEvent(GameEvent.Type.PlayerTurnEnd);
		gameState.UIManager.subtitlePanel.HideSubtitle();
		yield return WaitUntilTriggerEvent(GameEvent.Type.BeforeBreakItem);
		yield return WaitUntilCloseHint(guideUIPanel.introHint_teach3_3);
		List<DialogueText> list = new List<DialogueText>();
		list.Add(new DialogueText
		{
			text = teach3_dialogue4
		});
		list.Add(new DialogueText
		{
			text = teach3_dialogue5
		});
		WaitForFinishDialogue(mentorPlayer, list).MoveNext();
		bool finishBattleEndDialogue = false;
		gameState.UIManager.subtitlePanel.ShowSubtitle(teach3_subtitle2);
		yield return WaitUntilTriggerEvent(GameEvent.Type.TurnDrawResolve, _OnTurnDrawResolve);
		Item drawPileItemOfDef5 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.armorItemDef, 0);
		Item drawPileItemOfDef6 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.fistItemDef, 0);
		Item drawPileItemOfDef7 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.fistItemDef, 1);
		Item drawPileItemOfDef8 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.swordItemDef, 0);
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef5, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef6, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef7, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef8, null));
		yield return WaitUntilTriggerEvent(GameEvent.Type.BeforeBattlEncounterEnd, _OnBattleEncounterEnd);
		gameState.UIManager.subtitlePanel.HideSubtitle();
		SetEndTurnButtonVisible(v: false);
		yield return WaitForFinishDialogue(mentorPlayer, teach3_dialogue6);
		yield return WaitForFinishDialogue(mentorPlayer, teach3_dialogue7);
		yield return WaitForFinishDialogue(mentorPlayer, teach3_dialogue8);
		yield return WaitForFinishDialogue(mentorPlayer, teach3_dialogue9);
		finishBattleEndDialogue = true;
		SetBonusLeaveButtonInteractable(v: false);
		gameState.UIManager.subtitlePanel.ShowSubtitle(teach3_subtitle3);
		yield return WaitUntilTriggerEvent(GameEvent.Type.AddItem);
		gameState.UIManager.subtitlePanel.HideSubtitle();
		EnqueueAction(new SpawnItemAction(new Item(ScriptableObjectSingleton<GuideData>.instance.armorItemDef), gameState.playerState.DeckContainer));
		EnqueueAction(new SpawnItemAction(new Item(ScriptableObjectSingleton<GuideData>.instance.energyItemDef), gameState.playerState.DeckContainer));
		yield return new WaitUntil(() => !gameState.UIManager.multiInfoDisplayPanel.gameObject.activeInHierarchy);
		SetBonusLeaveButtonInteractable(v: true);
		yield return WaitUntilTriggerEvent(GameEvent.Type.ExitLocation);
		yield return WaitUntilCloseHint(guideUIPanel.introHint_teach3_4);
		ThinkingAnalyticsExtends.TrackFortressProgressNode(gameState.applicationState, "完成引导教学3");
		bool _OnBattleEncounterEnd(GameEvent.Type type, GameEvent eve)
		{
			EnqueueAction(new WaitUntilAction(() => finishBattleEndDialogue));
			return true;
		}
		static bool _OnTurnDrawResolve(GameEvent.Type type, GameEvent eve)
		{
			if (eve.triggerType == GameEvent.TriggerType.Real && eve.TargetOwner.isPlayer)
			{
				eve.resolveInfo.reject = true;
				return true;
			}
			return false;
		}
	}

	private IEnumerator TeachCor_4()
	{
		PathLocation location = gameState.dungeonMapManager.currentDungeonMap.locations.Find((PathLocation loc) => loc.type == PathLocation.LocationType.HealFire);
		PathLocationDisplay locationDisplay = gameState.UIManager.dungeonMapPanel.dungeonMapDisplay.GetLocationDisplay(location);
		guideUIPanel.guideHoverHint_enterLocation.ShowOnTarget(locationDisplay.rectTransform, follow: true);
		gameState.UIManager.subtitlePanel.ShowSubtitle(teach4_subtitle1);
		yield return WaitUntilTriggerEvent(GameEvent.Type.EnterLocation);
		SetEndTurnButtonVisible(v: true);
		SetDeckButtonVisible(v: true);
		gameState.UIManager.subtitlePanel.HideSubtitle();
		guideUIPanel.guideHoverHint_enterLocation.Hide();
		yield return WaitUntilTriggerEvent(GameEvent.Type.ExitLocation);
		SetMapPauseButtonVisible(v: true);
		gameState.UIManager.subtitlePanel.ShowSubtitle(teach4_subtitle1);
		yield return WaitUntilTriggerEvent(GameEvent.Type.EnterLocation);
		gameState.UIManager.subtitlePanel.HideSubtitle();
		applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BftDeathCheckResolve, PlayerDeathAutoReviveInIntro);
		yield return WaitUntilTriggerEvent(GameEvent.Type.TurnDrawResolve, _OnTurnDrawResolve);
		gameState.playerState.DrawPileContainer.GetAllItems();
		Item drawPileItemOfDef = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.armorItemDef, 0);
		Item drawPileItemOfDef2 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.swordItemDef, 0);
		Item drawPileItemOfDef3 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.fistItemDef, 0);
		Item drawPileItemOfDef4 = GetDrawPileItemOfDef(ScriptableObjectSingleton<GuideData>.instance.energyItemDef, 0);
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef3, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef2, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef4, null));
		EnqueueAction(new DrawSpecificItemFromPileAction(gameState.playerState.bft, drawPileItemOfDef, null));
		gameState.currentEncounter.afterEncounterEnd = null;
		Bootstrap.instance.audioManager.PlaySustained(battleMusicRef, "BgmTag");
		gameState.applicationState.guideManager.skipDeathPendingFinalPanel = true;
		bool cutSceneFinished = false;
		yield return WaitUntilTriggerEvent(GameEvent.Type.BeforeBattleEnd, _OnBattleEnd);
		gameState.applicationState.guideManager.skipDeathPendingFinalPanel = false;
		yield return new WaitUntil(() => cutSceneFinished);
		ThinkingAnalyticsExtends.TrackFortressProgressNode(gameState.applicationState, "完成新手引导");
		applicationState.savedGameState = null;
		gameState.applicationState.guideManager.introFinished = true;
		Finish();
		applicationState.savingManager.SaveProfile();
		yield return WaitShotDarkScreenText(endDarkScreen1);
		yield return WaitShotDarkScreenText(endDarkScreen2);
		yield return WaitShotDarkScreenText(endDarkScreen3);
		if (GlobalConfigManager.instance.enableNetwork)
		{
			GeneralGameAsyncOperationWithoutRequestOrResult generalGameAsyncOperationWithoutRequestOrResult = new GeneralGameAsyncOperationWithoutRequestOrResult(applicationState.ChillyRoomSDK.Guide.FinishIntroAsync);
			generalGameAsyncOperationWithoutRequestOrResult.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_generating_guide_progress);
			yield return generalGameAsyncOperationWithoutRequestOrResult.WaitForEnd();
		}
		yield return new WaitUntil(() => gameState.actionManager.isEmpty);
		applicationState.newGameConfigManager.currentNewGameConfig = NewGameConfigData.DefaultStartGameConfig(applicationState);
		applicationState.newGameConfigManager.currentNewGameConfig.contract.difficultyLevel = DifficultyLevel.Normal;
		applicationState.newGameConfigManager.StartGame(applicationState.newGameConfigManager.currentNewGameConfig);
		bool _OnBattleEnd(GameEvent.Type type, GameEvent eve)
		{
			if (type == GameEvent.Type.BeforeBattleEnd)
			{
				applicationState.eventManager.IntroFinalBossBattleEnd.TriggerEvent(eve.battleFinishType);
				if (eve.battleFinishType == FinishBattleEncounterAction.FinishType.Win)
				{
					EnqueueAction(new TriggerDialogueAction(successEnemyDialogue, gameState.battleState.enemy.GetOrAddDialogPlayer(), halt: true));
					Item sourceItem = gameState.battleState.enemy.bft.GetAllRealItems().Find((Item t) => t.GetDef() == enemyStrongAttackDef);
					EnqueueAction(new PlayPerformanceAction(sourceItem, gameState.battleState.enemy.bft, EnemyAnimationType.attack1, waitForCompletion: true));
				}
				EnqueueAction(new ActionAction(delegate
				{
					gameState.UIManager.darkScreenPanel.Show();
					gameState.UIManager.darkScreenPanel.tween.timeScale = 10f;
				}));
				EnqueueAction(new PlayCutsceneAction(teach4_cutscene.GetAssetPreData(applicationState.assetCatalog)));
				EnqueueAction(new ActionAction(delegate
				{
					cutSceneFinished = true;
				}));
				return true;
			}
			return false;
		}
		static bool _OnTurnDrawResolve(GameEvent.Type type, GameEvent eve)
		{
			if (eve.triggerType == GameEvent.TriggerType.Real && eve.TargetOwner.isPlayer)
			{
				eve.resolveInfo.reject = true;
				return true;
			}
			return false;
		}
	}

	public DungeonMap GenerateIntroDungeonMap()
	{
		AssetCatalog assetCatalog = gameState.applicationState.assetCatalog;
		guideDraftMapRef.LoadSync(assetCatalog);
		DraftMapInfoData assetPreData = guideDraftMapRef.GetAssetPreData(assetCatalog);
		DungeonMap dungeonMap = new DraftDungeonMapGenerator(new DraftDungeonMapGenerator.DraftMapGenerationConfig
		{
			gameState = gameState,
			draftMap = assetPreData,
			stage = 1,
			level = ScriptableObjectSingleton<GuideData>.instance.introLevel
		}).GenerateMap();
		EnemyInfoData assetPreData2 = ScriptableObjectSingleton<GuideData>.instance.introEnemy.GetAssetPreData(gameState.applicationState.assetCatalog);
		EnemyInfoData assetPreData3 = ScriptableObjectSingleton<GuideData>.instance.activeIntroEnemy.GetAssetPreData(gameState.applicationState.assetCatalog);
		EnemyInfoData assetPreData4 = ScriptableObjectSingleton<GuideData>.instance.finalEnemy.GetAssetPreData(gameState.applicationState.assetCatalog);
		PathLocation pathLocation = dungeonMap.locations[0];
		pathLocation.overrideName = loc2_name;
		pathLocation.overrideDescription = loc2_description;
		pathLocation.type = PathLocation.LocationType.NormalFight;
		pathLocation.attachEncounterSetupInfo = new EncounterSetupInfo
		{
			fixedEnemyInfoData = assetPreData2,
			fixedEnemySetup = assetPreData2.setups[0],
			level = ScriptableObjectSingleton<GuideData>.instance.introLevel,
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, dungeonMap, pathLocation),
			roomBonus = new BattleBonus(new List<BattleBonusObject>
			{
				new SingleItemBonus(new Item(ScriptableObjectSingleton<GuideData>.instance.armorItemDef))
			}),
			enemyLevel = 1f
		};
		PathLocation pathLocation2 = dungeonMap.locations[1];
		pathLocation2.overrideName = loc3_name;
		pathLocation2.overrideDescription = loc3_description;
		pathLocation2.type = PathLocation.LocationType.NormalFight;
		pathLocation2.attachEncounterSetupInfo = new EncounterSetupInfo
		{
			fixedEnemyInfoData = assetPreData3,
			fixedEnemySetup = assetPreData3.setups[0],
			level = ScriptableObjectSingleton<GuideData>.instance.introLevel,
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, dungeonMap, pathLocation2),
			roomBonus = new BattleBonus(new List<BattleBonusObject>
			{
				new SingleItemBonus(new Item(ScriptableObjectSingleton<GuideData>.instance.energyItemDef))
			}),
			enemyLevel = 1f
		};
		PathLocation pathLocation3 = dungeonMap.locations[2];
		pathLocation3.type = PathLocation.LocationType.HealFire;
		pathLocation3.attachEncounterSetupInfo = new EncounterSetupInfo
		{
			level = ScriptableObjectSingleton<GuideData>.instance.introLevel,
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.healSpringEventTemplate,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, dungeonMap, pathLocation3),
			enemyLevel = 1f
		};
		PathLocation pathLocation4 = dungeonMap.locations[3];
		pathLocation4.overrideName = loc4_name;
		pathLocation4.overrideDescription = loc4_description;
		pathLocation4.type = PathLocation.LocationType.Boss;
		pathLocation4.attachEncounterSetupInfo = new EncounterSetupInfo
		{
			fixedEnemyInfoData = assetPreData4,
			fixedEnemySetup = ScriptableObjectSingleton<GuideData>.instance.finalEnemySetup,
			level = ScriptableObjectSingleton<GuideData>.instance.introLevel,
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, dungeonMap, pathLocation4),
			enemyLevel = 1f
		};
		dungeonMap.initLocation = pathLocation;
		return dungeonMap;
	}
}
