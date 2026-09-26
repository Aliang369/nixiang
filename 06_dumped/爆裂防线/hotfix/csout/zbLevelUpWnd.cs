using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLevelUpWnd : zbBaseWnd
{
	private SpineVisualElement m_spineBg;

	private Label m_lbLevel;

	private Button m_btnTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineBg = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_lbLevel).style.opacity = StyleFloat.op_Implicit(0f);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((TextElement)m_lbLevel).text = zbSelf.Instance.GetLevel().ToString();
		m_spineBg.AnimationState.SetAnimation(0, "in", false);
		m_spineBg.AnimationState.AddAnimation(0, "loop", true, 0f);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
		((MonoBehaviour)this).StartCoroutine(FadeInEx());
	}

	private IEnumerator FadeInEx()
	{
		((MonoBehaviour)this).StartCoroutine(FadeCoroutine((VisualElement)(object)m_lbLevel, 0f, 1f, 0.33f));
		yield return (object)new WaitForSecondsRealtime(1f);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
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

	public void OnTouch(ClickEvent evt)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spineBg.AnimationState.SetAnimation(0, "out", false).Complete += new TrackEntryDelegate(OnOutAnimationComplete);
	}

	private void OnOutAnimationComplete(TrackEntry trackEntry)
	{
		if (trackEntry.Animation.Name == "out")
		{
			Hide();
			zbSelf.Instance.funHandler.CheckNewFunc(isLevelUp: true);
		}
	}
}
