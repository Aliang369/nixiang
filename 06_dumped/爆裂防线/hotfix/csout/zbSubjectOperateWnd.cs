using System;
using System.Collections;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbSubjectOperateWnd : zbBaseWnd
{
	private SpineVisualElement m_spine;

	private VisualElement m_bg;

	private Label m_lbTitle;

	private Button m_btnGoto;

	private VisualElement m_btnTip;

	private VisualElement m_spriteOk;

	private Button m_btnTouch;

	private EOperateType m_operateType;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "btn_goto", (string)null);
		m_btnTip = UQueryExtensions.Q<VisualElement>(m_RootElement, "tip", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action<ClickEvent>)OnGoto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnTip, (Action<ClickEvent>)OnTip, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(EOperateType operateType, Action<bool> callback)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		m_operateType = operateType;
		m_callback = callback;
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_bg.style.opacity = StyleFloat.op_Implicit(0f);
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_operateType == EOperateType.LimitTimeCatcher)
		{
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.11343");
			zbUtil.RefreshSpineUI(m_spine, "animation/ui/huodong4/ui_wawaji/ui_wawaji_SkeletonData.asset");
		}
		else if (m_operateType == EOperateType.ZhanLingKill)
		{
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.11344");
			zbUtil.RefreshSpineUI(m_spine, "animation/ui/huodong4/ui_jisha/ui_jisha_SkeletonData.asset");
		}
		m_spine.AnimationState.SetAnimation(0, "in", false);
		m_spine.AnimationState.AddAnimation(0, "loop", true, 0f);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
		((MonoBehaviour)this).StartCoroutine(FadeInEx());
	}

	private IEnumerator FadeInEx()
	{
		yield return (object)new WaitForSeconds(1f / 3f);
		((MonoBehaviour)this).StartCoroutine(FadeCoroutine(m_bg, 0f, 1f, 1f / 3f));
		yield return (object)new WaitForSeconds(13f / 30f);
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
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spine.AnimationState.SetAnimation(0, "out", false).Complete += new TrackEntryDelegate(OnOutAnimationComplete);
	}

	private void OnOutAnimationComplete(TrackEntry trackEntry)
	{
		if (trackEntry.Animation.Name == "out")
		{
			OnClose();
			m_callback?.Invoke(obj: true);
		}
	}

	private void OnClose()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			zbGameSave.Instance.SetString("SubjectOperateDate" + zbSelf.Instance.GetRoleId(), zbSelf.Instance.timeHandler.GetServerDateStr());
		}
		Hide();
	}

	private void OnGoto(ClickEvent evt)
	{
		if (m_operateType == EOperateType.LimitTimeCatcher)
		{
			foreach (LimitTimeCatcherDetail limitTimeCatcher in zbServerData.sGameData.OperateInfo.LimitTimeCatcherList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeCatcher.BaseInfo))
				{
					zbUIMgr.Instance.ShowTimeCatcher(limitTimeCatcher);
					break;
				}
			}
		}
		else if (m_operateType == EOperateType.ZhanLingKill)
		{
			foreach (ZhanLingKillDetail zhanLingKill in zbServerData.sGameData.OperateInfo.ZhanLingKillList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(zhanLingKill.BaseInfo))
				{
					zbUIMgr.Instance.ShowZhanLingKill(zhanLingKill);
					break;
				}
			}
		}
		OnClose();
	}

	private void OnTip(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
	}
}
