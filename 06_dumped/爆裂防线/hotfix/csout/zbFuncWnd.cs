using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFuncWnd : zbBaseWnd
{
	private VisualElement m_bg;

	private Label m_lbFunc;

	private Button m_btnFunc;

	private Button m_btnTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbFunc = UQueryExtensions.Q<Label>(m_RootElement, "lb_func", (string)null);
		m_btnFunc = UQueryExtensions.Q<Button>(m_RootElement, "btn_func", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFunc, (Action<ClickEvent>)m_OnFunc, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
	}

	public void RefreshCustom(zbFunctionUnlockCsvRecord record)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		m_bg.style.opacity = StyleFloat.op_Implicit(0f);
		((VisualElement)m_btnTouch).visible = false;
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(record.icon);
		((VisualElement)m_btnFunc).style.backgroundImage = new StyleBackground(val);
		((TextElement)m_lbFunc).text = record.name;
		((MonoBehaviour)this).StartCoroutine(FadeInEx());
	}

	private IEnumerator FadeInEx()
	{
		((MonoBehaviour)this).StartCoroutine(FadeCoroutine(m_bg, 0f, 1f, 0.33f));
		yield return (object)new WaitForSeconds(0.33f);
		((VisualElement)m_btnTouch).visible = true;
	}

	private IEnumerator FadeCoroutine(VisualElement element, float startOpacity, float endOpacity, float duration)
	{
		float time = 0f;
		while (time < duration)
		{
			float num = Mathf.SmoothStep(startOpacity, endOpacity, time / duration);
			element.style.opacity = StyleFloat.op_Implicit(num);
			time += Time.deltaTime;
			yield return null;
		}
		element.style.opacity = StyleFloat.op_Implicit(endOpacity);
	}

	public void m_OnFunc(ClickEvent evt)
	{
		Hide();
	}

	public void OnTouch(ClickEvent evt)
	{
		Hide();
	}
}
