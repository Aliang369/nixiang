using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutsceneRankController : MonoBehaviour
{
	[SerializeField]
	private PlayableDirector director;

	private const string RankPrefix = "rank_";

	private const string EndPrefix = "end";

	[NonSerialized]
	private Dictionary<GameObject, bool> initialRankObjectActiveStates;

	public void SetRank(QualityLevel rank)
	{
		PlayableAsset playableAsset = director.playableAsset;
		TimelineAsset val = (TimelineAsset)(object)((playableAsset is TimelineAsset) ? playableAsset : null);
		if (!((UnityEngine.Object)(object)val == null))
		{
			int targetIndex = RankToTrackIndex(rank);
			MuteTracksExcept(val, targetIndex);
			SyncControlledObjectActive(val, targetIndex);
		}
	}

	private static int RankToTrackIndex(QualityLevel rank)
	{
		return (int)(rank + 2);
	}

	private static void MuteTracksExcept(TimelineAsset timeline, int targetIndex)
	{
		foreach (TrackAsset outputTrack in timeline.GetOutputTracks())
		{
			if (TryParseRankTrack(((UnityEngine.Object)(object)outputTrack).name, out var index, out var _))
			{
				outputTrack.muted = index != targetIndex;
			}
		}
	}

	private void SyncControlledObjectActive(TimelineAsset timeline, int targetIndex)
	{
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		HashSet<GameObject> hashSet2 = new HashSet<GameObject>();
		foreach (TrackAsset outputTrack in timeline.GetOutputTracks())
		{
			if (!(outputTrack is ControlTrack) || !TryParseRankTrack(((UnityEngine.Object)(object)outputTrack).name, out var index, out var isRankTrack) || !isRankTrack)
			{
				continue;
			}
			foreach (TimelineClip clip in outputTrack.GetClips())
			{
				UnityEngine.Object asset = clip.asset;
				ControlPlayableAsset val = (ControlPlayableAsset)(object)((asset is ControlPlayableAsset) ? asset : null);
				if (val == null)
				{
					continue;
				}
				GameObject gameObject = val.sourceGameObject.Resolve((IExposedPropertyTable)director);
				if (!(gameObject == null))
				{
					hashSet.Add(gameObject);
					if (index == targetIndex)
					{
						hashSet2.Add(gameObject);
					}
				}
			}
		}
		if (initialRankObjectActiveStates == null)
		{
			initialRankObjectActiveStates = new Dictionary<GameObject, bool>();
		}
		foreach (GameObject item in hashSet)
		{
			if (!initialRankObjectActiveStates.TryGetValue(item, out var value))
			{
				value = item.activeSelf;
				initialRankObjectActiveStates.Add(item, value);
			}
			item.SetActive(hashSet2.Contains(item) & value);
		}
	}

	private static bool TryParseRankTrack(string trackName, out int index, out bool isRankTrack)
	{
		index = 0;
		isRankTrack = false;
		if (string.IsNullOrEmpty(trackName))
		{
			return false;
		}
		string s;
		if (trackName.StartsWith("rank_", StringComparison.Ordinal))
		{
			isRankTrack = true;
			s = trackName.Substring("rank_".Length);
		}
		else
		{
			if (!trackName.StartsWith("end", StringComparison.Ordinal))
			{
				return false;
			}
			s = trackName.Substring("end".Length);
		}
		return int.TryParse(s, out index);
	}
}
