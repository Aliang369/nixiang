using UnityEngine;

public class zbRenderTarget : MonoBehaviour
{
	private Camera m_renderCamera;

	private RenderTexture m_renderTexture;

	public int textureWidth = 128;

	public int textureHeight = 128;

	private GameObject m_renderTarget;

	private Vector3 m_cameraOffset;

	private float m_cameraPadding = 1f;

	private void Awake()
	{
		m_renderCamera = ((Component)((Component)this).transform.Find("Camera")).GetComponent<Camera>();
	}

	public void SetCameraPadding(float value)
	{
		m_cameraPadding = Mathf.Max(1f, value);
	}

	public void StartRender(GameObject renderTarget, int textureW, int textureH)
	{
		textureWidth = textureW;
		textureHeight = textureH;
		m_renderTarget = renderTarget;
		if ((Object)(object)m_renderTarget == (Object)null)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		((Component)this).gameObject.SetActive(true);
		CreateRenderTexture();
		AdjustCameraToFit();
		UpdateCamera();
	}

	public void StartRender(Transform targetPoint, int textureW, int textureH)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		textureWidth = textureW;
		textureHeight = textureH;
		if ((Object)(object)targetPoint == (Object)null)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		((Component)this).gameObject.SetActive(true);
		m_renderTarget = null;
		CreateRenderTexture();
		AdjustCameraToArea(targetPoint.position, (float)textureW / 50f, (float)textureH / 50f);
	}

	private void AdjustCameraToArea(Vector3 center, float width, float height)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		((Component)m_renderCamera).transform.position = new Vector3(center.x, center.y, -10f);
		float num = (float)m_renderCamera.pixelWidth / (float)m_renderCamera.pixelHeight;
		float num2 = height * m_cameraPadding / 2f;
		float num3 = width * m_cameraPadding / (2f * num);
		m_renderCamera.orthographicSize = Mathf.Max(num2, num3);
		m_cameraOffset = Vector3.zero;
	}

	public void StopRender()
	{
		if ((Object)(object)m_renderCamera != (Object)null)
		{
			m_renderCamera.targetTexture = null;
		}
		if ((Object)(object)m_renderTexture != (Object)null)
		{
			m_renderTexture.Release();
			Object.Destroy((Object)(object)m_renderTexture);
			m_renderTexture = null;
		}
		((Component)this).gameObject.SetActive(false);
	}

	private void CreateRenderTexture()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		if ((Object)(object)m_renderTexture != (Object)null)
		{
			m_renderTexture.Release();
			Object.Destroy((Object)(object)m_renderTexture);
		}
		m_renderTexture = new RenderTexture(textureWidth, textureHeight, 16, (RenderTextureFormat)0);
		m_renderTexture.Create();
		m_renderCamera.targetTexture = m_renderTexture;
	}

	private void AdjustCameraToFit()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = GetBounds();
		Vector3 center = ((Bounds)(ref bounds)).center;
		Vector3 size = ((Bounds)(ref bounds)).size;
		Vector3 position = m_renderTarget.transform.position;
		((Component)m_renderCamera).transform.position = new Vector3(center.x, center.y, -10f);
		m_cameraOffset = ((Component)m_renderCamera).transform.position - position;
		float num = (float)m_renderCamera.pixelWidth / (float)m_renderCamera.pixelHeight;
		float num2 = size.y * m_cameraPadding / 2f;
		float num3 = size.x * m_cameraPadding / (2f * num);
		m_renderCamera.orthographicSize = Mathf.Max(num2, num3);
	}

	private void UpdateCamera()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_renderTarget == (Object)null))
		{
			Vector3 position = m_renderTarget.transform.position;
			((Component)m_renderCamera).transform.position = position + m_cameraOffset;
		}
	}

	private Bounds GetBounds()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Renderer[] componentsInChildren = m_renderTarget.GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length == 0)
		{
			return new Bounds(m_renderTarget.transform.position, Vector3.zero);
		}
		Bounds bounds = componentsInChildren[0].bounds;
		Renderer[] array = componentsInChildren;
		foreach (Renderer val in array)
		{
			((Bounds)(ref bounds)).Encapsulate(val.bounds);
		}
		return bounds;
	}

	public RenderTexture GetRenderTexture()
	{
		return m_renderTexture;
	}

	private void LateUpdate()
	{
		if ((Object)(object)m_renderTarget != (Object)null)
		{
			UpdateCamera();
		}
	}
}
