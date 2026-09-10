using UnityEngine.Playables;
using UnityEngine.Video;

namespace UnityEngine.Timeline;

public class VideoPlayableBehaviour : PlayableBehaviour
{
	public VideoPlayer videoPlayer;

	public VideoClip videoClip;

	public bool mute;

	public bool loop = true;

	public double preloadTime = 0.3;

	public double clipInTime;

	private bool playedOnce;

	private bool preparing;

	public void PrepareVideo()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Invalid comparison between Unknown and I4
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Invalid comparison between Unknown and I4
		if ((Object)(object)videoPlayer == null || (Object)(object)videoClip == null)
		{
			return;
		}
		videoPlayer.targetCameraAlpha = 0f;
		if ((Object)(object)videoPlayer.clip != (Object)(object)videoClip)
		{
			StopVideo();
		}
		if (videoPlayer.isPrepared || preparing)
		{
			return;
		}
		videoPlayer.source = (VideoSource)0;
		videoPlayer.clip = videoClip;
		videoPlayer.playOnAwake = false;
		videoPlayer.waitForFirstFrame = true;
		videoPlayer.isLooping = loop;
		for (ushort num = 0; num < videoClip.audioTrackCount; num++)
		{
			if ((int)videoPlayer.audioOutputMode == 2)
			{
				videoPlayer.SetDirectAudioMute(num, mute || !Application.isPlaying);
			}
			else if ((int)videoPlayer.audioOutputMode == 1)
			{
				AudioSource targetAudioSource = videoPlayer.GetTargetAudioSource(num);
				if ((Object)(object)targetAudioSource != null)
				{
					targetAudioSource.mute = mute || !Application.isPlaying;
				}
			}
		}
		videoPlayer.loopPointReached += new EventHandler(LoopPointReached);
		videoPlayer.time = clipInTime;
		videoPlayer.Prepare();
		preparing = true;
	}

	private void LoopPointReached(VideoPlayer vp)
	{
		playedOnce = !loop;
	}

	public override void PrepareFrame(Playable playable, FrameData info)
	{
		if (!((Object)(object)videoPlayer == null) && !((Object)(object)videoClip == null))
		{
			videoPlayer.timeReference = (VideoTimeReference)(Application.isPlaying ? 2 : 0);
			if (videoPlayer.isPlaying && Application.isPlaying)
			{
				videoPlayer.externalReferenceTime = playable.GetTime();
			}
			else if (!Application.isPlaying)
			{
				SyncVideoToPlayable(playable);
			}
		}
	}

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		if (!((Object)(object)videoPlayer == null) && !playedOnce)
		{
			PlayVideo();
			SyncVideoToPlayable(playable);
		}
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		if (!((Object)(object)videoPlayer == null))
		{
			if (Application.isPlaying)
			{
				PauseVideo();
			}
			else
			{
				StopVideo();
			}
		}
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Invalid comparison between Unknown and I4
		if ((Object)(object)videoPlayer == null || (Object)(object)videoPlayer.clip == null)
		{
			return;
		}
		videoPlayer.targetCameraAlpha = info.weight;
		if (!Application.isPlaying)
		{
			return;
		}
		for (ushort num = 0; num < videoPlayer.clip.audioTrackCount; num++)
		{
			if ((int)videoPlayer.audioOutputMode == 2)
			{
				videoPlayer.SetDirectAudioVolume(num, info.weight);
			}
			else if ((int)videoPlayer.audioOutputMode == 1)
			{
				AudioSource targetAudioSource = videoPlayer.GetTargetAudioSource(num);
				if ((Object)(object)targetAudioSource != null)
				{
					targetAudioSource.volume = info.weight;
				}
			}
		}
	}

	public override void OnGraphStart(Playable playable)
	{
		playedOnce = false;
	}

	public override void OnGraphStop(Playable playable)
	{
		if (!Application.isPlaying)
		{
			StopVideo();
		}
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		StopVideo();
	}

	public void PlayVideo()
	{
		if (!((Object)(object)videoPlayer == null))
		{
			videoPlayer.Play();
			preparing = false;
			if (!Application.isPlaying)
			{
				PauseVideo();
			}
		}
	}

	public void PauseVideo()
	{
		if (!((Object)(object)videoPlayer == null))
		{
			videoPlayer.Pause();
			preparing = false;
		}
	}

	public void StopVideo()
	{
		if (!((Object)(object)videoPlayer == null))
		{
			playedOnce = false;
			videoPlayer.Stop();
			preparing = false;
		}
	}

	private void SyncVideoToPlayable(Playable playable)
	{
		if (!((Object)(object)videoPlayer == null) && !((Object)(object)videoPlayer.clip == null))
		{
			videoPlayer.time = (clipInTime + playable.GetTime() * (double)videoPlayer.playbackSpeed) % videoPlayer.clip.length;
		}
	}
}
