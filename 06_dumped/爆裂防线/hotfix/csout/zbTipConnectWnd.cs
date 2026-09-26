using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTipConnectWnd : zbBaseWnd
{
	private VisualElement m_rootEffect;

	private float m_lastRealtime;

	private const float NET_TIMEOUT = 10f;

	private bool m_isResponse;

	private bool m_needTimeOutClose = true;

	private Coroutine m_coroutine;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_rootEffect = UQueryExtensions.Q<VisualElement>(m_RootElement, "container", (string)null);
	}

	public void RefreshCustom(bool bNeedTimeOutClose = true, float effectDelay = 1.5f)
	{
		if (effectDelay > 0f)
		{
			SetTipRootVisible(visible: false);
		}
		else
		{
			SetTipRootVisible(visible: true);
		}
		m_needTimeOutClose = bNeedTimeOutClose;
		m_lastRealtime = Time.realtimeSinceStartup;
		m_isResponse = false;
		if (m_coroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
		m_coroutine = ((MonoBehaviour)this).StartCoroutine(StartShowEffect(effectDelay));
	}

	private IEnumerator StartShowEffect(float delayTm)
	{
		yield return (object)new WaitForSecondsRealtime(delayTm);
		SetTipRootVisible(visible: true);
	}

	private void SetTipRootVisible(bool visible)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (m_rootEffect != null)
		{
			m_rootEffect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
		}
	}

	public void SetResponsed(bool responsed)
	{
		m_isResponse = responsed;
		OnClose();
	}

	private void TimeOutClose()
	{
		_ = m_isResponse;
		SetTipRootVisible(visible: false);
		OnClose();
	}

	private void OnClose()
	{
		SetTipRootVisible(visible: false);
		if (m_coroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup - m_lastRealtime > 10f && m_needTimeOutClose)
		{
			TimeOutClose();
			m_lastRealtime = realtimeSinceStartup;
		}
	}
}
