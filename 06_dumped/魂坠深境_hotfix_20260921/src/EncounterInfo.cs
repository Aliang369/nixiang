using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using Spine;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EncounterInfo
{
	private readonly struct EnemySpawnInfo(EnemyInfoData enemyInfoData, CharacterAIComponent.EnemySpawnSetup enemySpawnSetup, PVPDefender pvpDefender)
	{
		public readonly EnemyInfoData enemyInfoData = enemyInfoData;

		public readonly CharacterAIComponent.EnemySpawnSetup enemySpawnSetup = enemySpawnSetup;

		public readonly PVPDefender pvpDefender = pvpDefender;
	}

	public CharacterAIComponent.EnemySpawnSetup enemySetup;

	public CharacterAI enemyAI;

	public DungeonRoom room;

	public WorldEvent triggerEvent;

	public BattleBonus roomBonus;

	public float enemyLevel;

	public Level level;

	public bool autoEndAfterFinished = true;

	public EncounterSetupInfo setupInfo;

	public GameStateInfo gameState;

	public EnvironmentInfluence environmentInfluence;

	public List<RoomObjectDisplay> roomObjectDisplays = new List<RoomObjectDisplay>();

	public RoomObjectDisplay majorRoomObjectDisplay;

	public Action<bool> afterEncounterEnd;

	public IEnumerator SetupCor(EncounterSetupInfo setupInfo, GameStateInfo gameState)
	{
		this.setupInfo = setupInfo;
		this.gameState = gameState;
		enemyLevel = setupInfo.enemyLevel;
		level = setupInfo.level;
		PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
		if (currentLocation != null && currentLocation.variantType == PathLocation.LocationVariantType.EliteFightHighLevelEnemy)
		{
			enemyLevel += 2f;
		}
		PathLocation currentLocation2 = gameState.dungeonMapManager.currentLocation;
		if (currentLocation2 != null && currentLocation2.variantType == PathLocation.LocationVariantType.EliteVariant)
		{
			enemyLevel++;
		}
		if (setupInfo.roomInfoData == null)
		{
			Debug.LogError($"setupInfo.roomInfoData == null, {setupInfo.fixedEventTemplate}, using default room");
			setupInfo.roomInfoData = gameState.applicationState.assetCatalog.roomInfoDataCatalog.assets[0];
		}
		SetTriggerEvent();
		RoomInfoData finalRoom = GetFinalRoomInfoData();
		AsyncOperationHandle<GameObject> asyncOperationHandle = finalRoom.LoadAsync();
		if (asyncOperationHandle.IsValid() && !asyncOperationHandle.IsDone)
		{
			yield return asyncOperationHandle;
		}
		DungeonRoom finalRoomPrefab = finalRoom.loadedAsset;
		if (finalRoomPrefab == null)
		{
			TraceLog.Error("房间资源加载失败: " + finalRoom.FullId);
			yield break;
		}
		yield return null;
		SetupRoom(finalRoomPrefab);
		EnemySpawnInfo enemySpawnInfo = ResolveEnemySpawnInfo();
		AsyncOperationHandle<GameObject>? asyncOperationHandle2 = enemySpawnInfo.enemyInfoData?.LoadAsync();
		if (asyncOperationHandle2.HasValue && asyncOperationHandle2.Value.IsValid() && !asyncOperationHandle2.Value.IsDone)
		{
			yield return asyncOperationHandle2.Value;
		}
		if (enemySpawnInfo.enemyInfoData != null && enemySpawnInfo.enemyInfoData.loadedAsset == null)
		{
			TraceLog.Error("敌人资源加载失败: " + enemySpawnInfo.enemyInfoData.FullId);
			yield break;
		}
		if (enemySpawnInfo.enemyInfoData != null)
		{
			yield return null;
		}
		SpawnEnemy(enemySpawnInfo);
		SetCameraState();
		SpawnTriggerEvent();
		SetupEncounterBonus();
		SetupEncounterEndAction();
	}

	private void SetCameraState()
	{
		gameState.StartCoroutine(ShowCamera);
		IEnumerator ShowCamera()
		{
			VirtualCamera.State cameraState = VirtualCamera.State.None;
			if (setupInfo.fixedEventTemplate != null)
			{
				VirtualCamera.State state = ((setupInfo.fixedEventTemplate.displayType == WorldEventDisplayType.Battle) ? VirtualCamera.State.EnterBattle : ((gameState.dungeonMapManager.currentGlobalProgress == 0) ? VirtualCamera.State.StartNewGame : VirtualCamera.State.EnterEvent));
				cameraState = state;
			}
			yield return EnumeratorExtends.WaitFrame(1f);
			yield return new WaitUntil(() => !BootstrapExtra.instance.loadingPanel.showing);
			gameState.UIManager.gameVirtualCamera.SetState(cameraState);
		}
	}

	public void SpawnRoomObjectDisplay(RoomObjectDisplay prefab, bool major)
	{
		if (prefab != null)
		{
			RoomObjectDisplay roomObjectDisplay = UnityEngine.Object.Instantiate(prefab, gameState.UIManager.scene3DRoot);
			roomObjectDisplay.Setup(gameState);
			roomObjectDisplay.transform.position = room.enemySpawnPivot.transform.position;
			roomObjectDisplays.Add(roomObjectDisplay);
			if (major)
			{
				majorRoomObjectDisplay = roomObjectDisplay;
			}
			if (roomObjectDisplay.dialoguePivot != null && !roomObjectDisplay.enterDialogues.IsNullOrEmpty())
			{
				DialogueSequence sequence = roomObjectDisplay.enterDialogues.SelectRandom(gameState.randomManager.NextDouble);
				gameState.UIManager.dialoguePanel.TriggerDialogue(sequence, roomObjectDisplay.dialoguePivot, null);
			}
			if ((UnityEngine.Object)(object)roomObjectDisplay.spine != null && !roomObjectDisplay.enterAnimName.IsNullOrEmpty())
			{
				roomObjectDisplay.spine.PlayAnimation(roomObjectDisplay.enterAnimName, loop: false);
			}
		}
	}

	private void CleanEnvironmentInfluence()
	{
		if (environmentInfluence != null)
		{
			EnqueueAction(new SetEnvironmentInfluenceAction(null, hint: false));
		}
	}

	public void Clear()
	{
		CleanEnvironmentInfluence();
		ClearEnemy();
		ClearTriggerEvent();
		ClearRoom();
	}

	public IEnumerator EndEncounterCor(bool finish)
	{
		List<RoomObjectDisplay> playingDialogue = new List<RoomObjectDisplay>();
		List<TrackEntry> playingSpineTrack = new List<TrackEntry>();
		foreach (RoomObjectDisplay roomObjectDisplay in roomObjectDisplays)
		{
			roomObjectDisplay.finishing = true;
			if (roomObjectDisplay.dialoguePivot != null && !roomObjectDisplay.exitDialogues.IsNullOrEmpty())
			{
				RoomObjectDisplay ro = roomObjectDisplay;
				DialogueSequence sequence = roomObjectDisplay.exitDialogues.SelectRandom(gameState.randomManager.NextDouble);
				playingDialogue.Add(roomObjectDisplay);
				gameState.UIManager.dialoguePanel.TriggerDialogue(sequence, roomObjectDisplay.dialoguePivot, delegate
				{
					playingDialogue.Remove(ro);
				});
			}
			if ((UnityEngine.Object)(object)roomObjectDisplay.spine != null && !roomObjectDisplay.exitAnimName.IsNullOrEmpty())
			{
				TrackEntry track = roomObjectDisplay.spine.PlayAnimation(roomObjectDisplay.exitAnimName, loop: false);
				track.Complete += delegate
				{
					playingSpineTrack.Remove(track);
				};
				playingSpineTrack.Add(track);
			}
		}
		yield return new WaitUntil(() => playingDialogue.Count == 0 && playingSpineTrack.Count == 0);
		gameState.currentEncounter.afterEncounterEnd?.Invoke(finish);
	}

	public void SetupRoom(DungeonRoom roomPrefab)
	{
		if (room != null)
		{
			UnityEngine.Object.Destroy(room);
		}
		room = roomPrefab.InstantiatePrefabIn(gameState.UIManager.scene3DRoot);
		room.ApplyRoomRenderSetting(gameState.applicationState);
	}

	private RoomInfoData GetFinalRoomInfoData()
	{
		RoomInfoData result = setupInfo.roomInfoData;
		if (triggerEvent.eventRoomRef != null && triggerEvent.eventRoomRef.IsValidRef(gameState.applicationState.assetCatalog))
		{
			result = triggerEvent.eventRoomRef.GetAssetPreData(gameState.applicationState.assetCatalog);
		}
		return result;
	}

	public void ClearRoom()
	{
		gameState.UIManager.gameVirtualCamera.SetState(VirtualCamera.State.None);
		foreach (RoomObjectDisplay roomObjectDisplay in roomObjectDisplays)
		{
			UnityEngine.Object.Destroy(roomObjectDisplay.gameObject);
		}
		roomObjectDisplays.Clear();
		majorRoomObjectDisplay = null;
		if (room != null)
		{
			UnityEngine.Object.Destroy(room.gameObject);
		}
		room = null;
	}

	private EnemySpawnInfo ResolveEnemySpawnInfo()
	{
		if (setupInfo.fixedEnemyInfoData != null)
		{
			if (setupInfo.fixedEnemySetupIndex >= 0 && setupInfo.fixedEnemySetupIndex < setupInfo.fixedEnemyInfoData.setups.Count)
			{
				setupInfo.fixedEnemySetup = setupInfo.fixedEnemyInfoData.setups[setupInfo.fixedEnemySetupIndex];
			}
			else if (setupInfo.fixedEnemySetup?.difficultyInfo == null)
			{
				setupInfo.fixedEnemySetupIndex = FindMatchingEnemySetupIndex(setupInfo.fixedEnemyInfoData, setupInfo.fixedEnemySetup);
				setupInfo.fixedEnemySetup = ((setupInfo.fixedEnemySetupIndex >= 0) ? setupInfo.fixedEnemyInfoData.setups[setupInfo.fixedEnemySetupIndex] : setupInfo.fixedEnemyInfoData.setups.SelectRandom());
			}
			return new EnemySpawnInfo(setupInfo.fixedEnemyInfoData, setupInfo.fixedEnemySetup, null);
		}
		CharacterAIComponent.EnemySpawnSetup preselectedEnemySetup = setupInfo.PreselectedEnemySetup;
		if (setupInfo.preselectedEnemyInfoData != null && preselectedEnemySetup != null)
		{
			(EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) tuple = gameState.generationManager.ApplyEnemyHealthPercentDelta(gameState, (setupInfo.preselectedEnemyInfoData, preselectedEnemySetup));
			return new EnemySpawnInfo(tuple.Item1, tuple.Item2, null);
		}
		if (setupInfo.enemySpawnType != EnemyEncounterType.None)
		{
			PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
			bool spawnEliteVariant = currentLocation != null && currentLocation.variantType == PathLocation.LocationVariantType.EliteVariant;
			HashSet<EnemyInfoData> excludedEnemies = (from enemyRef in setupInfo.excludedEnemyRefs.OrEmptyIfNull()
				where enemyRef?.IsValidRef(gameState.applicationState.assetCatalog) ?? false
				select enemyRef.GetAssetPreData(gameState.applicationState.assetCatalog)).ToHashSet();
			(EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) tuple2 = gameState.generationManager.GenerateEnemy(gameState, setupInfo.enemySpawnType, spawnEliteVariant, excludedEnemies);
			return new EnemySpawnInfo(tuple2.Item1, tuple2.Item2, null);
		}
		if (setupInfo.pvpDefender != null)
		{
			return new EnemySpawnInfo(setupInfo.pvpDefender.enemyAvatarRef.GetAssetPreData(gameState.applicationState.assetCatalog), null, setupInfo.pvpDefender);
		}
		return default(EnemySpawnInfo);
	}

	private static int FindMatchingEnemySetupIndex(EnemyInfoData enemyInfoData, CharacterAIComponent.EnemySpawnSetup setup)
	{
		if (enemyInfoData?.setups == null || setup?.itemGenerations == null)
		{
			return -1;
		}
		for (int i = 0; i < enemyInfoData.setups.Count; i++)
		{
			CharacterAIComponent.EnemySpawnSetup enemySpawnSetup = enemyInfoData.setups[i];
			if (enemySpawnSetup?.itemGenerations == null || enemySpawnSetup.itemGenerations.Count != setup.itemGenerations.Count)
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < enemySpawnSetup.itemGenerations.Count; j++)
			{
				if (enemySpawnSetup.itemGenerations[j]?.item != setup.itemGenerations[j]?.item)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	private void SpawnEnemy(EnemySpawnInfo spawnInfo)
	{
		if (spawnInfo.enemyInfoData != null)
		{
			CharacterAIComponent characterAIComponent = spawnInfo.enemyInfoData.loadedAsset.InstantiatePrefabIn(gameState.UIManager.scene3DRoot);
			BattleFieldTransformDisplay bftDisplay = characterAIComponent.BftDisplay;
			BattleFieldTransform bft = new BattleFieldTransform(gameState, isPlayer: false);
			bftDisplay.Init(bft);
			if (spawnInfo.pvpDefender == null)
			{
				enemySetup = spawnInfo.enemySpawnSetup;
				enemyAI = new CharacterAI(bft, characterAIComponent, spawnInfo.enemySpawnSetup, setupInfo.enemySpawnType);
			}
			else
			{
				enemyAI = new CharacterAI(bft, characterAIComponent, spawnInfo.pvpDefender);
			}
			gameState.battleState.enemy = enemyAI;
			gameState.applicationState.renderingManager.ApplyPerformanceSettingToEnemy(enemyAI);
			enemyAI.component.transform.position = room.enemySpawnPivot.position;
			gameState.UIManager.enemyBftUIDisplay.Setup(enemyAI.bft);
			if (enemyAI.component.debutCutscene != null && enemyAI.component.debutCutscene.IsValidRef(gameState.applicationState.assetCatalog) && !gameState.applicationState.recordManager.cutsceneDebutFinishedEnemies.Contains(enemyAI.component.GetInfoData(gameState.applicationState)))
			{
				EnqueueAction(new PlayCutsceneAction(enemyAI.component.debutCutscene.GetAssetPreData(gameState.applicationState.assetCatalog)));
				gameState.applicationState.recordManager.cutsceneDebutFinishedEnemies.Add(enemyAI.component.GetInfoData(gameState.applicationState));
			}
		}
	}

	private void ClearEnemy()
	{
		if (enemyAI != null)
		{
			BlockyDelicacyPersistentFlyVfx.ClearFor(gameState.applicationState, enemyAI.bft);
			enemyAI.Dispose();
			enemyAI = null;
		}
	}

	private void SetTriggerEvent()
	{
		if (setupInfo.fixedEventTemplate != null)
		{
			triggerEvent = UnityEngine.Object.Instantiate(setupInfo.fixedEventTemplate);
		}
		else if (setupInfo.spawnRandomWorldEvent)
		{
			triggerEvent = UnityEngine.Object.Instantiate(gameState.generationManager.GenerateRandomWorldEventTemplate());
		}
	}

	private void SetupEncounterBonus()
	{
		if (setupInfo.roomBonus != null)
		{
			roomBonus = setupInfo.roomBonus;
		}
	}

	private void SpawnTriggerEvent()
	{
		triggerEvent.SetupAndShow(gameState);
	}

	private void SetupEncounterEndAction()
	{
		PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
		bool flag = currentLocation != null;
		bool num = currentLocation != null && currentLocation.type == PathLocation.LocationType.Boss;
		bool flag2 = currentLocation != null && currentLocation == gameState.dungeonMapManager.currentDungeonMap.initLocation;
		if (num)
		{
			afterEncounterEnd = ExitBossRoom;
		}
		else if (flag2)
		{
			afterEncounterEnd = ExitWelcome;
		}
		else if (flag)
		{
			afterEncounterEnd = ExitNormalLocRoom;
		}
		else
		{
			afterEncounterEnd = ExitRoomNotInLocation;
		}
		void ExitBossRoom(bool finished)
		{
			if (gameState.currentEncounter.setupInfo.fixedEventTemplate == ScriptableObjectSingleton<DungeonGenerationManager>.instance.afterBossWorldEvent)
			{
				if (Level.GetStageLevels(gameState.contractState.stage + 1, gameState.applicationState.assetCatalog).Count > 1)
				{
					EncounterSetupInfo info = new EncounterSetupInfo
					{
						fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.selectNextStageLevelTemplate,
						level = gameState.contractState.currentLevel,
						roomInfoData = gameState.currentEncounter.setupInfo.roomInfoData,
						enemyLevel = currentLocation.enemyDifficultyLevel
					};
					EnqueueAction(new TransitReloadRoomAction(info));
				}
				else
				{
					EnqueueAction(new EnterNewStageAction(gameState.contractState.stage + 1, toWelcome: true, showLoading: true));
				}
			}
			else
			{
				EncounterSetupInfo info2 = new EncounterSetupInfo
				{
					fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.afterBossWorldEvent,
					level = gameState.contractState.currentLevel,
					roomInfoData = gameState.currentEncounter.setupInfo.roomInfoData,
					enemyLevel = currentLocation.enemyDifficultyLevel
				};
				EnqueueAction(new TransitReloadRoomAction(info2));
			}
		}
		void ExitNormalLocRoom(bool finished)
		{
			EnqueueAction(new ExitCurrentLocationToMapAction(finished));
		}
		void ExitRoomNotInLocation(bool finished)
		{
			EnqueueAction(new ExitRoomTransitAnimAction());
			if (gameState.currentEncounter != null)
			{
				gameState.actionManager.EnqueueAction(new ClearCurrentEncounterAction());
			}
			EnqueueAction(new EnterMapTransitAnimAction(scrollMap: false));
		}
		void ExitWelcome(bool finished)
		{
			EnqueueAction(new ExitCurrentLocationToMapAction(finished));
		}
	}

	private void ClearTriggerEvent()
	{
		if (gameState.UIManager.currentEventPanel != null)
		{
			gameState.UIManager.currentEventPanel.Unset();
			gameState.UIManager.currentEventPanel = null;
		}
		triggerEvent = null;
	}

	private void EnqueueAction(GameAction action)
	{
		gameState.actionManager.EnqueueAction(action);
	}
}
