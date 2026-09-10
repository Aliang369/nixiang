using System;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using FMODUnity;

public class AudioManager
{
	[NonSerialized]
	private Bus musicBus;

	[NonSerialized]
	private Bus sfxBus;

	public const string BgmTag = "BgmTag";

	public const string majorEnvironmentTag = "majorEnvironmentTag";

	public const string minorEnvironmentTag = "minorEnvironmentTag";

	public Dictionary<string, EventInstance?> sustainedTagEventInstances = new Dictionary<string, EventInstance?>();

	public void Init()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		TraceLog.Debug("[AudioManager] - Init()");
		try
		{
			musicBus = RuntimeManager.GetBus("bus:/Volume_BGM");
			sfxBus = RuntimeManager.GetBus("bus:/Volume_SFX");
		}
		catch (Exception e)
		{
			TraceLog.Exception(e);
		}
	}

	public void OnGlobalConfigChanged(SavingUtil.GlobalConfigSaveData config)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		TraceLog.TryDebug(() => $"[AudioManager] - _OnGlobalConfigChanged({config.sfxVolume}, {config.musicVolume})");
		((Bus)(ref sfxBus)).setVolume(config.sfxVolume);
		((Bus)(ref musicBus)).setVolume(config.musicVolume);
	}

	public EventInstance? PlayOneShot(EventReference eventRef)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		TraceLog.Debug($"[AudioManager] - PlayOneShot({eventRef})");
		if (((EventReference)(ref eventRef)).IsNull)
		{
			return null;
		}
		try
		{
			EventInstance value = RuntimeManager.CreateInstance(eventRef);
			((EventInstance)(ref value)).start();
			((EventInstance)(ref value)).release();
			return value;
		}
		catch (Exception e)
		{
			TraceLog.Exception(e);
		}
		return null;
	}

	public void PlayOneShot(string eventName)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		TraceLog.Debug("[AudioManager] - PlayOneShot(" + eventName + ")");
		try
		{
			EventInstance val = RuntimeManager.CreateInstance(eventName);
			((EventInstance)(ref val)).start();
			((EventInstance)(ref val)).release();
		}
		catch (Exception e)
		{
			TraceLog.Exception(e);
		}
	}

	public unsafe void PlaySoleSustained(EventReference eventRef)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		TraceLog.Debug($"[AudioManager] - PlaySoleSustained({eventRef})");
		PlaySustained(eventRef, ((object)(*(GUID*)(&eventRef.Guid))/*cast due to constrained. prefix*/).ToString());
	}

	public unsafe void StopSoleSustained(EventReference eventRef)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!((EventReference)(ref eventRef)).IsNull)
		{
			TraceLog.Debug($"[AudioManager] - StopSoleSustained({eventRef})");
			StopSustained(((object)(*(GUID*)(&eventRef.Guid))/*cast due to constrained. prefix*/).ToString());
		}
	}

	public void PlaySustained(EventReference eventRef, string tag)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (((EventReference)(ref eventRef)).IsNull)
		{
			return;
		}
		try
		{
			if (sustainedTagEventInstances.ContainsKey(tag))
			{
				StopSustained(tag);
			}
			EventInstance value = RuntimeManager.CreateInstance(eventRef);
			((EventInstance)(ref value)).start();
			sustainedTagEventInstances[tag] = value;
		}
		catch (Exception e)
		{
			TraceLog.Exception(e);
		}
	}

	public void PlaySustained(GUID guid, string tag)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (sustainedTagEventInstances.ContainsKey(tag))
			{
				StopSustained(tag);
			}
			EventInstance value = RuntimeManager.CreateInstance(guid);
			((EventInstance)(ref value)).start();
			sustainedTagEventInstances[tag] = value;
		}
		catch (Exception e)
		{
			TraceLog.Exception(e);
		}
	}

	public void StopSustained(string tag)
	{
		if (sustainedTagEventInstances.TryGetValue(tag, out var value) && value.HasValue)
		{
			TraceLog.Debug("[AudioManager] - StopSustained(" + tag + ")");
			Stop(value, (STOP_MODE)0);
			sustainedTagEventInstances.Remove(tag);
		}
	}

	public void StopAllSustained()
	{
		TraceLog.Debug("[AudioManager] - StopAllSustained()");
		foreach (string key in new Dictionary<string, EventInstance?>(sustainedTagEventInstances).Keys)
		{
			StopSustained(key);
		}
	}

	public unsafe bool IsSustainedEventPlaying(EventReference eventRef)
	{
		return IsSustainedEventPlaying(((object)(*(GUID*)(&eventRef.Guid))/*cast due to constrained. prefix*/).ToString());
	}

	public bool IsSustainedEventPlaying(string tag)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		EventInstance? sustainedEventInstance = GetSustainedEventInstance(tag);
		if (sustainedEventInstance.HasValue)
		{
			EventInstance value = sustainedEventInstance.Value;
			if (((EventInstance)(ref value)).isValid())
			{
				value = sustainedEventInstance.Value;
				PLAYBACK_STATE val = default(PLAYBACK_STATE);
				((EventInstance)(ref value)).getPlaybackState(ref val);
				return (int)val == 0;
			}
		}
		return false;
	}

	public EventInstance? GetSustainedEventInstance(string tag)
	{
		if (sustainedTagEventInstances.TryGetValue(tag, out var value))
		{
			return value;
		}
		return null;
	}

	public GUID? GetSustainedEventGUID(string tag)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		EventInstance? sustainedEventInstance = GetSustainedEventInstance(tag);
		if (sustainedEventInstance.HasValue)
		{
			EventInstance value = sustainedEventInstance.Value;
			if (((EventInstance)(ref value)).isValid())
			{
				value = sustainedEventInstance.Value;
				EventDescription val = default(EventDescription);
				((EventInstance)(ref value)).getDescription(ref val);
				GUID value2 = default(GUID);
				((EventDescription)(ref val)).getID(ref value2);
				return value2;
			}
		}
		return null;
	}

	public static void Stop(EventInstance? eventInstance, STOP_MODE stopMode)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (eventInstance.HasValue)
		{
			EventInstance value = eventInstance.Value;
			if (((EventInstance)(ref value)).isValid())
			{
				value = eventInstance.Value;
				((EventInstance)(ref value)).stop(stopMode);
				value = eventInstance.Value;
				((EventInstance)(ref value)).release();
				value = eventInstance.Value;
				((EventInstance)(ref value)).clearHandle();
			}
		}
	}
}
