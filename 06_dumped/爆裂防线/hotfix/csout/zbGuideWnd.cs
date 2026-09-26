using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGuideWnd : zbBaseWnd
{
	private Action onFinished;

	private VisualElement m_nodeFinger;

	private Button m_btnFinger;

	private SpineVisualElement m_spineFinger;

	private VisualElement watchNode;

	[NonSerialized]
	public Vector2 killCountPoint;

	private Button m_btnTouchTest;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineFinger = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_finger", (string)null);
		m_nodeFinger = UQueryExtensions.Q<VisualElement>(m_RootElement, "_finger", (string)null);
		m_btnFinger = UQueryExtensions.Q<Button>(m_RootElement, "sp_finger", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFinger, (Action<ClickEvent>)OnTouchFinger, "", (TrickleDown)0);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(VisualElement _watchNode, Action _onFinished = null)
	{
		watchNode = _watchNode;
		onFinished = _onFinished;
		((MonoBehaviour)this).StartCoroutine(ActionCoroutine());
	}

	public IEnumerator ActionCoroutine()
	{
		yield return null;
		RefreshDetail();
	}

	public void RefreshDetail()
	{
		zbUtil.SetNodePosition(m_nodeFinger, watchNode);
		PlayAnimation2();
	}

	public void OnTouchFinger(ClickEvent evt)
	{
		Hide();
		onFinished?.Invoke();
	}

	public void PlayAnimation2()
	{
		TrackEntry current = m_spineFinger.AnimationState.GetCurrent(0);
		if (current == null || !(current.Animation.Name == "animation2") || current.IsComplete)
		{
			TrackEntry val = m_spineFinger.AnimationState.SetAnimation(0, "animation2", false);
			if (val != null)
			{
				m_spineFinger.AnimationState.AddAnimation(0, "animation", true, val.AnimationEnd);
			}
			else
			{
				m_spineFinger.AnimationState.AddAnimation(0, "animation", true, 1.6666666f);
			}
		}
	}

	public void OnTouchTest(ClickEvent evt)
	{
		PlayAnimation2();
	}
}
