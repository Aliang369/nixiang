using Spine.Unity;
using UnityEngine;

public class zbCardSpineOption : MonoBehaviour
{
	public Transform cardRenderRoot;

	public zbRenderTarget renderTarget;

	public SkeletonAnimation skeletonAnimation;

	public float m_maxRotX = 40f;

	public float m_maxRotY = 30f;

	public float m_sensitivity = 0.3f;

	private Vector3 m_initialRotation;

	private void Awake()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cardRenderRoot == (Object)null)
		{
			cardRenderRoot = ((Component)this).transform;
		}
		if ((Object)(object)renderTarget == (Object)null)
		{
			renderTarget = ((Component)((Component)this).transform.parent.Find("rendertarget")).GetComponent<zbRenderTarget>();
			if (Object.op_Implicit((Object)(object)renderTarget))
			{
				((Component)renderTarget).gameObject.SetActive(false);
			}
		}
		if ((Object)(object)skeletonAnimation == (Object)null)
		{
			skeletonAnimation = ((Component)((Component)this).transform.Find("cardspine")).GetComponent<SkeletonAnimation>();
		}
		m_initialRotation = cardRenderRoot.localEulerAngles;
	}

	public void SetRenderSpine(int width, int height)
	{
		if (Object.op_Implicit((Object)(object)renderTarget))
		{
			renderTarget.SetCameraPadding(1.5f);
			renderTarget.StartRender(((Component)skeletonAnimation).gameObject, width, height);
		}
	}

	public void StopRender()
	{
		if (Object.op_Implicit((Object)(object)renderTarget))
		{
			renderTarget.StopRender();
		}
	}

	public RenderTexture GetRenderTexture()
	{
		if (Object.op_Implicit((Object)(object)renderTarget))
		{
			return renderTarget.GetRenderTexture();
		}
		return null;
	}

	public void OnSwipe(Vector2 delta)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)cardRenderRoot == (Object)null))
		{
			float num = (0f - delta.x) * m_sensitivity;
			float num2 = delta.y * m_sensitivity;
			num2 = Mathf.Clamp(num2, 0f - m_maxRotX, m_maxRotX);
			num = Mathf.Clamp(num, 0f - m_maxRotY, m_maxRotY);
			cardRenderRoot.localEulerAngles = m_initialRotation + new Vector3(num2, num, 0f);
		}
	}

	public void OnSwipeEnd()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cardRenderRoot != (Object)null)
		{
			cardRenderRoot.localEulerAngles = m_initialRotation;
		}
	}
}
