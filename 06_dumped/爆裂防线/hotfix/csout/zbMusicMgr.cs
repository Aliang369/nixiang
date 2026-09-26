using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zbMusicMgr : MonoBehaviour
{
	public static zbMusicMgr Instance;

	private AudioSource m_audioSource;

	private const float m_fadeDuration = 1f;

	private float m_maxVolume = 1f;

	private bool m_bMute;

	private string m_strAudioPath;

	private string m_configVolume = "musicVolume";

	private string m_configMute = "musicMute";

	private Coroutine m_coroutine;

	private Dictionary<string, AudioClip> m_musicCache = new Dictionary<string, AudioClip>();

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
		m_maxVolume = zbGameSave.Instance.GetFloat(m_configVolume, 1f);
		m_bMute = zbGameSave.Instance.GetBool(m_configMute);
	}

	private void Start()
	{
		m_audioSource = ((Component)this).gameObject.AddComponent<AudioSource>();
		m_audioSource.loop = true;
		m_audioSource.mute = m_bMute;
	}

	public float GetMaxVolume()
	{
		return m_maxVolume;
	}

	public void SetMaxVolume(float volume)
	{
		m_maxVolume = Mathf.Clamp(volume, 0f, 1f);
		m_audioSource.volume = m_maxVolume;
		zbGameSave.Instance.SetFloat(m_configVolume, m_maxVolume);
	}

	public bool IsMute()
	{
		return m_bMute;
	}

	public void SetMute(bool value)
	{
		m_bMute = value;
		zbGameSave.Instance.SetBool(m_configMute, value);
		m_audioSource.mute = m_bMute;
	}

	public void Play(string strAudioPath, float scaleFactor = 1f)
	{
		if (m_strAudioPath == strAudioPath)
		{
			return;
		}
		m_strAudioPath = strAudioPath;
		AudioClip value = null;
		if (!m_musicCache.TryGetValue(strAudioPath, out value))
		{
			value = zbAssetLoader.Instance.LoadResouce<AudioClip>(strAudioPath);
			if (!((Object)(object)value != (Object)null))
			{
				return;
			}
			m_musicCache[strAudioPath] = value;
		}
		if (m_coroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
		m_coroutine = ((MonoBehaviour)this).StartCoroutine(SwitchMusicCoroutine(value, scaleFactor));
	}

	private IEnumerator SwitchMusicCoroutine(AudioClip newClip, float scaleFactor)
	{
		yield return FadeOutCoroutine();
		m_audioSource.clip = newClip;
		m_audioSource.mute = m_bMute;
		((MonoBehaviour)this).StartCoroutine(FadeInCoroutine(scaleFactor));
	}

	private IEnumerator FadeInCoroutine(float scaleFactor)
	{
		if ((Object)(object)m_audioSource.clip == (Object)null)
		{
			yield return null;
		}
		float currentTime = 0f;
		m_audioSource.volume = 0f;
		m_audioSource.Play();
		float maxVolume = m_maxVolume * scaleFactor;
		while (currentTime < 1f)
		{
			m_audioSource.volume = Mathf.Lerp(0f, maxVolume, currentTime / 1f);
			currentTime += Time.deltaTime;
			yield return null;
		}
		m_audioSource.volume = maxVolume;
	}

	private IEnumerator FadeOutCoroutine()
	{
		if ((Object)(object)m_audioSource.clip == (Object)null || !m_audioSource.isPlaying)
		{
			yield return null;
		}
		float currentVolume = m_audioSource.volume;
		float currentTime = 0f;
		while (currentTime < 1f)
		{
			m_audioSource.volume = Mathf.Lerp(currentVolume, 0f, currentTime / 1f);
			currentTime += Time.deltaTime;
			yield return null;
		}
		m_audioSource.volume = 0f;
		m_audioSource.Stop();
	}

	public void ClearClipCache()
	{
		m_musicCache.Clear();
	}
}
