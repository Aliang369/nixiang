using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossPeakBestRankWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private VisualElement m_ui;

	private SpineVisualElement m_spine;

	private Label m_lbNewRank;

	private Coroutine m_stateCoroutine;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnTouchTest, "", (TrickleDown)0);
		m_ui = UQueryExtensions.Q<VisualElement>(m_RootElement, "ui", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_lbNewRank = UQueryExtensions.Q<Label>(m_RootElement, "lb_rank", (string)null);
		InitUI();
		AddDark();
	}

	private void InitUI()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		m_ui.style.opacity = StyleFloat.op_Implicit(0f);
		m_ui.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_ui.style.transitionProperty = StyleList<StylePropertyName>.op_Implicit(new List<StylePropertyName>
		{
			new StylePropertyName("opacity")
		});
		m_ui.style.transitionDuration = StyleList<TimeValue>.op_Implicit(new List<TimeValue>
		{
			new TimeValue(0.5f)
		});
	}

	public void RefreshCustom()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		zbUIMgr.Instance.ShowModel();
		((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_ui.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		RefreshDetail();
		m_spine.AnimationState.SetAnimation(0, "enter", false);
		m_spine.AnimationState.AddAnimation(0, "loop", true, 0f);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_stateCoroutine = ((MonoBehaviour)this).StartCoroutine(FadeIn());
	}

	private IEnumerator FadeIn()
	{
		yield return (object)new WaitForSeconds(1.3f);
		m_ui.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return null;
		m_ui.style.opacity = StyleFloat.op_Implicit(1f);
		yield return (object)new WaitForSeconds(0.5f);
		zbUIMgr.Instance.HideModel();
		((VisualElement)m_btnTouchTest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private void RefreshDetail()
	{
		((TextElement)m_lbNewRank).text = zbSelf.Instance.crossPeakHandler.settleRankInfo.NewBestRank.ToString();
	}

	private void OnTouchTest()
	{
		zbSelf.Instance.crossPeakHandler.settleRankInfo = null;
		Hide();
	}
}
