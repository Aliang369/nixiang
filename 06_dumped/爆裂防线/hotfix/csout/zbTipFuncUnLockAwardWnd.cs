using System;
using System.Collections;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTipFuncUnLockAwardWnd : zbBaseWnd
{
	private VisualElement m_btnTouch;

	private VisualElement m_root;

	private SpineVisualElement m_spineBg;

	private Label m_lbTitle;

	private VisualElement m_prePeerageInfo;

	private VisualElement m_curPeerageInfo;

	private Label m_lbAwardTitle;

	private Label m_lbFuncName;

	private Label m_lbDesc;

	private EFuncType m_unLockType;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<VisualElement>(m_RootElement, "btn_touch", (string)null);
		m_root = UQueryExtensions.Q<VisualElement>(m_RootElement, "root", (string)null);
		m_lbFuncName = UQueryExtensions.Q<Label>(m_RootElement, "lb_funcName", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_spineBg = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_bg", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbAwardTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_awardTitle", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnTouch, (Action)OnTouchMask, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(EFuncType unLockType)
	{
		m_spineBg.AnimationState.SetAnimation(0, "in", false);
		m_spineBg.AnimationState.AddAnimation(0, "loop", true, 0f);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_success.ogg");
		((MonoBehaviour)this).StartCoroutine(DelayShowBg());
		m_unLockType = unLockType;
		InitFuncType();
	}

	private IEnumerator DelayShowBg()
	{
		yield return (object)new WaitForSeconds(0.16f);
		SetVisible(visible: true);
		((VisualElement)m_spineBg).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private void OnTouchMask()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		SetVisible(visible: false);
		m_spineBg.AnimationState.SetAnimation(0, "out", false).Complete += new TrackEntryDelegate(OnOutAnimationComplete);
	}

	private void OnOutAnimationComplete(TrackEntry trackEntry)
	{
		if (trackEntry.Animation.Name == "out")
		{
			Hide();
		}
	}

	private void InitFuncType()
	{
		zbFunctionUnlockCsvRecord recordByKey = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)m_unLockType);
		((TextElement)m_lbFuncName).text = zbUtil.Language(recordByKey.name);
		((TextElement)m_lbDesc).text = zbUtil.Language(recordByKey.desc);
	}

	private void SetVisible(bool visible)
	{
		((VisualElement)m_lbTitle).visible = visible;
		((VisualElement)m_lbFuncName).visible = visible;
		((VisualElement)m_lbDesc).visible = visible;
	}
}
