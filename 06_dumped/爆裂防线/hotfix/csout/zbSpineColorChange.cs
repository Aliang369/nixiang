using Spine;
using Spine.Unity;
using UnityEngine;

public class zbSpineColorChange
{
	private SkeletonRenderer m_skeletonRenderer;

	private Color m_normalColor;

	private Color m_redColor;

	private float m_lerpTime;

	private float m_lerpTimeKeep;

	private int m_Lerping;

	private Slot slot;

	private bool useSlot;

	public zbSpineGpuAnimPlayer gpuTintSink;

	private void WriteColor(Color color)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (gpuTintSink != null)
		{
			gpuTintSink.SetTint(color);
		}
		else if (Object.op_Implicit((Object)(object)m_skeletonRenderer))
		{
			SkeletonExtensions.SetColor(m_skeletonRenderer.skeleton, color);
		}
	}

	public void Reset()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		WriteColor(m_normalColor);
		m_Lerping = 0;
	}

	public void Init(SkeletonRenderer renderer, bool _useSlot = false)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		useSlot = _useSlot;
		m_skeletonRenderer = renderer;
		if (useSlot)
		{
			slot = m_skeletonRenderer.skeleton.FindSlot("01");
			if (slot != null)
			{
				m_normalColor = SkeletonExtensions.GetColor(slot);
				return;
			}
			useSlot = false;
			m_normalColor = SkeletonExtensions.GetColor(m_skeletonRenderer.skeleton);
		}
		else
		{
			m_normalColor = SkeletonExtensions.GetColor(m_skeletonRenderer.skeleton);
		}
	}

	public void LerpChangeColor()
	{
		if (m_Lerping == 0)
		{
			m_Lerping = 1;
			m_lerpTimeKeep = 0f;
		}
	}

	public void DeltaUpdate()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		if (useSlot)
		{
			if (m_Lerping == 1)
			{
				if (m_lerpTimeKeep < m_lerpTime)
				{
					m_lerpTimeKeep += Time.deltaTime;
					float num = Mathf.Clamp01(m_lerpTimeKeep / m_lerpTime);
					m_redColor.a = slot.A;
					m_normalColor.a = slot.A;
					SkeletonExtensions.SetColor(slot, Color.Lerp(m_normalColor, m_redColor, num));
					return;
				}
				m_lerpTimeKeep = 0f;
				m_Lerping = 2;
			}
			if (m_Lerping == 2 && m_lerpTimeKeep < m_lerpTime)
			{
				m_lerpTimeKeep += Time.deltaTime;
				float num2 = Mathf.Clamp01(m_lerpTimeKeep / m_lerpTime);
				m_redColor.a = slot.A;
				m_normalColor.a = slot.A;
				SkeletonExtensions.SetColor(slot, Color.Lerp(m_redColor, m_normalColor, num2));
				return;
			}
		}
		else
		{
			if (m_Lerping == 1)
			{
				if (m_lerpTimeKeep < m_lerpTime)
				{
					m_lerpTimeKeep += Time.deltaTime;
					float num3 = Mathf.Clamp01(m_lerpTimeKeep / m_lerpTime);
					float a = ((gpuTintSink != null) ? 1f : m_skeletonRenderer.skeleton.A);
					m_redColor.a = a;
					m_normalColor.a = a;
					WriteColor(Color.Lerp(m_normalColor, m_redColor, num3));
					return;
				}
				m_lerpTimeKeep = 0f;
				m_Lerping = 2;
			}
			if (m_Lerping == 2 && m_lerpTimeKeep < m_lerpTime)
			{
				m_lerpTimeKeep += Time.deltaTime;
				float num4 = Mathf.Clamp01(m_lerpTimeKeep / m_lerpTime);
				float a2 = ((gpuTintSink != null) ? 1f : m_skeletonRenderer.skeleton.A);
				m_redColor.a = a2;
				m_normalColor.a = a2;
				WriteColor(Color.Lerp(m_redColor, m_normalColor, num4));
				return;
			}
		}
		m_Lerping = 0;
	}

	public zbSpineColorChange()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_redColor = Color.red;
		m_lerpTime = 0.2f;
		base._002Ector();
	}
}
