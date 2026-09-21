using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class Cutscene : CTPrefabAsset
{
	public bool hideCurrentRoom;

	public Camera cameraPivot;

	public float screenFadeTime = 0.5f;

	public EventReference cutsceneAudioTrack;

	[NonSerialized]
	public bool enableCameraPivotUpdate;

	[NonSerialized]
	public bool skip;

	[NonSerialized]
	public PlayCutSceneState currentPlayState;

	public override string CTAssetIDPrepend => "CUT";

	public PlayableDirector Director => GetComponentInChildren<PlayableDirector>();

	public DungeonRoom Room => GetComponentInChildren<DungeonRoom>();

	private void Update()
	{
		if (enableCameraPivotUpdate)
		{
			UpdateCameraPivot();
		}
	}

	public void Setup(PlayCutSceneState playState)
	{
		try
		{
			currentPlayState = playState;
			skip = false;
			PlayableDirector director = Director;
			director.time = 0.0;
			director.Evaluate();
			director.playOnAwake = false;
			director.extrapolationMode = DirectorWrapMode.Hold;
			Animator[] componentsInChildren = GetComponentsInChildren<Animator>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].runtimeAnimatorController = null;
			}
		}
		catch (Exception arg)
		{
			TraceLog.Error($"[Cutscene] Setup() - {assetGuid} - Exception: {arg}");
		}
	}

	public void Unset()
	{
		currentPlayState = null;
	}

	public void UpdateCameraPivot()
	{
		if (cameraPivot != null && currentPlayState.VirtualCamera != null)
		{
			currentPlayState.VirtualCamera.cutSceneCameraPivot.position = cameraPivot.transform.position;
			currentPlayState.VirtualCamera.cutSceneCameraPivot.rotation = cameraPivot.transform.rotation;
			currentPlayState.VirtualCamera.bindedCamera.CameraFOV = cameraPivot.fieldOfView;
		}
	}

	public IEnumerator PlayTimeline(bool stayAfterFinish, Action onTimelineStarted = null)
	{
		PlayableDirector director = Director;
		if ((UnityEngine.Object)(object)director == null)
		{
			yield break;
		}
		DungeonRoom room = Room;
		bool stopped = false;
		director.stopped += OnStop;
		if (skip)
		{
			director.stopped -= OnStop;
			yield break;
		}
		director.Play();
		onTimelineStarted?.Invoke();
		while (!stopped && !skip && (stayAfterFinish || !(director.time + 1.0 / 60.0 >= director.duration)))
		{
			yield return new WaitForEndOfFrame();
			if (room != null)
			{
				room.ApplyRoomRenderSetting(currentPlayState.ApplicationState);
			}
		}
		director.stopped -= OnStop;
		void OnStop(PlayableDirector d)
		{
			stopped = true;
		}
	}

	public void OnSkipEvent()
	{
		if (currentPlayState != null)
		{
			currentPlayState.Skip();
		}
	}

	public List<UnityEngine.Object> GetAllAsset()
	{
		List<UnityEngine.Object> list = new List<UnityEngine.Object>();
		PlayableAsset playableAsset = GetComponentInChildren<PlayableDirector>(includeInactive: true).playableAsset;
		foreach (TrackAsset outputTrack in ((TimelineAsset)((playableAsset is TimelineAsset) ? playableAsset : null)).GetOutputTracks())
		{
			foreach (TimelineClip clip in outputTrack.GetClips())
			{
				list.Add(clip.asset);
			}
		}
		return list;
	}
}
