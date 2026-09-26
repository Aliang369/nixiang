using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFightValueUpWnd : zbBaseWnd
{
	private SpineVisualElement m_spine;

	private Bone m_bone;

	private VisualElement m_ui;

	private Label m_lbFight;

	private Label m_lbAdd;

	private bool m_actionAdd;

	private int m_curFv;

	private int m_maxFv;

	private int m_addStep = 1;

	private Coroutine m_stateCoroutine;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_ui = UQueryExtensions.Q<VisualElement>(m_RootElement, "ui", (string)null);
		m_lbFight = UQueryExtensions.Q<Label>(m_RootElement, "lb_fight", (string)null);
		m_lbAdd = UQueryExtensions.Q<Label>(m_RootElement, "lb_add", (string)null);
	}

	public void RefreshCustom(int lastFv)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (m_spine != null)
		{
			m_bone = m_spine.skeleton.FindBone("bone");
		}
		m_actionAdd = false;
		m_maxFv = zbSelf.Instance.GetFightValue();
		m_curFv = lastFv;
		m_addStep = (int)Math.Ceiling((double)(m_maxFv - lastFv) * 0.04);
		m_ui.style.opacity = StyleFloat.op_Implicit(0f);
		((TextElement)m_lbFight).text = m_curFv.ToString();
		((TextElement)m_lbAdd).text = "+" + (m_maxFv - lastFv);
		m_spine.AnimationState.SetAnimation(0, "animation", false);
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_stateCoroutine = ((MonoBehaviour)this).StartCoroutine(FadeInAndOut());
	}

	private IEnumerator FadeInAndOut()
	{
		((MonoBehaviour)this).StartCoroutine(FadeCoroutine(0f, 1f, 0.33f));
		yield return (object)new WaitForSeconds(1.33f);
		m_actionAdd = true;
		yield return (object)new WaitForSeconds(0.5f);
		((MonoBehaviour)this).StartCoroutine(FadeCoroutine(1f, 0f, 0.67f));
		yield return (object)new WaitForSeconds(0.5f);
		Hide();
	}

	private IEnumerator FadeCoroutine(float startOpacity, float endOpacity, float duration)
	{
		float time = 0f;
		while (time < duration)
		{
			float num = Mathf.Lerp(startOpacity, endOpacity, time / duration);
			m_ui.style.opacity = StyleFloat.op_Implicit(num);
			time += Time.deltaTime;
			yield return null;
		}
		m_ui.style.opacity = StyleFloat.op_Implicit(endOpacity);
	}

	protected override void UpdateAtShow()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		if (m_spine == null || m_spine.skeleton == null || m_ui == null)
		{
			return;
		}
		m_bone = m_spine.skeleton.FindBone("bone");
		if (m_bone == null)
		{
			return;
		}
		m_ui.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_ui.style.top = StyleLength.op_Implicit(730f - m_bone.Y * 110f);
		if (!m_actionAdd)
		{
			return;
		}
		if (m_curFv != m_maxFv)
		{
			m_curFv += m_addStep;
			if (m_curFv >= m_maxFv)
			{
				m_curFv = m_maxFv;
			}
			((TextElement)m_lbFight).text = m_curFv.ToString();
		}
		else
		{
			m_actionAdd = false;
		}
	}
}
