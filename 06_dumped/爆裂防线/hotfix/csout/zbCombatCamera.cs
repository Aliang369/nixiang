using UnityEngine;
using UnityEngine.UIElements;

public class zbCombatCamera : MonoBehaviour
{
	private Camera m_renderCamera;

	private Image m_descImg;

	private void Awake()
	{
		m_renderCamera = ((Component)((Component)this).transform.Find("Camera")).GetComponent<Camera>();
	}

	public void StartRender(Image _descImg)
	{
		m_descImg = _descImg;
		m_renderCamera.orthographicSize = 8.4f;
		((Component)this).gameObject.SetActive(true);
		((Behaviour)m_renderCamera).enabled = true;
		m_descImg.image = (Texture)(object)m_renderCamera.targetTexture;
	}

	public void StopRender()
	{
		m_descImg = null;
		((Component)this).gameObject.SetActive(false);
	}

	public void FreezeRender()
	{
		if ((Object)(object)m_renderCamera != (Object)null)
		{
			((Behaviour)m_renderCamera).enabled = false;
		}
	}
}
