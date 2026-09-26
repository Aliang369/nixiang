using UnityEngine;

public class zbAppLifecycleHandler : MonoBehaviour
{
	public static zbAppLifecycleHandler Instance;

	private float m_foregroundStartTime;

	private bool m_isForeground = true;

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
		m_foregroundStartTime = Time.unscaledTime;
		m_isForeground = true;
	}

	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)27))
		{
			zbSdkHelper.Option("exit", "");
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
			m_isForeground = false;
			return;
		}
		m_foregroundStartTime = Time.unscaledTime;
		m_isForeground = true;
	}

	public float GetForegroundElapsedSeconds()
	{
		if (!m_isForeground)
		{
			return 0f;
		}
		return Time.unscaledTime - m_foregroundStartTime;
	}
}
