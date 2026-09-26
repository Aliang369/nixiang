using UnityEngine;

public class zbSceneParallax : MonoBehaviour
{
	public float parallaxFactor = 0.5f;

	private Transform m_mainCameraT;

	private Vector3 m_previousCamPos;

	private void Start()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		m_mainCameraT = ((Component)zbCameraHelper.Instance.mainCamera).transform;
		m_previousCamPos = m_mainCameraT.position;
	}

	private void LateUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = m_mainCameraT.position - m_previousCamPos;
		Transform transform = ((Component)this).transform;
		transform.position += new Vector3(val.x * parallaxFactor, 0f, 0f);
		m_previousCamPos = m_mainCameraT.position;
	}
}
