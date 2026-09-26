using UnityEngine;

public class zbHeroAudioMgr : MonoBehaviour
{
	public static zbHeroAudioMgr Instance;

	private AudioSource m_HeroAudioSource;

	private float m_maxAudioVolume = 1f;

	private bool m_bMute;

	private string m_configVolume = "heroAudioVolume";

	private string m_configMute = "heroAudioMute";

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
		m_HeroAudioSource = ((Component)this).gameObject.AddComponent<AudioSource>();
		m_HeroAudioSource.loop = false;
		m_HeroAudioSource.mute = m_bMute;
	}

	public float GetMaxAudioVolume()
	{
		return m_maxAudioVolume;
	}

	public void SetMaxAudiovVolume(float volume)
	{
		m_maxAudioVolume = Mathf.Clamp(volume, 0f, 1f);
		m_HeroAudioSource.volume = volume;
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
		m_HeroAudioSource.mute = m_bMute;
	}

	public AudioSource PlayAudio(string strAudioPath)
	{
		if (m_bMute)
		{
			return null;
		}
		if (m_maxAudioVolume <= 0.01f)
		{
			return null;
		}
		if (Object.op_Implicit((Object)(object)m_HeroAudioSource))
		{
			m_HeroAudioSource.clip = zbAssetLoader.Instance.LoadResouce<AudioClip>(strAudioPath);
			if ((Object)(object)m_HeroAudioSource.clip != (Object)null)
			{
				m_HeroAudioSource.volume = m_maxAudioVolume;
				m_HeroAudioSource.Play();
				return m_HeroAudioSource;
			}
		}
		return null;
	}
}
