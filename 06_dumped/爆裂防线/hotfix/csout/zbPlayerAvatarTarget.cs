using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerAvatarTarget : MonoBehaviour
{
	public Camera m_renderCamera;

	private Image m_descImg;

	public zbPlayerAvatar m_spineAvatar;

	private const int kRenderEveryNFrames = 4;

	private int m_phase;

	private bool m_forceNextRender;

	private void Awake()
	{
		m_renderCamera = ((Component)((Component)this).transform.Find("Camera")).GetComponent<Camera>();
		m_spineAvatar = ((Component)((Component)this).transform.Find("Camera").Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		m_phase = Mathf.Abs(((Object)this).GetInstanceID()) % 4;
	}

	public void StartRender(Image _descImg)
	{
		m_descImg = _descImg;
		((Component)this).gameObject.SetActive(true);
		m_forceNextRender = true;
		m_descImg.image = (Texture)(object)m_renderCamera.targetTexture;
	}

	public void RefreshAvatar(int avatarId, int gunId)
	{
		m_spineAvatar.RefreshCustom(avatarId, gunId);
		m_forceNextRender = true;
	}

	public void StopRender()
	{
		m_descImg = null;
		((Component)this).gameObject.SetActive(false);
	}

	private void Update()
	{
		if (!((Object)(object)m_renderCamera == (Object)null))
		{
			bool flag = m_forceNextRender || Time.frameCount % 4 == m_phase;
			if (((Behaviour)m_renderCamera).enabled != flag)
			{
				((Behaviour)m_renderCamera).enabled = flag;
			}
			if (flag)
			{
				m_forceNextRender = false;
			}
		}
	}
}
