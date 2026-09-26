using UnityEngine;
using UnityEngine.UIElements;

public class zbTouchBlockWnd : zbBaseWnd
{
	private VisualElement m_rootEffect;

	private float m_hideTime = -1f;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_rootEffect = UQueryExtensions.Q<VisualElement>(m_RootElement, "container", (string)null);
	}

	public void RefreshCustom(float hideTm)
	{
		if (hideTm > 0f)
		{
			m_hideTime = Time.unscaledTime + hideTm;
		}
		else
		{
			m_hideTime = Time.unscaledTime;
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_hideTime > 0f && Time.unscaledTime >= m_hideTime)
		{
			m_hideTime = -1f;
			Hide();
		}
	}
}
