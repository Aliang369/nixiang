using System.Collections.Generic;
using UnityEngine;

public class zbAudioMgr : MonoBehaviour
{
	public static zbAudioMgr Instance;

	private List<AudioSource> m_audioSources;

	private int m_nMaxNum = 15;

	private float m_maxAudioVolume = 1f;

	private bool m_bMute;

	private string m_configVolume = "audioVolume";

	private string m_configMute = "audioMute";

	private Dictionary<string, AudioClip> m_clipCache = new Dictionary<string, AudioClip>();

	private void Awake()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		m_maxAudioVolume = zbGameSave.Instance.GetFloat(m_configVolume, 1f);
		m_bMute = zbGameSave.Instance.GetBool(m_configMute);
	}

	private void Start()
	{
		m_audioSources = new List<AudioSource>();
		for (int i = 0; i < m_nMaxNum; i++)
		{
			AudioSource val = ((Component)this).gameObject.AddComponent<AudioSource>();
			val.loop = false;
			val.mute = m_bMute;
			m_audioSources.Add(val);
		}
	}

	public float GetMaxAudioVolume()
	{
		return m_maxAudioVolume;
	}

	public void SetMaxAudiovVolume(float volume)
	{
		m_maxAudioVolume = Mathf.Clamp(volume, 0f, 1f);
		foreach (AudioSource audioSource in m_audioSources)
		{
			audioSource.volume = volume;
		}
		zbGameSave.Instance.SetFloat(m_configVolume, volume);
	}

	public bool IsMute()
	{
		return m_bMute;
	}

	public void SetMute(bool value)
	{
		m_bMute = value;
		zbGameSave.Instance.SetBool(m_configMute, value);
		foreach (AudioSource audioSource in m_audioSources)
		{
			audioSource.mute = m_bMute;
		}
	}

	public AudioSource PlayAudio(string strAudioPath, bool loop = false)
	{
		if (m_bMute || m_maxAudioVolume <= 0.01f)
		{
			return null;
		}
		AudioSource freeAudioSource = GetFreeAudioSource();
		if ((Object)(object)freeAudioSource != (Object)null)
		{
			freeAudioSource.loop = loop;
			AudioClip value = null;
			if (!m_clipCache.TryGetValue(strAudioPath, out value))
			{
				value = zbAssetLoader.Instance.LoadResouce<AudioClip>(strAudioPath);
				if (!((Object)(object)value != (Object)null))
				{
					return null;
				}
				m_clipCache[strAudioPath] = value;
			}
			freeAudioSource.clip = value;
			freeAudioSource.volume = m_maxAudioVolume;
			freeAudioSource.Play();
			return freeAudioSource;
		}
		return null;
	}

	public void PauseAudio(AudioSource audioSource)
	{
		if (Object.op_Implicit((Object)(object)audioSource))
		{
			audioSource.Pause();
		}
	}

	public void ResumeAudio(AudioSource audioSource)
	{
		if (Object.op_Implicit((Object)(object)audioSource))
		{
			audioSource.UnPause();
		}
	}

	public void PauseAllAudio()
	{
		foreach (AudioSource audioSource in m_audioSources)
		{
			if (audioSource.isPlaying && audioSource.loop)
			{
				audioSource.Pause();
			}
		}
	}

	public void ResumeAllAudio()
	{
		foreach (AudioSource audioSource in m_audioSources)
		{
			audioSource.UnPause();
		}
	}

	public void StopAudio(AudioSource audioSource)
	{
		if (Object.op_Implicit((Object)(object)audioSource))
		{
			audioSource.Stop();
			audioSource.loop = false;
		}
	}

	public void StopAllAudio()
	{
		foreach (AudioSource audioSource in m_audioSources)
		{
			if (Object.op_Implicit((Object)(object)audioSource) && audioSource.isPlaying && audioSource.loop)
			{
				audioSource.Stop();
				audioSource.loop = false;
			}
		}
	}

	private AudioSource GetFreeAudioSource()
	{
		foreach (AudioSource audioSource in m_audioSources)
		{
			if (!audioSource.isPlaying && !audioSource.loop)
			{
				return audioSource;
			}
		}
		return null;
	}

	public void ClearClipCache()
	{
		m_clipCache.Clear();
	}
}
