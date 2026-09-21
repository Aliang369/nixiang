using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using Spine.Unity;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayCutSceneState
{
	private enum Phase
	{
		None,
		Start,
		Playing,
		End
	}

	public CutsceneInfoData cutsceneInfo;

	public bool stayCutscene;

	public bool skipFadeIn;

	public Cutscene cutsceneInstance;

	public float initCameraFov;

	public IEnumerator currentCor;

	private Phase phase;

	private bool skiping;

	private bool skipRequested;

	private bool canSkip;

	private EventInstance? beforeBGM;

	public Action<Cutscene> onConfigCutscene;

	public Action onTimelineStarted;

	public float audioLeadTime;

	public Func<IEnumerator> onHideGame;

	public Func<IEnumerator> onRecoverGame;

	private bool HideRoom => cutsceneInfo.hideCurrentRoom;

	public ApplicationStateInfo ApplicationState => Bootstrap.applicationState;

	public GameStateInfo gameState => ApplicationState.runningGameState;

	public FortressStateInfo fortress => ApplicationState.fortressState;

	public VirtualCamera VirtualCamera => gameState?.UIManager.gameVirtualCamera ?? fortress.uiManager.fortressCamera;

	public CutscenePanel CutscenePanel => gameState?.UIManager.cutscenePanel ?? fortress.uiManager.cutscenePanel;

	public DialoguePanel CutsceneDialoguePanel => gameState?.UIManager.cutsceneDialoguePanel ?? fortress.uiManager.cutsceneDialoguePanel;

	public SkeletonGraphic CutsceneOverlaySpineGraphic => CutscenePanel.cutsceneOverlaySpineGraphic;

	public List<GameObject> CutsceneBlackSlide => CutscenePanel.cutsceneBlackSlide;

	public PlayCutSceneState(CutsceneInfoData cutScene, GameStateInfo gameState, bool stayCutscene, bool skipFadeIn, bool canSkip = true)
	{
		cutsceneInfo = cutScene;
		this.stayCutscene = stayCutscene;
		this.skipFadeIn = skipFadeIn;
		this.canSkip = canSkip;
	}

	public PlayCutSceneState(CutsceneInfoData cutScene, FortressStateInfo fortress, bool stayCutscene, bool skipFadeIn, bool canSkip = true)
	{
		cutsceneInfo = cutScene;
		this.stayCutscene = stayCutscene;
		this.skipFadeIn = skipFadeIn;
		this.canSkip = canSkip;
	}

	public void Skip()
	{
		if (!canSkip || skiping)
		{
			return;
		}
		if (currentCor == null)
		{
			if (phase == Phase.Playing && cutsceneInstance == null)
			{
				skipRequested = true;
			}
		}
		else if (gameState != null)
		{
			gameState.StartCoroutine(SkipCor);
		}
		else if (fortress != null)
		{
			fortress.StartCoroutine(SkipCor);
		}
		IEnumerator SkipCor()
		{
			skiping = true;
			if (phase != Phase.End && phase != Phase.None)
			{
				yield return DarkenIn(skipFadeIn: false);
				cutsceneInstance.skip = true;
				CutscenePanel.cutsceneDarkenImage.SetA(1f);
				skiping = false;
			}
		}
	}

	public IEnumerator PlayCutsceneCor()
	{
		skiping = false;
		skipRequested = false;
		phase = Phase.None;
		if (ApplicationState.currentCutSceneState != null)
		{
			yield break;
		}
		CutscenePanel.Clear();
		beforeBGM = Bootstrap.instance.audioManager.GetSustainedEventInstance("BgmTag");
		EventInstance value;
		if (beforeBGM.HasValue)
		{
			value = beforeBGM.Value;
			((EventInstance)(ref value)).setPaused(true);
		}
		phase = Phase.Start;
		ApplicationState.currentCutSceneState = this;
		EventInstance? cutSound = null;
		bool unloaded = false;
		try
		{
			AsyncOperationHandle<GameObject> cutsceneLoadHandle = cutsceneInfo.LoadAsync();
			yield return DarkenIn(skipFadeIn);
			if (cutsceneLoadHandle.IsValid() && !cutsceneLoadHandle.IsDone)
			{
				yield return cutsceneLoadHandle;
			}
			phase = Phase.Playing;
			CutscenePanel.skipButton.gameObject.SetActive(canSkip);
			if (audioLeadTime > 0f)
			{
				onTimelineStarted?.Invoke();
				yield return new WaitForSeconds(audioLeadTime);
			}
			SpawnCutscene();
			string trackCutsceneFullId = cutsceneInstance.FullId;
			try
			{
				ThinkingAnalyticsExtends.TrackEnterCutscene(ApplicationState, trackCutsceneFullId);
			}
			catch (Exception arg)
			{
				TraceLog.Error($"[Cutscene] enter tracking failed: {arg}");
			}
			HideExistedObject();
			SetupCamera();
			cutSound = Bootstrap.instance.audioManager.PlayOneShot(cutsceneInstance.cutsceneAudioTrack);
			currentCor = cutsceneInstance.PlayTimeline(stayCutscene, (audioLeadTime > 0f) ? null : onTimelineStarted);
			if (skipRequested)
			{
				skipRequested = false;
				Skip();
			}
			CutscenePanel.cutsceneDarkenImage.gameObject.SetActive(value: false);
			yield return currentCor;
			currentCor = null;
			yield return new WaitUntil(() => !skiping);
			phase = Phase.End;
			bool trackIsSkipped = cutsceneInstance.skip;
			UnsetCamera();
			ShowExistedObject();
			CleanCutscene();
			yield return null;
			cutsceneInfo.Unload();
			unloaded = true;
			yield return DarkenOut();
			AudioManager.Stop(cutSound, (STOP_MODE)0);
			if (beforeBGM.HasValue)
			{
				value = beforeBGM.Value;
				((EventInstance)(ref value)).setPaused(false);
			}
			phase = Phase.None;
			ApplicationState.currentCutSceneState = null;
			CutscenePanel.Clear();
			ThinkingAnalyticsExtends.TrackFinishCutscene(ApplicationState, trackCutsceneFullId, trackIsSkipped);
		}
		finally
		{
			PlayCutSceneState playCutSceneState = this;
			if (playCutSceneState.phase != Phase.None)
			{
				playCutSceneState.ForceRestore(cutSound, unloaded);
			}
		}
	}

	private void ForceRestore(EventInstance? cutSound, bool alreadyUnloaded)
	{
		TryStep(delegate
		{
			if (cutsceneInstance != null)
			{
				UnsetCamera();
			}
		});
		TryStep(ShowExistedObject);
		TryStep(delegate
		{
			if (cutsceneInstance != null)
			{
				CleanCutscene();
			}
		});
		TryStep(delegate
		{
			if (!alreadyUnloaded)
			{
				cutsceneInfo.Unload();
			}
		});
		TryStep(delegate
		{
			CutscenePanel.cutsceneDarkenImage.DOKill();
			CutscenePanel.cutsceneDarkenImage.SetA(0f);
			CutscenePanel.cutsceneDarkenImage.gameObject.SetActive(value: false);
			CutscenePanel.cutsceneAnimDarkenImage.SetA(0f);
			CutscenePanel.cutsceneAnimDarkenImage.gameObject.SetActive(value: false);
			CutsceneBlackSlide.ForEach(delegate(GameObject go)
			{
				go.SetActive(value: false);
			});
			CutscenePanel.skipButton.gameObject.SetActive(value: false);
			CutscenePanel.subtitleText.gameObject.SetActive(value: false);
		});
		TryStep(delegate
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			AudioManager.Stop(cutSound, (STOP_MODE)0);
			if (beforeBGM.HasValue)
			{
				EventInstance value = beforeBGM.Value;
				((EventInstance)(ref value)).setPaused(false);
			}
		});
		currentCor = null;
		skipRequested = false;
		phase = Phase.None;
		ApplicationState.currentCutSceneState = null;
		TraceLog.Error("[Cutscene] 播放中断，已强制恢复到可玩状态");
	}

	private static void TryStep(Action step)
	{
		try
		{
			step();
		}
		catch (Exception arg)
		{
			TraceLog.Error($"[Cutscene] ForceRestore step failed: {arg}");
		}
	}

	private void SpawnCutscene()
	{
		Cutscene orSyncLoadAsset = cutsceneInfo.GetOrSyncLoadAsset();
		onConfigCutscene?.Invoke(orSyncLoadAsset);
		cutsceneInstance = orSyncLoadAsset.InstantiatePrefabIn(gameState?.UIManager.scene3DRoot);
		cutsceneInstance.transform.position = Vector3.zero;
		cutsceneInstance.Setup(this);
	}

	private void CleanCutscene()
	{
		UnityEngine.Object.Destroy(cutsceneInstance.gameObject);
		if (gameState?.currentEncounter?.room != null)
		{
			gameState.currentEncounter.room.ApplyRoomRenderSetting(gameState.applicationState);
		}
		if (fortress?.fortressRoomManager?.room != null)
		{
			fortress.fortressRoomManager.room.ApplyRoomRenderSetting(fortress.applicationState);
		}
	}

	private void HideExistedObject()
	{
		if (gameState != null)
		{
			EncounterInfo currentEncounter = gameState.currentEncounter;
			if (currentEncounter != null)
			{
				if (currentEncounter.room != null && HideRoom)
				{
					currentEncounter.room.gameObject.SetActive(value: false);
				}
				if (currentEncounter.enemyAI?.component != null)
				{
					currentEncounter.enemyAI.component.gameObject.SetActive(value: false);
				}
				foreach (RoomObjectDisplay item in currentEncounter?.roomObjectDisplays.OrEmptyIfNull())
				{
					item.gameObject.SetActive(value: false);
				}
			}
			((Component)(object)gameState.UIManager.basePanelsRoot).gameObject.SetActive(value: false);
			((Component)(object)gameState.UIManager.secondaryPanelsRoot).gameObject.SetActive(value: false);
			((Component)(object)gameState.UIManager.tertiaryPanelsRoot).gameObject.SetActive(value: false);
			((Component)(object)gameState.UIManager.ui3DBaseCanvas).gameObject.SetActive(value: false);
			((Component)(object)gameState.UIManager.ui3DCameraCanvas).gameObject.SetActive(value: false);
			((Component)(object)gameState.UIManager.ui3DWorldCanvas).gameObject.SetActive(value: false);
		}
		if (fortress != null)
		{
			((Component)(object)fortress.uiManager.uiCanvas).gameObject.SetActive(value: false);
			if (HideRoom)
			{
				fortress.fortressRoomManager.room?.gameObject.SetActive(value: false);
			}
		}
	}

	private void ShowExistedObject()
	{
		if (gameState != null)
		{
			EncounterInfo currentEncounter = gameState.currentEncounter;
			if (currentEncounter != null)
			{
				if (currentEncounter.room != null && HideRoom)
				{
					currentEncounter.room.gameObject.SetActive(value: true);
					currentEncounter.room.ApplyRoomRenderSetting(gameState.applicationState);
				}
				if (currentEncounter.enemyAI?.component != null)
				{
					currentEncounter.enemyAI.component.gameObject.SetActive(value: true);
				}
				foreach (RoomObjectDisplay item in currentEncounter?.roomObjectDisplays.OrEmptyIfNull())
				{
					item.gameObject.SetActive(value: true);
				}
			}
			((Component)(object)gameState.UIManager.basePanelsRoot).gameObject.SetActive(value: true);
			((Component)(object)gameState.UIManager.secondaryPanelsRoot).gameObject.SetActive(value: true);
			((Component)(object)gameState.UIManager.tertiaryPanelsRoot).gameObject.SetActive(value: true);
			((Component)(object)gameState.UIManager.ui3DBaseCanvas).gameObject.SetActive(value: true);
			((Component)(object)gameState.UIManager.ui3DCameraCanvas).gameObject.SetActive(value: true);
			((Component)(object)gameState.UIManager.ui3DWorldCanvas).gameObject.SetActive(value: true);
		}
		if (fortress != null)
		{
			((Component)(object)fortress.uiManager.uiCanvas).gameObject.SetActive(value: true);
			if (HideRoom)
			{
				fortress.fortressRoomManager.room?.gameObject.SetActive(value: true);
			}
		}
	}

	private IEnumerator DarkenIn(bool skipFadeIn)
	{
		CutscenePanel.cutsceneDarkenImage.gameObject.SetActive(value: true);
		CutscenePanel.cutsceneDarkenImage.color = Color.black.SetA(0f);
		if (skipFadeIn)
		{
			CutscenePanel.cutsceneDarkenImage.color = Color.black;
		}
		else
		{
			yield return DOTweenModuleUI.DOFade(CutscenePanel.cutsceneDarkenImage, 1f, cutsceneInfo.screenFadeTime).WaitForCompletion();
		}
		if (onHideGame != null)
		{
			yield return onHideGame();
		}
		CutscenePanel.cutsceneAnimDarkenImage.gameObject.SetActive(value: true);
		CutsceneBlackSlide.ForEach(delegate(GameObject go)
		{
			go.SetActive(value: true);
		});
	}

	private IEnumerator DarkenOut()
	{
		CutscenePanel.cutsceneAnimDarkenImage.gameObject.SetActive(value: false);
		CutscenePanel.cutsceneDarkenImage.gameObject.SetActive(value: true);
		CutscenePanel.cutsceneDarkenImage.color = Color.black.SetA(1f);
		CutscenePanel.subtitleText.gameObject.SetActive(value: false);
		CutsceneBlackSlide.ForEach(delegate(GameObject go)
		{
			go.SetActive(value: false);
		});
		CutscenePanel.skipButton.gameObject.SetActive(value: false);
		if (onRecoverGame != null)
		{
			yield return onRecoverGame();
		}
		yield return DOTweenModuleUI.DOFade(CutscenePanel.cutsceneDarkenImage, 0f, cutsceneInfo.screenFadeTime).WaitForCompletion();
		CutscenePanel.cutsceneDarkenImage.gameObject.SetActive(value: false);
	}

	private void SetupCamera()
	{
		VirtualCamera.lookPivotCameraController.lockCameraTransform = false;
		VirtualCamera.gyroscopeCameraController.EnableGyroscope = false;
		VirtualCamera.mouseShiftingCameraController.enableMouseShifting = false;
		VirtualCamera.cutSceneCameraPivot.localPosition = Vector3.zero;
		VirtualCamera.cutSceneCameraPivot.localRotation = Quaternion.identity;
		initCameraFov = VirtualCamera.bindedCamera.CameraFOV;
		cutsceneInstance.enableCameraPivotUpdate = true;
	}

	private void UnsetCamera()
	{
		VirtualCamera.lookPivotCameraController.lockCameraTransform = true;
		VirtualCamera.gyroscopeCameraController.EnableGyroscope = true;
		VirtualCamera.mouseShiftingCameraController.enableMouseShifting = true;
		VirtualCamera.cutSceneCameraPivot.localPosition = Vector3.zero;
		VirtualCamera.cutSceneCameraPivot.localRotation = Quaternion.identity;
		VirtualCamera.bindedCamera.CameraFOV = initCameraFov;
		cutsceneInstance.enableCameraPivotUpdate = false;
	}
}
