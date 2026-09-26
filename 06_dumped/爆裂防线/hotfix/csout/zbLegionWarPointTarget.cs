using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarPointTarget : MonoBehaviour
{
	private Camera m_renderCamera;

	private Image m_descImg;

	private float m_defaultSize = 1f;

	private void Awake()
	{
		m_renderCamera = ((Component)((Component)this).transform.Find("Camera")).GetComponent<Camera>();
	}

	public void StartRender(Image _descImg, Vector2 pos2)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		m_descImg = _descImg;
		((Component)m_renderCamera).transform.position = new Vector3(pos2.x, pos2.y, -10f);
		m_renderCamera.orthographicSize = m_defaultSize * 2f;
		((Component)this).gameObject.SetActive(true);
		m_descImg.image = (Texture)(object)m_renderCamera.targetTexture;
	}

	public void StopRender()
	{
		m_descImg = null;
		((Component)this).gameObject.SetActive(false);
	}
}
