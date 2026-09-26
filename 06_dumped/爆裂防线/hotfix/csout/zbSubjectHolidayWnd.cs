using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSubjectHolidayWnd : zbBaseWnd
{
	private SpineVisualElement m_spine;

	private VisualElement m_bg;

	private VisualElement m_nodeAward;

	private VisualElement m_spriteAward;

	private Label m_lbAward;

	private VisualElement m_btnAward;

	private VisualElement m_notify;

	private Label m_lbTitle;

	private Button m_btnGoto;

	private VisualElement m_btnTip;

	private VisualElement m_spriteOk;

	private Button m_btnTouch;

	private Action<bool> m_callback;

	private RestaurantDetail m_restaurantDetail;

	private EndlessMineDetail m_endlessMineDetail;

	private SnowVeilDetail m_snowVeilDetail;

	protected override void OnEnable()
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_nodeAward = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_award", (string)null);
		m_spriteAward = UQueryExtensions.Q<VisualElement>(m_nodeAward, "bg_award", (string)null);
		m_lbAward = UQueryExtensions.Q<Label>(m_nodeAward, "lb_award", (string)null);
		m_btnAward = UQueryExtensions.Q<VisualElement>(m_nodeAward, "btn_award", (string)null);
		m_notify = zbUtil.CreateNotifyTag(m_btnAward, (Vector2?)new Vector2(20f, 15f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "btn_goto", (string)null);
		m_btnTip = UQueryExtensions.Q<VisualElement>(m_RootElement, "tip", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnAward, (Action<ClickEvent>)OnAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action<ClickEvent>)OnGoto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnTip, (Action<ClickEvent>)OnTip, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_RestaurantActivateAward_Ack, OnAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SnowVeilActivate_Ack, OnAck);
	}

	public void RefreshCustom(Action<bool> callback)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		m_callback = callback;
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_bg.style.opacity = StyleFloat.op_Implicit(0f);
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_restaurantDetail = zbSelf.Instance.restaurantHandler.GetRestaurantDetail();
		m_endlessMineDetail = zbSelf.Instance.endlessMineHandler.GetEndlessMineDetail();
		m_snowVeilDetail = zbSelf.Instance.snowVeilHandler.GetSnowVeilDetail();
		if (m_restaurantDetail != null)
		{
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.11298");
			zbUtil.RefreshSpineUI(m_spine, "animation/ui/ui_cantinghd/ui_cantinghd_SkeletonData.asset");
			if (m_restaurantDetail.ActivateAward == EBoolState.False)
			{
				m_nodeAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_btnGoto).text = zbUtil.Language("lang.10090");
				int[][] array = zbDataConfig.Instance.restaurantBaseJsonTable.GetRecord()["activateAward"].ToObject<int[][]>();
				if (array.Length != 0 && array[0] != null)
				{
					int[] array2 = array[0];
					stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)array2[0], array2[1]);
					m_spriteAward.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
					((TextElement)m_lbAward).text = "x" + array2[2];
				}
			}
			else
			{
				m_nodeAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((TextElement)m_btnGoto).text = zbUtil.Language("lang.10155");
			}
		}
		else if (m_endlessMineDetail != null)
		{
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.11299");
			zbUtil.RefreshSpineUI(m_spine, "animation/ui/huodong4/ui_wakuang/ui_wakuang_SkeletonData.asset");
			m_nodeAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_btnGoto).text = zbUtil.Language("lang.10155");
		}
		else if (m_snowVeilDetail != null)
		{
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.11300");
			zbUtil.RefreshSpineUI(m_spine, "animation/ui/huodong4/ui_feixu/ui_feixu_SkeletonData.asset");
			if (m_snowVeilDetail.ActivateAward == EBoolState.False)
			{
				m_nodeAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_btnGoto).text = zbUtil.Language("lang.10090");
				int[][] array3 = zbDataConfig.Instance.snowVeilBaseJsonTable.GetRecord()["activateAward"].ToObject<int[][]>();
				if (array3.Length != 0 && array3[0] != null)
				{
					int[] array4 = array3[0];
					stResorceIconInfo resorceIconPath2 = zbResourceHelper.GetResorceIconPath((EResourceType)array4[0], array4[1]);
					m_spriteAward.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath2.strIconPath));
					((TextElement)m_lbAward).text = "x" + array4[2];
				}
			}
			else
			{
				m_nodeAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((TextElement)m_btnGoto).text = zbUtil.Language("lang.10155");
			}
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
			zbGameSave.Instance.SetString("SubjectHolidayDate" + zbSelf.Instance.GetRoleId(), zbSelf.Instance.timeHandler.GetServerDateStr());
		}
		Hide();
	}

	private void OnAward(ClickEvent evt)
	{
		if (m_restaurantDetail != null)
		{
			zbWsMsgHandler.Instance.Operate_RestaurantActivateAward_Req(m_restaurantDetail.BaseInfo.UUID);
		}
		else if (m_snowVeilDetail != null)
		{
			zbWsMsgHandler.Instance.Operate_SnowVeilActivate_Req(m_snowVeilDetail.BaseInfo.UUID);
		}
	}

	private void OnGoto(ClickEvent evt)
	{
		if (m_restaurantDetail != null)
		{
			if (m_restaurantDetail.ActivateAward == EBoolState.False)
			{
				zbWsMsgHandler.Instance.Operate_RestaurantActivateAward_Req(m_restaurantDetail.BaseInfo.UUID);
				return;
			}
			zbUIMgr.Instance.ShowRestaurant(m_restaurantDetail);
			OnClose();
		}
		else if (m_endlessMineDetail != null)
		{
			zbWsMsgHandler.Instance.EndlessMine_Data_Req(openWnd: true);
			OnClose();
		}
		else if (m_snowVeilDetail != null)
		{
			if (m_snowVeilDetail.ActivateAward == EBoolState.False)
			{
				zbWsMsgHandler.Instance.Operate_SnowVeilActivate_Req(m_snowVeilDetail.BaseInfo.UUID);
				return;
			}
			zbUIMgr.Instance.ShowSnowVeil(m_snowVeilDetail);
			OnClose();
		}
		else
		{
			OnClose();
		}
	}

	private void OnTip(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
	}

	private void OnAck(object data)
	{
		if (data is OperateRestaurantActivateAwardAck operateRestaurantActivateAwardAck)
		{
			zbSelf.Instance.dataHandler.DataChange(operateRestaurantActivateAwardAck.DataChange);
			zbUIMgr.Instance.ShowAward(((IEnumerable<AwardDetail>)operateRestaurantActivateAwardAck.AwardList).ToArray(), OnCallBackRestaurant);
		}
		else if (data is OperateSnowVeilActivateAck operateSnowVeilActivateAck)
		{
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilActivateAck.DataChange);
			zbUIMgr.Instance.ShowAward(((IEnumerable<AwardDetail>)operateSnowVeilActivateAck.AwardList).ToArray(), OnCallBackSnowVeil);
		}
	}

	private void OnCallBackRestaurant(bool isOK)
	{
		m_restaurantDetail = zbSelf.Instance.operateHandler.GetRestaurantByUUID(m_restaurantDetail.BaseInfo.UUID);
		zbUIMgr.Instance.ShowRestaurant(m_restaurantDetail);
		OnClose();
	}

	private void OnCallBackSnowVeil(bool isOK)
	{
		m_snowVeilDetail = zbSelf.Instance.operateHandler.GetSnowVeilByUUID(m_snowVeilDetail.BaseInfo.UUID);
		zbUIMgr.Instance.ShowSnowVeil(m_snowVeilDetail);
		OnClose();
	}
}
